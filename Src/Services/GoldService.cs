using Microsoft.EntityFrameworkCore.Storage;

namespace SinaMN75U.Services;

public interface IGoldService {
	Task<UResponse<GoldAccountResponse?>> ReadAccount(BaseParams p, CancellationToken ct);
	Task<UResponse<GoldQuoteResponse?>> ReadQuote(GoldQuoteParams p, CancellationToken ct);
	Task<UResponse<GoldUserBalanceResponse?>> ReadUserBalance(GoldReadUserBalanceParams p, CancellationToken ct);
	Task<UResponse<GoldTxnResponse?>> Buy(GoldBuyParams p, CancellationToken ct);
	Task<UResponse<GoldTxnResponse?>> Sell(GoldSellParams p, CancellationToken ct);
	Task<UResponse<GoldTxnResponse?>> SyncTxn(IdParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<GoldTxnResponse>?>> ReadUserTxns(GoldReadUserTxnsParams p, CancellationToken ct);
	Task<UResponse<GoldOrderResponse?>> CreateOrder(GoldCreateOrderParams p, CancellationToken ct);
	Task<UResponse<GoldOrderListResponse?>> ReadOrders(GoldReadOrdersParams p, CancellationToken ct);
	Task<UResponse<GoldOrderResponse?>> ReadOrderById(GoldReadOrderParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<GoldBalanceResponse>?>> ReadBalances(BaseParams p, CancellationToken ct);
	Task<UResponse<GoldBalanceResponse?>> ReadBalance(GoldReadBalanceParams p, CancellationToken ct);
	Task<UResponse<GoldTransactionListResponse?>> ReadTransactions(GoldReadTransactionsParams p, CancellationToken ct);
	Task<UResponse<GoldTradeLimitsResponse?>> ReadTradeLimits(BaseParams p, CancellationToken ct);
	Task<UResponse<GoldCreditFacilitiesResponse?>> ReadCreditFacilities(BaseParams p, CancellationToken ct);
	Task<UResponse<GoldApiTokenResponse?>> CreateApiToken(GoldCreateApiTokenParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<GoldApiTokenResponse>?>> ReadApiTokens(BaseParams p, CancellationToken ct);
	Task<UResponse> DeleteApiToken(GoldDeleteApiTokenParams p, CancellationToken ct);
}

public class GoldService(
	IHttpClientService httpClient,
	ILocalizationService ls,
	ITokenService ts,
	IHttpContextAccessor httpContext,
	IWalletService wallet,
	DbContext db
) : IGoldService {
	private const string ClientPath = "api/v1/client/";
	private const int MinPageLimit = 1;
	private const int MaxPageLimit = 100;
	private const int OrderAttempts = 3;

	// Taline's GOLD18 amounts use 3 decimal places and IRR has none; sending more digits fails with AMOUNT_PRECISION_EXCEEDED.
	private const int GoldDecimals = 3;
	private const decimal BuyReserveBuffer = 1.02m;

	// Every token this service creates for itself carries this label, so leaked ones can be found and revoked.
	private const string AutoTokenLabelPrefix = "SinaMN75U-";

	// Taline allows 120 requests per minute for the whole account, so the price shown to users is shared for a few seconds.
	private static readonly TimeSpan QuoteCacheDuration = TimeSpan.FromSeconds(10);

	// A pending txn whose order still can't be found at Taline after this long is treated as never placed and released.
	private static readonly TimeSpan PendingGiveUpAfter = TimeSpan.FromMinutes(2);

	// Gold-ledger progress of a txn, kept in JsonData.Detail2 (no schema change), so a retried settlement never moves
	// the same gold twice. Buy: "" -> GoldCredited. Sell: "" -> GoldReserved -> GoldSettled or GoldReleased.
	private const string GoldReserved = "goldReserved";
	private const string GoldCredited = "goldCredited";
	private const string GoldSettled = "goldSettled";
	private const string GoldReleased = "goldReleased";

	// Provider errors that make sense to an app user; anything else is about our business account (IP, token, scopes,
	// business wallet balance...) and is shown to users as "temporarily unavailable" while the real error is logged.
	private static readonly HashSet<string> UserFacingErrors = [
		"TRADE_SIDE_CLOSED", "PRICE_UNAVAILABLE", "INVALID_TRADE_AMOUNT", "AMOUNT_PRECISION_EXCEEDED", "INSUFFICIENT_QUOTE_AMOUNT",
		"ORDER_TRADE_WINDOW_LIMIT_EXCEEDED", "UNSUPPORTED_TRADE_PAIR", "RATE_LIMIT_EXCEEDED"
	];

	private static string? _cachedApiToken;
	private static readonly SemaphoreSlim TokenLock = new(1, 1);
	private static QuoteCacheEntry? _quoteCache;

	// Settling moves money and gold, so only one settlement runs at a time (Buy/Sell, SyncTxn and the automatic sync).
	private static readonly SemaphoreSlim SettleLock = new(1, 1);
	private static bool _warnedAboutApiToken;

	private static Guid HouseUserId => Core.App.Users.AvaPlus.Id;

	public async Task<UResponse<GoldAccountResponse?>> ReadAccount(BaseParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<GoldAccountResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<GoldAccountResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		GoldResult r = await CallWithToken(HttpMethod.Get, $"{ClientPath}account", null, ct);
		if (!r.Ok) return new UResponse<GoldAccountResponse?>(null, r.Status, r.Message);

		JsonElement item = r.Item;
		return new UResponse<GoldAccountResponse?>(new GoldAccountResponse {
			Name = item.GetStringOrNull("name") ?? "",
			Active = item.GetStringOrNull("status")?.ToUpperInvariant() == "ACTIVE",
			IpWhitelist = ReadStringList(item, "ipWhitelist")
		});
	}

	public async Task<UResponse<GoldQuoteResponse?>> ReadQuote(GoldQuoteParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<GoldQuoteResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<GoldQuoteResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (p.BaseAsset == p.QuoteAsset) return new UResponse<GoldQuoteResponse?>(null, Usc.BadRequest, ls.Get("thisTradePairIsNotSupported"));

		(GoldQuoteResponse? quote, GoldResult? error) = await FetchQuote(p.BaseAsset, p.QuoteAsset, false, ct);
		if (quote != null) return new UResponse<GoldQuoteResponse?>(quote);
		return new UResponse<GoldQuoteResponse?>(null, userData.IsAdmin ? error!.Status : UserStatus(error!), userData.IsAdmin ? error!.Message : UserMessage(error!));
	}

	public async Task<UResponse<GoldOrderResponse?>> CreateOrder(GoldCreateOrderParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<GoldOrderResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<GoldOrderResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (p.BaseAsset == p.QuoteAsset) return new UResponse<GoldOrderResponse?>(null, Usc.BadRequest, ls.Get("thisTradePairIsNotSupported"));

		decimal? baseAmount = p.BaseAmount is > 0 ? RoundGold(p.BaseAmount.Value) : null;
		decimal? quoteAmount = p.QuoteAmount is > 0 ? Math.Floor(p.QuoteAmount.Value) : null;
		if (baseAmount is > 0 == quoteAmount is > 0) return new UResponse<GoldOrderResponse?>(null, Usc.BadRequest, ls.Get("sendExactlyOneOfBaseAmountOrQuoteAmountGreaterThanZero"));

		OrderOutcome outcome = await PlaceOrder(p.IdempotencyKey, p.Side ?? TagGoldOrderSide.Buy, baseAmount, quoteAmount, ct);
		return outcome.State switch {
			OrderState.Filled => new UResponse<GoldOrderResponse?>(outcome.Order, Usc.Created),
			OrderState.Rejected => new UResponse<GoldOrderResponse?>(null, outcome.Error!.Status, outcome.Error.Message),
			_ => new UResponse<GoldOrderResponse?>(null, Usc.ThirdPartyError, ls.Get("theGoldOrderIsBeingProcessedAndWillBeSettledShortly"))
		};
	}

	public async Task<UResponse<GoldOrderListResponse?>> ReadOrders(GoldReadOrdersParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<GoldOrderListResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<GoldOrderListResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		string query = PagingQuery(p.Cursor, p.Limit) + (p.IdempotencyKey.IsNotNullOrEmpty() ? $"&idempotencyKey={Uri.EscapeDataString(p.IdempotencyKey)}" : "");
		GoldResult r = await CallWithToken(HttpMethod.Get, $"{ClientPath}orders{query}", null, ct);
		if (!r.Ok) return new UResponse<GoldOrderListResponse?>(null, r.Status, r.Message);

		return new UResponse<GoldOrderListResponse?>(new GoldOrderListResponse {
			Items = r.Items.Select(MapOrder).ToList(),
			NextCursor = r.NextCursor
		});
	}

	public async Task<UResponse<GoldOrderResponse?>> ReadOrderById(GoldReadOrderParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<GoldOrderResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<GoldOrderResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		GoldResult r = await CallWithToken(HttpMethod.Get, $"{ClientPath}orders/{Uri.EscapeDataString(p.Id)}", null, ct);
		return !r.Ok ? new UResponse<GoldOrderResponse?>(null, r.Status, r.Message) : new UResponse<GoldOrderResponse?>(MapOrder(r.Item));
	}

	public async Task<UResponse<IEnumerable<GoldBalanceResponse>?>> ReadBalances(BaseParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<GoldBalanceResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<GoldBalanceResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		GoldResult r = await CallWithToken(HttpMethod.Get, $"{ClientPath}wallets/main/balances", null, ct);
		if (!r.Ok) return new UResponse<IEnumerable<GoldBalanceResponse>?>(null, r.Status, r.Message);
		return new UResponse<IEnumerable<GoldBalanceResponse>?>(r.Items.Select(MapBalance).ToList());
	}

	public async Task<UResponse<GoldBalanceResponse?>> ReadBalance(GoldReadBalanceParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<GoldBalanceResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<GoldBalanceResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		GoldResult r = await CallWithToken(HttpMethod.Get, $"{ClientPath}wallets/main/balances/{AssetCode(p.Asset)}", null, ct);
		return !r.Ok ? new UResponse<GoldBalanceResponse?>(null, r.Status, r.Message) : new UResponse<GoldBalanceResponse?>(MapBalance(r.Item));
	}

	public async Task<UResponse<GoldTransactionListResponse?>> ReadTransactions(GoldReadTransactionsParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<GoldTransactionListResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<GoldTransactionListResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		GoldResult r = await CallWithToken(HttpMethod.Get, $"{ClientPath}wallets/main/transactions{PagingQuery(p.Cursor, p.Limit)}", null, ct);
		if (!r.Ok) return new UResponse<GoldTransactionListResponse?>(null, r.Status, r.Message);

		return new UResponse<GoldTransactionListResponse?>(new GoldTransactionListResponse {
			Items = r.Items.Select(x => new GoldTransactionResponse {
				Id = x.GetStringOrNull("id") ?? "",
				IdempotencyKey = x.GetStringOrNull("idempotencyKey"),
				CreatedAt = ReadDate(x, "createdAt"),
				Entries = ReadEntries(x),
				Detail = x.TryGetProperty("detail", out JsonElement d) && d.ValueKind is JsonValueKind.Object or JsonValueKind.Array ? d.GetRawText() : null
			}).ToList(),
			NextCursor = r.NextCursor
		});
	}

	public async Task<UResponse<GoldTradeLimitsResponse?>> ReadTradeLimits(BaseParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<GoldTradeLimitsResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<GoldTradeLimitsResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		GoldResult r = await CallWithToken(HttpMethod.Get, $"{ClientPath}trade-limits", null, ct);
		if (!r.Ok) return new UResponse<GoldTradeLimitsResponse?>(null, r.Status, r.Message);

		JsonElement data = r.Data;
		return new UResponse<GoldTradeLimitsResponse?>(new GoldTradeLimitsResponse {
			Timezone = data.GetStringOrNull("timezone"),
			CurrentTime = data.GetStringOrNull("currentTime"),
			Items = ReadArray(data, "items").Select(MapTradeLimit).ToList(),
			CurrentLimits = ReadArray(data, "currentLimits").Select(MapTradeLimit).ToList()
		});
	}

	public async Task<UResponse<GoldCreditFacilitiesResponse?>> ReadCreditFacilities(BaseParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<GoldCreditFacilitiesResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<GoldCreditFacilitiesResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		GoldResult r = await CallWithToken(HttpMethod.Get, $"{ClientPath}credit-facilities", null, ct);
		if (!r.Ok) return new UResponse<GoldCreditFacilitiesResponse?>(null, r.Status, r.Message);

		JsonElement data = r.Data;
		return new UResponse<GoldCreditFacilitiesResponse?>(new GoldCreditFacilitiesResponse {
			Timezone = data.GetStringOrNull("timezone"),
			CurrentTime = data.GetStringOrNull("currentTime"),
			Items = ReadArray(data, "items").Select(x => new GoldCreditFacilityResponse {
				Type = x.GetStringOrNull("type"),
				Asset = x.GetStringOrNull("asset"),
				CreditUsed = x.GetDecimalOrNull("creditUsed"),
				AvailableCredit = x.GetDecimalOrNull("availableCredit"),
				Limits = (x.TryGetProperty("detail", out JsonElement detail) && detail.ValueKind == JsonValueKind.Object ? ReadArray(detail, "limits") : [])
					.Select(l => new GoldCreditLimitResponse {
						Interval = l.GetStringOrNull("interval"),
						Limit = l.GetDecimalOrNull("limit"),
						Used = l.GetDecimalOrNull("used"),
						Remaining = l.GetDecimalOrNull("remaining"),
						ResetsAt = l.GetStringOrNull("resetsAt")
					}).ToList()
			}).ToList(),
			Balances = ReadArray(data, "balances").Select(x => new GoldAssetBalanceResponse {
				Asset = x.GetStringOrNull("asset"),
				Balance = x.GetDecimalOrNull("balance"),
				AvailableToTrade = x.GetDecimalOrNull("availableToTrade")
			}).ToList()
		});
	}

	public async Task<UResponse<GoldApiTokenResponse?>> CreateApiToken(GoldCreateApiTokenParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<GoldApiTokenResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<GoldApiTokenResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse<GoldApiTokenResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		GoldResult r = await PostApiToken(p.Label, p.Scopes, p.IpWhitelist, ct);
		return !r.Ok ? new UResponse<GoldApiTokenResponse?>(null, r.Status, r.Message) : new UResponse<GoldApiTokenResponse?>(MapApiToken(r.Item), Usc.Created);
	}

	public async Task<UResponse<IEnumerable<GoldApiTokenResponse>?>> ReadApiTokens(BaseParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<GoldApiTokenResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<GoldApiTokenResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse<IEnumerable<GoldApiTokenResponse>?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		GoldResult r = await CallWithBasic(HttpMethod.Get, $"{ClientPath}auth/api-tokens", null, ct);
		return !r.Ok ? new UResponse<IEnumerable<GoldApiTokenResponse>?>(null, r.Status, r.Message) : new UResponse<IEnumerable<GoldApiTokenResponse>?>(r.Items.Select(MapApiToken).ToList());
	}

	public async Task<UResponse> DeleteApiToken(GoldDeleteApiTokenParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		GoldResult r = await CallWithBasic(HttpMethod.Delete, $"{ClientPath}auth/api-tokens/{Uri.EscapeDataString(p.TokenId)}", null, ct);
		return r.Ok ? new UResponse(Usc.Deleted) : new UResponse(r.Status, r.Message);
	}

	// The user's gold lives in our own ledger (GoldWallets), so the balance is always shown even when the provider is down;
	// only the rial value and prices depend on the live quote.
	public async Task<UResponse<GoldUserBalanceResponse?>> ReadUserBalance(GoldReadUserBalanceParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<GoldUserBalanceResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<GoldUserBalanceResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		Guid userId = p.UserId != null && userData.IsAdmin ? p.UserId.Value : userData.Id;
		await SyncStalePendingTxns(userId, ct);

		GoldWalletEntity e = await ReadOrCreateWallet(userId, ct);
		(GoldQuoteResponse? quote, GoldResult? error) = await FetchQuote(TagGoldAsset.Gold18, TagGoldAsset.Irr, false, ct);
		decimal? valuePrice = quote?.SellUnitPrice ?? quote?.BaseUnitPrice;

		return new UResponse<GoldUserBalanceResponse?>(new GoldUserBalanceResponse {
			Balance = e.Balance,
			Unit = quote?.Unit,
			BaseUnitPrice = quote?.BaseUnitPrice,
			BuyUnitPrice = quote?.BuyUnitPrice,
			SellUnitPrice = quote?.SellUnitPrice,
			CanBuy = quote?.CanBuy ?? false,
			CanSell = quote?.CanSell ?? false,
			Value = e.Balance == 0 ? 0 : valuePrice == null ? null : Math.Floor(e.Balance * valuePrice.Value),
			PriceError = error == null ? null : UserMessage(error),
			UpdatedAt = quote?.UpdatedAt
		});
	}

	public async Task<UResponse<GoldTxnResponse?>> Buy(GoldBuyParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<GoldTxnResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<GoldTxnResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		decimal? amount = p.Amount is > 0 ? Math.Floor(p.Amount.Value) : null;
		decimal? goldAmount = p.GoldAmount is > 0 ? RoundGold(p.GoldAmount.Value) : null;
		if (amount is > 0 == goldAmount is > 0) return new UResponse<GoldTxnResponse?>(null, Usc.BadRequest, ls.Get("sendExactlyOneOfBaseAmountOrQuoteAmountGreaterThanZero"));

		(GoldQuoteResponse? quote, GoldResult? quoteError) = await FetchQuote(TagGoldAsset.Gold18, TagGoldAsset.Irr, true, ct);
		if (quote == null) return new UResponse<GoldTxnResponse?>(null, UserStatus(quoteError!), UserMessage(quoteError!));
		if (!quote.CanBuy || quote.BuyUnitPrice is not > 0) return new UResponse<GoldTxnResponse?>(null, Usc.BadRequest, ls.Get("buyingGoldIsClosedRightNow"));
		decimal unitPrice = quote.BuyUnitPrice.Value;

		// Buying by rial spends exactly that amount; buying by grams costs whatever the fill price is, so a small buffer is
		// reserved and the difference is returned after the fill.
		decimal reserve = amount ?? Math.Ceiling(goldAmount!.Value * unitPrice * BuyReserveBuffer);
		if (!await wallet.HasEnoughBalance(userData.Id, reserve, ct)) return new UResponse<GoldTxnResponse?>(null, Usc.BalanceIsLow, ls.Get("yourBalanceIsNotEnough"));

		GoldTxnEntity e = NewTxn(userData.Id, TagGoldTxn.Buy, unitPrice, new GoldTxnJson {
			Detail1 = nameof(TagGoldTxn.Buy),
			RequestedAmount = amount,
			RequestedGoldAmount = goldAmount,
			ReservedAmount = reserve
		});
		await db.Set<GoldTxnEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);

		decimal estimatedGold = goldAmount ?? RoundGold(reserve / unitPrice);
		if (!await MoveMoneyOnce(e, "reserve", userData.Id, HouseUserId, reserve, estimatedGold, TagWalletTxn.GoldPurchase, false, ct))
			return await FailTxn(e, Usc.BalanceIsLow, ls.Get("yourBalanceIsNotEnough"), ct);

		OrderOutcome outcome = await PlaceOrder(e.IdempotencyKey, TagGoldOrderSide.Buy, goldAmount, amount, ct);
		return await ResolveOutcome(e, outcome, ct);
	}

	public async Task<UResponse<GoldTxnResponse?>> Sell(GoldSellParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<GoldTxnResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<GoldTxnResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		decimal? amount = p.Amount is > 0 ? Math.Floor(p.Amount.Value) : null;
		decimal? goldAmount = p.GoldAmount is > 0 ? RoundGold(p.GoldAmount.Value) : null;
		if (amount is > 0 == goldAmount is > 0) return new UResponse<GoldTxnResponse?>(null, Usc.BadRequest, ls.Get("sendExactlyOneOfBaseAmountOrQuoteAmountGreaterThanZero"));

		(GoldQuoteResponse? quote, GoldResult? quoteError) = await FetchQuote(TagGoldAsset.Gold18, TagGoldAsset.Irr, true, ct);
		if (quote == null) return new UResponse<GoldTxnResponse?>(null, UserStatus(quoteError!), UserMessage(quoteError!));
		if (!quote.CanSell || quote.SellUnitPrice is not > 0) return new UResponse<GoldTxnResponse?>(null, Usc.BadRequest, ls.Get("sellingGoldIsClosedRightNow"));
		decimal unitPrice = quote.SellUnitPrice.Value;

		// A sell order is always placed by grams (converted from the rial amount if needed), so exactly the reserved gold is
		// sold and the user's gold ledger can never go below what the provider actually sold.
		decimal grams = goldAmount ?? RoundGold(amount!.Value / unitPrice);
		if (grams <= 0) return new UResponse<GoldTxnResponse?>(null, Usc.BadRequest, ls.Get("theTradeAmountIsNotValid"));

		GoldWalletEntity goldWallet = await ReadOrCreateWallet(userData.Id, ct);
		if (goldWallet.JsonData.Locked) return new UResponse<GoldTxnResponse?>(null, Usc.Forbidden, ls.Get("yourGoldWalletIsLocked"));
		if (goldWallet.Balance < grams) return new UResponse<GoldTxnResponse?>(null, Usc.BalanceIsLow, ls.Get("yourGoldBalanceIsNotEnoughForThisOrder"));

		GoldTxnEntity e = NewTxn(userData.Id, TagGoldTxn.Sell, unitPrice, new GoldTxnJson {
			Detail1 = nameof(TagGoldTxn.Sell),
			RequestedAmount = amount,
			RequestedGoldAmount = goldAmount,
			ReservedGoldAmount = grams
		});
		await db.Set<GoldTxnEntity>().AddAsync(e, ct);
		if (!await MoveGoldOnce(e, -grams, "", GoldReserved, false, ct)) {
			db.Entry(e).State = EntityState.Detached;
			return new UResponse<GoldTxnResponse?>(null, Usc.BalanceIsLow, ls.Get("yourGoldBalanceIsNotEnoughForThisOrder"));
		}

		OrderOutcome outcome = await PlaceOrder(e.IdempotencyKey, TagGoldOrderSide.Sell, grams, null, ct);
		return await ResolveOutcome(e, outcome, ct);
	}

	public async Task<UResponse<GoldTxnResponse?>> SyncTxn(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<GoldTxnResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<GoldTxnResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		GoldTxnEntity? e = await db.Set<GoldTxnEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<GoldTxnResponse?>(null, Usc.NotFound, ls.Get("theGoldTransactionWasNotFound"));
		if (e.UserId != userData.Id && !userData.IsAdmin) return new UResponse<GoldTxnResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		return await SyncPending(e, ct);
	}

	public async Task<UResponse<IEnumerable<GoldTxnResponse>?>> ReadUserTxns(GoldReadUserTxnsParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<GoldTxnResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<GoldTxnResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		Guid userId = p.UserId != null && userData.IsAdmin ? p.UserId.Value : userData.Id;
		IQueryable<GoldTxnResponse> q = db.Set<GoldTxnEntity>().ApplyReadParams(p)
			.Where(x => x.UserId == userId)
			.Select(Projections.GoldTxnSelector(p.SelectorArgs));

		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	// ===== Orders =====

	private enum OrderState { Filled, Rejected, Unknown }

	private sealed record OrderOutcome(OrderState State, GoldOrderResponse? Order = null, GoldResult? Error = null);

	// Places an order following Taline's idempotency rules: a timeout or 5xx is retried with the SAME key, a
	// DUPLICATE_IDEMPOTENCY_KEY means an earlier attempt went through, and when the outcome is still unclear the order is
	// looked up by its key. Unknown means "don't refund yet": the txn stays pending until SyncTxn finds out.
	private async Task<OrderOutcome> PlaceOrder(string idempotencyKey, TagGoldOrderSide side, decimal? baseAmount, decimal? quoteAmount, CancellationToken ct) {
		// Without a token the order request is never sent, so this is a clean rejection, not an unknown outcome.
		(string? token, GoldResult? tokenError) = await ResolveApiToken(null, ct);
		if (token == null) return new OrderOutcome(OrderState.Rejected, Error: tokenError);

		Dictionary<string, object> body = new() {
			{ "idempotencyKey", idempotencyKey },
			{ "side", SideCode(side) },
			{ "baseAsset", AssetCode(TagGoldAsset.Gold18) },
			{ "quoteAsset", AssetCode(TagGoldAsset.Irr) }
		};
		if (baseAmount is > 0) body.Add("baseAmount", FormatGold(baseAmount.Value));
		if (quoteAmount is > 0) body.Add("quoteAmount", FormatIrr(quoteAmount.Value));

		GoldResult last = GoldResult.Fail(Usc.ThirdPartyError, ls.Get("theGoldProviderIsNotReachableRightNow"));
		for (int attempt = 0; attempt < OrderAttempts; attempt++) {
			if (attempt > 0) await Task.Delay(TimeSpan.FromSeconds(attempt), ct);
			last = await CallWithToken(HttpMethod.Post, $"{ClientPath}orders", body, ct);
			if (last.Ok) return new OrderOutcome(OrderState.Filled, MapOrder(last.Item));
			if (last.ErrorCode == "DUPLICATE_IDEMPOTENCY_KEY") break;
			if (!last.NetworkError && last.HttpCode < 500) return new OrderOutcome(OrderState.Rejected, Error: last);
		}

		(GoldOrderResponse? found, bool lookupOk) = await FindOrderByKey(idempotencyKey, ct);
		if (found != null) return new OrderOutcome(OrderState.Filled, found);
		if (lookupOk && last.ErrorCode != "DUPLICATE_IDEMPOTENCY_KEY") return new OrderOutcome(OrderState.Rejected, Error: last);
		ULog.Error($"Gold order {idempotencyKey}: outcome unknown ({last.ErrorCode ?? last.HttpCode.ToString()}), left pending for sync.");
		return new OrderOutcome(OrderState.Unknown, Error: last);
	}

	private async Task<(GoldOrderResponse? Order, bool Ok)> FindOrderByKey(string idempotencyKey, CancellationToken ct) {
		GoldResult r = await CallWithToken(HttpMethod.Get, $"{ClientPath}orders?limit=1&idempotencyKey={Uri.EscapeDataString(idempotencyKey)}", null, ct);
		if (!r.Ok) return (null, false);
		JsonElement? first = r.Items.Select(x => (JsonElement?)x).FirstOrDefault();
		return (first == null ? null : MapOrder(first.Value), true);
	}

	private async Task<UResponse<GoldTxnResponse?>> ResolveOutcome(GoldTxnEntity e, OrderOutcome outcome, CancellationToken ct) {
		switch (outcome.State) {
			case OrderState.Filled:
				return await Settle(e.Id, outcome.Order!, ct);
			case OrderState.Rejected:
				ULog.Error($"Gold txn {e.Id}: order rejected: {outcome.Error!.ErrorCode} {outcome.Error.Message}");
				return await Release(e.Id, UserMessage(outcome.Error), UserStatus(outcome.Error), ct);
			default:
				e.JsonData.ProviderStatus = outcome.Error?.ErrorCode ?? "UNKNOWN";
				await db.SaveChangesAsync(ct);
				return new UResponse<GoldTxnResponse?>(MapTxn(e), Usc.Success, ls.Get("theGoldOrderIsBeingProcessedAndWillBeSettledShortly"));
		}
	}

	// ===== Settlement =====

	private async Task<UResponse<GoldTxnResponse?>> SyncPending(GoldTxnEntity e, CancellationToken ct) {
		if (!e.Tags.Contains(TagGoldTxn.Pending)) return new UResponse<GoldTxnResponse?>(MapTxn(e));

		GoldOrderResponse? order = null;
		bool lookupOk;
		if (e.OrderId.IsNotNullOrEmpty()) {
			GoldResult r = await CallWithToken(HttpMethod.Get, $"{ClientPath}orders/{Uri.EscapeDataString(e.OrderId)}", null, ct);
			lookupOk = r.Ok;
			if (r.Ok) order = MapOrder(r.Item);
		}
		else (order, lookupOk) = await FindOrderByKey(e.IdempotencyKey, ct);

		if (order != null) return await Settle(e.Id, order, ct);
		if (!lookupOk || DateTime.UtcNow - e.CreatedAt < PendingGiveUpAfter)
			return new UResponse<GoldTxnResponse?>(MapTxn(e), Usc.Success, ls.Get("theGoldOrderIsBeingProcessedAndWillBeSettledShortly"));

		return await Release(e.Id, ls.Get("theGoldOrderWasNotCompletedAnyReservedAmountHasBeenReturned"), Usc.ThirdPartyError, ct);
	}

	// Pending txns are normally settled right away; this catches the ones left open by a timeout or a crash, so the
	// balance the user sees is final. It never breaks the balance request.
	private async Task SyncStalePendingTxns(Guid userId, CancellationToken ct) {
		try {
			DateTime before = DateTime.UtcNow.AddSeconds(-30);
			List<GoldTxnEntity> pending = await db.Set<GoldTxnEntity>().AsTracking()
				.Where(x => x.UserId == userId && x.Tags.Contains(TagGoldTxn.Pending) && x.CreatedAt < before)
				.OrderBy(x => x.CreatedAt)
				.Take(3)
				.ToListAsync(ct);
			foreach (GoldTxnEntity e in pending) await SyncPending(e, ct);
		}
		catch (Exception ex) {
			ULog.Error(ex, $"Gold: syncing pending txns of user {userId} failed");
		}
	}

	private async Task<UResponse<GoldTxnResponse?>> Settle(Guid txnId, GoldOrderResponse order, CancellationToken ct) {
		await SettleLock.WaitAsync(ct);
		try {
			GoldTxnEntity? e = await ReloadTxn(txnId, ct);
			if (e == null) return new UResponse<GoldTxnResponse?>(null, Usc.NotFound, ls.Get("theGoldTransactionWasNotFound"));
			if (!e.Tags.Contains(TagGoldTxn.Pending)) return new UResponse<GoldTxnResponse?>(MapTxn(e));

			e.OrderId = order.Id;
			e.JsonData.ProviderStatus = order.Status?.ToString();
			decimal dealtGold = order.DealtBaseAmount ?? 0;
			decimal dealtAmount = order.DealtQuoteAmount ?? 0;
			bool settled = order.Status == TagGoldOrderStatus.Filled && dealtGold > 0 && dealtAmount > 0 &&
			               (e.Tags.Contains(TagGoldTxn.Sell) ? await SettleSell(e, dealtGold, dealtAmount, ct) : await SettleBuy(e, dealtGold, dealtAmount, ct));

			if (!settled) {
				await db.SaveChangesAsync(ct);
				return new UResponse<GoldTxnResponse?>(MapTxn(e), Usc.Success, ls.Get("theGoldOrderIsBeingProcessedAndWillBeSettledShortly"));
			}

			return await FillTxn(e, order, dealtGold, dealtAmount, ct);
		}
		finally {
			SettleLock.Release();
		}
	}

	private async Task<bool> SettleBuy(GoldTxnEntity e, decimal dealtGold, decimal dealtAmount, CancellationToken ct) {
		if (!await MoveGoldOnce(e, dealtGold, "", GoldCredited, false, ct)) return false;

		// The provider already charged our account, so a rare price jump beyond the buffer is still collected (overdraft).
		decimal delta = dealtAmount - (e.JsonData.ReservedAmount ?? 0);
		if (delta > 0) return await MoveMoneyOnce(e, "extra", e.UserId, HouseUserId, delta, dealtGold, TagWalletTxn.GoldPurchase, true, ct);
		if (delta < 0) return await MoveMoneyOnce(e, "change", HouseUserId, e.UserId, -delta, dealtGold, TagWalletTxn.GoldPurchaseRefund, true, ct);
		return true;
	}

	private async Task<bool> SettleSell(GoldTxnEntity e, decimal dealtGold, decimal dealtAmount, CancellationToken ct) {
		decimal reserved = e.JsonData.ReservedGoldAmount ?? 0;
		if (!await MoveGoldOnce(e, reserved - dealtGold, GoldReserved, GoldSettled, true, ct)) return false;

		// The rial for a sale arrives in the provider account, not in the house wallet, so the house wallet may overdraft.
		return await MoveMoneyOnce(e, "payout", HouseUserId, e.UserId, dealtAmount, dealtGold, TagWalletTxn.GoldSale, true, ct);
	}

	// Undo the reservation of an order that was never placed: rial back for a buy, gold back for a sell.
	private async Task<UResponse<GoldTxnResponse?>> Release(Guid txnId, string message, Usc status, CancellationToken ct) {
		await SettleLock.WaitAsync(ct);
		try {
			GoldTxnEntity? e = await ReloadTxn(txnId, ct);
			if (e == null) return new UResponse<GoldTxnResponse?>(null, Usc.NotFound, ls.Get("theGoldTransactionWasNotFound"));
			if (!e.Tags.Contains(TagGoldTxn.Pending)) return new UResponse<GoldTxnResponse?>(MapTxn(e));

			bool released;
			if (e.Tags.Contains(TagGoldTxn.Sell))
				released = await MoveGoldOnce(e, e.JsonData.ReservedGoldAmount ?? 0, GoldReserved, GoldReleased, false, ct);
			else
				released = !await WalletStepExists(e, "reserve", e.UserId) ||
				           await MoveMoneyOnce(e, "refund", HouseUserId, e.UserId, e.JsonData.ReservedAmount ?? 0, 0, TagWalletTxn.GoldPurchaseRefund, true, ct);

			if (!released) {
				e.JsonData.Error = message;
				await db.SaveChangesAsync(ct);
				return new UResponse<GoldTxnResponse?>(MapTxn(e), Usc.Success, ls.Get("theGoldOrderIsBeingProcessedAndWillBeSettledShortly"));
			}

			return await FailTxn(e, status, message, ct);
		}
		finally {
			SettleLock.Release();
		}
	}

	// Settlement always starts from what is in the database, so two settlements of the same txn can't both apply.
	private async Task<GoldTxnEntity?> ReloadTxn(Guid txnId, CancellationToken ct) {
		await db.SaveChangesAsync(ct);
		db.ChangeTracker.Clear();
		return await db.Set<GoldTxnEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == txnId, ct);
	}

	// A wallet transfer that is done at most once per txn step: the step key is stored on the wallet txn (Detail2) and
	// checked first, so a retried settlement can't pay or refund twice.
	private async Task<bool> MoveMoneyOnce(GoldTxnEntity e, string step, Guid senderId, Guid receiverId, decimal amount, decimal goldAmount, TagWalletTxn tag, bool allowOverdraft, CancellationToken ct) {
		if (amount <= 0) return true;
		if (await WalletStepExists(e, step, senderId)) return true;

		UResponse<WalletTxnResponse?> r = await wallet.Transfer(new WalletTransferParams {
			ApiKey = Core.App.ApiKey,
			SenderId = senderId,
			ReceiverId = receiverId,
			Amount = amount,
			Detail1 = e.Id.ToString(),
			Detail2 = WalletStepKey(e, step),
			AllowOverdraft = allowOverdraft,
			KeyValues = GoldKeyValues(e, goldAmount, e.UnitPrice),
			TagWalletTxn = [tag]
		}, ct);
		if (r.Status == Usc.Success) return true;

		ULog.Error($"Gold txn {e.Id}: wallet step '{step}' failed: {r.Message}");
		return false;
	}

	private Task<bool> WalletStepExists(GoldTxnEntity e, string step, Guid senderId) {
		string key = WalletStepKey(e, step);
		return db.Set<WalletTxnEntity>().AnyAsync(x => x.SenderId == senderId && x.JsonData.Detail2 == key);
	}

	private static string WalletStepKey(GoldTxnEntity e, string step) => $"gold:{e.Id}:{step}";

	// Moves gold in the user's ledger and records the new state on the txn in one database transaction.
	private async Task<bool> MoveGoldOnce(GoldTxnEntity e, decimal amount, string fromState, string toState, bool allowNegative, CancellationToken ct) {
		if (e.JsonData.Detail2 == toState) return true;
		if (e.JsonData.Detail2 != fromState) {
			ULog.Error($"Gold txn {e.Id}: expected gold state '{fromState}' but found '{e.JsonData.Detail2}'");
			return false;
		}

		GoldWalletEntity goldWallet = await ReadOrCreateWallet(e.UserId, ct);
		IExecutionStrategy strategy = db.Database.CreateExecutionStrategy();
		return await strategy.ExecuteAsync(async () => {
			await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync(ct);
			if (amount != 0) {
				int rows = await db.Set<GoldWalletEntity>()
					.Where(x => x.Id == goldWallet.Id && (allowNegative || amount >= 0 || x.Balance >= -amount))
					.ExecuteUpdateAsync(u => u.SetProperty(x => x.Balance, x => x.Balance + amount), ct);
				if (rows == 0) return false;
			}

			e.JsonData.Detail2 = toState;
			await db.SaveChangesAsync(ct);
			await transaction.CommitAsync(ct);
			return true;
		});
	}

	private static List<KeyValue> GoldKeyValues(GoldTxnEntity e, decimal goldAmount, decimal unitPrice) => [
		new KeyValue { Key = ULocalizedConstants.GoldWeight, Value = goldAmount.ToDecimalString() },
		new KeyValue { Key = ULocalizedConstants.UnitPrice, Value = unitPrice.ToIntString() },
		new KeyValue { Key = ULocalizedConstants.OrderId, Value = e.Id.ToString() }
	];

	private async Task<UResponse<GoldTxnResponse?>> FillTxn(GoldTxnEntity e, GoldOrderResponse order, decimal dealtGold, decimal dealtAmount, CancellationToken ct) {
		GoldOrderFeeResponse? fee = order.Fees.FirstOrDefault();
		e.GoldAmount = dealtGold;
		e.Amount = dealtAmount;
		e.UnitPrice = order.EffectivePrice ?? order.BaseUnitPrice ?? (dealtGold > 0 ? dealtAmount / dealtGold : e.UnitPrice);
		e.OrderId = order.Id;
		e.JsonData.FeeAmount = fee?.Amount;
		e.JsonData.FeeAsset = fee?.Asset;
		e.JsonData.ProviderStatus = order.Status?.ToString();
		e.JsonData.Error = null;
		e.Tags = [e.Tags.Contains(TagGoldTxn.Sell) ? TagGoldTxn.Sell : TagGoldTxn.Buy, TagGoldTxn.Filled];
		await db.SaveChangesAsync(ct);
		return new UResponse<GoldTxnResponse?>(MapTxn(e), Usc.Created);
	}

	private async Task<UResponse<GoldTxnResponse?>> FailTxn(GoldTxnEntity e, Usc status, string message, CancellationToken ct) {
		e.JsonData.Error = message;
		e.Tags = [e.Tags.Contains(TagGoldTxn.Sell) ? TagGoldTxn.Sell : TagGoldTxn.Buy, TagGoldTxn.Failed];
		await db.SaveChangesAsync(ct);
		return new UResponse<GoldTxnResponse?>(MapTxn(e), status == Usc.Success ? Usc.ThirdPartyError : status, message);
	}

	private static GoldTxnEntity NewTxn(Guid userId, TagGoldTxn side, decimal unitPrice, GoldTxnJson json) => new() {
		Id = Guid.CreateVersion7(),
		CreatedAt = DateTime.UtcNow,
		CreatorId = userId,
		UserId = userId,
		Tags = [side, TagGoldTxn.Pending],
		GoldAmount = 0,
		Amount = 0,
		UnitPrice = unitPrice,
		IdempotencyKey = Guid.CreateVersion7().ToString(),
		JsonData = json
	};

	private async Task<GoldWalletEntity> ReadOrCreateWallet(Guid userId, CancellationToken ct) {
		GoldWalletEntity? e = await db.Set<GoldWalletEntity>().AsTracking().FirstOrDefaultAsync(x => x.CreatorId == userId, ct);
		if (e != null) return e;

		e = new GoldWalletEntity {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = userId,
			Balance = 0,
			Tags = [TagGoldAsset.Gold18],
			JsonData = new GoldWalletJson()
		};
		await db.Set<GoldWalletEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return e;
	}

	private static GoldTxnResponse MapTxn(GoldTxnEntity e) => new() {
		Id = e.Id,
		CreatedAt = e.CreatedAt,
		Tags = e.Tags,
		JsonData = e.JsonData,
		CreatorId = e.CreatorId,
		AdminUserIds = e.AdminUserIds,
		UserId = e.UserId,
		GoldAmount = e.GoldAmount,
		Amount = e.Amount,
		UnitPrice = e.UnitPrice,
		OrderId = e.OrderId,
		IdempotencyKey = e.IdempotencyKey
	};

	// ===== Quote =====

	private sealed record QuoteCacheEntry(GoldQuoteResponse Quote, DateTime At);

	private async Task<(GoldQuoteResponse? Quote, GoldResult? Error)> FetchQuote(TagGoldAsset baseAsset, TagGoldAsset quoteAsset, bool fresh, CancellationToken ct) {
		bool defaultPair = baseAsset == TagGoldAsset.Gold18 && quoteAsset == TagGoldAsset.Irr;
		QuoteCacheEntry? cached = _quoteCache;
		if (defaultPair && !fresh && cached != null && DateTime.UtcNow - cached.At < QuoteCacheDuration) return (cached.Quote, null);

		GoldResult r = await CallWithToken(HttpMethod.Get, $"{ClientPath}assets/{AssetCode(baseAsset)}/price?quoteAsset={AssetCode(quoteAsset)}", null, ct);
		if (!r.Ok) return (null, r);

		JsonElement item = r.Item;
		decimal? buy = item.GetDecimalOrNull("buyUnitPrice");
		decimal? sell = item.GetDecimalOrNull("sellUnitPrice");
		GoldQuoteResponse quote = new() {
			BaseAsset = AssetTag(item.GetStringOrNull("baseAsset")) ?? baseAsset,
			QuoteAsset = AssetTag(item.GetStringOrNull("quoteAsset")) ?? quoteAsset,
			Unit = item.GetStringOrNull("unit"),
			BaseUnitPrice = item.GetDecimalOrNull("baseUnitPrice"),
			BuyUnitPrice = buy,
			SellUnitPrice = sell,
			CanBuy = (item.GetBoolOrNull("canBuy") ?? true) && buy is > 0,
			CanSell = (item.GetBoolOrNull("canSell") ?? true) && sell is > 0,
			UpdatedAt = ReadDate(item, "updatedAt")
		};
		if (defaultPair) _quoteCache = new QuoteCacheEntry(quote, DateTime.UtcNow);
		return (quote, null);
	}

	// ===== Provider calls & API token =====

	private async Task<GoldResult> PostApiToken(string? label, ICollection<string> scopes, ICollection<string>? ipWhitelist, CancellationToken ct) {
		Dictionary<string, object> body = new() { { "scopes", scopes } };
		if (label.IsNotNullOrEmpty()) body.Add("label", label);
		if (ipWhitelist.IsNotNullOrEmpty()) body.Add("ipWhitelist", ipWhitelist);
		return await CallWithBasic(HttpMethod.Post, $"{ClientPath}auth/api-tokens", body, ct);
	}

	private async Task<GoldResult> CallWithBasic(HttpMethod method, string path, object? body, CancellationToken ct) {
		if (!Core.App.Gold.ClientKey.IsNotNullOrEmpty() || !Core.App.Gold.ClientSecret.IsNotNullOrEmpty())
			return GoldResult.Fail(Usc.InternalServerError, ls.Get("theGoldProviderIsNotConfigured"));

		string basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Core.App.Gold.ClientKey}:{Core.App.Gold.ClientSecret}"));
		return await Send(method, path, body, $"Basic {basic}", ct);
	}

	private async Task<GoldResult> CallWithToken(HttpMethod method, string path, object? body, CancellationToken ct) {
		(string? token, GoldResult? error) = await ResolveApiToken(null, ct);
		if (token == null) return error!;

		GoldResult result = await Send(method, path, body, $"Bearer {token}", ct);
		if (result.HttpCode != 401 || ConfiguredApiToken() != null) return result;

		(string? refreshed, _) = await ResolveApiToken(token, ct);
		return refreshed == null ? result : await Send(method, path, body, $"Bearer {refreshed}", ct);
	}

	// Gold.ApiToken must be a rawToken returned by "create API token", not the client key. A client key there made every
	// call fail with 401, so that mistake is ignored (with a warning) and the service creates its own token instead.
	private static string? ConfiguredApiToken() {
		string? token = Core.App.Gold.ApiToken?.Trim();
		if (!token.IsNotNullOrEmpty()) return null;
		if (token != Core.App.Gold.ClientKey && token != Core.App.Gold.ClientSecret) return token;

		if (!_warnedAboutApiToken) {
			_warnedAboutApiToken = true;
			ULog.Error("Gold: AppSettings Gold.ApiToken contains the client key/secret, not an API token. It is ignored; leave it empty or paste a rawToken.");
		}
		return null;
	}

	// Without a configured token, one is created with the client credentials and kept in memory. Taline allows only 10
	// active tokens, so when that limit is hit the tokens this service created earlier are revoked and creation is retried.
	private async Task<(string? Token, GoldResult? Error)> ResolveApiToken(string? rejectedToken, CancellationToken ct) {
		string? configured = ConfiguredApiToken();
		if (configured != null) return (configured, null);

		string? cached = _cachedApiToken;
		if (cached != null && cached != rejectedToken) return (cached, null);

		await TokenLock.WaitAsync(ct);
		try {
			if (_cachedApiToken != null && _cachedApiToken != rejectedToken) return (_cachedApiToken, null);
			_cachedApiToken = null;

			GoldResult created = await PostApiToken($"{AutoTokenLabelPrefix}{DateTime.UtcNow:yyyyMMddHHmmss}", Core.App.Gold.Scopes.ToList(), null, ct);
			if (created.ErrorCode == "MAX_ACTIVE_TOKENS_REACHED" && await RevokeAutoTokens(ct))
				created = await PostApiToken($"{AutoTokenLabelPrefix}{DateTime.UtcNow:yyyyMMddHHmmss}", Core.App.Gold.Scopes.ToList(), null, ct);

			string? raw = created.Ok ? created.Item.GetStringOrNull("rawToken") : null;
			if (!raw.IsNotNullOrEmpty()) {
				ULog.Error($"Gold: creating an API token failed: {created.ErrorCode ?? created.HttpCode.ToString()} {created.Message}");
				return (null, created.Ok ? GoldResult.Fail(Usc.ThirdPartyError, ls.Get("theGoldProviderReturnedAnUnexpectedError")) : created);
			}

			_cachedApiToken = raw;
			return (raw, null);
		}
		finally {
			TokenLock.Release();
		}
	}

	private async Task<bool> RevokeAutoTokens(CancellationToken ct) {
		GoldResult list = await CallWithBasic(HttpMethod.Get, $"{ClientPath}auth/api-tokens", null, ct);
		if (!list.Ok) return false;

		bool revoked = false;
		foreach (JsonElement token in list.Items.Where(x => x.GetBoolOrNull("active") != false && (x.GetStringOrNull("label") ?? "").StartsWith(AutoTokenLabelPrefix))) {
			string? id = token.GetStringOrNull("id");
			if (!id.IsNotNullOrEmpty()) continue;
			GoldResult deleted = await CallWithBasic(HttpMethod.Delete, $"{ClientPath}auth/api-tokens/{Uri.EscapeDataString(id)}", null, ct);
			revoked |= deleted.Ok;
		}
		ULog.Info($"Gold: revoked old auto-created API tokens: {revoked}");
		return revoked;
	}

	private async Task<GoldResult> Send(HttpMethod method, string path, object? body, string authorization, CancellationToken ct) {
		string uri = $"{Core.App.Gold.BaseUrl.TrimEnd('/')}/{path}";
		Dictionary<string, string> headers = new() {
			{ "Authorization", authorization },
			{ "Accept", "application/json" },
			{ "Accept-Language", httpContext.HttpContext?.Request.Headers["Locale"].FirstOrDefault() == "fa" ? "fa" : "en" }
		};

		HttpResponseMessage? response = method.Method switch {
			"GET" => await httpClient.Get(uri, headers),
			"DELETE" => await httpClient.Delete(uri, headers),
			_ => await httpClient.Post(uri, body, headers)
		};

		if (response == null) return GoldResult.Fail(Usc.ThirdPartyError, ls.Get("theGoldProviderIsNotReachableRightNow"), networkError: true);

		string raw = await response.Content.ReadAsStringAsync(ct);
		JsonElement root = default;
		if (raw.IsNotNullOrEmpty())
			try {
				root = JsonSerializer.Deserialize<JsonElement>(raw);
			}
			catch (JsonException) {
				return GoldResult.Fail(Usc.ThirdPartyError, ls.Get("theGoldProviderReturnedAnUnexpectedError"), (int)response.StatusCode);
			}

		if (response.IsSuccessStatusCode) return new GoldResult { Ok = true, Root = root, HttpCode = (int)response.StatusCode };

		string? errorCode = root.ValueKind == JsonValueKind.Object ? root.GetStringOrNull("error") : null;
		string localized = errorCode != null && ErrorKeys.TryGetValue(errorCode, out string? key) ? ls.Get(key) : "";
		if (!localized.IsNotNullOrEmpty()) localized = (root.ValueKind == JsonValueKind.Object ? root.GetStringOrNull("message") : null) ?? ls.Get("theGoldProviderReturnedAnUnexpectedError");

		return GoldResult.Fail(MapStatus((int)response.StatusCode, errorCode), localized, (int)response.StatusCode, errorCode);
	}

	private string UserMessage(GoldResult error) =>
		error.ErrorCode != null && UserFacingErrors.Contains(error.ErrorCode) ? error.Message : ls.Get("theGoldServiceIsTemporarilyUnavailablePleaseTryAgainLater");

	private static Usc UserStatus(GoldResult error) => error.ErrorCode != null && UserFacingErrors.Contains(error.ErrorCode) ? error.Status : Usc.ThirdPartyError;

	private static string PagingQuery(string? cursor, int limit) {
		int safeLimit = Math.Clamp(limit, MinPageLimit, MaxPageLimit);
		return cursor.IsNotNullOrEmpty() ? $"?limit={safeLimit}&cursor={Uri.EscapeDataString(cursor)}" : $"?limit={safeLimit}";
	}

	// Truncated (not rounded up) so a sell never asks for more gold than the user has.
	private static decimal RoundGold(decimal value) => Math.Round(value, GoldDecimals, MidpointRounding.ToZero);

	// Amounts are sent as strings; decimals coming from JSON can carry a scale like "5000000.0", which Taline rejects.
	private static string FormatGold(decimal value) => RoundGold(value).ToString("0.###", CultureInfo.InvariantCulture);

	private static string FormatIrr(decimal value) => Math.Floor(value).ToString("0", CultureInfo.InvariantCulture);

	private static string AssetCode(TagGoldAsset asset) => asset switch {
		TagGoldAsset.Irr => "IRR",
		_ => "GOLD18"
	};

	private static TagGoldAsset? AssetTag(string? code) => code?.ToUpperInvariant() switch {
		"GOLD18" => TagGoldAsset.Gold18,
		"IRR" => TagGoldAsset.Irr,
		_ => null
	};

	private static string SideCode(TagGoldOrderSide side) => side == TagGoldOrderSide.Sell ? "SELL" : "BUY";

	private static TagGoldOrderSide? SideTag(string? value) => value?.ToUpperInvariant() switch {
		"BUY" => TagGoldOrderSide.Buy,
		"SELL" => TagGoldOrderSide.Sell,
		_ => null
	};

	private static TagGoldOrderStatus? StatusTag(string? value) => value?.ToUpperInvariant() switch {
		"FILLED" => TagGoldOrderStatus.Filled,
		"PENDING" => TagGoldOrderStatus.Pending,
		"FAILED" => TagGoldOrderStatus.Failed,
		"CANCELLED" or "CANCELED" => TagGoldOrderStatus.Cancelled,
		_ => null
	};

	private static Usc MapStatus(int httpCode, string? errorCode) => errorCode switch {
		"DUPLICATE_IDEMPOTENCY_KEY" => Usc.Conflict,
		"RATE_LIMIT_EXCEEDED" or "AUTH_RATE_LIMITED" => Usc.TooManyRequests,
		"BUSINESS_SUSPENDED" or "CLIENT_INACTIVE" or "IP_NOT_ALLOWED" or "FORBIDDEN" => Usc.Forbidden,
		_ => httpCode switch {
			400 or 405 or 406 or 415 or 422 => Usc.BadRequest,
			401 => Usc.UnAuthorized,
			403 => Usc.Forbidden,
			404 => Usc.NotFound,
			409 => Usc.Conflict,
			429 => Usc.TooManyRequests,
			_ => Usc.ThirdPartyError
		}
	};

	private static readonly Dictionary<string, string> ErrorKeys = new() {
		{ "VALIDATION_ERROR", "theGoldProviderRejectedTheRequestData" },
		{ "INVALID_PARAMETER", "theGoldProviderRejectedTheRequestData" },
		{ "INVALID_FIELD", "theGoldProviderRejectedTheRequestData" },
		{ "INVALID_BODY", "theGoldProviderRejectedTheRequestData" },
		{ "INVALID_CURSOR", "thePaginationCursorIsInvalidOrExpiredStartFromTheFirstPage" },
		{ "NOT_FOUND", "theRequestedGoldResourceWasNotFound" },
		{ "INTERNAL_ERROR", "theGoldProviderReturnedAnUnexpectedError" },
		{ "INVALID_AUTH_HEADER", "authenticationWithTheGoldProviderIsRequired" },
		{ "INVALID_CREDENTIALS", "theGoldProviderCredentialsAreInvalid" },
		{ "INVALID_TOKEN", "theGoldProviderTokenIsInvalidOrExpired" },
		{ "FORBIDDEN", "accessToThisGoldResourceIsDenied" },
		{ "IP_NOT_ALLOWED", "thisIPAddressIsNotAllowedByTheGoldProvider" },
		{ "CLIENT_INACTIVE", "theGoldProviderClientAccountIsInactive" },
		{ "TOKEN_NOT_FOUND", "theGoldProviderTokenWasNotFound" },
		{ "MAX_ACTIVE_TOKENS_REACHED", "theMaximumNumberOfActiveGoldTokensHasBeenReachedRevokeOneFirst" },
		{ "IP_NOT_SUBSET_OF_CLIENT", "theTokenIPWhitelistMustBeASubsetOfTheClientIPWhitelist" },
		{ "SCOPE_NOT_GRANTED", "aRequestedGoldScopeIsNotEnabledForThisAccount" },
		{ "RATE_LIMIT_EXCEEDED", "tooManyRequestsToTheGoldProviderPleaseTryAgainShortly" },
		{ "AUTH_RATE_LIMITED", "tooManyFailedAuthenticationAttemptsPleaseTryAgainShortly" },
		{ "BUSINESS_SUSPENDED", "yourGoldBusinessAccountIsSuspended" },
		{ "UNSUPPORTED_TRADE_PAIR", "thisTradePairIsNotSupported" },
		{ "ASSET_NOT_FOUND", "theAssetWasNotFoundOrIsInactive" },
		{ "INVALID_TRADE_AMOUNT", "theTradeAmountIsNotValid" },
		{ "AMOUNT_PRECISION_EXCEEDED", "theAmountHasMoreDecimalPlacesThanTheAssetAllows" },
		{ "INSUFFICIENT_QUOTE_AMOUNT", "theAmountIsBelowTheMinimumTradeSize" },
		{ "TRADE_SIDE_CLOSED", "goldTradingIsClosedOnThisSideRightNow" },
		{ "PRICE_UNAVAILABLE", "theGoldPriceIsNotAvailableRightNowPleaseTryAgainShortly" },
		{ "DUPLICATE_IDEMPOTENCY_KEY", "thisIdempotencyKeyHasAlreadyBeenUsedGenerateANewOneForEachOrder" },
		{ "ORDER_TRADE_WINDOW_LIMIT_EXCEEDED", "yourOrderVolumeHasReachedTheLimitForTheCurrentTimeWindow" },
		{ "ORDER_NOT_FOUND", "theOrderWasNotFound" },
		{ "WALLET_NOT_FOUND", "theRequestedGoldResourceWasNotFound" },
		{ "WALLET_LOCKED", "theGoldProviderWalletIsLocked" },
		{ "INSUFFICIENT_BALANCE", "theGoldProviderAccountBalanceIsNotEnough" }
	};

	private static GoldOrderResponse MapOrder(JsonElement x) => new() {
		Id = x.GetStringOrNull("id") ?? "",
		IdempotencyKey = x.GetStringOrNull("idempotencyKey"),
		Status = StatusTag(x.GetStringOrNull("status")),
		Side = SideTag(x.GetStringOrNull("side")),
		BaseAsset = AssetTag(x.GetStringOrNull("baseAsset")),
		QuoteAsset = AssetTag(x.GetStringOrNull("quoteAsset")),
		RequestedBaseAmount = x.GetDecimalOrNull("requestedBaseAmount"),
		RequestedQuoteAmount = x.GetDecimalOrNull("requestedQuoteAmount"),
		DealtBaseAmount = x.GetDecimalOrNull("dealtBaseAmount"),
		DealtQuoteAmount = x.GetDecimalOrNull("dealtQuoteAmount"),
		EffectivePrice = x.GetDecimalOrNull("effectivePrice"),
		BaseUnitPrice = x.GetDecimalOrNull("baseUnitPrice"),
		CreatedAt = ReadDate(x, "createdAt"),
		Fees = ReadArray(x, "fees").Select(f => new GoldOrderFeeResponse {
			Asset = f.GetStringOrNull("asset"),
			Amount = f.GetDecimalOrNull("amount"),
			Type = f.GetStringOrNull("type"),
			Rate = f.GetDecimalOrNull("rate")
		}).ToList(),
		Transactions = ReadArray(x, "transactions").Select(t => new GoldOrderTransactionResponse {
			Id = t.GetStringOrNull("id") ?? "",
			CreatedAt = ReadDate(t, "createdAt"),
			Entries = ReadEntries(t)
		}).ToList()
	};

	private static GoldBalanceResponse MapBalance(JsonElement x) => new() {
		AssetCode = x.GetStringOrNull("asset") ?? "",
		Asset = AssetTag(x.GetStringOrNull("asset")),
		Balance = x.GetDecimalOrNull("balance"),
		Locked = x.GetBoolOrNull("locked") ?? false
	};

	private static GoldTradeLimitResponse MapTradeLimit(JsonElement x) {
		JsonElement detail = x.TryGetProperty("detail", out JsonElement d) && d.ValueKind == JsonValueKind.Object ? d : default;
		return new GoldTradeLimitResponse {
			Type = x.GetStringOrNull("type"),
			Asset = x.GetStringOrNull("asset"),
			MaxVolume = x.GetDecimalOrNull("maxVolume"),
			UsedVolume = x.GetDecimalOrNull("usedVolume"),
			RemainingVolume = x.GetDecimalOrNull("remainingVolume"),
			Interval = x.GetStringOrNull("interval"),
			ResetsAt = x.GetStringOrNull("resetsAt"),
			Side = detail.ValueKind == JsonValueKind.Object ? SideTag(detail.GetStringOrNull("side")) : null,
			WindowStart = detail.ValueKind == JsonValueKind.Object ? detail.GetStringOrNull("windowStart") : null,
			WindowEnd = detail.ValueKind == JsonValueKind.Object ? detail.GetStringOrNull("windowEnd") : null
		};
	}

	private static GoldApiTokenResponse MapApiToken(JsonElement x) => new() {
		Id = x.GetStringOrNull("id") ?? "",
		TokenPrefix = x.GetStringOrNull("tokenPrefix"),
		Label = x.GetStringOrNull("label"),
		Scopes = ReadStringList(x, "scopes"),
		IpWhitelist = ReadStringList(x, "ipWhitelist"),
		Active = x.GetBoolOrNull("active") ?? false,
		ExpiresAt = ReadDate(x, "expiresAt"),
		CreatedAt = ReadDate(x, "createdAt"),
		RawToken = x.GetStringOrNull("rawToken")
	};

	private static ICollection<GoldWalletEntryResponse> ReadEntries(JsonElement x) => ReadArray(x, "entries").Select(e => new GoldWalletEntryResponse {
		Asset = e.GetStringOrNull("asset"),
		Amount = e.GetDecimalOrNull("amount")
	}).ToList();

	private static IEnumerable<JsonElement> ReadArray(JsonElement element, string propertyName) {
		if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out JsonElement value) || value.ValueKind != JsonValueKind.Array) return [];
		return value.EnumerateArray().ToList();
	}

	private static ICollection<string> ReadStringList(JsonElement element, string propertyName) =>
		ReadArray(element, propertyName).Where(x => x.ValueKind == JsonValueKind.String).Select(x => x.GetString()!).ToList();

	private static DateTime? ReadDate(JsonElement element, string propertyName) {
		string? raw = element.GetStringOrNull(propertyName);
		if (!raw.IsNotNullOrEmpty()) return null;
		return DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out DateTime parsed) ? parsed : null;
	}

	private sealed class GoldResult {
		public bool Ok { get; init; }
		public JsonElement Root { get; init; }
		public int HttpCode { get; init; }
		public string? ErrorCode { get; init; }

		// True when no HTTP response arrived at all (timeout, DNS, TLS...), so the provider may or may not have acted.
		public bool NetworkError { get; init; }
		public Usc Status { get; init; } = Usc.ThirdPartyError;
		public string Message { get; init; } = "";

		public JsonElement Data => Root.ValueKind == JsonValueKind.Object && Root.TryGetProperty("data", out JsonElement d) ? d : default;

		public JsonElement Item => Data.ValueKind == JsonValueKind.Object && Data.TryGetProperty("item", out JsonElement i) ? i : default;

		public IEnumerable<JsonElement> Items {
			get {
				JsonElement data = Data;
				if (data.ValueKind != JsonValueKind.Object || !data.TryGetProperty("items", out JsonElement items) || items.ValueKind != JsonValueKind.Array) return [];
				return items.EnumerateArray().ToList();
			}
		}

		public string? NextCursor {
			get {
				if (Root.ValueKind != JsonValueKind.Object) return null;
				if (!Root.TryGetProperty("meta", out JsonElement meta) || meta.ValueKind != JsonValueKind.Object) return null;
				if (!meta.TryGetProperty("pagination", out JsonElement pagination) || pagination.ValueKind != JsonValueKind.Object) return null;
				return pagination.GetStringOrNull("nextCursor");
			}
		}

		public static GoldResult Fail(Usc status, string message, int httpCode = 0, string? errorCode = null, bool networkError = false) =>
			new() { Ok = false, Status = status, Message = message, HttpCode = httpCode, ErrorCode = errorCode, NetworkError = networkError };
	}
}
