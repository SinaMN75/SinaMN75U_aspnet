namespace SinaMN75U.Data.Responses;

public sealed class GoldAccountResponse {
	public required string Name { get; set; }
	public required bool Active { get; set; }
	public ICollection<string> IpWhitelist { get; set; } = [];
}

public sealed class GoldQuoteResponse {
	public required TagGoldAsset BaseAsset { get; set; }
	public required TagGoldAsset QuoteAsset { get; set; }
	public string? Unit { get; set; }
	public decimal? BaseUnitPrice { get; set; }
	public decimal? BuyUnitPrice { get; set; }
	public decimal? SellUnitPrice { get; set; }
	public DateTime? UpdatedAt { get; set; }
}

public sealed class GoldWalletEntryResponse {
	public string? Asset { get; set; }
	public decimal? Amount { get; set; }
}

public sealed class GoldOrderFeeResponse {
	public string? Asset { get; set; }
	public decimal? Amount { get; set; }
	public string? Type { get; set; }
	public decimal? Rate { get; set; }
}

public sealed class GoldOrderTransactionResponse {
	public required string Id { get; set; }
	public DateTime? CreatedAt { get; set; }
	public ICollection<GoldWalletEntryResponse> Entries { get; set; } = [];
}

public sealed class GoldOrderResponse {
	public required string Id { get; set; }
	public string? IdempotencyKey { get; set; }
	public TagGoldOrderStatus? Status { get; set; }
	public TagGoldOrderSide? Side { get; set; }
	public TagGoldAsset? BaseAsset { get; set; }
	public TagGoldAsset? QuoteAsset { get; set; }
	public decimal? RequestedBaseAmount { get; set; }
	public decimal? RequestedQuoteAmount { get; set; }
	public decimal? DealtBaseAmount { get; set; }
	public decimal? DealtQuoteAmount { get; set; }
	public decimal? EffectivePrice { get; set; }
	public decimal? BaseUnitPrice { get; set; }
	public DateTime? CreatedAt { get; set; }
	public ICollection<GoldOrderFeeResponse> Fees { get; set; } = [];
	public ICollection<GoldOrderTransactionResponse> Transactions { get; set; } = [];
}

public sealed class GoldOrderListResponse {
	public ICollection<GoldOrderResponse> Items { get; set; } = [];
	public string? NextCursor { get; set; }
}

public sealed class GoldBalanceResponse {
	public TagGoldAsset? Asset { get; set; }
	public required string AssetCode { get; set; }
	public decimal? Balance { get; set; }
	public bool Locked { get; set; }
}

public sealed class GoldTransactionResponse {
	public required string Id { get; set; }
	public string? IdempotencyKey { get; set; }
	public DateTime? CreatedAt { get; set; }
	public ICollection<GoldWalletEntryResponse> Entries { get; set; } = [];
	public string? Detail { get; set; }
}

public sealed class GoldTransactionListResponse {
	public ICollection<GoldTransactionResponse> Items { get; set; } = [];
	public string? NextCursor { get; set; }
}

public sealed class GoldTradeLimitResponse {
	public string? Type { get; set; }
	public string? Asset { get; set; }
	public decimal? MaxVolume { get; set; }
	public decimal? UsedVolume { get; set; }
	public decimal? RemainingVolume { get; set; }
	public string? Interval { get; set; }
	public string? ResetsAt { get; set; }
	public TagGoldOrderSide? Side { get; set; }
	public string? WindowStart { get; set; }
	public string? WindowEnd { get; set; }
}

public sealed class GoldTradeLimitsResponse {
	public string? Timezone { get; set; }
	public string? CurrentTime { get; set; }
	public ICollection<GoldTradeLimitResponse> Items { get; set; } = [];
	public ICollection<GoldTradeLimitResponse> CurrentLimits { get; set; } = [];
}

public sealed class GoldCreditLimitResponse {
	public string? Interval { get; set; }
	public decimal? Limit { get; set; }
	public decimal? Used { get; set; }
	public decimal? Remaining { get; set; }
	public string? ResetsAt { get; set; }
}

public sealed class GoldCreditFacilityResponse {
	public string? Type { get; set; }
	public string? Asset { get; set; }
	public decimal? CreditUsed { get; set; }
	public decimal? AvailableCredit { get; set; }
	public ICollection<GoldCreditLimitResponse> Limits { get; set; } = [];
}

public sealed class GoldAssetBalanceResponse {
	public string? Asset { get; set; }
	public decimal? Balance { get; set; }
	public decimal? AvailableToTrade { get; set; }
}

public sealed class GoldCreditFacilitiesResponse {
	public string? Timezone { get; set; }
	public string? CurrentTime { get; set; }
	public ICollection<GoldCreditFacilityResponse> Items { get; set; } = [];
	public ICollection<GoldAssetBalanceResponse> Balances { get; set; } = [];
}

public sealed class GoldApiTokenResponse {
	public required string Id { get; set; }
	public string? TokenPrefix { get; set; }
	public string? Label { get; set; }
	public ICollection<string> Scopes { get; set; } = [];
	public ICollection<string> IpWhitelist { get; set; } = [];
	public bool Active { get; set; }
	public DateTime? ExpiresAt { get; set; }
	public DateTime? CreatedAt { get; set; }
	public string? RawToken { get; set; }
}

public sealed class GoldUserBalanceResponse {
	public required decimal Balance { get; set; }
	public decimal? BuyUnitPrice { get; set; }
	public decimal? SellUnitPrice { get; set; }
	public decimal? Value { get; set; }
	public string? Unit { get; set; }
	public DateTime? UpdatedAt { get; set; }
}

public sealed class GoldTxnResponse : BaseResponse<TagGoldTxn, GoldTxnJson> {
	public UserResponse? User { get; set; }
	public required Guid UserId { get; set; }
	public required decimal GoldAmount { get; set; }
	public required decimal Amount { get; set; }
	public decimal UnitPrice { get; set; }
	public string? OrderId { get; set; }
	public required string IdempotencyKey { get; set; }
}
