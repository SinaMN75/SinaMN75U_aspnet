namespace SinaMN75U.Services;

public interface IWalletService {
	Task<UResponse> Charge(WalletChargeParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<WalletResponse>?>> ReadByUserId(WalletReadParams p, CancellationToken ct);
	Task<UResponse<WalletTxnResponse?>> Transfer(WalletTransferParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<WalletTxnResponse>?>> ReadTxn(WalletTxnReadParams p, CancellationToken ct);
	Task<UResponse> Purchase(WalletPurchaseParams p, CancellationToken ct);

	Task<bool> HasEnoughBalance(Guid userId, decimal amount, CancellationToken ct);
}

public class WalletService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IWalletService {
	public async Task<UResponse> Purchase(WalletPurchaseParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TagTxnErrorCodes>(TagTxnErrorCodes.UnAuthorized, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		Guid receiverId;
		decimal amount;
		string detail1 = p.Tag.ToString();

		switch (p.Tag) {
			case TagWalletTxn.MobileAndNationalCodeVerification:
				receiverId = Core.App.Users.ITHub.Id;
				amount = Core.App.ApiCallCosts.MobileAndNationalCodeVerification;
				break;
			case TagWalletTxn.ZipCodeToAddressDetail:
				receiverId = Core.App.Users.ITHub.Id;
				amount = Core.App.ApiCallCosts.ZipCodeToAddressDetail;
				break;
			case TagWalletTxn.DrivingLicenceNegativePoint:
				receiverId = Core.App.Users.ITHub.Id;
				amount = Core.App.ApiCallCosts.DrivingLicenceNegativePoint;
				break;
			case TagWalletTxn.VehicleViolationsDetail:
				receiverId = Core.App.Users.ITHub.Id;
				amount = Core.App.ApiCallCosts.VehicleViolationsDetail;
				break;
			case TagWalletTxn.LicencePlateDetail:
				receiverId = Core.App.Users.ITHub.Id;
				amount = Core.App.ApiCallCosts.LicencePlateDetail;
				break;
			case TagWalletTxn.IBanToBankAccountDetail:
				receiverId = Core.App.Users.ITHub.Id;
				amount = Core.App.ApiCallCosts.IBanToBankAccountDetail;
				break;
			case TagWalletTxn.DrivingLicenceStatus:
				receiverId = Core.App.Users.ITHub.Id;
				amount = Core.App.ApiCallCosts.DrivingLicenceStatus;
				break;
			case TagWalletTxn.FreewayTolls:
				receiverId = Core.App.Users.ITHub.Id;
				amount = Core.App.ApiCallCosts.FreewayToll;
				break;
			case TagWalletTxn.ChargeSimPin:
			case TagWalletTxn.ChargeSimTopup:
			case TagWalletTxn.InternetSim:
				receiverId = Core.App.Users.Mobtakeran.Id;
				amount = p.Amount!.Value;
				break;
			case TagWalletTxn.Transfer:
			case TagWalletTxn.Charge:
			default:
				throw new Exception();
		}

		return await Transfer(new WalletTransferParams {
			ApiKey = p.ApiKey,
			Token = p.Token,
			SenderId = userData.Id,
			ReceiverId = receiverId,
			Amount = amount,
			Detail1 = detail1,
			TagWalletTxn = [p.Tag]
		}, ct);
	}

	public async Task<bool> HasEnoughBalance(Guid userId, decimal amount, CancellationToken ct) {
		WalletEntity? e = await db.Set<WalletEntity>().FirstOrDefaultAsync(x => x.CreatorId == userId, ct);
		return amount <= e?.Balance;
	}

	public async Task<UResponse> Charge(WalletChargeParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		WalletEntity? e = await db.Set<WalletEntity>().AsTracking().FirstOrDefaultAsync(x => x.CreatorId == p.UserId, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("WalletNotFound"));

		if (!userData.IsAdmin && userData.Id != e.CreatorId) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		return await Transfer(new WalletTransferParams {
			ApiKey = p.ApiKey,
			Token = p.Token,
			SenderId = Core.App.Users.ITHub.Id,
			ReceiverId = p.UserId,
			Amount = p.Amount,
			Detail1 = "chargeWallet",
			TagWalletTxn = [TagWalletTxn.Charge]
		}, ct);
	}

	public async Task<UResponse<IEnumerable<WalletResponse>?>> ReadByUserId(WalletReadParams p, CancellationToken ct) {
		IQueryable<WalletResponse> q = db.Set<WalletEntity>().Where(x => x.CreatorId == p.UserId).Select(Projections.WalletSelector(p.SelectorArgs));
		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}
	
	public async Task<UResponse<WalletTxnResponse?>> Transfer(WalletTransferParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<WalletTxnResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		Guid senderId = p.SenderId ?? userData.Id;

		WalletEntity? senderWallet = await db.Set<WalletEntity>().AsTracking().FirstOrDefaultAsync(x => x.CreatorId == senderId, ct);
		WalletEntity? receiverWallet = await db.Set<WalletEntity>().AsTracking().FirstOrDefaultAsync(x => x.CreatorId == p.ReceiverId, ct);
		if (senderWallet == null) return new UResponse<WalletTxnResponse?>(null, Usc.NotFound, ls.Get("SenderWalletNotFound"));
		if (receiverWallet == null) return new UResponse<WalletTxnResponse?>(null, Usc.NotFound, ls.Get("ReceiverWalletNotFound"));
		if (senderWallet.Balance < p.Amount) return new UResponse<WalletTxnResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

		decimal senderBalance = senderWallet.Balance - p.Amount;
		decimal receiverBalance = receiverWallet.Balance + p.Amount;

		senderWallet.Balance = senderBalance;
		receiverWallet.Balance = receiverBalance;

		WalletTxnEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatorId = userData.Id,
			CreatedAt = DateTime.UtcNow,
			SenderId = senderId,
			ReceiverId = p.ReceiverId,
			Amount = p.Amount,
			JsonData = new BaseJson { Detail1 = p.Detail1 ?? "" },
			Tags = [TagWalletTxn.Transfer]
		};
		await db.Set<WalletTxnEntity>().AddAsync(e, ct);

		db.Set<WalletEntity>().Update(senderWallet);
		db.Set<WalletEntity>().Update(receiverWallet);
		await db.SaveChangesAsync(ct);
		
		return new UResponse<WalletTxnResponse?>(new WalletTxnResponse {
			SenderId = e.SenderId,
			ReceiverId = e.ReceiverId,
			Amount = e.Amount,
			JsonData = e.JsonData,
			Tags = e.Tags
		});
	}

	public async Task<UResponse<IEnumerable<WalletTxnResponse>?>> ReadTxn(WalletTxnReadParams p, CancellationToken ct) {
		IQueryable<WalletTxnResponse> q = db.Set<WalletTxnEntity>()
			.Where(x => x.SenderId == p.UserId || x.ReceiverId == p.UserId)
			.Select(Projections.WalletTxnSelector(p.SelectorArgs));

		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}
}