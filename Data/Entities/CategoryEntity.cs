namespace SinaMN75U.Data.Entities;

[Table("Categories")]
public class CategoryEntity : BaseEntity<TagCategory, CategoryJson> {
	[Required]
	[MaxLength(100)]
	public required string Title { get; set; }

	public Guid? ParentId { get; set; }
	public CategoryEntity? Parent { get; set; }

	[InverseProperty("Parent")]
	public IEnumerable<CategoryEntity>? Children { get; set; }

	public IEnumerable<ProductEntity>? Products { get; set; }

	public IEnumerable<UserEntity>? Users { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }

	public CategoryEntity MapToEntity(bool media = false) => new() {
		Id = Id,
		Title = Title,
		Json = Json,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		Tags = Tags,
		Children = Children?.Select(x => x.MapToEntity(media)),
		Media = media ? Media?.Select(x => x.MapToEntity()) : null,
		ParentId = ParentId
	};
}

public class CategoryJson {
	public string? Subtitle { get; set; }
}

public static class CategoryEntityExtension {
	public static CategoryResponse MapToResponse(this CategoryEntity x, bool showMedia = false, bool showChildren = false) => new() {
		Id = x.Id,
		Title = x.Title,
		Tags = x.Tags,
		Json = x.Json,
		Children = showChildren || showMedia
			? x.Children!.Select(c => new CategoryResponse {
				Id = c.Id,
				Title = c.Title,
				Json = c.Json,
				Tags = c.Tags,
				Media = showMedia ? c.Media!.Select(m => m.MapToResponse()) : null
			})
			: null,
		Media = showMedia ? x.Media!.Select(m => m.MapToResponse()) : null
	};
}