namespace SinaMN75U.Data.Params;

public sealed class CategoryCreateParams : BaseCreateParams<TagCategory> {
	public string Title { get; set; } = null!;
	public string? Subtitle { get; set; }
	public string? Location { get; set; }
	public string? Type { get; set; }
	public string? Link { get; set; }
	public string? Address { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Code { get; set; }
	public Guid? ParentId { get; set; }
	public int? Order { get; set; }

	public List<Guid> RelatedProducts { get; set; } = [];
	public IEnumerable<CategoryCreateParams> Children { get; set; } = [];
	public ICollection<Guid> Media { get; set; } = [];
}

public sealed class CategoryUpdateParams : BaseUpdateParams<TagCategory> {
	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	public string? Link { get; set; }
	public string? Location { get; set; }
	public string? Type { get; set; }
	public string? Address { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Code { get; set; }
	public int? Order { get; set; }
	public Guid? ParentId { get; set; }
	public ICollection<Guid>? RelatedProducts { get; set; }
	public ICollection<Guid>? Media { get; set; }

	public decimal? ProductDeposit { get; set; }
	public decimal? ProductRent { get; set; }
	public bool UpdateInvoicesRent { get; set; }
}

public sealed class CategoryReadParams : BaseReadParams<TagCategory> {
	public bool OrderByOrder { get; set; }
	public bool OrderByOrderDesc { get; set; }
	public CategorySelectorArgs SelectorArgs { get; set; } = new();
}