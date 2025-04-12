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

	public CategoryEntity MapToEntity(bool media = false) => new() {
		Id = Id,
		Title = Title,
		JsonDetail = JsonDetail,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		Tags = Tags,
		Children = Children?.Select(x => x.MapToEntity(media)),
		Media = media ? Media?.Select(x => x.MapToEntity()) : null,
		ParentId = ParentId
	};
}

public class CategoryJsonDetail {
	public string? Subtitle { get; set; }
}

public static class CategoryEntityExtension {
	public static CategoryResponse MapToResponse(this CategoryEntity x, bool showMedia = false, bool showChildren = false) => new() {
		Id = x.Id,
		Title = x.Title,
		Subtitle = x.JsonDetail.Subtitle,
		Tags = x.Tags,
		Children = showChildren || showMedia
			? x.Children!.Select(c => new CategoryResponse {
				Id = c.Id,
				Title = c.Title,
				Subtitle = c.JsonDetail.Subtitle,
				Tags = c.Tags,
				Media = showMedia ? c.Media!.Select(m => m.MapToResponse()) : null
			})
			: null,
		Media = showMedia ? x.Media!.Select(m => m.MapToResponse()) : null
	};

	public static IQueryable<CategoryResponse> ToResponse(this IQueryable<CategoryEntity> query, bool media, bool children) => query.Select(x => new CategoryResponse {
			Id = x.Id,
			Title = x.Title,
			Subtitle = x.JsonDetail.Subtitle,
			Tags = x.Tags,
			Children = children
				? x.Children!.Select(c => new CategoryResponse {
					Id = x.Id,
					Title = x.Title,
					Subtitle = x.JsonDetail.Subtitle,
					Tags = x.Tags,
					Media = media
						? x.Media!.Select(m => new MediaResponse {
							Path = m.Path,
							Id = m.Id,
							Tags = m.Tags
						})
						: null
				})
				: null,
			Media = media
				? x.Media!.Select(m => new MediaResponse {
					Path = m.Path,
					Id = m.Id,
					Tags = m.Tags
				})
				: null
		}
	);
}