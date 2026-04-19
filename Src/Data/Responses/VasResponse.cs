namespace SinaMN75U.Data.Responses;

public sealed class VasResponse : BaseResponse<TagVas, BaseJsonData> {
	public required decimal Amount { get; set; }
	public required string AuthorizeCode { get; set; }
	public required string OrganizationType { get; set; }
	public required string OrganizationName { get; set; }
	public required string BillId { get; set; }
	public required string PaymentId { get; set; }

	public Guid? TxnId { get; set; }
	public TxnResponse? Txn { get; set; }

	public Guid? WalletTxnId { get; set; }
	public WalletTxnResponse? WalletTxn { get; set; }
}