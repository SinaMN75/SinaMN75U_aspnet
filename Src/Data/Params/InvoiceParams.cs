namespace SinaMN75U.Data.Params;

public sealed class InvoiceCreateParams : BaseCreateParams<TagInvoice> {
	[UValidationRequired("PriceRequired")]
	public required decimal DebtAmount { get; set; }

	[UValidationRequired("PriceRequired")]
	public required decimal CreditorAmount { get; set; }

	[UValidationRequired("PriceRequired")]
	public required decimal PaidAmount { get; set; }

	[UValidationRequired("PriceRequired")]
	public required decimal PenaltyAmount { get; set; }

	public int PenaltyPrecentEveryDate { get; set; }

	[UValidationRequired("UserIdRequired")]
	public required Guid UserId { get; set; }

	[UValidationRequired("ContractIdRequired")]
	public required Guid ContractId { get; set; }

	[UValidationRequired("DateRequired")]
	public DateTime DueDate { get; set; }

	public required string Description { get; set; }
}

public sealed class InvoiceUpdateParams : BaseUpdateParams<TagInvoice> {
	public decimal? DebtAmount { get; set; }
	public decimal? CreditorAmount { get; set; }
	public decimal? PaidAmount { get; set; }
	public decimal? PenaltyAmount { get; set; }
	public int? PenaltyPrecentEveryDate { get; set; }
	public DateTime? DueDate { get; set; }
	public string? Description { get; set; }
	public Guid? UserId { get; set; }
	public Guid? ContractId { get; set; }
}

public sealed class InvoiceReadParams : BaseReadParams<TagInvoice> {
	public InvoiceSelectorArgs SelectorArgs { get; set; } = new();

	public Guid? ContractId { get; set; }
	public Guid? UserId { get; set; }
}