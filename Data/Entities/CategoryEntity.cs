namespace SinaMN75U.Data.Entities;

[Table("Categories")]
public class CategoryEntity : BaseEntity<TagCategory, CategoryJson> {
	[Required]
	[MaxLength(100)]
	public required string Title { get; set; }

	public Guid? ParentId { get; set; }
	public CategoryEntity? Parent { get; set; }

	public int? Order { get; set; }
	public string? Location { get; set; }
	public string? Type { get; set; }

	[InverseProperty("Parent")]
	public IEnumerable<CategoryEntity>? Children { get; set; }

	public IEnumerable<ProductEntity>? Products { get; set; }

	public IEnumerable<UserEntity>? Users { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }
}

public class CategoryJson {
	public string? Subtitle { get; set; }
	public string? Link { get; set; }
}

public static class CategoryEntityExtension {
	public static CategoryResponse MapToResponse(this CategoryEntity x, bool showMedia = false) => new() {
		Id = x.Id,
		Title = x.Title,
		Tags = x.Tags,
		JsonData = x.JsonData,
		ParentId = x.ParentId,
		Location = x.Location,
		Type = x.Type,
		Order = x.Order,
		Children =
			x.Children?.Select(c => new CategoryResponse {
				Id = c.Id,
				Title = c.Title,
				JsonData = c.JsonData,
				Tags = c.Tags,
				ParentId = x.ParentId,
				Location = x.Location,
				Type = x.Type,
				Order = x.Order,
				Media = showMedia ? c.Media?.Select(m => m.MapToResponse()) : null
			}),
		Media = showMedia ? x.Media?.Select(m => m.MapToResponse()) : null
	};
}