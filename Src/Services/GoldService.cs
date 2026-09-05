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

	// Buying a fixed weight reserves a little more than the quoted price so a tick up between the quote and the fill
	// never leaves the order underfunded; the unused part is refunded in the same request.
	private const decimal BuyReserveBuffer = 1.02m;

	// Taline caps the number of active tokens per account, so a token minted from the client credentials
	// is reused for the whole process and only re-created after a 401.
	private static string? _cachedApiToken;
	private static readonly SemaphoreSlim TokenLock = new(1, 1);

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

		GoldResult r = await CallWithToken(HttpMethod.Get, $"{ClientPath}assets/{AssetCode(p.BaseAsset)}/price?quoteAsset={AssetCode(p.QuoteAsset)}", null, ct);
		if (!r.Ok) return new UResponse<GoldQuoteResponse?>(null, r.Status, r.Message);

		JsonElement item = r.Item;
		return new UResponse<GoldQuoteResponse?>(new GoldQuoteResponse {
			BaseAsset = AssetTag(item.GetStringOrNull("baseAsset")) ?? p.BaseAsset,
			QuoteAsset = AssetTag(item.GetStringOrNull("quoteAsset")) ?? p.QuoteAsset,
			Unit = item.GetStringOrNull("unit"),
			BaseUnitPrice = item.GetDecimalOrNull("baseUnitPrice"),
			BuyUnitPrice = item.GetDecimalOrNull("buyUnitPrice"),
			SellUnitPrice = item.GetDecimalOrNull("sellUnitPrice"),
			UpdatedAt = ReadDate(item, "updatedAt")
		});
	}

	public async Task<UResponse<GoldOrderResponse?>> CreateOrder(GoldCreateOrderParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<GoldOrderResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<GoldOrderResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (p.BaseAsset == p.QuoteAsset) return new UResponse<GoldOrderResponse?>(null, Usc.BadRequest, ls.Get("thisTradePairIsNotSupported"));
		if (p.BaseAmount is > 0 == p.QuoteAmount is > 0) return new UResponse<GoldOrderResponse?>(null, Usc.BadRequest, ls.Get("sendExactlyOneOfBaseAmountOrQuoteAmountGreaterThanZero"));

		Dictionary<string, object> body = new() {
			{ "idempotencyKey", p.IdempotencyKey },
			{ "side", SideCode(p.Side ?? TagGoldOrderSide.Buy) },
			{ "baseAsset", AssetCode(p.BaseAsset) },
			{ "quoteAsset", AssetCode(p.QuoteAsset) }
		};
		if (p.BaseAmount is > 0) body.Add("baseAmount", p.BaseAmount.Value.ToString(CultureInfo.InvariantCulture));
		if (p.QuoteAmount is > 0) body.Add("quoteAmount", p.QuoteAmount.Value.ToString(CultureInfo.InvariantCulture));

		GoldResult r = await CallWithToken(HttpMethod.Post, $"{ClientPath}orders", body, ct);
		if (!r.Ok) return new UResponse<GoldOrderResponse?>(null, r.Status, r.Message);
		return new UResponse<GoldOrderResponse?>(MapOrder(r.Item), Usc.Created);
	}

	public async Task<UResponse<GoldOrderListResponse?>> ReadOrders(GoldReadOrdersParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<GoldOrderListResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<GoldOrderListResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		GoldResult r = await CallWithToken(HttpMethod.Get, $"{ClientPath}orders{PagingQuery(p.Cursor, p.Limit)}", null, ct);
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
		if (!r.Ok) return new UResponse<GoldOrderResponse?>(null, r.Status, r.Message);
		return new UResponse<GoldOrderResponse?>(MapOrder(r.Item));
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
		if (!r.Ok) return new UResponse<GoldBalanceResponse?>(null, r.Status, r.Message);
		return new UResponse<GoldBalanceResponse?>(MapBalance(r.Item));
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

		return await CreateApiToken(p.Label, p.Scopes, p.IpWhitelist, ct);
	}

	public async Task<UResponse<IEnumerable<GoldApiTokenResponse>?>> ReadApiTokens(BaseParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<GoldApiTokenResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<GoldApiTokenResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse<IEnumerable<GoldApiTokenResponse>?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		GoldResult r = await CallWithBasic(HttpMethod.Get, $"{ClientPath}auth/api-tokens", null, ct);
		if (!r.Ok) return new UResponse<IEnumerable<GoldApiTokenResponse>?>(null, r.Status, r.Message);
		return new UResponse<IEnumerable<GoldApiTokenResponse>?>(r.Items.Select(MapApiToken).ToList());
	}

	public async Task<UResponse> DeleteApiToken(GoldDeleteApiTokenParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		GoldResult r = await CallWithBasic(HttpMethod.Delete, $"{ClientPath}auth/api-tokens/{Uri.EscapeDataString(p.TokenId)}", null, ct);
		return r.Ok ? new UResponse(Usc.Deleted) : new UResponse(r.Status, r.Message);
	}

	public async Task<UResponse<GoldUserBalanceResponse?>> ReadUserBalance(GoldReadUserBalanceParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<GoldUserBalanceResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<GoldUserBalanceResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		Guid userId = p.UserId != null && userData.IsAdmin ? p.UserId.Value : userData.Id;
		GoldWalletEntity e = await ReadOrCreateWallet(userId, ct);

		UResponse<GoldQuoteResponse?> quote = await ReadQuote(new GoldQuoteParams { ApiKey = p.ApiKey, Token = p.Token }, ct);
		return new UResponse<GoldUserBalanceResponse?>(new GoldUserBalanceResponse {
			Balance = e.Balance,
			Unit = quote.Result?.Unit,
			BuyUnitPrice = quote.Result?.BuyUnitPrice ?? quote.Result?.BaseUnitPrice,
			SellUnitPrice = quote.Result?.SellUnitPrice ?? quote.Result?.BaseUnitPrice,
			Value = e.Balance * (quote.Result?.SellUnitPrice ?? quote.Result?.BaseUnitPrice ?? 0),
			UpdatedAt = quote.Result?.UpdatedAt
		});
	}

	public async Task<UResponse<GoldTxnResponse?>> Buy(GoldBuyParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<GoldTxnResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<GoldTxnResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (p.Amount is > 0 == p.GoldAmount is > 0) return new UResponse<GoldTxnResponse?>(null, Usc.BadRequest, ls.Get("sendExactlyOneOfBaseAmountOrQuoteAmountGreaterThanZero"));

		UResponse<GoldQuoteResponse?> quote = await ReadQuote(new GoldQuoteParams { ApiKey = p.ApiKey, Token = p.Token }, ct);
		decimal unitPrice = quote.Result?.BuyUnitPrice ?? quote.Result?.BaseUnitPrice ?? 0;
		if (unitPrice <= 0) return new UResponse<GoldTxnResponse?>(null, quote.Status == Usc.Success ? Usc.ThirdPartyError : quote.Status, ls.Get("theGoldPriceIsNotAvailableRightNowPleaseTryAgainShortly"));

		decimal reserve = p.Amount is > 0 ? p.Amount!.Value : Math.Ceiling(p.GoldAmount!.Value * unitPrice * BuyReserveBuffer);
		if (!await wallet.HasEnoughBalance(userData.Id, reserve, ct)) return new UResponse<GoldTxnResponse?>(null, Usc.BalanceIsLow, ls.Get("yourBalanceIsNotEnough"));

		GoldTxnEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = userData.Id,
			UserId = userData.Id,
			Tags = [TagGoldTxn.Buy, TagGoldTxn.Pending],
			GoldAmount = 0,
			Amount = 0,
			UnitPrice = unitPrice,
			IdempotencyKey = Guid.CreateVersion7().ToString("N"),
			JsonData = new GoldTxnJson {
				Detail1 = TagGoldTxn.Buy.ToString(),
				RequestedAmount = p.Amount,
				RequestedGoldAmount = p.GoldAmount,
				ReservedAmount = reserve
			}
		};
		await db.Set<GoldTxnEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);

		UResponse<WalletTxnResponse?> reserved = await wallet.Transfer(new WalletTransferParams {
			ApiKey = p.ApiKey,
			Token = p.Token,
			SenderId = userData.Id,
			ReceiverId = Core.App.Users.AvaPlus.Id,
			Amount = reserve,
			Detail1 = e.Id.ToString(),
			TagWalletTxn = [TagWalletTxn.GoldPurchase]
		}, ct);
		if (reserved.Status != Usc.Success) return await FailTxn(e, reserved.Status, reserved.Message, ct);

		UResponse<GoldOrderResponse?> order = await CreateOrder(new GoldCreateOrderParams {
			ApiKey = p.ApiKey,
			Token = p.Token,
			IdempotencyKey = e.IdempotencyKey,
			Side = TagGoldOrderSide.Buy,
			BaseAmount = p.GoldAmount,
			QuoteAmount = p.Amount
		}, ct);

		if (order.Result == null) {
			await RefundReservedAmount(e, p, ct);
			return await FailTxn(e, order.Status, order.Message, ct);
		}

		e.OrderId = order.Result.Id;
		return await SettleBuy(e, order.Result, p.ApiKey, p.Token, ct);
	}

	public async Task<UResponse<GoldTxnResponse?>> Sell(GoldSellParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<GoldTxnResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<GoldTxnResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (p.Amount is > 0 == p.GoldAmount is > 0) return new UResponse<GoldTxnResponse?>(null, Usc.BadRequest, ls.Get("sendExactlyOneOfBaseAmountOrQuoteAmountGreaterThanZero"));

		UResponse<GoldQuoteResponse?> quote = await ReadQuote(new GoldQuoteParams { ApiKey = p.ApiKey, Token = p.Token }, ct);
		decimal unitPrice = quote.Result?.SellUnitPrice ?? quote.Result?.BaseUnitPrice ?? 0;
		if (unitPrice <= 0) return new UResponse<GoldTxnResponse?>(null, quote.Status == Usc.Success ? Usc.ThirdPartyError : quote.Status, ls.Get("theGoldPriceIsNotAvailableRightNowPleaseTryAgainShortly"));

		GoldWalletEntity goldWallet = await ReadOrCreateWallet(userData.Id, ct);
		if (goldWallet.JsonData.Locked) return new UResponse<GoldTxnResponse?>(null, Usc.Forbidden, ls.Get("yourGoldWalletIsLocked"));

		decimal reserve = p.GoldAmount is > 0 ? p.GoldAmount!.Value : p.Amount!.Value / unitPrice;
		if (goldWallet.Balance < reserve) return new UResponse<GoldTxnResponse?>(null, Usc.BalanceIsLow, ls.Get("yourGoldBalanceIsNotEnoughForThisOrder"));

		GoldTxnEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = userData.Id,
			UserId = userData.Id,
			Tags = [TagGoldTxn.Sell, TagGoldTxn.Pending],
			GoldAmount = 0,
			Amount = 0,
			UnitPrice = unitPrice,
			IdempotencyKey = Guid.CreateVersion7().ToString("N"),
			JsonData = new GoldTxnJson {
				Detail1 = TagGoldTxn.Sell.ToString(),
				RequestedAmount = p.Amount,
				RequestedGoldAmount = p.GoldAmount,
				ReservedGoldAmount = reserve
			}
		};
		await db.Set<GoldTxnEntity>().AddAsync(e, ct);
		await ChangeGoldBalance(goldWallet, -reserve, ct);

		UResponse<GoldOrderResponse?> order = await CreateOrder(new GoldCreateOrderParams {
			ApiKey = p.ApiKey,
			Token = p.Token,
			IdempotencyKey = e.IdempotencyKey,
			Side = TagGoldOrderSide.Sell,
			BaseAmount = p.GoldAmount,
			QuoteAmount = p.Amount
		}, ct);

		if (order.Result == null) {
			await ChangeGoldBalance(goldWallet, reserve, ct);
			return await FailTxn(e, order.Status, order.Message, ct);
		}

		e.OrderId = order.Result.Id;
		return await SettleSell(e, order.Result, p.ApiKey, p.Token, ct);
	}

	// A provider order that came back PENDING is settled the next time the client asks about it, so no background poller is needed.
	public async Task<UResponse<GoldTxnResponse?>> SyncTxn(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<GoldTxnResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<GoldTxnResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		GoldTxnEntity? e = await db.Set<GoldTxnEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<GoldTxnResponse?>(null, Usc.NotFound, ls.Get("theGoldTransactionWasNotFound"));
		if (e.UserId != userData.Id && !userData.IsAdmin) return new UResponse<GoldTxnResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (!e.Tags.Contains(TagGoldTxn.Pending)) return new UResponse<GoldTxnResponse?>(MapTxn(e));
		if (!e.OrderId.IsNotNullOrEmpty()) return new UResponse<GoldTxnResponse?>(MapTxn(e));

		UResponse<GoldOrderResponse?> order = await ReadOrderById(new GoldReadOrderParams { ApiKey = p.ApiKey, Token = p.Token, Id = e.OrderId! }, ct);
		if (order.Result == null) return new UResponse<GoldTxnResponse?>(MapTxn(e), order.Status, order.Message);

		return e.Tags.Contains(TagGoldTxn.Sell)
			? await SettleSell(e, order.Result, p.ApiKey, p.Token, ct)
			: await SettleBuy(e, order.Result, p.ApiKey, p.Token, ct);
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

	private async Task<UResponse<GoldTxnResponse?>> SettleBuy(GoldTxnEntity e, GoldOrderResponse order, string apiKey, string? token, CancellationToken ct) {
		if (order.Status is TagGoldOrderStatus.Failed or TagGoldOrderStatus.Cancelled) {
			await RefundReservedAmount(e, null, ct);
			return await FailTxn(e, Usc.ThirdPartyError, ls.Get("theGoldOrderWasNotCompletedAnyReservedAmountHasBeenReturned"), ct, order.Status == TagGoldOrderStatus.Cancelled);
		}

		decimal dealtGold = order.DealtBaseAmount ?? 0;
		decimal dealtAmount = order.DealtQuoteAmount ?? 0;
		if (dealtGold <= 0 || dealtAmount <= 0) {
			e.JsonData.ProviderStatus = order.Status?.ToString();
			db.Set<GoldTxnEntity>().Update(e);
			await db.SaveChangesAsync(ct);
			return new UResponse<GoldTxnResponse?>(MapTxn(e), Usc.Success, ls.Get("theGoldOrderIsBeingProcessedAndWillBeSettledShortly"));
		}

		decimal reserved = e.JsonData.ReservedAmount ?? dealtAmount;
		decimal delta = dealtAmount - reserved;
		if (delta != 0)
			await wallet.Transfer(new WalletTransferParams {
				ApiKey = apiKey,
				Token = token,
				SenderId = delta > 0 ? e.UserId : Core.App.Users.AvaPlus.Id,
				ReceiverId = delta > 0 ? Core.App.Users.AvaPlus.Id : e.UserId,
				Amount = Math.Abs(delta),
				Detail1 = e.Id.ToString(),
				TagWalletTxn = [delta > 0 ? TagWalletTxn.GoldPurchase : TagWalletTxn.GoldPurchaseRefund]
			}, ct);

		GoldWalletEntity goldWallet = await ReadOrCreateWallet(e.UserId, ct);
		await ChangeGoldBalance(goldWallet, dealtGold, ct);

		return await FillTxn(e, order, dealtGold, dealtAmount, ct);
	}

	private async Task<UResponse<GoldTxnResponse?>> SettleSell(GoldTxnEntity e, GoldOrderResponse order, string apiKey, string? token, CancellationToken ct) {
		GoldWalletEntity goldWallet = await ReadOrCreateWallet(e.UserId, ct);
		decimal reserved = e.JsonData.ReservedGoldAmount ?? 0;

		if (order.Status is TagGoldOrderStatus.Failed or TagGoldOrderStatus.Cancelled) {
			await ChangeGoldBalance(goldWallet, reserved, ct);
			return await FailTxn(e, Usc.ThirdPartyError, ls.Get("theGoldOrderWasNotCompletedAnyReservedAmountHasBeenReturned"), ct, order.Status == TagGoldOrderStatus.Cancelled);
		}

		decimal dealtGold = order.DealtBaseAmount ?? 0;
		decimal dealtAmount = order.DealtQuoteAmount ?? 0;
		if (dealtGold <= 0 || dealtAmount <= 0) {
			e.JsonData.ProviderStatus = order.Status?.ToString();
			db.Set<GoldTxnEntity>().Update(e);
			await db.SaveChangesAsync(ct);
			return new UResponse<GoldTxnResponse?>(MapTxn(e), Usc.Success, ls.Get("theGoldOrderIsBeingProcessedAndWillBeSettledShortly"));
		}

		if (reserved > dealtGold) await ChangeGoldBalance(goldWallet, reserved - dealtGold, ct);

		await wallet.Transfer(new WalletTransferParams {
			ApiKey = apiKey,
			Token = token,
			SenderId = Core.App.Users.AvaPlus.Id,
			ReceiverId = e.UserId,
			Amount = dealtAmount,
			Detail1 = e.Id.ToString(),
			TagWalletTxn = [TagWalletTxn.GoldSale]
		}, ct);

		return await FillTxn(e, order, dealtGold, dealtAmount, ct);
	}

	private async Task RefundReservedAmount(GoldTxnEntity e, BaseParams? p, CancellationToken ct) {
		decimal reserved = e.JsonData.ReservedAmount ?? 0;
		if (reserved <= 0) return;
		await wallet.Transfer(new WalletTransferParams {
			ApiKey = p?.ApiKey ?? "",
			Token = p?.Token,
			SenderId = Core.App.Users.AvaPlus.Id,
			ReceiverId = e.UserId,
			Amount = reserved,
			Detail1 = e.Id.ToString(),
			TagWalletTxn = [TagWalletTxn.GoldPurchaseRefund]
		}, ct);
	}

	private async Task<UResponse<GoldTxnResponse?>> FillTxn(GoldTxnEntity e, GoldOrderResponse order, decimal dealtGold, decimal dealtAmount, CancellationToken ct) {
		GoldOrderFeeResponse? fee = order.Fees.FirstOrDefault();
		e.GoldAmount = dealtGold;
		e.Amount = dealtAmount;
		e.UnitPrice = order.EffectivePrice ?? order.BaseUnitPrice ?? (dealtGold > 0 ? dealtAmount / dealtGold : e.UnitPrice);
		e.OrderId = order.Id;
		e.JsonData.FeeAmount = fee?.Amount;
		e.JsonData.FeeAsset = fee?.Asset;
		e.JsonData.ProviderStatus = order.Status?.ToString();
		e.Tags = [e.Tags.Contains(TagGoldTxn.Sell) ? TagGoldTxn.Sell : TagGoldTxn.Buy, TagGoldTxn.Filled];
		db.Set<GoldTxnEntity>().Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse<GoldTxnResponse?>(MapTxn(e), Usc.Created);
	}

	private async Task<UResponse<GoldTxnResponse?>> FailTxn(GoldTxnEntity e, Usc status, string message, CancellationToken ct, bool cancelled = false) {
		e.JsonData.Error = message;
		e.Tags = [e.Tags.Contains(TagGoldTxn.Sell) ? TagGoldTxn.Sell : TagGoldTxn.Buy, cancelled ? TagGoldTxn.Cancelled : TagGoldTxn.Failed];
		db.Set<GoldTxnEntity>().Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse<GoldTxnResponse?>(MapTxn(e), status == Usc.Success ? Usc.ThirdPartyError : status, message);
	}

	private async Task ChangeGoldBalance(GoldWalletEntity e, decimal amount, CancellationToken ct) {
		e.Balance += amount;
		db.Set<GoldWalletEntity>().Update(e);
		await db.SaveChangesAsync(ct);
	}

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

	private async Task<UResponse<GoldApiTokenResponse?>> CreateApiToken(string? label, ICollection<string> scopes, ICollection<string>? ipWhitelist, CancellationToken ct) {
		Dictionary<string, object> body = new() { { "scopes", scopes } };
		if (label.IsNotNullOrEmpty()) body.Add("label", label);
		if (ipWhitelist.IsNotNullOrEmpty()) body.Add("ipWhitelist", ipWhitelist);

		GoldResult r = await CallWithBasic(HttpMethod.Post, $"{ClientPath}auth/api-tokens", body, ct);
		if (!r.Ok) return new UResponse<GoldApiTokenResponse?>(null, r.Status, r.Message);
		return new UResponse<GoldApiTokenResponse?>(MapApiToken(r.Item), Usc.Created);
	}

	private async Task<GoldResult> CallWithBasic(HttpMethod method, string path, object? body, CancellationToken ct) {
		if (!Core.App.Gold.ClientKey.IsNotNullOrEmpty() || !Core.App.Gold.ClientSecret.IsNotNullOrEmpty())
			return GoldResult.Fail(Usc.InternalServerError, ls.Get("theGoldProviderIsNotConfigured"));

		string basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Core.App.Gold.ClientKey}:{Core.App.Gold.ClientSecret}"));
		return await Send(method, path, body, $"Basic {basic}", ct);
	}

	private async Task<GoldResult> CallWithToken(HttpMethod method, string path, object? body, CancellationToken ct) {
		string? token = await ResolveApiToken(false, ct);
		if (!token.IsNotNullOrEmpty()) return GoldResult.Fail(Usc.InternalServerError, ls.Get("theGoldProviderIsNotConfigured"));

		GoldResult result = await Send(method, path, body, $"Bearer {token}", ct);
		if (result.Ok || result.HttpCode != 401 || Core.App.Gold.ApiToken.IsNotNullOrEmpty()) return result;

		// The cached auto-created token was revoked or expired: mint a new one and replay once.
		string? refreshed = await ResolveApiToken(true, ct);
		return refreshed.IsNotNullOrEmpty() ? await Send(method, path, body, $"Bearer {refreshed}", ct) : result;
	}

	private async Task<string?> ResolveApiToken(bool force, CancellationToken ct) {
		if (Core.App.Gold.ApiToken.IsNotNullOrEmpty()) return Core.App.Gold.ApiToken;
		if (!force && _cachedApiToken.IsNotNullOrEmpty()) return _cachedApiToken;

		await TokenLock.WaitAsync(ct);
		try {
			if (!force && _cachedApiToken.IsNotNullOrEmpty()) return _cachedApiToken;
			_cachedApiToken = null;

			UResponse<GoldApiTokenResponse?> created = await CreateApiToken($"SinaMN75U-{DateTime.UtcNow:yyyyMMddHHmmss}", Core.App.Gold.Scopes.ToList(), null, ct);
			string? rawToken = created.Result?.RawToken;
			if (rawToken.IsNotNullOrEmpty()) _cachedApiToken = rawToken;
			return _cachedApiToken;
		}
		finally {
			TokenLock.Release();
		}
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

		if (response == null) return GoldResult.Fail(Usc.ThirdPartyError, ls.Get("theGoldProviderIsNotReachableRightNow"));

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

		return GoldResult.Fail(MapStatus((int)response.StatusCode, errorCode), localized, (int)response.StatusCode);
	}

	private static string PagingQuery(string? cursor, int limit) {
		int safeLimit = Math.Clamp(limit, MinPageLimit, MaxPageLimit);
		return cursor.IsNotNullOrEmpty() ? $"?limit={safeLimit}&cursor={Uri.EscapeDataString(cursor)}" : $"?limit={safeLimit}";
	}

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
		"INSUFFICIENT_BALANCE" => Usc.BalanceIsLow,
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
		{ "UNAUTHENTICATED", "authenticationWithTheGoldProviderIsRequired" },
		{ "INVALID_AUTH_HEADER", "authenticationWithTheGoldProviderIsRequired" },
		{ "INVALID_CREDENTIALS", "theGoldProviderCredentialsAreInvalid" },
		{ "INVALID_TOKEN", "theGoldProviderTokenIsInvalidOrExpired" },
		{ "FORBIDDEN", "accessToThisGoldResourceIsDenied" },
		{ "IP_NOT_ALLOWED", "thisIPAddressIsNotAllowedByTheGoldProvider" },
		{ "CLIENT_INACTIVE", "theGoldProviderClientAccountIsInactive" },
		{ "TOKEN_NOT_FOUND", "theGoldProviderTokenWasNotFound" },
		{ "MAX_ACTIVE_TOKENS_REACHED", "theMaximumNumberOfActiveGoldTokensHasBeenReachedRevokeOneFirst" },
		{ "IP_NOT_SUBSET_OF_CLIENT", "theTokenIPWhitelistMustBeASubsetOfTheClientIPWhitelist" },
		{ "RATE_LIMIT_EXCEEDED", "tooManyRequestsToTheGoldProviderPleaseTryAgainShortly" },
		{ "AUTH_RATE_LIMITED", "tooManyFailedAuthenticationAttemptsPleaseTryAgainShortly" },
		{ "BUSINESS_SUSPENDED", "yourGoldBusinessAccountIsSuspended" },
		{ "UNSUPPORTED_TRADE_PAIR", "thisTradePairIsNotSupported" },
		{ "ASSET_PAIR_MISMATCH", "theAssetPairDoesNotMatchTheRequest" },
		{ "ASSET_NOT_FOUND", "theAssetWasNotFoundOrIsInactive" },
		{ "INVALID_TRADE_REQUEST", "theTradeRequestIsNotValid" },
		{ "INVALID_TRADE_AMOUNT", "theTradeAmountIsNotValid" },
		{ "AMOUNT_PRECISION_EXCEEDED", "theAmountHasMoreDecimalPlacesThanTheAssetAllows" },
		{ "INSUFFICIENT_QUOTE_AMOUNT", "theAmountIsBelowTheMinimumTradeSize" },
		{ "DUPLICATE_IDEMPOTENCY_KEY", "thisIdempotencyKeyHasAlreadyBeenUsedGenerateANewOneForEachOrder" },
		{ "ORDER_TRADE_WINDOW_LIMIT_EXCEEDED", "yourOrderVolumeHasReachedTheLimitForTheCurrentTimeWindow" },
		{ "ORDER_NOT_FOUND", "theOrderWasNotFound" },
		{ "INSUFFICIENT_BALANCE", "yourGoldWalletBalanceIsNotEnough" }
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
		if (element.ValueKind != JsonValueKind.Object) return [];
		if (!element.TryGetProperty(propertyName, out JsonElement value) || value.ValueKind != JsonValueKind.Array) return [];
		return value.EnumerateArray().ToList();
	}

	private static ICollection<string> ReadStringList(JsonElement element, string propertyName) =>
		ReadArray(element, propertyName).Where(x => x.ValueKind == JsonValueKind.String).Select(x => x.GetString()!).ToList();

	private static DateTime? ReadDate(JsonElement element, string propertyName) {
		string? raw = element.GetStringOrNull(propertyName);
		if (!raw.IsNotNullOrEmpty()) return null;
		return DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out DateTime parsed) ? parsed : null;
	}

	// Unwraps the provider envelope: { success, data: { item | items }, meta: { pagination: { nextCursor } } }
	private sealed class GoldResult {
		public bool Ok { get; init; }
		public JsonElement Root { get; init; }
		public int HttpCode { get; init; }
		public Usc Status { get; init; } = Usc.ThirdPartyError;
		public string Message { get; init; } = "";

		public JsonElement Data => Root.ValueKind == JsonValueKind.Object && Root.TryGetProperty("data", out JsonElement d) ? d : default;

		public JsonElement Item => Data.ValueKind == JsonValueKind.Object && Data.TryGetProperty("item", out JsonElement i) ? i : default;

		public IEnumerable<JsonElement> Items {
			get {
				JsonElement data = Data;
				if (data.ValueKind != JsonValueKind.Object) return [];
				if (!data.TryGetProperty("items", out JsonElement items) || items.ValueKind != JsonValueKind.Array) return [];
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

		public static GoldResult Fail(Usc status, string message, int httpCode = 0) => new() { Ok = false, Status = status, Message = message, HttpCode = httpCode };
	}
}
