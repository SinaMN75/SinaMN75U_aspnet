namespace SinaMN75U.Data.Params;

public sealed class WalletPurchaseParams : BaseParams {
	public required TagWalletTxn Tag { get; set; }
}

public sealed class WalletTransferParams : BaseParams {
	public Guid? SenderId { get; set; }

	[UValidationRequired("UserIdRequired")]
	public Guid ReceiverId { get; set; }

	[UValidationRequired("AmountRequired")]
	public decimal Amount { get; set; }

	public string? Detail1 { get; set; }
	
	public required IEnumerable<TagWalletTxn> TagWalletTxn { get; set; }
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