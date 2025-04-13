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

	public ProductResponse MapToResponse(
		bool media = false,
		bool categories = false,
		bool user = false
	) => new() {
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
		Json = Json,
		Children = Children?.Select(x => x.MapToResponse()),
		Media = media
			? Media?.Select(x => x.MapToResponse())
			: null,
		Categories = categories
			? Categories?.Select(x => x.MapToResponse())
			: null,
		User = user
			? User!.MapToResponse()
			: null,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt
	};

	public ProductEntity MapToEntity(
		bool media = false,
		bool categories = false,
		bool user = false
	) => new() {
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
		UserId = UserId,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		Json = Json,
		Children = Children?.Select(x => x.MapToEntity()),
		Media = media
			? Media?.Select(x => x.MapToEntity())
			: null,
		Categories = categories
			? Categories?.Select(x => x.MapToEntity()).ToList()
			: null,
		User = user
			? User!.MapToEntity()
			: null
	};
}

public class ProductJson {
	public string? Details { get; set; }
	public List<VisitCount> VisitCounts { get; set; } = [];
	public List<Guid> RelatedProducts { get; set; } = [];
}

public class VisitCount {
	public required Guid UserId { get; set; }
	public required int Count { get; set; } = 1;
}