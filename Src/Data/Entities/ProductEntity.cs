namespace SinaMN75U.Data.Entities;

[Table("Products")]
[Index(nameof(Slug), IsUnique = true)]
[Index(nameof(Code), IsUnique = true)]
[Index(nameof(CreatorId))]
public class ProductEntity : BaseEntity<TagProduct, ProductJson> {
	[Required, MaxLength(100)]
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

	public decimal? Latitude { get; set; }
	public decimal? Longitude { get; set; }
	public decimal? Deposit { get; set; }
	public decimal? Rent { get; set; }

	public int Stock { get; set; }
	public int Point { get; set; }
	public int Order { get; set; }

	public ProductEntity? Parent { get; set; }
	public Guid? ParentId { get; set; }

	[Required]
	public Guid CreatorId { get; set; }

	public UserEntity Creator { get; set; } = null!;

	[InverseProperty("Parent")]
	public ICollection<ProductEntity> Children { get; set; } = [];

	public ICollection<MediaEntity> Media { get; set; } = [];
	public ICollection<CategoryEntity> Categories { get; set; } = [];
	public ICollection<CommentEntity> Comments { get; set; } = [];
	public ICollection<FollowEntity> Followers { get; set; } = [];
	public ICollection<ContractEntity> Contracts { get; set; } = [];

	public new ProductResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		DeletedAt = DeletedAt,
		JsonData = JsonData,
		Tags = Tags,
		Title = Title,
		Code = Code,
		Subtitle = Subtitle,
		Description = Description,
		Slug = Slug,
		Type = Type,
		Content = Content,
		Latitude = Latitude,
		Longitude = Longitude,
		Deposit = Deposit,
		Rent = Rent,
		Stock = Stock,
		Point = Point,
		Order = Order,
		ParentId = ParentId,
		CreatorId = CreatorId,
	};
}

public class ProductJson {
	public string? ActionType { get; set; }
	public string? ActionTitle { get; set; }
	public string? ActionUri { get; set; }
	public string? Details { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Address { get; set; }
	public ICollection<VisitCount> VisitCounts { get; set; } = [];
	public ICollection<PointCount> PointCounts { get; set; } = [];
	public ICollection<Guid> RelatedProducts { get; set; } = [];
}

public class PointCount {
	public required string UserId { get; set; }
	public required int Point { get; set; }
}