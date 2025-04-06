namespace SinaMN75U.Data.Params;

public class ProductCreateParams: BaseParams {
	public required string Title { get; set; }
	public string? Code { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public int? Stock { get; set; }
	public double? Price { get; set; }
	
	public string? Details { get; set; }
	
	public required List<TagProduct> Tags { get; set; }

	public Guid? ParentId { get; set; }
	public Guid? UserId { get; set; }
}

public class ProductUpdateParams: BaseParams {
	public required Guid Id { get; set; }
	public required string Title { get; set; }
	public string? Code { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public int? Stock { get; set; }
	public double? Price { get; set; }
	
	public List<TagProduct>? AddTags { get; set; }
	public List<TagProduct>? RemoveTags { get; set; }

	public string? Details { get; set; }
	public List<Guid>? AddRelatedProducts { get; set; }
	public List<Guid>? RemoveRelatedProducts { get; set; }
	
	public Guid? ParentId { get; set; }
	public Guid? UserId { get; set; }
}

public class ProductReadParams: BaseFilterParams {
	public IEnumerable<Guid>? Ids { get; set; }
	public string? Query { get; set; }
	public string? Title { get; set; }
	public string? Code { get; set; }

	public Guid? ParentId { get; set; }
	public Guid? UserId { get; set; }
	
	public bool Categories { get; set; } = false;
	public bool Media { get; set; } = false;
	public bool User { get; set; } = false;
}