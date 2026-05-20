namespace SinaMN75U.Data.Entities;

[Table("Vas")]
public sealed class VasEntity : BaseEntity<TagVas, VasJson> {
	[Column(TypeName = "decimal(4,2)")]
	public required decimal Amount { get; set; }

	[Required, MaxLength(100)]
	public required string AuthorizeCode { get; set; }

	[MaxLength(100)]
	public string? BillId { get; set; }

	[MaxLength(100)]
	public string? PaymentId { get; set; }

	public Guid? TxnId { get; set; }
	public TxnEntity? Txn { get; set; }
	
	public Guid? WalletTxnId { get; set; }
	public WalletTxnEntity? WalletTxn { get; set; }
}

public sealed class VasJson: BaseJson {
	public string? ChargePin { get; set; }
}