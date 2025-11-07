namespace SinaMN75U.Data.Params;

public class InvoiceCreateParams : BaseCreateParams<TagInvoice> {
	[UValidationRequired("PriceRequired")]
	public required double DebtAmount { get; set; }

	[UValidationRequired("PriceRequired")]
	public required double CreditorAmount { get; set; }

	[UValidationRequired("PriceRequired")]
	public required double SettlementAmount { get; set; }

	[UValidationRequired("PriceRequired")]
	public required double PenaltyAmount { get; set; }

	[UValidationRequired("UserIdRequired")]
	public required Guid UserId { get; set; }

	[UValidationRequired("ContractIdRequired")]
	public required Guid ContractId { get; set; }

	public required string Description { get; set; }
}

public class InvoiceUpdateParams : BaseUpdateParams<TagInvoice> {
	public double? DebtAmount { get; set; }
	public double? CreditorAmount { get; set; }
	public double? SettlementAmount { get; set; }
	public double? PenaltyAmount { get; set; }
}

public class InvoiceReadParams : BaseReadParams<TagInvoice> {
	public Guid? UserId { get; set; }
}