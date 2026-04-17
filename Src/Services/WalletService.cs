namespace SinaMN75U.Services;

public interface IWalletService {
	Task<UResponse<IEnumerable<WalletResponse>?>> ReadByUserId(WalletReadParams p, CancellationToken ct);
	Task<UResponse> Transfer(WalletTransferParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<WalletTxnResponse>?>> ReadTxn(WalletTxnReadParams p, CancellationToken ct);
	Task<UResponse> Purchase(WalletPurchaseParams p, CancellationToken ct);
}

public class WalletService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IWalletService {
	public async Task<UResponse> Purchase(WalletPurchaseParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		UserEntity receiver = (await db.Set<UserEntity>().Select(x => new UserEntity {
			Id = x.Id,
			CreatorId = userData.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = x.JsonData,
			Tags = x.Tags
		}).FirstOrDefaultAsync(x => x.UserName == Core.App.ItHub.WalletOwnerUserName, ct))!;
		return p.Tag switch {
			TagPurchase.MobileAndNationalCodeVerification => await Transfer(new WalletTransferParams {
				ApiKey = p.ApiKey,
				Token = p.Token,
				SenderId = userData.Id,
				ReceiverId = receiver.Id,
				Amount = Core.App.ItHub.ShahkarVerifyNationalCodeAndMobilePrice,
				Description = "سامانه شاهکار"
			}, ct),
			TagPurchase.ZipCodeToAddressDetail => await Transfer(new WalletTransferParams {
				ApiKey = p.ApiKey,
				Token = p.Token,
				SenderId = userData.Id,
				ReceiverId = receiver.Id,
				Amount = Core.App.ItHub.ShahkarVerifyNationalCodeAndMobilePrice,
				Description = "سامانه شاهکار"
			}, ct),
			TagPurchase.Test => throw new Exception(),
			_ => throw new Exception()
		};
	}

	public async Task<UResponse<IEnumerable<WalletResponse>?>> ReadByUserId(WalletReadParams p, CancellationToken ct) {
		IQueryable<WalletResponse> q = db.Set<WalletEntity>()
			.Where(x => x.CreatorId == p.UserId)
			.Select(Projections.WalletSelector(p.SelectorArgs));
		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> Transfer(WalletTransferParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		Guid senderId = p.SenderId ?? userData.Id;

		WalletEntity? senderWallet = await db.Set<WalletEntity>().AsTracking().FirstOrDefaultAsync(x => x.CreatorId == senderId, ct);
		WalletEntity? receiverWallet = await db.Set<WalletEntity>().AsTracking().FirstOrDefaultAsync(x => x.CreatorId == p.ReceiverId, ct);
		if (senderWallet == null) return new UResponse(Usc.NotFound, ls.Get("SenderWalletNotFound"));
		if (receiverWallet == null) return new UResponse(Usc.NotFound, ls.Get("ReceiverWalletNotFound"));
		if (senderWallet.Balance < p.Amount) return new UResponse(Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

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
			JsonData = new BaseJsonData {Detail1 = p.Description ?? ""},
			Tags = []
		}, ct);

		db.Set<WalletEntity>().Update(senderWallet);
		db.Set<WalletEntity>().Update(receiverWallet);

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<IEnumerable<WalletTxnResponse>?>> ReadTxn(WalletTxnReadParams p, CancellationToken ct) {
		IQueryable<WalletTxnResponse> q = db.Set<WalletTxnEntity>()
			.Where(x => x.SenderId == p.UserId || x.ReceiverId == p.UserId)
			.Select(Projections.WalletTxnSelector(p.SelectorArgs));

		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}
}