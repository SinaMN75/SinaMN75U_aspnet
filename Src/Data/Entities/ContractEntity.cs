namespace SinaMN75U.Data.Entities;

[Table("Contracts")]
public class ContractEntity : BaseEntity<TagContract, ContractJson> {
	public required DateTime StartDate { get; set; }
	public required DateTime EndDate { get; set; }

	public required double Deposit { get; set; }
	public required double Rent { get; set; }

	public UserEntity User { get; set; } = null!;
	public required Guid UserId { get; set; }

	public UserEntity Creator { get; set; } = null!;
	public required Guid CreatorId { get; set; }

	public ProductEntity Product { get; set; } = null!;
	public required Guid ProductId { get; set; }

	public ICollection<InvoiceEntity> Invoices { get; set; } = [];
	
	public new ContractResponse MapToResponse() => new() {
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
		ProductId = ProductId,

		User = null,
		Creator = null,
		Product = null,
		Invoices = []
	};


}

public class ContractJson {
	public string? Description { get; set; }
}