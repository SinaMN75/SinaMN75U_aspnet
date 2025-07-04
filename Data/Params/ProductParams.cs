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
	[UAssignIfNotNull(nameof(ProductEntity.Title))]
	public string? Title { get; set; }

	[UAssignIfNotNull(nameof(ProductEntity.Code))]
	public string? Code { get; set; }

	[UAssignIfNotNull(nameof(ProductEntity.Subtitle))]
	public string? Subtitle { get; set; }

	[UAssignIfNotNull(nameof(ProductEntity.Description))]
	public string? Description { get; set; }

	[UAssignNested(nameof(ProductEntity.JsonData), nameof(ProductJson.ActionType))]
	public string? ActionType { get; set; }

	[UAssignNested(nameof(ProductEntity.JsonData), nameof(ProductJson.ActionTitle))]
	public string? ActionTitle { get; set; }

	[UAssignNested(nameof(ProductEntity.JsonData), nameof(ProductJson.ActionUri))]
	public string? ActionUri { get; set; }

	[UAssignIfNotNull(nameof(ProductEntity.Slug))]
	public string? Slug { get; set; }

	[UAssignIfNotNull(nameof(ProductEntity.Type))]
	public string? Type { get; set; }

	[UAssignIfNotNull(nameof(ProductEntity.Content))]
	public string? Content { get; set; }

	[UAssignIfNotNull(nameof(ProductEntity.Latitude))]
	public double? Latitude { get; set; }

	[UAssignIfNotNull(nameof(ProductEntity.Longitude))]
	public double? Longitude { get; set; }

	[UAssignIfNotNull(nameof(ProductEntity.Stock))]
	public int? Stock { get; set; }

	[UAssignIfNotNull(nameof(ProductEntity.Price))]
	public double? Price { get; set; }

	[UAssignNested(nameof(ProductEntity.JsonData), nameof(ProductJson.Details))]
	public string? Details { get; set; }

	public IEnumerable<Guid>? AddRelatedProducts { get; set; }
	public IEnumerable<Guid>? RemoveRelatedProducts { get; set; }

	public IEnumerable<Guid>? AddCategories { get; set; }
	public IEnumerable<Guid>? RemoveCategories { get; set; }

	[UAssignIfNotNull(nameof(ProductEntity.ParentId))]
	public Guid? ParentId { get; set; }

	[UAssignIfNotNull(nameof(ProductEntity.UserId))]
	public Guid? UserId { get; set; }
}

public class ProductReadParams : BaseReadParams<TagProduct> {
	public IEnumerable<Guid>? Ids { get; set; }
	public string? Query { get; set; }
	public string? Title { get; set; }
	public string? Code { get; set; }

	public int? MinStock { get; set; }
	public int? MaxStock { get; set; }
	public double? MinPrice { get; set; }
	public double? MaxPrice { get; set; }

	public Guid? ParentId { get; set; }
	public Guid? UserId { get; set; }

	public bool ShowCategories { get; set; } = false;
	public bool ShowMedia { get; set; } = false;
	public bool ShowChildren { get; set; } = false;
	public bool ShowUser { get; set; } = false;
}