namespace SinaMN75U.Routes;

public static class WalletRoutes {
	public static void MapWalletRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Charge", async (WalletChargeParams p, IWalletService s, CancellationToken c) => (await s.Charge(p, c)).ToResult()).Produces<UResponse>();
		// Auth is enforced here (not inside IWalletService.Transfer) because internal flows (gold, hotel refunds, purchases)
		// legitimately transfer from system/other wallets.
		r.MapPost("Transfer", async (WalletTransferParams p, IWalletService s, ITokenService ts, ILocalizationService ls, CancellationToken c) => {
			JwtClaimData? userData = ts.ExtractClaims(p.Token);
			if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue")).ToResult();
			if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired")).ToResult();
			if (!userData.IsAdmin && userData.Id != p.SenderId) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction")).ToResult();
			return (await s.Transfer(p, c)).ToResult();
		}).Produces<UResponse>();
		r.MapPost("ReadTxn", async (WalletTxnReadParams p, IWalletService s, CancellationToken c) => (await s.ReadTxn(p, c)).ToResult()).Produces<UResponse<IEnumerable<WalletTxnResponse>?>>();
		r.MapPost("Read", async (WalletReadParams p, IWalletService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Produces<UResponse<IEnumerable<WalletResponse>?>>();
		r.MapPost("ReadByUserId", async (IdParams<WalletSelectorArgs> p, IWalletService s, CancellationToken c) => (await s.ReadByUserId(p, c)).ToResult()).Produces<UResponse<IEnumerable<WalletResponse>?>>();
		r.MapPost("Purchase", async (WalletPurchaseParams p, IWalletService s, CancellationToken c) => (await s.Purchase(p, c)).ToResult()).Produces<UResponse>();
	}
}