namespace SinaMN75U.Services;

public interface IIpgService {
	Task<UResponse<IpgPayResponse?>> Pay(IpgPayParams p, CancellationToken ct);
	Task<bool> Verify(IpgAdditionalData additionalData, CancellationToken ct);
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
		if (p.Amount <= 0) return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("amountRequired"));

		TagIpgPayment kind = Kind(p);

		string trackingNumber = Random.Shared.NextInt64(100_000_000_000, 999_999_999_999).ToString();
		BillInfoResponse? bill = null;

		switch (kind) {
			case TagIpgPayment.Bill: {
				try {
					bill = new BillParser(ls).Parse(p.BillId!, p.PaymentId!);
				}
				catch {
					return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("billInformationCouldNotBeParsed"));
				}

				if (bill is not { IsValid: true }) return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("theBillIsNotValid"));
				if (bill.BillAmount is not > 0) return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("theBillAmountIsNotAvailable"));
				p.Amount = bill.BillAmount.Value;
				break;
			}
			case TagIpgPayment.TopUp: {
				if (p.TopUpType == null) return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("topUpTypeIsRequired"));
				break;
			}
			case TagIpgPayment.MultiplexedSale: {
				List<IpgMultiplexedAccountParams> accounts = p.MultiplexedAccounts!.ToList();
				if (accounts.Any(x => x.Iban.IsNullOrEmpty())) return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("iBanIsRequired"));
				if (accounts.Any(x => x.Amount <= 0)) return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("amountRequired"));
				if (accounts.Sum(x => x.Amount) != p.Amount) return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("theSumOfMultiplexedAmountsMustBeEqualToTheTotalAmount"));
				break;
			}
			case TagIpgPayment.NormalSale:
				break;
			default:
				throw new Exception();
		}

		TxnEntity txn = new() {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = userData.Id,
			UserId = userData.Id,
			Amount = p.Amount,
			TrackingNumber = trackingNumber,
			Tags = TxnTags(kind, p.Tag),
			JsonData = new TxnJson { Detail1 = Detail(kind, p, bill) }
		};

		IpgAdditionalData additionalData = new() {
			Amount = p.Amount,
			TrackingNumber = trackingNumber,
			Tag = p.Tag,
			Kind = kind,
			InvoiceId = p.InvoiceId,
			BillId = bill?.BillId,
			PaymentId = bill?.PaymentId,
			ChargeMobileNumber = p.ChargeMobileNumber
		};

		await db.Set<TxnEntity>().AddAsync(txn, ct);
		await db.SaveChangesAsync(ct);

		if (Core.App.Test) {
			txn.JsonData.Detail2 = "FAKE";
			db.Set<TxnEntity>().Update(txn);
			await db.SaveChangesAsync(ct);
			return new UResponse<IpgPayResponse?>(new IpgPayResponse {
				Url = $"{Core.App.BaseUrl}/{RouteTags.Ipg}Gateway?additionalData={additionalData.ToJson().ToBase58()}",
				AdditionalData = additionalData
			});
		}

		try {
			IpgProviderPayResult result = await Provider.Pay(new IpgProviderPayParams {
				Kind = additionalData.Kind,
				Amount = (long)txn.Amount,
				OrderId = Math.Abs(Guid.NewGuid().GetHashCode()),
				CallBackUrl = $"{Core.App.BaseUrl}/{RouteTags.Ipg}Verify?additionalData={additionalData.ToJson().ToBase58()}",
				AdditionalData = additionalData.ToJson().ToBase58(),
				Originator = userData.PhoneNumber,
				BillId = additionalData.BillId,
				PaymentId = additionalData.PaymentId,
				ChargeMobileNumber = additionalData.ChargeMobileNumber,
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
				AdditionalData = additionalData
			});
		}
		catch (Exception ex) {
			httpContext.CaptureForApiLog(ex);
			await MarkFailed(txn, ct);
			return new UResponse<IpgPayResponse?>(null, Usc.InternalServerError, ls.Get("internalServerError"));
		}
	}

	public async Task<bool> Verify(IpgAdditionalData additionalData, CancellationToken ct) {
		TxnEntity? txn = await db.Set<TxnEntity>().AsTracking().FirstOrDefaultAsync(x => x.TrackingNumber == additionalData.TrackingNumber, ct);
		if (txn == null) return false;
		
		if (additionalData.Status != 0) {
			await MarkFailed(txn, ct);
			return false;
		}

		if (txn.JsonData.Detail2.IsNullOrEmpty() || txn.JsonData.Detail2 != additionalData.Token) return false;

		TagIpgPayment kind = additionalData.Kind == TagIpgPayment.NormalSale && additionalData.BillId.IsNotNullOrEmpty() ? TagIpgPayment.Bill : additionalData.Kind;

		try {
			if (!Core.App.Test && !await Provider.Confirm(additionalData.Token, ct)) return false;

			txn.Tags = [..txn.Tags.Where(x => x != TagTxn.Pending), TagTxn.Paid];
			db.Set<TxnEntity>().Update(txn);
			await db.SaveChangesAsync(ct);

			if (kind != TagIpgPayment.NormalSale) return true;

			await db.Set<WalletTxnEntity>().AddAsync(new WalletTxnEntity {
				Id = Guid.CreateVersion7(),
				CreatorId = Core.App.Users.SystemAdmin.Id,
				CreatedAt = DateTime.UtcNow,
				JsonData = new WalletTxnJson { Detail2 = "شارژ کیف پول" },
				Tags = [TagWalletTxn.Charge],
				SenderId = Core.App.Users.AvaPlus.Id,
				ReceiverId = txn.UserId,
				Amount = txn.Amount
			}, ct);

			WalletEntity wallet = await ReadOrCreateWallet(txn.UserId, ct);
			wallet.Balance += txn.Amount;
			db.Update(wallet);
			await db.SaveChangesAsync(ct);

			if (additionalData is { InvoiceId: not null }) {
				if (additionalData.Tag == TagTxn.HotelInvoice)
					await hs.PayHotelInvoiceInternal(new HotelInvoicePayParams {
						InvoiceId = additionalData.InvoiceId.ToGuid(),
						UserId = txn.UserId
					}, ct);
				else
					await hs.PayDormBedInvoice(new DormBedInvoicePayParams {
						InvoiceId = additionalData.InvoiceId.ToGuid(),
						UserId = txn.UserId
					}, ct);
			}

			return true;
		}
		catch (Exception ex) {
			httpContext.CaptureForApiLog(ex);
			return txn.Tags.Contains(TagTxn.Paid);
		}
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

	private async Task<WalletEntity> ReadOrCreateWallet(Guid userId, CancellationToken ct) {
		WalletEntity? e = await db.Set<WalletEntity>().AsTracking().FirstOrDefaultAsync(x => x.CreatorId == userId, ct);
		if (e != null) return e;

		e = new WalletEntity {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = userId,
			Balance = 0,
			Tags = [TagWallet.Primary],
			JsonData = new WalletJson()
		};
		await db.Set<WalletEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return e;
	}

	private async Task MarkFailed(TxnEntity txn, CancellationToken ct) {
		if (txn.Tags.Contains(TagTxn.Paid)) return;
		txn.Tags = [..txn.Tags.Where(x => x != TagTxn.Pending), TagTxn.Failed];
		db.Set<TxnEntity>().Update(txn);
		await db.SaveChangesAsync(ct);
	}
}