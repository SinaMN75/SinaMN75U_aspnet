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

	public int PenaltyPrecentEveryDate { get; set; } = 0;

	[UValidationRequired("UserIdRequired")]
	public required Guid UserId { get; set; }

	[UValidationRequired("ContractIdRequired")]
	public required Guid ContractId { get; set; }

	[UValidationRequired("DateRequired")]
	public DateTime DueDate { get; set; }

	public required string Description { get; set; }
	
	public InvoiceEntity MapToEntity() => new() {
		DebtAmount = DebtAmount,
		CreditorAmount = CreditorAmount,
		PaidAmount = PaidAmount,
		PenaltyAmount = PenaltyAmount,
		ContractId = ContractId,
		DueDate = DueDate,
		JsonData = new InvoiceJson {
			Description = Description,
			PenaltyPrecentEveryDate =  PenaltyPrecentEveryDate,
		},
		Tags = Tags
	};

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
	
	public void MapToEntity(InvoiceEntity e) {
		if (DebtAmount.HasValue) e.DebtAmount = DebtAmount.Value;
		if (CreditorAmount.HasValue) e.CreditorAmount = CreditorAmount.Value;
		if (PaidAmount.HasValue) e.PaidAmount = PaidAmount.Value;
		if (PenaltyAmount.HasValue) e.PenaltyAmount = PenaltyAmount.Value;
		if (DueDate.HasValue) e.DueDate = DueDate.Value;
		if (Description != null) e.JsonData.Description = Description;
		if (ContractId.HasValue) e.ContractId = ContractId.Value;
		if (PenaltyPrecentEveryDate.HasValue) e.JsonData.PenaltyPrecentEveryDate = PenaltyPrecentEveryDate.Value;
		if (Tags != null) e.Tags = Tags;
	}
}

public sealed class InvoiceReadParams : BaseReadParams<TagInvoice> {
	public InvoiceSelectorArgs SelectorArgs { get; set; } = new();
}