namespace SinaMN75U.Data.Entities;

[Table("Contents")]
public class ContentEntity : BaseEntity<TagContent, ContentJson> {
	public ICollection<MediaEntity> Media { get; set; } = [];
	
	public ContentResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		DeletedAt = DeletedAt,
		JsonData = JsonData,
		Tags = Tags
	};

}

public class ContentJson {
	public string? Title { get; set; }
	public string? SubTitle { get; set; }
	public string? Description { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? Whatsapp { get; set; }
	public string? Phone { get; set; }
}