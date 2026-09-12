namespace SinaMN75U.Data.Params;

public sealed class IpgPayParams : BaseParams {
	public decimal? Amount { get; set; }

	public TagTxn Tag { get; set; } = TagTxn.ChargeWallet;

	public string? InvoiceId { get; set; }

	public string? BillId { get; set; }

	public string? PaymentId { get; set; }

	public string? ChargeMobileNumber { get; set; }

	public TagSimOperator? TopUpType { get; set; }

	public IEnumerable<IpgMultiplexedAccountParams>? MultiplexedAccounts { get; set; }
}

public sealed class IpgMultiplexedAccountParams {
	public string Iban { get; set; } = null!;
	public decimal Amount { get; set; }
	public long? PayId { get; set; }
}

public sealed class IpgStatusParams : BaseParams {
	[UValidationRequired("trackingNumberRequired")]
	public string TrackingNumber { get; set; } = null!;
}

public sealed class IpgAdditionalData {
	public required string TrackingNumber { get; set; }
	public required TagTxn Tag { get; set; }
	public TagIpgPayment Kind { get; set; } = TagIpgPayment.NormalSale;
	public string? InvoiceId { get; set; }
	public string? BillId { get; set; }
	public string? PaymentId { get; set; }
	public string? ChargeMobileNumber { get; set; }
}
