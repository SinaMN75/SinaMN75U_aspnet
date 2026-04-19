namespace SinaMN75U.Data.Entities;

[Table("Vas")]
public sealed class VasEntity : BaseEntity<TagVas, BaseJsonData> {
	[Column(TypeName = "decimal(4,2)")]
	public required decimal Amount { get; set; }

	[Required, MaxLength(100)]
	public required string AuthorizeCode { get; set; }

	[Required, MaxLength(100)]
	public required string OrganizationType { get; set; }

	[Required, MaxLength(100)]
	public required string OrganizationName { get; set; }

	[Required, MaxLength(100)]
	public required string BillId { get; set; }

	[Required, MaxLength(100)]
	public required string PaymentId { get; set; }

	public Guid? TxnId { get; set; }
	public TxnEntity? Txn { get; set; }
	
	public Guid? WalletTxnId { get; set; }
	public WalletTxnEntity? WalletTxn { get; set; }
}