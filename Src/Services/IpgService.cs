namespace SinaMN75U.Services;

public interface IIpgService {
	// Opens the gateway: returns the url the app must show in a WebView (real PNA page, or our test page in Test mode).
	Task<UResponse<IpgPayResponse?>> GetSaleIpgLink(IpgSaleParams p, CancellationToken ct);

	// The callback the gateway redirects to (our Verify endpoint). Does Confirm + credits the wallet, fully server-side.
	Task Verify(string token, short status, string? cardNumberMasked, long? rrn, string additionalData, CancellationToken ct);
}

// ts added so the payer's identity and the charge amount are taken from the JWT/DB, never from the client callback.
// httpContext added so we build our own Gateway/Verify urls from THIS request's host (works on localhost and prod).
public class IpgService(IHttpClientService http, DbContext db, ILocalizationService ls, ITokenService ts, IHttpContextAccessor httpContext) : IIpgService {
	public async Task<UResponse<IpgPayResponse?>> GetSaleIpgLink(IpgSaleParams p, CancellationToken ct) {
		// Identify the payer from the token instead of trusting a client-supplied user id.
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IpgPayResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (p.Amount <= 0) return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("AmountRequired"));

		// Create an authoritative pending order up-front; the credited amount will come from this row, closing the tampering hole.
		string trackingNumber = Guid.CreateVersion7().ToString("N");
		int orderId = Math.Abs(Guid.NewGuid().GetHashCode());
		TxnEntity txn = new() {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = userData.Id,
			UserId = userData.Id,
			Amount = p.Amount,
			TrackingNumber = trackingNumber,
			Tags = [TagTxn.ChargeWallet, TagTxn.Pending],
			JsonData = new BaseJson { Detail1 = "IPG" }
		};
		await db.Set<TxnEntity>().AddAsync(txn, ct);
		await db.SaveChangesAsync(ct);

		// Build absolute urls to OUR OWN Gateway/Verify endpoints from this request (e.g. https://localhost:7110/api/ipg/...).
		HttpRequest request = httpContext.HttpContext!.Request;
		string root = $"{request.Scheme}://{request.Host}";
		string basePath = request.Path.Value![..(request.Path.Value!.LastIndexOf('/') + 1)]; // e.g. /api/ipg/
		string verifyUrl = $"{root}{basePath}Verify";
		string gatewayUrl = $"{root}{basePath}Gateway";

		// Test mode (appsettings "Test": true): skip the bank. We open our own fake gateway PAGE (the Gateway endpoint,
		// NOT the callback) whose two buttons later redirect to the callback. Detail2 "FAKE" tells Verify to skip Confirm.
		if (Core.App.Test) {
			txn.JsonData.Detail2 = "FAKE";
			db.Set<TxnEntity>().Update(txn);
			await db.SaveChangesAsync(ct);
			return new UResponse<IpgPayResponse?>(new IpgPayResponse {
				Url = $"{gatewayUrl}?additionalData={trackingNumber}&amount={(long)p.Amount}",
				TrackingNumber = trackingNumber
			});
		}

		try {
			HttpResponseMessage? response = await http.Post("https://pna.shaparak.ir/mhipg/api/Payment/NormalSale", new {
					CorporationPin = Core.App.Ipg.Token,
					Amount = (long)p.Amount,
					OrderId = orderId,
					CallBackUrl = $"{verifyUrl}?additionalData={trackingNumber}",
					AdditionalData = trackingNumber,
					Originator = userData.PhoneNumber ?? ""
				}
			);

			if (response?.IsSuccessStatusCode ?? false) {
				string responseBody = await response.Content.ReadAsStringAsync(ct);
				JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody);
				if (data.GetProperty("Status").GetInt16() == 0) {
					string gatewayToken = data.GetProperty("Token").GetString()!;
					// Bind this order to the gateway token so a forged callback for someone else's tracking number cannot be accepted.
					txn.JsonData.Detail2 = gatewayToken;
					db.Set<TxnEntity>().Update(txn);
					await db.SaveChangesAsync(ct);
					return new UResponse<IpgPayResponse?>(new IpgPayResponse {
						Url = $"https://pna.shaparak.ir/mhui/home/index/{gatewayToken}",
						TrackingNumber = trackingNumber
					});
				}
			}

