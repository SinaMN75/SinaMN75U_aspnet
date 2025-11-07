namespace SinaMN75U.Data.Entities;

public class ContractEntity : BaseEntity<TagContract, ContractJson> {
	public required DateTime StartDate { get; set; }
	public required DateTime EndDate { get; set; }

	public required double Price1 { get; set; }
	public required double Price2 { get; set; }

	public UserEntity User { get; set; } = null!;
	public required Guid UserId { get; set; }

	public UserEntity Creator { get; set; } = null!;
	public required Guid CreatorId { get; set; }

	public ProductEntity Product { get; set; } = null!;
	public required Guid ProductId { get; set; }

	public ICollection<InvoiceEntity> Invoices { get; set; } = [];
}

public class ContractJson {
	public string? Description { get; set; }
}