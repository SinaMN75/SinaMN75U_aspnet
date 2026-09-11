namespace SinaMN75U.Data.Params;

public sealed class IpgSaleParams : BaseParams {
	[UValidationRequired("amountRequired")]
	public decimal Amount { get; set; }

	public TagTxn Tag { get; set; } = TagTxn.ChargeWallet;

	public string? InvoiceId { get; set; }
}

public sealed class IpgBillParams : BaseParams {
	[UValidationRequired("billIdRequired")]
	public string BillId { get; set; } = null!;

	[UValidationRequired("paymentIdRequired")]
	public string PaymentId { get; set; } = null!;
}

public sealed class IpgStatusParams : BaseParams {
	[UValidationRequired("trackingNumberRequired")]
	public string TrackingNumber { get; set; } = null!;
}

public sealed class IpgAdditionalData {
	public required string TrackingNumber { get; set; }
	public required TagTxn Tag { get; set; }
	public string? InvoiceId { get; set; }
	public string? BillId { get; set; }
	public string? PaymentId { get; set; }
}
