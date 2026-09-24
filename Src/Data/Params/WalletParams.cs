namespace SinaMN75U.Data.Params;

public sealed class WalletPurchaseParams : BaseParams {
	public required TagWalletTxn Tag { get; set; }
	public decimal? Amount { get; set; }
	public ICollection<KeyValue> KeyValues { get; set; } = [];

	/// <summary>Internal only (not bindable from JSON): the service was already delivered, so the debit must be recorded even if the balance goes negative.</summary>
	[JsonIgnore] public bool AllowOverdraft { get; set; }
}

public sealed class WalletChargeParams : BaseParams {
	public required Guid UserId { get; set; }
	public Guid? WalletId { get; set; }
	public required decimal Amount { get; set; }
}

public sealed class WalletTransferParams : BaseParams {
	[UValidationRequired("userIsRequired")]
	public required Guid SenderId { get; set; }

	[UValidationRequired("userIsRequired")]
	public Guid ReceiverId { get; set; }

	[UValidationRequired("amountRequired")]
	public decimal Amount { get; set; }

	public string? Detail1 { get; set; }

	public ICollection<KeyValue> KeyValues { get; set; } = [];

	public required ICollection<TagWalletTxn> TagWalletTxn { get; set; }

	/// <summary>Internal only (not bindable from JSON). See <see cref="WalletPurchaseParams.AllowOverdraft"/>.</summary>
	[JsonIgnore] public bool AllowOverdraft { get; set; }
}

public sealed class WalletTxnReadParams : BaseReadParams<TagWalletTxn> {
	[UValidationRequired("userIsRequired")]
	public Guid UserId { get; set; }

	public WalletTxnSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class WalletReadParams : BaseReadParams<TagWallet> {
	public WalletSelectorArgs SelectorArgs { get; set; } = new();
}