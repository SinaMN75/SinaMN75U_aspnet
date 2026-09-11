namespace SinaMN75U.Services;

public interface IIpgService {
	Task<UResponse<IpgPayResponse?>> GetSaleIpgLink(IpgSaleParams p, CancellationToken ct);
	Task<UResponse<IpgPayResponse?>> GetBillIpgLink(IpgBillParams p, CancellationToken ct);
	Task<UResponse<IpgVerifyResponse?>> Status(IpgStatusParams p, CancellationToken ct);
	Task<string?> Verify(string token, short status, string? cardNumberMasked, long? rrn, string additionalData, CancellationToken ct);
}

public class IpgService(
	IHttpClientService http,
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IHttpContextAccessor httpContext,
	IHotelService hs
) : IIpgService {
	private const string NormalSaleUrl = "https://pna.shaparak.ir/mhipg/api/Payment/NormalSale";
	private const string BillUrl = "https://pna.shaparak.ir/mhipg/api/Payment/bill";
	private const string ConfirmUrl = "https://pna.shaparak.ir/mhipg/api/Payment/confirm";
	private const string RedirectUrl = "https://pna.shaparak.ir/mhui/home/index/";

	public async Task<UResponse<IpgPayResponse?>> GetSaleIpgLink(IpgSaleParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IpgPayResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IpgPayResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (p.Amount <= 0) return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("amountRequired"));

		string trackingNumber = Guid.CreateVersion7().ToString("N");
		return await Sale(new TxnEntity {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = userData.Id,
			UserId = userData.Id,
			Amount = p.Amount,
			TrackingNumber = trackingNumber,
			Tags = [TagTxn.ChargeWallet, p.Tag, TagTxn.Pending],
			JsonData = new TxnJson { Detail1 = "IPG" }
		}, new IpgAdditionalData {
			TrackingNumber = trackingNumber,
			Tag = p.Tag,
			InvoiceId = p.InvoiceId
		}, userData.PhoneNumber, ct);
	}

	public async Task<UResponse<IpgPayResponse?>> GetBillIpgLink(IpgBillParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IpgPayResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IpgPayResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		BillInfoResponse bill;
		try {
			bill = new BillParser(ls).Parse(p.BillId, p.PaymentId);
		}
		catch {
			return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("billInformationCouldNotBeParsed"));
		}

		if (!bill.IsValid) return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("theBillIsNotValid"));
		if (bill.BillAmount is not > 0) return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("theBillAmountIsNotAvailable"));

		string trackingNumber = Guid.CreateVersion7().ToString("N");
		return await Sale(new TxnEntity {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = userData.Id,
			UserId = userData.Id,
			Amount = bill.BillAmount.Value,
			TrackingNumber = trackingNumber,
			Tags = [TagTxn.BillPayment, TagTxn.Pending],
			JsonData = new TxnJson { Detail1 = "BILL", BillId = bill.BillId, PaymentId = bill.PaymentId }
		}, new IpgAdditionalData {
			TrackingNumber = trackingNumber,
			Tag = TagTxn.BillPayment,
			BillId = bill.BillId,
			PaymentId = bill.PaymentId
		}, userData.PhoneNumber, ct);
	}

	public async Task<UResponse<IpgVerifyResponse?>> Status(IpgStatusParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IpgVerifyResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IpgVerifyResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		TxnEntity? txn = await db.Set<TxnEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.TrackingNumber == p.TrackingNumber && x.UserId == userData.Id, ct);
		if (txn == null) return new UResponse<IpgVerifyResponse?>(null, Usc.NotFound, ls.Get("notFound"));

		return new UResponse<IpgVerifyResponse?>(new IpgVerifyResponse {
			Paid = txn.Tags.Contains(TagTxn.Paid),
			Failed = txn.Tags.Contains(TagTxn.Failed),
			Balance = await db.Set<WalletEntity>().AsNoTracking().Where(x => x.CreatorId == userData.Id).Select(x => x.Balance).FirstOrDefaultAsync(ct)
		});
	}

	public async Task<string?> Verify(string token, short status, string? cardNumberMasked, long? rrn, string additionalData, CancellationToken ct) {
		if (additionalData.IsNullOrEmpty()) return null;

		IpgAdditionalData? data;
		try {
			data = JsonSerializer.Deserialize<IpgAdditionalData>(additionalData.FromBase64Url());
		}
		catch {
			return null;
		}

		if (data == null) return null;

		TxnEntity? txn = await db.Set<TxnEntity>().AsTracking().FirstOrDefaultAsync(x => x.TrackingNumber == data.TrackingNumber, ct);
		if (txn == null) return null;

		if (txn.Tags.Contains(TagTxn.Paid)) return txn.TrackingNumber;

		if (status != 0) {
			await MarkFailed(txn, ct);
			return txn.TrackingNumber;
		}

		if (txn.JsonData.Detail2.IsNullOrEmpty() || txn.JsonData.Detail2 != token) return txn.TrackingNumber;

		try {
			if (data.Tag != TagTxn.BillPayment && !await Confirm(token, ct)) return txn.TrackingNumber;

			txn.JsonData.CardNumberMasked = cardNumberMasked;
			txn.JsonData.Rrn = rrn;

			if (data.Tag == TagTxn.BillPayment) {
				txn.Tags = [TagTxn.BillPayment, TagTxn.Paid];
				db.Set<TxnEntity>().Update(txn);
				await db.SaveChangesAsync(ct);
				return txn.TrackingNumber;
			}

			txn.Tags = [TagTxn.ChargeWallet, TagTxn.Paid, data.Tag];
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
			if (wallet == null) return txn.TrackingNumber;
			wallet.Balance += txn.Amount;
			db.Update(wallet);
			await db.SaveChangesAsync(ct);

			if (data is { InvoiceId: not null }) {
				if (data.Tag == TagTxn.HotelInvoice)
					await hs.PayHotelInvoiceInternal(new HotelInvoicePayParams {
						InvoiceId = data.InvoiceId.ToGuid(),
						UserId = txn.UserId
					}, ct);
				else
					await hs.PayDormBedInvoice(new DormBedInvoicePayParams {
						InvoiceId = data.InvoiceId.ToGuid(),
						UserId = txn.UserId
					}, ct);
			}
		}
		catch (Exception ex) {
			httpContext.CaptureForApiLog(ex);
		}

		return txn.TrackingNumber;
	}

	private async Task<UResponse<IpgPayResponse?>> Sale(TxnEntity txn, IpgAdditionalData ad, string? originator, CancellationToken ct) {
		string additionalData = JsonSerializer.SerializeToUtf8Bytes(ad).ToBase64Url();

		await db.Set<TxnEntity>().AddAsync(txn, ct);
		await db.SaveChangesAsync(ct);

		HttpRequest request = httpContext.HttpContext!.Request;
		string basePath = request.Path.Value![..(request.Path.Value!.LastIndexOf('/') + 1)];
		string verifyUrl = $"{Core.App.BaseUrl}{basePath}Verify";
		string callBackUrl = $"{verifyUrl}?additionalData={additionalData}";

		if (Core.App.Test) {
			txn.JsonData.Detail2 = "FAKE";
			db.Set<TxnEntity>().Update(txn);
			await db.SaveChangesAsync(ct);
			return new UResponse<IpgPayResponse?>(new IpgPayResponse {
				Url = $"{Core.App.BaseUrl}{basePath}Gateway?additionalData={additionalData}&amount={(long)txn.Amount}",
				TrackingNumber = txn.TrackingNumber
			});
		}

		bool isBill = ad.BillId.IsNotNullOrEmpty();
		object body = isBill
			? new {
				BillId = ad.BillId,
				PayId = ad.PaymentId,
				CorporationPin = Core.App.Ipg.Token,
				Amount = (long)txn.Amount,
				OrderId = Math.Abs(Guid.NewGuid().GetHashCode()),
				CallBackUrl = callBackUrl,
				AdditionalData = additionalData,
				Originator = originator ?? ""
			}
			: (object)new {
				CorporationPin = Core.App.Ipg.Token,
				Amount = (long)txn.Amount,
				OrderId = Math.Abs(Guid.NewGuid().GetHashCode()),
				CallBackUrl = callBackUrl,
				AdditionalData = additionalData,
				Originator = originator ?? ""
			};

		try {
			HttpResponseMessage? response = await http.Post(
				isBill ? BillUrl : NormalSaleUrl,
				body,
				headers: new Dictionary<string, string> { { "Referer", Core.App.BaseUrl } }
			);

			if (response?.IsSuccessStatusCode ?? false) {
				JsonElement responseData = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));
				if (GatewayStatus(responseData) == 0) {
					string gatewayToken = GatewayString(responseData, "token") ?? "---";
					txn.JsonData.Detail2 = gatewayToken;
					db.Set<TxnEntity>().Update(txn);
					await db.SaveChangesAsync(ct);
					return new UResponse<IpgPayResponse?>(new IpgPayResponse {
						Url = $"{RedirectUrl}{gatewayToken}",
						TrackingNumber = txn.TrackingNumber
					});
				}

				await MarkFailed(txn, ct);
				return new UResponse<IpgPayResponse?>(null, Usc.ThirdPartyError, GatewayString(responseData, "message") ?? ls.Get("paymentGatewayErrorPleaseTryAgain"));
			}

			await MarkFailed(txn, ct);
			return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("paymentGatewayErrorPleaseTryAgain"));
		}
		catch (Exception ex) {
			httpContext.CaptureForApiLog(ex);
			await MarkFailed(txn, ct);
			return new UResponse<IpgPayResponse?>(null, Usc.InternalServerError, ls.Get("internalServerError"));
		}
	}

	private async Task<bool> Confirm(string token, CancellationToken ct) {
		if (Core.App.Test) return true;

		HttpResponseMessage? response = await http.Post(ConfirmUrl, new {
			CorporationPin = Core.App.Ipg.Token,
			Token = token
		});
		if (!(response?.IsSuccessStatusCode ?? false)) return false;

		return GatewayStatus(JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct))) == 0;
	}

	private static short GatewayStatus(JsonElement e) {
		if (!e.TryGetProperty("status", out JsonElement s) && !e.TryGetProperty("Status", out s)) return -1;
		if (s.ValueKind == JsonValueKind.Number) return s.GetInt16();
		return short.TryParse(s.GetString(), out short parsed) ? parsed : (short)-1;
	}

	private static string? GatewayString(JsonElement e, string name) => e.GetStringOrNull(name) ?? e.GetStringOrNull(char.ToUpperInvariant(name[0]) + name[1..]);

	private async Task MarkFailed(TxnEntity txn, CancellationToken ct) {
		if (txn.Tags.Contains(TagTxn.Paid)) return;
		txn.Tags = txn.Tags.Contains(TagTxn.BillPayment) ? [TagTxn.BillPayment, TagTxn.Failed] : [TagTxn.ChargeWallet, TagTxn.Failed];
		db.Set<TxnEntity>().Update(txn);
		await db.SaveChangesAsync(ct);
	}
}
