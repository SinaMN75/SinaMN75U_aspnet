namespace SinaMN75U.Data.Entities;

[Table("Contents")]
public class ContentEntity : BaseEntity<int> {
	[Required]
	[MaxLength(100)]
	public required string Title { get; set; }

	[Required]
	[MaxLength(100)]
	public required string SubTitle { get; set; }

	[Required]
	[MaxLength(5000)]
	public required string Description { get; set; }

	[Required]
	public required ContentJsonDetail JsonDetail { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }

	public ContentResponse MapToResponse(bool showMedia = false) => new() {
		Title = Title,
		SubTitle = SubTitle,
		Description = Description,
		Tags = Tags,
		Instagram = JsonDetail.Instagram,
		Media = showMedia ? Media?.Select(x => x.MapToResponse()) : null,
		Id = Id,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
	};
}

public class ContentJsonDetail {
	public string? Instagram { get; set; }
}