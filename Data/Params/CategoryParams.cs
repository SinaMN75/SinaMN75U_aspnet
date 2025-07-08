namespace SinaMN75U.Data.Params;

public class CategoryCreateParams : BaseParams {
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
	public List<Guid>? RelatedProducts { get; set; }
}

public class CategoryUpdateParams : BaseUpdateParams<TagCategory> {
	[UUpdateAssignIfNotNull(nameof(CategoryEntity.Title))]
	public string? Title { get; set; }

	[UUpdateAssignNested(nameof(CategoryEntity.JsonData), nameof(CategoryJson.Subtitle))]
	public string? Subtitle { get; set; }

	[UUpdateAssignNested(nameof(CategoryEntity.JsonData), nameof(CategoryJson.Link))]
	public string? Link { get; set; }

	[UUpdateAssignIfNotNull(nameof(CategoryEntity.Order))]
	public int? Order { get; set; }

	[UUpdateAssignNested(nameof(CategoryEntity.JsonData), nameof(CategoryJson.Location))]
	public string? Location { get; set; }

	[UUpdateAssignNested(nameof(CategoryEntity.JsonData), nameof(CategoryJson.Type))]
	public string? Type { get; set; }
	
	[UUpdateAddRangeNestedIfNotExistIfNotNull(nameof(CategoryEntity.JsonData), nameof(CategoryJson.RelatedProducts))]
	public List<Guid>? RelatedProducts { get; set; }
}

public class CategoryReadParams : BaseReadParams<TagCategory> {
	public IEnumerable<Guid>? Ids { get; set; }
	public bool ShowMedia { get; set; } = false;
}