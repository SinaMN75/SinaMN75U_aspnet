namespace SinaMN75U.Data.Entities;

[Table("Contents")]
public sealed class ContentEntity : BaseEntity<TagContent, ContentJson> {
	public ICollection<MediaEntity> Media { get; set; } = [];
}

public sealed class ContentJson : BaseJson {
	public string? Title { get; set; }
	public string? SubTitle { get; set; }
	public string? Description { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? Whatsapp { get; set; }
	public string? Phone { get; set; }
	public List<ContentExtra> Extra { get; set; } = [];
}

public sealed class ContentExtra {
	public required string Title { get; set; }
	public required string Subtitle { get; set; }
	public required string Description { get; set; }
	public string? Icon1 { get; set; }
	public string? Icon2 { get; set; }
	public string? Icon3 { get; set; }
}