namespace SinaMN75U.Data.Params;

public sealed class VasCreateParams : BaseCreateParams<TagVas> {
	public required decimal Amount { get; set; }
	public required string AuthorizeCode { get; set; }
	public required string OrganizationType { get; set; }
	public required string OrganizationName { get; set; }
	public required string BillId { get; set; }
	public required string PaymentId { get; set; }
	public Guid? TxnId { get; set; }
	public Guid? WalletTxnId { get; set; }
}


public sealed class VasReadParams : BaseReadParams<TagVas> {
	public required string AuthorizeCode { get; set; }
	public required string OrganizationType { get; set; }
	public required string BillId { get; set; }
	public required string PaymentId { get; set; }
	public Guid? WalletTxnId { get; set; }
	public Guid? TxnId { get; set; }

	public VasSelectorArgs SelectorArgs { get; set; } = new();
}