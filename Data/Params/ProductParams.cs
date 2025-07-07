namespace SinaMN75U.Data.Params;

public class ProductCreateParams : BaseParams {
	[URequired("TitleRequired")]
	public required string Title { get; set; }

	public string? Code { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public string? ActionType { get; set; }
	public string? ActionTitle { get; set; }
	public string? ActionUri { get; set; }
	public string? Slug { get; set; }
	public string? Type { get; set; }
	public string? Content { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public int? Stock { get; set; }
	public double? Price { get; set; }

	public string? Details { get; set; }

	[UMinCollectionLength(1, "TagsRequired")]
	public required List<TagProduct> Tags { get; set; }

	public IEnumerable<Guid>? Categories { get; set; }

	public Guid? ParentId { get; set; }
	public Guid? UserId { get; set; }
}

public class ProductUpdateParams : BaseUpdateParams<TagProduct> {
	[UUpdateAssignIfNotNull(nameof(ProductEntity.Title))]
	public string? Title { get; set; }

	[UUpdateAssignIfNotNull(nameof(ProductEntity.Code))]
	public string? Code { get; set; }

	[UUpdateAssignIfNotNull(nameof(ProductEntity.Subtitle))]
	public string? Subtitle { get; set; }

	[UUpdateAssignIfNotNull(nameof(ProductEntity.Description))]
	public string? Description { get; set; }

	[UUpdateAssignNested(nameof(ProductEntity.JsonData), nameof(ProductJson.ActionType))]
	public string? ActionType { get; set; }

	[UUpdateAssignNested(nameof(ProductEntity.JsonData), nameof(ProductJson.ActionTitle))]
	public string? ActionTitle { get; set; }

	[UUpdateAssignNested(nameof(ProductEntity.JsonData), nameof(ProductJson.ActionUri))]
	public string? ActionUri { get; set; }

	[UUpdateAssignIfNotNull(nameof(ProductEntity.Slug))]
	public string? Slug { get; set; }

	[UUpdateAssignIfNotNull(nameof(ProductEntity.Type))]
	public string? Type { get; set; }

	[UUpdateAssignIfNotNull(nameof(ProductEntity.Content))]
	public string? Content { get; set; }

	[UUpdateAssignIfNotNull(nameof(ProductEntity.Latitude))]
	public double? Latitude { get; set; }

	[UUpdateAssignIfNotNull(nameof(ProductEntity.Longitude))]
	public double? Longitude { get; set; }

	[UUpdateAssignIfNotNull(nameof(ProductEntity.Stock))]
	public int? Stock { get; set; }

	[UUpdateAssignIfNotNull(nameof(ProductEntity.Price))]
	public double? Price { get; set; }

	[UUpdateAssignNested(nameof(ProductEntity.JsonData), nameof(ProductJson.Details))]
	public string? Details { get; set; }

	[UUpdateAddRangeNestedIfNotExistIfNotNull(nameof(ProductEntity.JsonData), nameof(ProductJson.RelatedProducts))]
	public IEnumerable<Guid>? AddRelatedProducts { get; set; }

	[UUpdateRemoveNestedMatchingIfNotNull(nameof(ProductEntity.JsonData), nameof(ProductJson.RelatedProducts))]
	public IEnumerable<Guid>? RemoveRelatedProducts { get; set; }

	public IEnumerable<Guid>? AddCategories { get; set; }
	public IEnumerable<Guid>? RemoveCategories { get; set; }

	[UUpdateAssignIfNotNull(nameof(ProductEntity.ParentId))]
	public Guid? ParentId { get; set; }

	[UUpdateAssignIfNotNull(nameof(ProductEntity.UserId))]
	public Guid? UserId { get; set; }
}

public class ProductReadParams : BaseReadParams<TagProduct> {
	[UFilterIn(nameof(ProductEntity.Id))]
	public IEnumerable<Guid>? Ids { get; set; }

	public string? Query { get; set; }

	[UFilterContains(nameof(ProductEntity.Title))]
	public string? Title { get; set; }

	[UFilterContains(nameof(ProductEntity.Code))]
	public string? Code { get; set; }

	[UFilterGreaterThanOrEqual(nameof(ProductEntity.Stock))]
	public int? MinStock { get; set; }

	[UFilterLessThanOrEqual(nameof(ProductEntity.Stock))]
	public int? MaxStock { get; set; }

	[UFilterGreaterThanOrEqual(nameof(ProductEntity.Price))]
	public double? MinPrice { get; set; }

	[UFilterLessThanOrEqual(nameof(ProductEntity.Price))]
	public double? MaxPrice { get; set; }

	[UFilterEqual(nameof(ProductEntity.ParentId))]
	public Guid? ParentId { get; set; }

	[UFilterEqual(nameof(ProductEntity.UserId))]
	public Guid? UserId { get; set; }

	public bool ShowCategories { get; set; } = false;
	public bool ShowCategoriesMedia { get; set; } = false;
	public bool ShowMedia { get; set; } = false;
	public bool ShowUser { get; set; } = false;
	public bool ShowUserMedia { get; set; } = false;
	public bool ShowUserCategory { get; set; } = false;
	public bool ShowChildren { get; set; } = false;
	public bool ShowChildrenDepth { get; set; } = false;
}