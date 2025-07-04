namespace SinaMN75U.Data.Entities;

[Table("Products")]
public class ProductEntity : BaseEntity<TagProduct, ProductJson> {
	[Required]
	[MaxLength(100)]
	public required string Title { get; set; }

	[Required]
	[MaxLength(10)]
	public required string Code { get; set; }

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
	public required Guid UserId { get; set; }

	public UserEntity? User { get; set; }

	[InverseProperty("Parent")]
	public IEnumerable<ProductEntity>? Children { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }
	public List<CategoryEntity>? Categories { get; set; }

	public ProductResponse MapToResponse() => new() {
		Id = Id,
		Title = Title,
		Code = Code,
		Subtitle = Subtitle,
		Description = Description,
		Latitude = Latitude,
		Longitude = Longitude,
		Stock = Stock,
		Price = Price,
		ParentId = ParentId,
		Tags = Tags,
		JsonData = JsonData,
		Children = Children?.Select(x => x.MapToResponse()),
		Media = Media?.Select(x => x.MapToResponse()),
		Categories = Categories?.Select(x => x.MapToResponse()),
		User = User?.MapToResponse(),
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		Content = Content,
		Type = Type,
		Slug = Slug
	};
}

public class ProductJson {
	public string? ActionType { get; set; }
	public string? ActionTitle { get; set; }
	public string? ActionUri { get; set; }
	public string? Details { get; set; }
	public List<VisitCount> VisitCounts { get; set; } = [];
	public List<Guid> RelatedProducts { get; set; } = [];
}

public class VisitCount {
	public required Guid UserId { get; set; }
	public required int Count { get; set; } = 1;
}