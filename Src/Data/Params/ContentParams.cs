namespace SinaMN75U.Data.Params;

public sealed class ContentCreateParams : BaseCreateParams<TagContent> {
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

	public ICollection<Guid>? Media { get; set; }
}

public sealed class ContentUpdateParams : BaseUpdateParams<TagContent> {
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

	public List<ContentLink>? Links { get; set; }
	public List<ContentItem>? Items { get; set; }

	public Guid? CreatorId { get; set; }
	public ICollection<Guid>? Media { get; set; }
}

public sealed class ContentReadParams : BaseReadParams<TagContent> {
	public string? Query { get; set; }
	public ContentSelectorArgs SelectorArgs { get; set; } = new();
}
