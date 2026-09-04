namespace SinaMN75U.Data.Params;

public sealed class GoldQuoteParams : BaseParams {
	public TagGoldAsset BaseAsset { get; set; } = TagGoldAsset.Gold18;
	public TagGoldAsset QuoteAsset { get; set; } = TagGoldAsset.Irr;
}

public sealed class GoldCreateOrderParams : BaseParams {
	[UValidationRequired("IdempotencyKeyRequired"), UValidationStringLength(1, 255, "IdempotencyKeyNotValid")]
	public string IdempotencyKey { get; set; } = null!;

	[UValidationRequired("OrderSideRequired")]
	public TagGoldOrderSide? Side { get; set; }

	public TagGoldAsset BaseAsset { get; set; } = TagGoldAsset.Gold18;
	public TagGoldAsset QuoteAsset { get; set; } = TagGoldAsset.Irr;

	public decimal? BaseAmount { get; set; }
	public decimal? QuoteAmount { get; set; }
}

public sealed class GoldReadOrdersParams : BaseParams {
	public string? Cursor { get; set; }
	public int Limit { get; set; } = 20;
}

public sealed class GoldReadOrderParams : BaseParams {
	[UValidationRequired("IdRequired")]
	public required string Id { get; set; }
}

public sealed class GoldReadBalanceParams : BaseParams {
	public TagGoldAsset Asset { get; set; } = TagGoldAsset.Gold18;
}

public sealed class GoldReadTransactionsParams : BaseParams {
	public string? Cursor { get; set; }
	public int Limit { get; set; } = 20;
}

public sealed class GoldCreateApiTokenParams : BaseParams {
	public string? Label { get; set; }

	[UValidationMinCollectionLength(1, "ScopesRequired")]
	public ICollection<string> Scopes { get; set; } = [];

	public ICollection<string>? IpWhitelist { get; set; }
}

public sealed class GoldDeleteApiTokenParams : BaseParams {
	[UValidationRequired("IdRequired")]
	public required string TokenId { get; set; }
}

public sealed class GoldReadUserBalanceParams : BaseParams {
	public Guid? UserId { get; set; }
}

// Send exactly one of Amount (rial to spend) or GoldAmount (grams to receive).
public sealed class GoldBuyParams : BaseParams {
	public decimal? Amount { get; set; }
	public decimal? GoldAmount { get; set; }
}

// Send exactly one of GoldAmount (grams to sell) or Amount (rial to receive).
public sealed class GoldSellParams : BaseParams {
	public decimal? Amount { get; set; }
	public decimal? GoldAmount { get; set; }
}

public sealed class GoldReadUserTxnsParams : BaseReadParams<TagGoldTxn> {
	public Guid? UserId { get; set; }

	public GoldTxnSelectorArgs SelectorArgs { get; set; } = new();
}
