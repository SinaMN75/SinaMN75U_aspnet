using SinaMN75U.Data.Responses.UserManagement;
using System.ComponentModel.DataAnnotations.Schema;

namespace SinaMN75U.Data.Entities;

[Table("Categories")]
public class CategoryEntity : BaseEntity {
	[Required]
	[MaxLength(100)]
	public required string Title { get; set; }

	[MaxLength(100)]
	public string? TitleTr1 { get; set; }

	[MaxLength(100)]
	public string? TitleTr2 { get; set; }

	[Required]
	public required CategoryJsonDetail JsonDetail { get; set; }
	
	[Required]
	public required List<TagCategory> Tags { get; set; }

	public Guid? ParentId { get; set; }
	public CategoryEntity? Parent { get; set; }

	[InverseProperty("Parent")]
	public IEnumerable<CategoryEntity>? Children { get; set; }

	public IEnumerable<UserEntity>? Users { get; set; }
}

public class CategoryJsonDetail {
	public string? Subtitle { get; set; }
}

public static class CategoryExtensions {
	public static CategoryResponse MapToResponse(this CategoryEntity e) {
		return new CategoryResponse {
			Title = e.Title,
			TitleTr1 = e.TitleTr1,
			TitleTr2 = e.TitleTr2,
			Subtitle = e.JsonDetail.Subtitle,
			Tags = e.Tags,
			Children = e.Children?.Select(MapToResponse).ToList(),
		};
	}
}