namespace SinaMN75U.Data.Params;

public sealed class InvoiceCreateParams : BaseCreateParams<TagInvoice> {
	[UValidationRequired("PriceRequired")]
	public decimal DebtAmount { get; set; }

	[UValidationRequired("PriceRequired")]
	public decimal CreditorAmount { get; set; }

	[UValidationRequired("PriceRequired")]
	public decimal PaidAmount { get; set; }

	[UValidationRequired("PriceRequired")]
	public decimal PenaltyAmount { get; set; }

	public int PenaltyPrecentEveryDate { get; set; }

	[UValidationRequired("ContractIdRequired")]
	public Guid ContractId { get; set; }

	[UValidationRequired("DateRequired")]
	public DateTime DueDate { get; set; }
}

public sealed class InvoiceUpdateParams : BaseUpdateParams<TagInvoice> {
	public decimal? DebtAmount { get; set; }
	public decimal? CreditorAmount { get; set; }
	public decimal? PaidAmount { get; set; }
	public decimal? PenaltyAmount { get; set; }
	public int? PenaltyPrecentEveryDate { get; set; }
	public DateTime? DueDate { get; set; }
	public Guid? UserId { get; set; }
	public Guid? ContractId { get; set; }
}

public sealed class InvoiceReadParams : BaseReadParams<TagInvoice> {
	public InvoiceSelectorArgs SelectorArgs { get; set; } = new();

	public Guid? ContractId { get; set; }
	public Guid? UserId { get; set; }
}