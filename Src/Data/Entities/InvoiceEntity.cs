namespace SinaMN75U.Data.Entities;

[Table("Invoices")]
public class InvoiceEntity: BaseEntity<TagInvoice, InvoiceJson> {
	public required double DebtAmount { get; set; }
	public required double CreditorAmount { get; set; }
	public required double PaidAmount { get; set; }
	public required double PenaltyAmount { get; set; }

	public DateTime? PaidDate { get; set; }
	public required DateTime DueDate { get; set; }

	[MaxLength(20)]
	public string? TrackingNumber { get; set; }

	public UserEntity User { get; set; } = null!;
	public required Guid UserId { get; set; }

	public ContractEntity Contract { get; set; } = null!;
	public required Guid ContractId { get; set; }
	
	public InvoiceResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		DeletedAt = DeletedAt,
		JsonData = JsonData,
		Tags = Tags,

		DebtAmount = DebtAmount,
		CreditorAmount = CreditorAmount,
		PaidAmount = PaidAmount,
		PenaltyAmount = PenaltyAmount,
		DueDate = DueDate,
		PaidDate = PaidDate,
		TrackingNumber = TrackingNumber,

		User = null,
		Contract = null
	};

}

public class InvoiceJson {
	public required string Description { get; set; }
}