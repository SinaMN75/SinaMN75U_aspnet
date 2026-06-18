namespace SinaMN75U.Data.Params;

public class BedCreateParams: BaseCreateParams<TagBed> {
	public decimal Deposit { get; set; }
	public decimal Rent { get; set; }

	public Guid? ParentId { get; set; }
}

public class BedUpdateParams: BaseUpdateParams<TagBed> {
	public decimal Deposit { get; set; }
	public decimal Rent { get; set; }

	public Guid? ParentId { get; set; }
	
	public ICollection<Guid>? Media { get; set; }
}

public class BedReadParams: BaseReadParams<TagBed> {
	public decimal Deposit { get; set; }
	public decimal Rent { get; set; }

	public Guid? ParentId { get; set; }

	public BedSelectorArgs SelectorArgs { get; set; } = new();
}