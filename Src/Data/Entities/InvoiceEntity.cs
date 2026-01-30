namespace SinaMN75U.Data.Entities;

[Table("Invoices")]
public class InvoiceEntity: BaseEntity<TagInvoice, InvoiceJson> {
	public required decimal DebtAmount { get; set; }
	public required decimal CreditorAmount { get; set; }
	public required decimal PaidAmount { get; set; }
	public required decimal PenaltyAmount { get; set; }

	public required DateTime DueDate { get; set; }
	
	public ContractEntity Contract { get; set; } = null!;
	public required Guid? ContractId { get; set; }
	
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
		DueDate = DueDate
	};

}

public class InvoiceJson {
	public string Description { get; set; } = "";
	public int PenaltyPrecentEveryDate { get; set; }
}