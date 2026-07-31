namespace SinaMN75U.Data.Params;

public sealed class IpgSaleParams : BaseParams {
	[UValidationRequired("AmountRequired")]
	public decimal Amount { get; set; }

	public TagTxn Tag { get; set; } = TagTxn.ChargeWallet;

	public string? InvoiceId { get; set; }
}

public sealed class IpgAdditionalData {
	public required string TrackingNumber { get; set; }
	public required TagTxn Tag { get; set; }
	public string? InvoiceId { get; set; }
}
