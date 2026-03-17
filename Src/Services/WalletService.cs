namespace SinaMN75U.Services;

public interface IWalletService {
	Task<UResponse> Transfer(WalletTransferParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<WalletTxnResponse>?>> ReadTxn(WalletTxnReadParams p, CancellationToken ct);
	Task<UResponse> Create(Guid userId, CancellationToken ct);
	Task<UResponse> Purchase(WalletPurchaseParams p, CancellationToken ct);
}

public class WalletService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IWalletService {
	public async Task<UResponse> Create(Guid userId, CancellationToken ct) {
		WalletEntity? existingWallet = await db.Set<WalletEntity>().FirstOrDefaultAsync(x => x.UserId == userId, ct);
		if (existingWallet != null) return new UResponse(Usc.WalletAlreadyExists, ls.Get("WalletForThisUserAlreadyExists"));

		WalletEntity e = new() {
			UserId = userId,
			JsonData = new WalletJson(),
			Tags = [],
			Balance = 0
		};
		await db.Set<WalletEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> Purchase(WalletPurchaseParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));
		UserEntity receiver = (await db.Set<UserEntity>().Select(x => new UserEntity {
			Id = x.Id,
			JsonData = x.JsonData,
			Tags = x.Tags
		}).FirstOrDefaultAsync(x => x.UserName == Core.App.ItHub.WalletOwnerUserName, ct))!;
		return p.Tag switch {
			TagPurchase.MobileAndNationalCodeVerification => await Transfer(new WalletTransferParams {
				ApiKey = p.ApiKey,
				Token = p.Token,
				Locale = p.Locale,
				SenderId = userData.Id,
				ReceiverId = receiver.Id,
				Amount = Core.App.ItHub.ShahkarVerifyNationalCodeAndMobilePrice,
				Description = "سامانه شاهکار"
			}, ct),
			TagPurchase.ZipCodeToAddressDetail => await Transfer(new WalletTransferParams {
				ApiKey = p.ApiKey,
				Token = p.Token,
				Locale = p.Locale,
				SenderId = userData.Id,
				ReceiverId = receiver.Id,
				Amount = Core.App.ItHub.ShahkarVerifyNationalCodeAndMobilePrice,
				Description = "سامانه شاهکار"
			}, ct),
			_ => throw new Exception()
		};
	}

	public async Task<UResponse> Transfer(WalletTransferParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		Guid senderId = p.SenderId ?? userData.Id;

		WalletEntity? senderWallet = await db.Set<WalletEntity>().AsTracking().FirstOrDefaultAsync(x => x.UserId == senderId, ct);
		WalletEntity? receiverWallet = await db.Set<WalletEntity>().AsTracking().FirstOrDefaultAsync(x => x.UserId == p.ReceiverId, ct);
		if (senderWallet == null) return new UResponse(Usc.NotFound, ls.Get("SenderWalletNotFound"));
		if (receiverWallet == null) return new UResponse(Usc.NotFound, ls.Get("ReceiverWalletNotFound"));
		if (senderWallet.Balance < p.Amount) return new UResponse(Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

		decimal senderBalance = senderWallet.Balance - p.Amount;
		decimal receiverBalance = receiverWallet.Balance + p.Amount;

		senderWallet.Balance = senderBalance;
		receiverWallet.Balance = senderBalance;

		await db.Set<WalletTxnEntity>().AddAsync(new WalletTxnEntity {
			SenderId = senderId,
			ReceiverId = p.ReceiverId,
			Amount = senderBalance,
			JsonData = new WalletTxnJson(),
			Tags = []
		}, ct);
		await db.Set<WalletTxnEntity>().AddAsync(new WalletTxnEntity {
			SenderId = senderId,
			ReceiverId = p.ReceiverId,
			Amount = receiverBalance,
			JsonData = new WalletTxnJson(),
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