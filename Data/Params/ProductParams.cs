namespace SinaMN75U.Data.Params;

public class ProductCreateParams : BaseParams {
	[URequired("TitleRequired")]
	public required string Title { get; set; }

	public string? Code { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public int? Stock { get; set; }
	public double? Price { get; set; }

	public string? Details { get; set; }

	[UMinCollectionLength(1, "TagsRequired")]
	public required List<TagProduct> Tags { get; set; }

	public Guid? ParentId { get; set; }
	public Guid? UserId { get; set; }
}

public class ProductUpdateParams : BaseParams {
	[URequired("IdRequired")]
	public required Guid Id { get; set; }

	public required string Title { get; set; }
	public string? Code { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public int? Stock { get; set; }
	public double? Price { get; set; }

	public string? Details { get; set; }

	public List<TagProduct>? AddTags { get; set; }
	public List<TagProduct>? RemoveTags { get; set; }

	public List<Guid>? AddRelatedProducts { get; set; }
	public List<Guid>? RemoveRelatedProducts { get; set; }

	public Guid? ParentId { get; set; }
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