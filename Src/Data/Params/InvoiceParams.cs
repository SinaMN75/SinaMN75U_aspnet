namespace SinaMN75U.Data.Params;

public sealed class InvoiceCreateParams : BaseCreateParams<TagInvoice> {
	[UValidationRequired("PriceRequired")]
	public required double DebtAmount { get; set; }

	[UValidationRequired("PriceRequired")]
	public required double CreditorAmount { get; set; }

	[UValidationRequired("PriceRequired")]
	public required double PaidAmount { get; set; }

	[UValidationRequired("PriceRequired")]
	public required double PenaltyAmount { get; set; }

	[UValidationRequired("UserIdRequired")]
	public required Guid UserId { get; set; }

	[UValidationRequired("ContractIdRequired")]
	public required Guid ContractId { get; set; }

	public DateTime? PaidDate { get; set; }

	[UValidationRequired("DateRequired")]
	public DateTime DueDate { get; set; }

	public required string Description { get; set; }
}

public sealed class InvoiceUpdateParams : BaseUpdateParams<TagInvoice> {
	public double? DebtAmount { get; set; }
	public double? CreditorAmount { get; set; }
	public double? PaidAmount { get; set; }
	public double? PenaltyAmount { get; set; }
	public DateTime? PaidDate { get; set; }
	public DateTime? DueDate { get; set; }
	public string? Description { get; set; }
	public Guid? UserId { get; set; }
	public Guid? ContractId { get; set; }
}

public sealed class InvoiceReadParams : BaseReadParams<TagInvoice> {
	public Guid? UserId { get; set; }
	public bool ShowUser { get; set; }
	public bool ShowContract { get; set; }
}