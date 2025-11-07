namespace SinaMN75U.Data.Entities;

[Table("Invoices")]
public class InvoiceEntity: BaseEntity<TagInvoice, InvoiceJson> {
	public required double DebtAmount { get; set; }
	public required double CreditorAmount { get; set; }
	public required double SettlementAmount { get; set; }
	public required double PenaltyAmount { get; set; }

	public UserEntity User { get; set; } = null!;
	public required Guid UserId { get; set; }

	public ContractEntity Contract { get; set; } = null!;
	public required Guid ContractId { get; set; }
}

public class InvoiceJson {
	public required string Description { get; set; }
}