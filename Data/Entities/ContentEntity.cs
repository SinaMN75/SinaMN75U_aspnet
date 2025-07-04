namespace SinaMN75U.Data.Entities;

[Table("Contents")]
public class ContentEntity : BaseEntity<int, ContentJson> {
	public IEnumerable<MediaEntity>? Media { get; set; }

	public ContentEntity MapToResponse(bool showMedia = false) => new() {
		JsonData = JsonData,
		Tags = Tags,
		Media = showMedia ? Media : null,
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