namespace SinaMN75U.Services;

public interface IIpgService {
	Task<UResponse<IpgPayResponse?>> GetSaleIpgLink(IpgSaleParams p, CancellationToken ct);
	Task Verify(string token, short status, string? cardNumberMasked, long? rrn, string additionalData, CancellationToken ct);
}

public class IpgService(
	IHttpClientService http,
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IHttpContextAccessor httpContext,
	IHotelService hs
) : IIpgService {
	public async Task<UResponse<IpgPayResponse?>> GetSaleIpgLink(IpgSaleParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IpgPayResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (userData.IsExpired) return new UResponse<IpgPayResponse?>(null, Usc.ExpiredToken, ls.Get("TokenExpired"));
		if (p.Amount <= 0) return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("AmountRequired"));
		string trackingNumber = Guid.CreateVersion7().ToString("N");
		string additionalData = JsonSerializer.SerializeToUtf8Bytes(new IpgAdditionalData {
			TrackingNumber = trackingNumber,
			Tag = p.Tag,
			InvoiceId = p.InvoiceId
		}).ToBase64Url();

		TxnEntity txn = new() {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = userData.Id,
			UserId = userData.Id,
			Amount = p.Amount,
			TrackingNumber = trackingNumber,
			Tags = [TagTxn.ChargeWallet, p.Tag, TagTxn.Pending],
			JsonData = new BaseJson { Detail1 = "IPG" }
		};
		await db.Set<TxnEntity>().AddAsync(txn, ct);
		await db.SaveChangesAsync(ct);

		HttpRequest request = httpContext.HttpContext!.Request;
		string basePath = request.Path.Value![..(request.Path.Value!.LastIndexOf('/') + 1)];
		string verifyUrl = $"{Core.App.BaseUrl}{basePath}Verify";
		string gatewayUrl = $"{Core.App.BaseUrl}{basePath}Gateway";

		if (Core.App.Test) {
			txn.JsonData.Detail2 = "FAKE";
			db.Set<TxnEntity>().Update(txn);
			await db.SaveChangesAsync(ct);
			return new UResponse<IpgPayResponse?>(new IpgPayResponse {
				Url = $"{gatewayUrl}?additionalData={additionalData}&amount={(long)p.Amount}",
				TrackingNumber = trackingNumber
			});
		}

		try {
			HttpResponseMessage? response = await http.Post("https://pna.shaparak.ir/mhipg/api/Payment/NormalSale", new {
					CorporationPin = Core.App.Ipg.Token,
					Amount = (long)p.Amount,
					OrderId = Math.Abs(Guid.NewGuid().GetHashCode()),
					CallBackUrl = $"{verifyUrl}?additionalData={additionalData}",
					AdditionalData = additionalData,
					Originator = userData.PhoneNumber ?? ""
				}
			);

			if (response?.IsSuccessStatusCode ?? false) {
				string responseBody = await response.Content.ReadAsStringAsync(ct);
				JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody);
				if (data.GetProperty("Status").GetInt16() == 0) {
					string gatewayToken = data.GetProperty("Token").GetString()!;
					txn.JsonData.Detail2 = gatewayToken;
					db.Set<TxnEntity>().Update(txn);
					await db.SaveChangesAsync(ct);
					return new UResponse<IpgPayResponse?>(new IpgPayResponse {
						Url = $"https://pna.shaparak.ir/mhui/home/index/{gatewayToken}",
						TrackingNumber = trackingNumber
					});
				}
			}

			await MarkFailed(txn, ct);
			return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("IpgError"));
		}
		catch (Exception ex) {
			httpContext.CaptureForApiLog(ex);
			await MarkFailed(txn, ct);
			return new UResponse<IpgPayResponse?>(null, Usc.InternalServerError, ls.Get("InternalServerError"));
		}
	}

	public async Task Verify(string token, short status, string? cardNumberMasked, long? rrn, string additionalData, CancellationToken ct) {
		if (additionalData.IsNullOrEmpty()) return;

		IpgAdditionalData? data;
		try {
			data = JsonSerializer.Deserialize<IpgAdditionalData>(additionalData.FromBase64Url());
		}
		catch {
			return;
		}
		if (data == null) return;

		TxnEntity? txn = await db.Set<TxnEntity>().AsTracking().FirstOrDefaultAsync(x => x.TrackingNumber == data.TrackingNumber, ct);
		if (txn == null) return;

		if (txn.Tags.Contains(TagTxn.Paid)) return;

		if (status != 0) {
			await MarkFailed(txn, ct);
			return;
		}

		if (txn.JsonData.Detail2.IsNullOrEmpty() || txn.JsonData.Detail2 != token) return;

		try {
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

			txn.Tags = [TagTxn.ChargeWallet, TagTxn.Paid, data.Tag];
			txn.JsonData.Detail1 = $"Card:{cardNumberMasked}";
			txn.JsonData.Detail2 = $"RRN:{rrn}";
			db.Set<TxnEntity>().Update(txn);

			await db.Set<WalletTxnEntity>().AddAsync(new WalletTxnEntity {
				Id = Guid.CreateVersion7(),
				CreatorId = Core.App.Users.SystemAdmin.Id,
				CreatedAt = DateTime.UtcNow,
				JsonData = new BaseJson { Detail2 = "شارژ کیف پول" },
				Tags = [TagWalletTxn.Charge],
				SenderId = Core.App.Users.AvaPlus.Id,
				ReceiverId = txn.UserId,
				Amount = txn.Amount
			}, ct);

			WalletEntity? wallet = await db.Set<WalletEntity>().AsTracking().FirstOrDefaultAsync(x => x.CreatorId == txn.UserId, ct);
			if (wallet == null) return;
			wallet.Balance += txn.Amount;
			db.Update(wallet);
			await db.SaveChangesAsync(ct);

			if (data is { InvoiceId: not null }) {
				await hs.PayDormBedInvoice(new DormBedInvoicePayParams {
					InvoiceId = data.InvoiceId.ToGuid(),
					UserId = txn.UserId
				}, ct);
			}
			
		}
		catch (Exception ex) {
			httpContext.CaptureForApiLog(ex);
		}
	}

	private async Task MarkFailed(TxnEntity txn, CancellationToken ct) {
		if (txn.Tags.Contains(TagTxn.Paid)) return;
		txn.Tags = [TagTxn.ChargeWallet, TagTxn.Failed];
		db.Set<TxnEntity>().Update(txn);
		await db.SaveChangesAsync(ct);
	}
}