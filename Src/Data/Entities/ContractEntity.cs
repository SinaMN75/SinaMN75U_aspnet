namespace SinaMN75U.Data.Entities;

[Table("Contracts")]
public sealed class ContractEntity : BaseEntity<TagContract, BaseJsonData> {
	public required DateTime StartDate { get; set; }
	public required DateTime EndDate { get; set; }

	[Required]
	[Column(TypeName = "decimal(24,2)")]
	public required decimal Deposit { get; set; }

	[Required]
	[Column(TypeName = "decimal(24,2)")]
	public required decimal Rent { get; set; }

	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;

	public required Guid ProductId { get; set; }
	public ProductEntity Product { get; set; } = null!;

	public ICollection<InvoiceEntity> Invoices { get; set; } = [];
}
