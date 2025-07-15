namespace SinaMN75U.Data.Entities;

[Table("Products")]
public class ProductEntity : BaseEntity<TagProduct, ProductJson> {
	[Required]
	[MaxLength(100)]
	public required string Title { get; set; }

	[MaxLength(100)]
	public string? Code { get; set; }

	[MaxLength(100)]
	public string? Subtitle { get; set; }

	[MaxLength(2000)]
	public string? Description { get; set; }

	[MaxLength(100)]
	public string? Slug { get; set; }

	[MaxLength(100)]
	public string? Type { get; set; }
	
	public string? Content { get; set; }

	public double? Latitude { get; set; }
	public double? Longitude { get; set; }

	public int? Stock { get; set; }
	public double? Price { get; set; }

	public ProductEntity? Parent { get; set; }
	public Guid? ParentId { get; set; }

	[Required]
	public Guid UserId { get; set; }

	public UserEntity User { get; set; } = null!;

	[InverseProperty("Parent")]
	public ICollection<ProductEntity> Children { get; set; } = [];

	public ICollection<MediaEntity> Media { get; set; } = [];
	public ICollection<CategoryEntity> Categories { get; set; } = [];
}

public class ProductJson {
	public string? ActionType { get; set; }
	public string? ActionTitle { get; set; }
	public string? ActionUri { get; set; }
	public string? Details { get; set; }
	public ICollection<VisitCount> VisitCounts { get; set; } = [];
	public ICollection<Guid> RelatedProducts { get; set; } = [];
}

public class VisitCount {
	public required Guid UserId { get; set; }
	public required int Count { get; set; } = 1;
}