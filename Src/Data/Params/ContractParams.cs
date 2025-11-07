namespace SinaMN75U.Data.Params;

public class ContractCreateParams : BaseCreateParams<TagContract> {
	[UValidationRequired("StartDateRequired")]
	public required DateTime StartDate { get; set; }

	[UValidationBeforeDate("BeforeDateSelected")]
	[UValidationRequired("EndDateRequired")]
	public required DateTime EndDate { get; set; }

	[UValidationRequired("PriceRequired")]
	public required double Price1 { get; set; }

	[UValidationRequired("PriceRequired")]
	public required double Price2 { get; set; }

	public required Guid UserId { get; set; }
	public required Guid CreatorId { get; set; }
	public required Guid ProductId { get; set; }
	
	public string? Description { get; set; }
	
}

public class ContractUpdateParams : BaseUpdateParams<TagContract> {
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public double? Price1 { get; set; }
	public double? Price2 { get; set; }
}

public class ContractReadParams : BaseReadParams<TagContract> {
	public Guid? UserId { get; set; }
	public Guid? CreatorId { get; set; }
	public Guid? ProductId { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
}