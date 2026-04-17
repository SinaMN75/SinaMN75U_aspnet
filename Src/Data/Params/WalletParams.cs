namespace SinaMN75U.Data.Params;

public sealed class WalletPurchaseParams : BaseParams {
	[UValidationRequired("AmountRequired")]
	public decimal Amount { get; set; }
	public TagPurchase Tag { get; set; }
	public bool AllowMinusBalance { get; set; }
}

public sealed class WalletTransferParams : BaseParams {
	public Guid? SenderId { get; set; }

	[UValidationRequired("UserIdRequired")]
	public Guid ReceiverId { get; set; }

	[UValidationRequired("AmountRequired")]
	public decimal Amount { get; set; }

	public string? Description { get; set; }
}

public sealed class WalletTxnReadParams : BaseReadParams<TagWalletTxn> {
	[UValidationRequired("UserIdRequired")]
	public Guid UserId { get; set; }

	public WalletTxnSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class WalletReadParams : BaseReadParams<TagWalletTxn> {
	[UValidationRequired("UserIdRequired")]
	public Guid UserId { get; set; }

	public WalletSelectorArgs SelectorArgs { get; set; } = new();
}