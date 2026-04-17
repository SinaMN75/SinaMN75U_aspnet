namespace SinaMN75U.Data.Params;

public sealed class TxnCreateParams : BaseCreateParams<TagTxn> {
	[UValidationRequired("AmountRequired")]
	public decimal Amount { get; set; }

	[UValidationRequired("TrackingNumberRequired")]
	public string TrackingNumber { get; set; } = null!;

	public Guid? InvoiceId { get; set; }
}

public sealed class TxnUpdateParams : BaseUpdateParams<TagTxn> {
	public decimal? Amount { get; set; }
	public string? TrackingNumber { get; set; }
	public DateTime? PaidAt { get; set; }
	public string? GatewayName { get; set; }
}

public sealed class TxnReadParams : BaseReadParams<TagTxn> {
	public TxnSelectorArgs SelectorArgs { get; set; } = new();
}