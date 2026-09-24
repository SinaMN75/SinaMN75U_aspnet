using Microsoft.EntityFrameworkCore.Storage;

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
				return new UResponse<IpgPayResponse?>(null, Usc.BadRequest, ls.Get("thisPaymentTypeIsNotSupportedByTheGateway"));
		}

		TxnEntity txn = new() {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = userData.Id,
			UserId = userData.Id,
			Amount = p.Amount,
			TrackingNumber = trackingNumber,
			Tags = TxnTags(kind, p.Tag),
			JsonData = new TxnJson { Detail1 = Detail(kind, p, bill), KeyValues = TxnKeyValues(kind, p, bill) }
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

		if (txn.Tags.Contains(TagTxn.Paid)) {
			additionalData.KeyValues = txn.JsonData.KeyValues.Select(x => new KeyValue { Key = x.Key, Value = x.Value }).ToList();
			return true;
		}

		if (additionalData.Status != 0) {
			await MarkFailed(txn, ct);
			return false;
		}

		if (txn.JsonData.Detail2.IsNullOrEmpty() || txn.JsonData.Detail2 != additionalData.Token) return false;

		TagIpgPayment kind = additionalData.Kind == TagIpgPayment.NormalSale && additionalData.BillId.IsNotNullOrEmpty() ? TagIpgPayment.Bill : additionalData.Kind;

		try {
			if (!Core.App.Test && !await Provider.Confirm(additionalData.Token, ct)) return false;

			List<TagTxn> paidTags = [..txn.Tags.Where(x => x != TagTxn.Pending), TagTxn.Paid];
			WalletEntity? wallet = kind == TagIpgPayment.NormalSale ? await ReadOrCreateWallet(txn.UserId, ct) : null;

			if (additionalData.Rrn.IsNotNullOrEmpty() && txn.JsonData.KeyValues.All(x => x.Key != ULocalizedConstants.Reference))
				txn.JsonData.KeyValues.Add(new KeyValue { Key = ULocalizedConstants.Reference, Value = additionalData.Rrn });

			WalletTxnEntity? walletTxn = wallet == null ? null : new WalletTxnEntity {
				Id = Guid.CreateVersion7(),
				CreatorId = Core.App.Users.SystemAdmin.Id,
				CreatedAt = DateTime.UtcNow,
				JsonData = new WalletTxnJson {
					Detail2 = "شارژ کیف پول",
					KeyValues = [
						new KeyValue { Key = ULocalizedConstants.TrackingNumber, Value = txn.TrackingNumber },
						..txn.JsonData.KeyValues.Select(x => new KeyValue { Key = x.Key, Value = x.Value })
					]
				},
				Tags = [TagWalletTxn.Charge],
				SenderId = Core.App.Users.AvaPlus.Id,
				ReceiverId = txn.UserId,
				Amount = txn.Amount
			};

			// Claiming Pending→Paid, the wallet txn and the balance credit commit together, and only the request that wins
			// the claim credits the wallet. Runs in the execution strategy because the context uses retry-on-failure.
			IExecutionStrategy strategy = db.Database.CreateExecutionStrategy();
			bool claimed = await strategy.ExecuteAsync(async () => {
				await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync(ct);
				int rows = await db.Set<TxnEntity>()
					.Where(x => x.Id == txn.Id && !x.Tags.Contains(TagTxn.Paid))
					.ExecuteUpdateAsync(u => u.SetProperty(x => x.Tags, paidTags), ct);
				if (rows == 0) return false;

				txn.Tags = paidTags;

				if (wallet != null && walletTxn != null) {
					if (db.Entry(walletTxn).State == EntityState.Detached) await db.Set<WalletTxnEntity>().AddAsync(walletTxn, ct);
					await db.Set<WalletEntity>().Where(x => x.Id == wallet.Id).ExecuteUpdateAsync(u => u.SetProperty(x => x.Balance, x => x.Balance + txn.Amount), ct);
				}

				await db.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				return true;
			});

			additionalData.KeyValues = txn.JsonData.KeyValues.Select(x => new KeyValue { Key = x.Key, Value = x.Value }).ToList();

			// Another request already completed this payment.
			if (!claimed) return true;
			if (kind != TagIpgPayment.NormalSale) return true;

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

	private static List<KeyValue> TxnKeyValues(TagIpgPayment kind, IpgPayParams p, BillInfoResponse? bill) {
		List<KeyValue> keyValues = [];
		switch (kind) {
			case TagIpgPayment.Bill:
				keyValues.Add(new KeyValue { Key = ULocalizedConstants.BillId, Value = bill?.BillId ?? "" });
				keyValues.Add(new KeyValue { Key = ULocalizedConstants.PaymentId, Value = bill?.PaymentId ?? "" });
				if (bill != null && bill.ServiceName.IsNotNullOrEmpty()) keyValues.Add(new KeyValue { Key = ULocalizedConstants.BillType, Value = bill.ServiceName });
				break;
			case TagIpgPayment.TopUp:
				keyValues.Add(new KeyValue { Key = ULocalizedConstants.PhoneNumber, Value = p.ChargeMobileNumber ?? "" });
				if (p.TopUpType != null) keyValues.Add(new KeyValue { Key = ULocalizedConstants.Operator, Value = ((int)p.TopUpType.Value).ToString() });
				break;
			case TagIpgPayment.MultiplexedSale:
				keyValues.AddRange(Enumerable.Select(p.MultiplexedAccounts ?? Array.Empty<IpgMultiplexedAccountParams>(), x => new KeyValue { Key = ULocalizedConstants.IBan, Value = x.Iban }));
				break;
			case TagIpgPayment.NormalSale:
			default:
				if (p.InvoiceId.IsNotNullOrEmpty()) keyValues.Add(new KeyValue { Key = ULocalizedConstants.InvoiceId, Value = p.InvoiceId });
				break;
		}
		return keyValues;
	}

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
		await db.SaveChangesAsync(ct);
	}
}