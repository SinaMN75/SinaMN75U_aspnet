namespace SinaMN75U.Data.Entities;

[Table("Contents")]
public class ContentEntity : BaseEntity<int> {
	[Required]
	public required ContentJsonDetail JsonDetail { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }

	public ContentResponse MapToResponse(bool showMedia = false) => new() {
		Title = JsonDetail.Title,
		SubTitle = JsonDetail.SubTitle,
		Description = JsonDetail.Description,
		Tags = Tags,
		Instagram = JsonDetail.Instagram,
		Media = showMedia ? Media?.Select(x => x.MapToResponse()) : null,
		Id = Id,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt
	};
}

public class ContentJsonDetail {
	public string? Title { get; set; }
	public string? SubTitle { get; set; }
	public string? Description { get; set; }
	public string? Instagram { get; set; }
}