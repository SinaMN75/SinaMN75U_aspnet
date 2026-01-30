namespace SinaMN75U.Data.Entities;

[Table("Contracts")]
public class ContractEntity : BaseEntity<TagContract, ContractJson> {
	public required DateTime StartDate { get; set; }
	public required DateTime EndDate { get; set; }

	[Required, Column(TypeName = "decimal(24,2)")]
	public required decimal Deposit { get; set; }

	[Required, Column(TypeName = "decimal(24,2)")]
	public required decimal Rent { get; set; }

	public UserEntity User { get; set; } = null!;
	public required Guid UserId { get; set; }

	public UserEntity Creator { get; set; } = null!;
	public required Guid CreatorId { get; set; }

	public ProductEntity Product { get; set; } = null!;
	public required Guid ProductId { get; set; }

	public ICollection<InvoiceEntity> Invoices { get; set; } = [];
	
	public ContractResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		DeletedAt = DeletedAt,
		JsonData = JsonData,
		Tags = Tags,
		StartDate = StartDate,
		EndDate = EndDate,
		Deposit = Deposit,
		Rent = Rent,
		UserId = UserId,
		CreatorId = CreatorId,
		ProductId = ProductId
	};


}

public class ContractJson {
	public string? Description { get; set; }
}