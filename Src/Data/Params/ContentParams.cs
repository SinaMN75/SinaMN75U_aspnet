namespace SinaMN75U.Data.Params;

public sealed class ContentCreateParams : BaseCreateParams<TagContent> {
	public string Title { get; set; } = null!;
	public string? Description { get; set; }
	public string? SubTitle { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? Whatsapp { get; set; }
	public string? Phone { get; set; }
}

public sealed class ContentUpdateParams : BaseUpdateParams<TagContent> {
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string? SubTitle { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? Whatsapp { get; set; }
	public string? Phone { get; set; }
}

public sealed class ContentReadParams : BaseReadParams<TagContent> {
	public ContentSelectorArgs SelectorArgs { get; set; } = new();
}