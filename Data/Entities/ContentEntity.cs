namespace SinaMN75U.Data.Entities;

[Table("Contents")]
public class ContentEntity : BaseEntity<int, ContentJson> {
	public IEnumerable<MediaEntity>? Media { get; set; }

	public ContentResponse MapToResponse(bool showMedia = false) => new() {
		JsonData = JsonData,
		Tags = Tags,
		Media = showMedia ? Media?.Select(x => x.MapToResponse()) : null,
		Id = Id,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt
	};
}

public class ContentJson {
	public string? Title { get; set; }
	public string? SubTitle { get; set; }
	public string? Description { get; set; }
	public string? Instagram { get; set; }
}