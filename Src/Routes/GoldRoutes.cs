namespace SinaMN75U.Routes;

public static class GoldRoutes {
	public static void MapGoldRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("ReadAccount", async (BaseParams p, IGoldService s, ITokenService ts, ILocalizationService ls, CancellationToken c) => AdminOnly(p, ts, ls) ?? (await s.ReadAccount(p, c)).ToResult()).Produces<UResponse<GoldAccountResponse>>();
		r.MapPost("ReadQuote", async (GoldQuoteParams p, IGoldService s, CancellationToken c) => (await s.ReadQuote(p, c)).ToResult()).Produces<UResponse<GoldQuoteResponse>>();
		r.MapPost("ReadUserBalance", async (GoldReadUserBalanceParams p, IGoldService s, CancellationToken c) => (await s.ReadUserBalance(p, c)).ToResult()).Produces<UResponse<GoldUserBalanceResponse>>();
		r.MapPost("Buy", async (GoldBuyParams p, IGoldService s, CancellationToken c) => (await s.Buy(p, c)).ToResult()).Produces<UResponse<GoldTxnResponse>>();
		r.MapPost("Sell", async (GoldSellParams p, IGoldService s, CancellationToken c) => (await s.Sell(p, c)).ToResult()).Produces<UResponse<GoldTxnResponse>>();
		r.MapPost("SyncTxn", async (IdParams p, IGoldService s, CancellationToken c) => (await s.SyncTxn(p, c)).ToResult()).Produces<UResponse<GoldTxnResponse>>();
		r.MapPost("ReadUserTxns", async (GoldReadUserTxnsParams p, IGoldService s, CancellationToken c) => (await s.ReadUserTxns(p, c)).ToResult()).Produces<UResponse<IEnumerable<GoldTxnResponse>>>();
		r.MapPost("CreateOrder", async (GoldCreateOrderParams p, IGoldService s, ITokenService ts, ILocalizationService ls, CancellationToken c) => AdminOnly(p, ts, ls) ?? (await s.CreateOrder(p, c)).ToResult()).Produces<UResponse<GoldOrderResponse>>();
		r.MapPost("ReadOrders", async (GoldReadOrdersParams p, IGoldService s, ITokenService ts, ILocalizationService ls, CancellationToken c) => AdminOnly(p, ts, ls) ?? (await s.ReadOrders(p, c)).ToResult()).Produces<UResponse<GoldOrderListResponse>>();
		r.MapPost("ReadOrderById", async (GoldReadOrderParams p, IGoldService s, ITokenService ts, ILocalizationService ls, CancellationToken c) => AdminOnly(p, ts, ls) ?? (await s.ReadOrderById(p, c)).ToResult()).Produces<UResponse<GoldOrderResponse>>();
		r.MapPost("ReadBalances", async (BaseParams p, IGoldService s, ITokenService ts, ILocalizationService ls, CancellationToken c) => AdminOnly(p, ts, ls) ?? (await s.ReadBalances(p, c)).ToResult()).Produces<UResponse<IEnumerable<GoldBalanceResponse>>>();
		r.MapPost("ReadBalance", async (GoldReadBalanceParams p, IGoldService s, ITokenService ts, ILocalizationService ls, CancellationToken c) => AdminOnly(p, ts, ls) ?? (await s.ReadBalance(p, c)).ToResult()).Produces<UResponse<GoldBalanceResponse>>();
		r.MapPost("ReadTransactions", async (GoldReadTransactionsParams p, IGoldService s, ITokenService ts, ILocalizationService ls, CancellationToken c) => AdminOnly(p, ts, ls) ?? (await s.ReadTransactions(p, c)).ToResult()).Produces<UResponse<GoldTransactionListResponse>>();
		r.MapPost("ReadTradeLimits", async (BaseParams p, IGoldService s, ITokenService ts, ILocalizationService ls, CancellationToken c) => AdminOnly(p, ts, ls) ?? (await s.ReadTradeLimits(p, c)).ToResult()).Produces<UResponse<GoldTradeLimitsResponse>>();
		r.MapPost("ReadCreditFacilities", async (BaseParams p, IGoldService s, ITokenService ts, ILocalizationService ls, CancellationToken c) => AdminOnly(p, ts, ls) ?? (await s.ReadCreditFacilities(p, c)).ToResult()).Produces<UResponse<GoldCreditFacilitiesResponse>>();
		r.MapPost("CreateApiToken", async (GoldCreateApiTokenParams p, IGoldService s, ITokenService ts, ILocalizationService ls, CancellationToken c) => AdminOnly(p, ts, ls) ?? (await s.CreateApiToken(p, c)).ToResult()).Produces<UResponse<GoldApiTokenResponse>>();
		r.MapPost("ReadApiTokens", async (BaseParams p, IGoldService s, ITokenService ts, ILocalizationService ls, CancellationToken c) => AdminOnly(p, ts, ls) ?? (await s.ReadApiTokens(p, c)).ToResult()).Produces<UResponse<IEnumerable<GoldApiTokenResponse>>>();
		r.MapPost("DeleteApiToken", async (GoldDeleteApiTokenParams p, IGoldService s, ITokenService ts, ILocalizationService ls, CancellationToken c) => AdminOnly(p, ts, ls) ?? (await s.DeleteApiToken(p, c)).ToResult()).Produces<UResponse>();
	}

	// These operate on the platform's single broker account (orders, balances, API tokens), so they are admin-only when
	// called over HTTP. Buy/Sell/SyncTxn still call them internally on behalf of the user.
	private static IResult? AdminOnly(BaseParams p, ITokenService ts, ILocalizationService ls) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue")).ToResult();
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired")).ToResult();
		return userData.IsAdmin ? null : new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction")).ToResult();
	}
}
