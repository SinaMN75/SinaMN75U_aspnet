namespace SinaMN75U.Data.Params;

public sealed class ContentCreateParams : BaseCreateParams<TagContent> {
	public string? SubTitle { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? Whatsapp { get; set; }
	public string? Phone { get; set; }
}

public sealed class ContentUpdateParams : BaseUpdateParams<TagContent> {
	public string? SubTitle { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? Whatsapp { get; set; }
	public string? Phone { get; set; }
}

public sealed class ContentReadParams : BaseReadParams<int> {
	public ContentSelectorArgs SelectorArgs { get; set; } = new();
}