namespace SinaMN75U.Data.Entities;

[Table("Categories")]
public class CategoryEntity : BaseEntity<TagCategory, CategoryJson> {
	[MaxLength(100)]
	public required string Title { get; set; }

	public int? Order { get; set; }
	
	[MaxLength(100)]
	public string? Code { get; set; }
	
	public Guid? ParentId { get; set; }
	public CategoryEntity? Parent { get; set; }

	[InverseProperty("Parent")]
	public ICollection<CategoryEntity> Children { get; set; } = [];

	public ICollection<ProductEntity> Products { get; set; } = [];
	public ICollection<UserEntity> Users { get; set; } = [];
	public ICollection<MediaEntity> Media { get; set; } = [];

	public new CategoryResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		DeletedAt = DeletedAt,
		JsonData = JsonData,
		Tags = Tags,
		Title = Title,
		Order = Order,
		Code = Code,
		ParentId = ParentId
	};
}

public class CategoryJson {
	public string? Subtitle { get; set; }
	public string? Link { get; set; }
	public string? Location { get; set; }
	public string? Type { get; set; }
	public string? Address { get; set; }
	public string? PhoneNumber { get; set; }
	public ICollection<Guid> RelatedProducts { get; set; } = [];
}