namespace SinaMN75U.Services;

public interface IIpgService {
	Task<UResponse<IpgPayResponse?>> Pay(IpgPayParams p, CancellationToken ct);
	Task<UResponse<IpgVerifyResponse?>> Status(IpgStatusParams p, CancellationToken ct);
	Task<string?> Verify(string token, short status, string? cardNumberMasked, long? rrn, string additionalData, CancellationToken ct);
}

public class IpgService(
	IEnumerable<IIpgProvider> providers,
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IHttpContextAccessor httpContext,
	IHotelService hs
) : IIpgService {
	private IIpgProvider Provider => providers.First(x => x.Tag == Core.App.Ipg.Tag);

	public async Task<UResponse<IpgPayResponse?>> Pay(IpgPayParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IpgPayResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IpgPayResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		TagIpgPayment kind = Kind(p);
		if (!Provider.SupportsKind(kind)) return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("thisPaymentTypeIsNotSupportedByTheGateway"));

		string trackingNumber = Guid.CreateVersion7().ToString("N");
		decimal amount = p.Amount ?? 0;
		BillInfoResponse? bill = null;

		switch (kind) {
			case TagIpgPayment.Bill: {
				try {
					bill = new BillParser(ls).Parse(p.BillId!, p.PaymentId!);
				}
				catch {
					return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("billInformationCouldNotBeParsed"));
				}

				if (bill == null || !bill.IsValid) return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("theBillIsNotValid"));
				if (bill.BillAmount is not > 0) return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("theBillAmountIsNotAvailable"));
				amount = bill.BillAmount.Value;
				break;
			}
			case TagIpgPayment.TopUp: {
				if (p.TopUpType == null) return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("topUpTypeIsRequired"));
				if (!Provider.SupportsTopUpOperator(p.TopUpType.Value)) return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("thisOperatorIsNotSupportedForDirectTopUp"));
				break;
			}
			case TagIpgPayment.MultiplexedSale: {
				List<IpgMultiplexedAccountParams> accounts = p.MultiplexedAccounts!.ToList();
				if (accounts.Any(x => x.Iban.IsNullOrEmpty())) return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("iBanIsRequired"));
				if (accounts.Any(x => x.Amount <= 0)) return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("amountRequired"));
				if (accounts.Sum(x => x.Amount) != amount) return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("theSumOfMultiplexedAmountsMustBeEqualToTheTotalAmount"));
				break;
			}
		}

		if (amount <= 0) return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("amountRequired"));

		return await Sale(new TxnEntity {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = userData.Id,
			UserId = userData.Id,
			Amount = amount,
			TrackingNumber = trackingNumber,
			Tags = TxnTags(kind, p.Tag),
			JsonData = new BaseJson { Detail1 = Detail(kind, p, bill) }
		}, new IpgAdditionalData {
			TrackingNumber = trackingNumber,
			Tag = p.Tag,
			Kind = kind,
			InvoiceId = p.InvoiceId,
			BillId = bill?.BillId,
			PaymentId = bill?.PaymentId,
			ChargeMobileNumber = p.ChargeMobileNumber
		}, p, userData.PhoneNumber, ct);
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

		TagIpgPayment kind = data.Kind == TagIpgPayment.NormalSale && data.BillId.IsNotNullOrEmpty() ? TagIpgPayment.Bill : data.Kind;

		try {
			if (Provider.RequiresConfirm(kind) && !await Confirm(token, ct)) return txn.TrackingNumber;

			txn.JsonData.Detail1 = $"Card:{cardNumberMasked}";
			txn.JsonData.Detail2 = $"RRN:{rrn}";
			txn.Tags = [..txn.Tags.Where(x => x != TagTxn.Pending), TagTxn.Paid];
			db.Set<TxnEntity>().Update(txn);
			await db.SaveChangesAsync(ct);

			if (kind != TagIpgPayment.NormalSale) return txn.TrackingNumber;

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

	private static TagIpgPayment Kind(IpgPayParams p) {
		if (p.BillId.IsNotNullOrEmpty() && p.PaymentId.IsNotNullOrEmpty()) return TagIpgPayment.Bill;
		if (p.ChargeMobileNumber.IsNotNullOrEmpty()) return TagIpgPayment.TopUp;
		return p.MultiplexedAccounts?.Any() ?? false ? TagIpgPayment.MultiplexedSale : TagIpgPayment.NormalSale;
	}

	private static List<TagTxn> TxnTags(TagIpgPayment kind, TagTxn tag) => kind switch {
		TagIpgPayment.Bill => [TagTxn.BillPayment, TagTxn.Pending],
		TagIpgPayment.TopUp => [TagTxn.TopUp, TagTxn.Pending],
		TagIpgPayment.MultiplexedSale => [TagTxn.MultiplexedSale, TagTxn.Pending],
		_ => [TagTxn.ChargeWallet, tag, TagTxn.Pending]
	};

	private static string Detail(TagIpgPayment kind, IpgPayParams p, BillInfoResponse? bill) => kind switch {
		TagIpgPayment.Bill => $"BILL|{bill?.BillId}|{bill?.PaymentId}",
		TagIpgPayment.TopUp => $"TOPUP|{p.ChargeMobileNumber}|{p.TopUpType}",
		TagIpgPayment.MultiplexedSale => $"MULTIPLEXED|{p.MultiplexedAccounts?.Count()}",
		_ => "IPG"
	};

	private async Task<UResponse<IpgPayResponse?>> Sale(TxnEntity txn, IpgAdditionalData ad, IpgPayParams p, string? originator, CancellationToken ct) {
		string additionalData = JsonSerializer.SerializeToUtf8Bytes(ad).ToBase64Url();

		await db.Set<TxnEntity>().AddAsync(txn, ct);
		await db.SaveChangesAsync(ct);

		if (Core.App.Test) {
			txn.JsonData.Detail2 = "FAKE";
			db.Set<TxnEntity>().Update(txn);
			await db.SaveChangesAsync(ct);
			return new UResponse<IpgPayResponse?>(new IpgPayResponse {
				Url = $"{Core.App.BaseUrl}/api/ipg/Gateway?additionalData={additionalData}&amount={(long)txn.Amount}",
				TrackingNumber = txn.TrackingNumber
			});
		}

		try {
			IpgProviderPayResult result = await Provider.Pay(new IpgProviderPayParams {
				Kind = ad.Kind,
				Amount = (long)txn.Amount,
				OrderId = Math.Abs(Guid.NewGuid().GetHashCode()),
				CallBackUrl = $"{Core.App.BaseUrl}/api/ipg/Verify?additionalData={additionalData}",
				AdditionalData = additionalData,
				Originator = originator,
				BillId = ad.BillId,
				PaymentId = ad.PaymentId,
				ChargeMobileNumber = ad.ChargeMobileNumber,
				TopUpType = p.TopUpType,
				MultiplexedAccounts = p.MultiplexedAccounts
			}, ct);

			if (!result.Succeed) {
				await MarkFailed(txn, ct);
				return new UResponse<IpgPayResponse?>(null, Usc.ThirdPartyError, result.Message ?? ls.Get("paymentGatewayErrorPleaseTryAgain"));
			}

			txn.JsonData.Detail2 = result.Token ?? "---";
			db.Set<TxnEntity>().Update(txn);
			await db.SaveChangesAsync(ct);
			return new UResponse<IpgPayResponse?>(new IpgPayResponse {
				Url = result.Url ?? "",
				TrackingNumber = txn.TrackingNumber
			});
		}
		catch (Exception ex) {
			httpContext.CaptureForApiLog(ex);
			await MarkFailed(txn, ct);
			return new UResponse<IpgPayResponse?>(null, Usc.InternalServerError, ls.Get("internalServerError"));
		}
	}

	private async Task<bool> Confirm(string token, CancellationToken ct) => Core.App.Test || await Provider.Confirm(token, ct);

	private async Task MarkFailed(TxnEntity txn, CancellationToken ct) {
		if (txn.Tags.Contains(TagTxn.Paid)) return;
		txn.Tags = [..txn.Tags.Where(x => x != TagTxn.Pending), TagTxn.Failed];
		db.Set<TxnEntity>().Update(txn);
		await db.SaveChangesAsync(ct);
	}
}
