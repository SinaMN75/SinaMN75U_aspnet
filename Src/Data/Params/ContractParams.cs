namespace SinaMN75U.Data.Params;

public sealed class ContractCreateParams : BaseCreateParams<TagContract> {
	[UValidationRequired("StartDateRequired")]
	public required DateTime StartDate { get; set; }

	[UValidationRequired("EndDateRequired")]
	public required DateTime EndDate { get; set; }

	public required Guid UserId { get; set; }
	public required Guid ProductId { get; set; }
	
	public string? Description { get; set; }
	
	public ContractEntity MapToEntity() => new() {
		StartDate = StartDate,
		EndDate = EndDate,
		UserId = UserId,
		ProductId = ProductId,
		JsonData = new ContractJson {
			Description = Description
		},
		Tags = Tags,
		Deposit = 0,
		Rent = 0,
		CreatorId = default
	};

	
}

public sealed class ContractUpdateParams : BaseUpdateParams<TagContract> {
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public double? Deposit { get; set; }
	public double? Rent { get; set; }
	
	public void MapToEntity(ContractEntity e) {
		if (StartDate.HasValue) e.StartDate = StartDate.Value;
		if (EndDate.HasValue) e.EndDate = EndDate.Value;
		if (Deposit.HasValue) e.Deposit = Deposit.Value;
		if (Rent.HasValue) e.Rent = Rent.Value;
		if (Tags != null) e.Tags = Tags;
	}

}

public class ContractReadParams : BaseReadParams<TagContract> {
	public Guid? UserId { get; set; }
	public string? UserName { get; set; }
	public Guid? CreatorId { get; set; }
	public Guid? ProductId { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public bool ShowInvoices { get; set; }
	public bool ShowUser { get; set; }
	public bool ShowCreator { get; set; }
	public bool ShowProduct { get; set; }
}