			// Gateway rejected the sale: fail the pending order.
			await MarkFailed(txn, ct);
			return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("IpgError"));
		}
		catch (Exception) {
			await MarkFailed(txn, ct);
			return new UResponse<IpgPayResponse?>(null, Usc.InternalServerError, ls.Get("InternalServerError"));
		}
	}

	public async Task Verify(string token, short status, string? cardNumberMasked, long? rrn, string additionalData, CancellationToken ct) {
		// additionalData is the tracking number we sent at pay-time; resolve the order from it.
		if (string.IsNullOrEmpty(additionalData)) return;

		TxnEntity? txn = await db.Set<TxnEntity>().AsTracking().FirstOrDefaultAsync(x => x.TrackingNumber == additionalData, ct);
		if (txn == null) return;

		// Idempotency: if this order was already settled, do nothing (prevents double-crediting on repeated callbacks).
		if (txn.Tags.Contains(TagTxn.Paid)) return;

		// User cancelled or gateway reported failure.
		if (status != 0) {
			await MarkFailed(txn, ct);
			return;
		}

		// The stored gateway token must match the callback token, otherwise this is not a legitimate callback for this order.
		if (string.IsNullOrEmpty(txn.JsonData.Detail2) || txn.JsonData.Detail2 != token) return;

		try {
			// Real gateway must be confirmed before crediting; the fake gateway (token "FAKE") has nothing to confirm.
			if (token != "FAKE") {
				HttpResponseMessage? confirmResponse = await http.Post("https://pna.shaparak.ir/mhipg/api/Payment/confirm", new {
					CorporationPin = Core.App.Ipg.Token,
					Token = token
				});
				if (!(confirmResponse?.IsSuccessStatusCode ?? false)) return;

				string confirmBody = await confirmResponse.Content.ReadAsStringAsync(ct);
				JsonElement confirmData = JsonSerializer.Deserialize<JsonElement>(confirmBody);
				if (confirmData.GetProperty("Status").GetInt16() != 0) return;
			}

			// Mark the order paid using the amount/user fixed at pay-time (trusted), not anything from the callback body.
			txn.Tags = [TagTxn.ChargeWallet, TagTxn.Paid];
			txn.JsonData.Detail1 = $"Card:{cardNumberMasked}";
			txn.JsonData.Detail2 = $"RRN:{rrn}";
			db.Set<TxnEntity>().Update(txn);

			await db.Set<WalletTxnEntity>().AddAsync(new WalletTxnEntity {
				Id = Guid.CreateVersion7(),
				CreatorId = Core.App.Users.SystemAdmin.Id,
				CreatedAt = DateTime.UtcNow,
				JsonData = new BaseJson { Detail1 = $"RRN:{rrn}", Detail2 = "شارژ کیف پول" },
				Tags = [TagWalletTxn.Charge],
				// Money source for a wallet top-up is the platform account (a seeded user), not the unseeded IpgUserId
				// which caused the FK_WalletTxns_Users_SenderId violation.
				SenderId = Core.App.Users.AvaPlus.Id,
				ReceiverId = txn.UserId,
				Amount = txn.Amount
			}, ct);

			WalletEntity? wallet = await db.Set<WalletEntity>().AsTracking().FirstOrDefaultAsync(x => x.CreatorId == txn.UserId, ct);
			if (wallet == null) return;
			wallet.Balance += txn.Amount;
			db.Update(wallet);

			await db.SaveChangesAsync(ct);
		}
		catch (Exception) {
			// Leave the order Pending (money may have been captured) so it can be reconciled rather than wrongly failed.
		}
	}

	// Helper: move a pending order to a failed state so it can never be settled later.
	private async Task MarkFailed(TxnEntity txn, CancellationToken ct) {
		if (txn.Tags.Contains(TagTxn.Paid)) return;
		txn.Tags = [TagTxn.ChargeWallet, TagTxn.Failed];
		db.Set<TxnEntity>().Update(txn);
		await db.SaveChangesAsync(ct);
	}
}