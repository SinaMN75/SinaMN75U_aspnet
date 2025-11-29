namespace SinaMN75U.Data.Params;

public sealed class ContractCreateParams : BaseCreateParams<TagContract> {
	[UValidationRequired("StartDateRequired")]
	public required DateTime StartDate { get; set; }

	[UValidationRequired("EndDateRequired")]
	public required DateTime EndDate { get; set; }

	public double? Deposit { get; set; }
	public double? Rent { get; set; }

	public required Guid UserId { get; set; }
	public required Guid ProductId { get; set; }
	
	public string? Description { get; set; }
	
}

public sealed class ContractUpdateParams : BaseUpdateParams<TagContract> {
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public double? Deposit { get; set; }
	public double? Rent { get; set; }
}

public class ContractReadParams : BaseReadParams<TagContract> {
	public Guid? UserId { get; set; }
	public Guid? CreatorId { get; set; }
	public Guid? ProductId { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
}