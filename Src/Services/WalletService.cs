namespace SinaMN75U.Services;

public interface IWalletService {
	Task<UResponse<IEnumerable<WalletResponse>?>> ReadByUserId(WalletReadParams p, CancellationToken ct);
	Task<UResponse<TagTxnErrorCodes>> Transfer(WalletTransferParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<WalletTxnResponse>?>> ReadTxn(WalletTxnReadParams p, CancellationToken ct);
	Task<UResponse<TagTxnErrorCodes>> Purchase(WalletPurchaseParams p, CancellationToken ct);
	
	Task<bool> HasEnoughBalance(Guid userId, decimal amount, CancellationToken ct);
}

public class WalletService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IWalletService {
	public async Task<UResponse<TagTxnErrorCodes>> Purchase(WalletPurchaseParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TagTxnErrorCodes>(TagTxnErrorCodes.UnAuthorized, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		Guid receiverId;
		decimal amount;
		string detail1 = p.Tag.ToString();

		switch (p.Tag) {
			case TagPurchase.MobileAndNationalCodeVerification:
				receiverId = Core.App.Users.ITHub.Id;
				amount = Core.App.ApiCallCosts.MobileAndNationalCodeVerification;
				break;
			case TagPurchase.ZipCodeToAddressDetail:
				receiverId = Core.App.Users.ITHub.Id;
				amount = Core.App.ApiCallCosts.ZipCodeToAddressDetail;
				break;
			case TagPurchase.DrivingLicenceNegativePoint:
				receiverId = Core.App.Users.ITHub.Id;
				amount = Core.App.ApiCallCosts.DrivingLicenceNegativePoint;
				break;
			case TagPurchase.VehicleViolationsDetail:
				receiverId = Core.App.Users.ITHub.Id;
				amount = Core.App.ApiCallCosts.VehicleViolationsDetail;
				break;
			case TagPurchase.LicencePlateDetail:
				receiverId = Core.App.Users.ITHub.Id;
				amount = Core.App.ApiCallCosts.LicencePlateDetail;
				break;
			case TagPurchase.IBanToBankAccountDetail:
				receiverId = Core.App.Users.ITHub.Id;
				amount = Core.App.ApiCallCosts.IBanToBankAccountDetail;
				break;
			case TagPurchase.DrivingLicenceStatus:
				receiverId = Core.App.Users.ITHub.Id;
				amount = Core.App.ApiCallCosts.DrivingLicenceStatus;
				break;
			default:
				throw new Exception();
		}

		return await Transfer(new WalletTransferParams {
			ApiKey = p.ApiKey,
			Token = p.Token,
			SenderId = userData.Id,
			ReceiverId = receiverId,
			Amount = amount,
			Detail1 = detail1
		}, ct);
	}

	public async Task<bool> HasEnoughBalance(Guid userId, decimal amount, CancellationToken ct) {
		WalletEntity? e = await db.Set<WalletEntity>().FirstOrDefaultAsync(x => x.CreatorId == userId, ct);
		return amount >= e?.Balance;
	}

	public async Task<UResponse<IEnumerable<WalletResponse>?>> ReadByUserId(WalletReadParams p, CancellationToken ct) {
		IQueryable<WalletResponse> q = db.Set<WalletEntity>().Where(x => x.CreatorId == p.UserId).Select(Projections.WalletSelector(p.SelectorArgs));
		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<TagTxnErrorCodes>> Transfer(WalletTransferParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TagTxnErrorCodes>(TagTxnErrorCodes.UnAuthorized, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		Guid senderId = p.SenderId ?? userData.Id;

		WalletEntity? senderWallet = await db.Set<WalletEntity>().AsTracking().FirstOrDefaultAsync(x => x.CreatorId == senderId, ct);
		WalletEntity? receiverWallet = await db.Set<WalletEntity>().AsTracking().FirstOrDefaultAsync(x => x.CreatorId == p.ReceiverId, ct);
		if (senderWallet == null) return new UResponse<TagTxnErrorCodes>(TagTxnErrorCodes.SenderWalletNotFound, Usc.NotFound, ls.Get("SenderWalletNotFound"));
		if (receiverWallet == null) return new UResponse<TagTxnErrorCodes>(TagTxnErrorCodes.ReceiverWalletNotFound, Usc.NotFound, ls.Get("ReceiverWalletNotFound"));
		if (senderWallet.Balance < p.Amount) return new UResponse<TagTxnErrorCodes>(TagTxnErrorCodes.LowBalance, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

		decimal senderBalance = senderWallet.Balance - p.Amount;
		decimal receiverBalance = receiverWallet.Balance + p.Amount;

		senderWallet.Balance = senderBalance;
		receiverWallet.Balance = receiverBalance;

		await db.Set<WalletTxnEntity>().AddAsync(new WalletTxnEntity {
			Id = Guid.CreateVersion7(),
			CreatorId = userData.Id,
			CreatedAt = DateTime.UtcNow,
			SenderId = senderId,
			ReceiverId = p.ReceiverId,
			Amount = p.Amount,
			JsonData = new BaseJsonData { Detail1 = p.Detail1 ?? "" },
			Tags = []
		}, ct);

		db.Set<WalletEntity>().Update(senderWallet);
		db.Set<WalletEntity>().Update(receiverWallet);

		await db.SaveChangesAsync(ct);
		return new UResponse<TagTxnErrorCodes>(TagTxnErrorCodes.Ok);
	}

	public async Task<UResponse<IEnumerable<WalletTxnResponse>?>> ReadTxn(WalletTxnReadParams p, CancellationToken ct) {
		IQueryable<WalletTxnResponse> q = db.Set<WalletTxnEntity>()
			.Where(x => x.SenderId == p.UserId || x.ReceiverId == p.UserId)
			.Select(Projections.WalletTxnSelector(p.SelectorArgs));

		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}
}