namespace SinaMN75U.Data.Entities;

[Table("Categories")]
public class CategoryEntity : BaseEntity<TagCategory> {
	[Required]
	[MaxLength(100)]
	public required string Title { get; set; }

	[Required]
	public required CategoryJsonDetail JsonDetail { get; set; }

	public Guid? ParentId { get; set; }
	public CategoryEntity? Parent { get; set; }

	[InverseProperty("Parent")]
	public IEnumerable<CategoryEntity>? Children { get; set; }

	public IEnumerable<ProductEntity>? Products { get; set; }
	
	public IEnumerable<UserEntity>? Users { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }

	public CategoryResponse MapToResponse(bool media = false) => new() {
		Id = Id,
		Title = Title,
		Subtitle = JsonDetail.Subtitle,
		Tags = Tags,
		Children = Children?.Select(x => x.MapToResponse()),
		Media = media ? Media?.Select(x => x.MapToResponse()) : null,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt
	};

	public CategoryEntity MapToEntity(bool media = false) => new() {
		Id = Id,
		Title = Title,
		JsonDetail = JsonDetail,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		Tags = Tags,
		Children = Children?.Select(x => x.MapToEntity(media)),
		Media = media ? Media?.Select(x => x.MapToEntity()) : null,
		ParentId = ParentId,
	};
}

public class CategoryJsonDetail {
	public string? Subtitle { get; set; }
}