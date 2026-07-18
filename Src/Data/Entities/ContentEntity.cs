namespace SinaMN75U.Data.Entities;

[Table("Contents")]
public sealed class ContentEntity : BaseEntity<TagContent, ContentJson> {
	public ICollection<MediaEntity> Media { get; set; } = [];
}

public sealed class ContentJson : BaseJson {
	public string? Title { get; set; }
	public string? SubTitle { get; set; }
	public string? Description { get; set; }

	public string? ImageBase64 { get; set; }
	public string? IconBase64 { get; set; }

	public string? ButtonText { get; set; }
	public string? ButtonLink { get; set; }
	public string? Link { get; set; }
	public int? Order { get; set; }

	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? Whatsapp { get; set; }
	public string? Phone { get; set; }

	public List<ContentLink> Links { get; set; } = [];
	public List<ContentItem> Items { get; set; } = [];
}

public sealed class ContentItem {
	public string? Title { get; set; }
	public string? SubTitle { get; set; }
	public string? Description { get; set; }
	public string? IconBase64 { get; set; }
	public string? ImageBase64 { get; set; }
	public string? Link { get; set; }
	public int? Order { get; set; }
}

public sealed class ContentLink {
	public string? Title { get; set; }
	public string? Url { get; set; }
	public string? IconBase64 { get; set; }
}
