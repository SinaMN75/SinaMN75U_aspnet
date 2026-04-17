namespace SinaMN75U.Data.Entities;

[Table("Products")]
[Index(nameof(Slug), IsUnique = true, Name = "IX_Products_Slug")]
[Index(nameof(Code), IsUnique = true, Name = "IX_Products_Code")]
[Index(nameof(CreatorId))]
public sealed class ProductEntity : BaseEntity<TagProduct, ProductJson> {
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
	
	[Column(TypeName = "decimal(18,2)")]
	public decimal? Deposit { get; set; }

	[Column(TypeName = "decimal(18,2)")]
	public decimal? Rent { get; set; }

	public int Stock { get; set; }
	public int Point { get; set; }
	public int Order { get; set; }

	public Guid? ParentId { get; set; }
	public ProductEntity? Parent { get; set; }

	[InverseProperty("Parent")]
	public ICollection<ProductEntity> Children { get; set; } = [];

	public ICollection<MediaEntity> Media { get; set; } = [];
	public ICollection<CategoryEntity> Categories { get; set; } = [];
	public ICollection<CommentEntity> Comments { get; set; } = [];
	public ICollection<FollowEntity> Followers { get; set; } = [];
	public ICollection<ContractEntity> Contracts { get; set; } = [];
}

public sealed class ProductJson : BaseJsonData {
	public string? ActionType { get; set; }
	public string? ActionTitle { get; set; }
	public string? ActionUri { get; set; }
	public string? Details { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Address { get; set; }
	public ICollection<Guid> RelatedProducts { get; set; } = [];
}