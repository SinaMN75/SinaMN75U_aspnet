namespace SinaMN75U.Data.Params;

public sealed class WalletPurchaseParams : BaseParams {
	public required decimal Amount { get; set; }
	public required TagPurchase Tag { get; set; }
	public bool AllowMinusBalance { get; set; }
}

public sealed class WalletTransferParams : BaseParams {
	public Guid? SenderId { get; set; }
	public required Guid ReceiverId { get; set; }
	public required decimal Amount { get; set; }
	public string? Description { get; set; }
}

public sealed class WalletTxnReadParams : BaseReadParams<TagWalletTxn> {
	public required Guid UserId { get; set; }

	public WalletTxnSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class WalletReadParams : BaseReadParams<TagWalletTxn> {
	public required Guid UserId { get; set; }
	
	public WalletSelectorArgs SelectorArgs { get; set; } = new();
}