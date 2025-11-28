namespace SinaMN75U.Data.Params;

public sealed class CategoryCreateParams : BaseParams {
	public Guid? Id { get; set; }

	[UValidationRequired("TitleRequired")]
	public required string Title { get; set; }

	public string? Subtitle { get; set; }

	[UValidationMinCollectionLength(1, "TagsRequired")]
	public required List<TagCategory> Tags { get; set; }

	public Guid? ParentId { get; set; }

	public int? Order { get; set; }
	public string? Location { get; set; }
	public string? Type { get; set; }
	public string? Link { get; set; }
	public string? Address { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Code { get; set; }
	public List<Guid>? RelatedProducts { get; set; }

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
	public ICollection<Guid>? AddRelatedProducts { get; set; }
	public ICollection<Guid>? RemoveRelatedProducts { get; set; }
	public ICollection<Guid> Media { get; set; } = [];

	public double? ProductPrice1 { get; set; }
	public double? ProductPrice2 { get; set; }
	public bool UpdateInvoicesPrices { get; set; } = false;
}

public sealed class CategoryReadParams : BaseReadParams<TagCategory> {
	public bool ShowMedia { get; set; }
	public bool ShowChildren { get; set; }
	public bool ShowChildrenMedia { get; set; }
	public bool OrderByOrder { get; set; }
	public bool OrderByOrderDesc { get; set; }
}