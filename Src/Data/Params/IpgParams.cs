namespace SinaMN75U.Data.Params;

public sealed class IpgPayParams : BaseParams {
	public required decimal Amount { get; set; }
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

public sealed class IpgAdditionalData {
	public decimal? Amount { get; set; }
	public string? TrackingNumber { get; set; }
	public TagTxn? Tag { get; set; }
	public TagIpgPayment Kind { get; set; } = TagIpgPayment.NormalSale;
	public string? InvoiceId { get; set; }
	public string? BillId { get; set; }
	public string? PaymentId { get; set; }
	public string? ChargeMobileNumber { get; set; }

	public int? Status { get; set; }
	public string? Rrn { get; set; }
	public string? Token { get; set; }
}