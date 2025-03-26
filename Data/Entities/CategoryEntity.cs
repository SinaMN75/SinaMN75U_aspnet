namespace SinaMN75U.Data.Entities;

[Table("Categories")]
public class CategoryEntity : BaseEntity<TagCategory> {
	[Required]
	[MaxLength(100)]
	public required string Title { get; set; }

	[MaxLength(100)]
	public string? TitleTr1 { get; set; }

	[MaxLength(100)]
	public string? TitleTr2 { get; set; }

	[Required]
	public required CategoryJsonDetail JsonDetail { get; set; }

	public Guid? ParentId { get; set; }
	public CategoryEntity? Parent { get; set; }

	[InverseProperty("Parent")]
	public IEnumerable<CategoryEntity>? Children { get; set; }

	public IEnumerable<UserEntity>? Users { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }

	public CategoryResponse MapToResponse() => new() {
		Title = Title,
		TitleTr1 = TitleTr1,
		TitleTr2 = TitleTr2,
		Subtitle = JsonDetail.Subtitle,
		Tags = Tags,
		Children = Children?.Select(x => x.MapToResponse()).ToList()
	};
}

public class CategoryJsonDetail {
	public string? Subtitle { get; set; }
}