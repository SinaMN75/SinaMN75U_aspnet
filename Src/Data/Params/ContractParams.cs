namespace SinaMN75U.Data.Params;

public sealed class ContractCreateParams : BaseCreateParams<TagContract> {
	[UValidationRequired("StartDateRequired")]
	public DateTime StartDate { get; set; }

	[UValidationRequired("EndDateRequired")]
	public DateTime EndDate { get; set; }

	public decimal? Deposit { get; set; }
	public decimal? Rent { get; set; }

	[UValidationRequired("UserIdRequired")]
	public Guid UserId { get; set; }
	
	[UValidationRequired("ProductRequired")]
	public Guid ProductId { get; set; }

	public int PenaltyPrecentEveryDate { get; set; }
}

public sealed class ContractUpdateParams : BaseUpdateParams<TagContract> {
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public decimal? Deposit { get; set; }
	public decimal? Rent { get; set; }
}

public sealed class ContractReadParams : BaseReadParams<TagContract> {
	public Guid? UserId { get; set; }
	public string? UserName { get; set; }
	public Guid? ProductId { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public ContractSelectorArgs SelectorArgs { get; set; } = new();
}