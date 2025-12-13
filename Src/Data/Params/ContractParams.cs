namespace SinaMN75U.Data.Params;

public sealed class ContractCreateParams : BaseCreateParams<TagContract> {
	[UValidationRequired("StartDateRequired")]
	public required DateTime StartDate { get; set; }

	[UValidationRequired("EndDateRequired")]
	public required DateTime EndDate { get; set; }
	
	public decimal? Deposit { get; set; }
	public decimal? Rent { get; set; }

	public required Guid UserId { get; set; }
	public required Guid ProductId { get; set; }
	
	public string? Description { get; set; }
	public int PenaltyPrecentEveryDate { get; set; } = 0;
}

public sealed class ContractUpdateParams : BaseUpdateParams<TagContract> {
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public decimal? Deposit { get; set; }
	public decimal? Rent { get; set; }
	
	public ContractEntity MapToEntity(ContractEntity e) {
		e.UpdatedAt = DateTime.UtcNow;
		if (Deposit.HasValue) e.Deposit = Deposit.Value;
		if (Rent.HasValue) e.Rent = Rent.Value;
		if (StartDate.HasValue) e.StartDate = StartDate.Value;
		if (EndDate.HasValue) e.EndDate = EndDate.Value;
		if (AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(AddTags);
		if (RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(x => RemoveTags.Contains(x));
		if (Tags.IsNotNullOrEmpty()) e.Tags = Tags;
		return e;
	}

}

public class ContractReadParams : BaseReadParams<TagContract> {
	public Guid? UserId { get; set; }
	public string? UserName { get; set; }
	public Guid? CreatorId { get; set; }
	public Guid? ProductId { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public ContractSelectorArgs SelectorArgs { get; set; } = new();
}