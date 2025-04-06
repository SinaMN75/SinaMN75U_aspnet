namespace SinaMN75U.Data.Entities;

[Table("Products")]
public class ProductEntity : BaseEntity<TagProduct> {
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

	public ProductJsonDetail JsonDetail { get; set; } = new();

	[Required]
	public required Guid UserId { get; set; }

	public UserEntity? User { get; set; }

	[InverseProperty("Parent")]
	public IEnumerable<ProductEntity>? Children { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }
	public IEnumerable<CategoryEntity>? Categories { get; set; }

	public ProductResponse MapToResponse(
		bool media = false,
		bool categories = false,
		bool children = false,
		bool user = false
	) => new() {
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
		Details = JsonDetail.Details,
		VisitCounts = JsonDetail.VisitCounts,
		RelatedProducts = JsonDetail.RelatedProducts,
		Children = Children?.Select(x => x.MapToResponse()),
		Media = Media?.Select(x => x.MapToResponse()),
		Categories = Categories?.Select(x => x.MapToResponse()),
		User = User!.MapToResponse(),
	};
}

public class ProductJsonDetail {
	public string? Details { get; set; }
	public List<VisitCount> VisitCounts { get; set; } = [];
	public List<Guid> RelatedProducts { get; set; } = [];
}

public class VisitCount {
	public required Guid UserId { get; set; }
	public required int Count { get; set; } = 1;
}