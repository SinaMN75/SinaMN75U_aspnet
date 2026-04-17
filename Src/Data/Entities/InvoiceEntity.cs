namespace SinaMN75U.Data.Entities;

[Table("Invoices")]
public sealed class InvoiceEntity : BaseEntity<TagInvoice, InvoiceJson> {
	public required decimal DebtAmount { get; set; }
	public required decimal CreditorAmount { get; set; }
	public required decimal PaidAmount { get; set; }
	public required decimal PenaltyAmount { get; set; }

	public required DateTime DueDate { get; set; }

	public Guid? ContractId { get; set; }
	public ContractEntity? Contract { get; set; }
}

public sealed class InvoiceJson : BaseJsonData {
	public int PenaltyPrecentEveryDate { get; set; }
}