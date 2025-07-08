namespace SinaMN75U.Data.Entities;

[Table("Categories")]
public class CategoryEntity : BaseEntity<TagCategory, CategoryJson> {
	[Required]
	[MaxLength(100)]
	public required string Title { get; set; }

	public Guid? ParentId { get; set; }
	public CategoryEntity? Parent { get; set; }

	public int? Order { get; set; }

	[InverseProperty("Parent")]
	public IEnumerable<CategoryEntity>? Children { get; set; }

	[JsonIgnore]
	public IEnumerable<ProductEntity>? Products { get; set; }

	[JsonIgnore]
	public IEnumerable<UserEntity>? Users { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }
}

public class CategoryJson {
	public string? Subtitle { get; set; }
	public string? Link { get; set; }
	public string? Location { get; set; }
	public string? Type { get; set; }
	public List<Guid> RelatedProducts { get; set; } = [];
}