namespace SinaMN75U.Data.Entities;

[Table("Categories")]
public class CategoryEntity : BaseEntity<TagCategory, CategoryJson> {
	[Required]
	[MaxLength(100)]
	public required string Title { get; set; }

	public Guid? ParentId { get; set; }
	public CategoryEntity? Parent { get; set; }

	public int? Order { get; set; }

	public string? Code { get; set; }

	[InverseProperty("Parent")]
	public ICollection<CategoryEntity> Children { get; set; } = [];

	[JsonIgnore]
	public ICollection<ProductEntity> Products { get; set; } = [];

	[JsonIgnore]
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

public static class CategoryProjections {
	public static Expression<Func<MediaEntity, MediaResponse>> MediaSelector =
		m => new MediaResponse {
			Tags = m.Tags,
			JsonData = m.JsonData,
			Path = m.Path
		};

	// This selector dynamically respects ShowMedia / ShowChildren / ShowChildrenMedia
	public static Expression<Func<CategoryEntity, CategoryResponse>> CategorySelector(CategoryReadParams p) {
		return x => new CategoryResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			DeletedAt = x.DeletedAt,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Title = x.Title,
			Order = x.Order,
			Code = x.Code,
			ParentId = x.ParentId,

			Media = p.ShowMedia
				? x.Media.AsQueryable().Select(MediaSelector).ToList()
				: new List<MediaResponse>(),

			Children = p.ShowChildren
				? x.Children
					.AsQueryable()
					.OrderBy(c => c.Order)
					.Select(c => new CategoryResponse {
						Id = c.Id,
						CreatedAt = c.CreatedAt,
						UpdatedAt = c.UpdatedAt,
						DeletedAt = c.DeletedAt,
						Tags = c.Tags,
						JsonData = c.JsonData,
						Title = c.Title,
						Order = c.Order,
						Code = c.Code,
						ParentId = c.ParentId,

						Media = p.ShowChildrenMedia
							? c.Media.AsQueryable().Select(MediaSelector).ToList()
							: new List<MediaResponse>(),

						Children = new List<CategoryResponse>() // no deep recursion unless you ask
					})
					.ToList()
				: new List<CategoryResponse>()
		};
	}
}