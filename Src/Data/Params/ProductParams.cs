namespace SinaMN75U.Data.Params;

public class ProductCreateParams : BaseCreateParams<TagProduct> {
	[UValidationRequired("TitleRequired")]
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
	public int? Point { get; set; }
	public double? Price { get; set; }

	public string? Details { get; set; }
	
	public IEnumerable<Guid>? Categories { get; set; }
	public IEnumerable<Guid>? RelatedProducts { get; set; }
	public IEnumerable<ProductCreateParams> Children { get; set; } = [];

	public Guid? ParentId { get; set; }
	public Guid? UserId { get; set; }
}

public class ProductUpdateParams : BaseUpdateParams<TagProduct> {
	public string? Title { get; set; }
	public string? Code { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public string? Slug { get; set; }
	public string? Type { get; set; }
	public string? Content { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public int? Stock { get; set; }
	public int? Point { get; set; }
	public double? Price { get; set; }
	public Guid? ParentId { get; set; }
	public Guid? UserId { get; set; }
	public string? ActionType { get; set; }
	public string? ActionTitle { get; set; }
	public string? ActionUri { get; set; }
	public string? Details { get; set; }
	public ICollection<Guid>? RelatedProducts { get; set; }
	public ICollection<Guid>? AddRelatedProducts { get; set; }
	public ICollection<Guid>? RemoveRelatedProducts { get; set; }
	public ICollection<Guid>? AddCategories { get; set; }
	public ICollection<Guid>? RemoveCategories { get; set; }
}

public class ProductReadParams : BaseReadParams<TagProduct> {
	public string? Query { get; set; }
	public string? Title { get; set; }
	public string? Code { get; set; }
	public string? Slug { get; set; }
	public Guid? ParentId { get; set; }
	public Guid? UserId { get; set; }
	public int? MinStock { get; set; }
	public int? MaxStock { get; set; }
	public double? MinPrice { get; set; }
	public double? MaxPrice { get; set; }
	public bool ShowCategories { get; set; } = false;
	public bool ShowCategoriesMedia { get; set; } = false;
	public bool ShowMedia { get; set; } = false;
	public bool ShowUser { get; set; } = false;
	public bool ShowUserMedia { get; set; } = false;
	public bool ShowUserCategory { get; set; } = false;
	public bool ShowChildren { get; set; } = false;
	public bool ShowChildrenDepth { get; set; } = false;
	public bool ShowCommentCount { get; set; }
	public bool ShowIsFollowing { get; set; }
	public bool ShowChildrenCount { get; set; }
	public IEnumerable<Guid>? Ids { get; set; }
}