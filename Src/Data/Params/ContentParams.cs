namespace SinaMN75U.Data.Params;

public sealed class ContentCreateParams : BaseCreateParams<TagContent> {
	// Note: Detail1 / Detail2 are inherited from BaseCreateParams.
	public string Title { get; set; } = null!;
	public string? SubTitle { get; set; }
	public string? Description { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? Whatsapp { get; set; }
	public string? Phone { get; set; }
	public List<ContentExtra> Extra { get; set; } = [];
}

public sealed class ContentUpdateParams : BaseUpdateParams<TagContent> {
	// Note: Detail1 / Detail2 are inherited from BaseUpdateParams and applied via ApplyUpdateParam.
	public string? Title { get; set; }
	public string? SubTitle { get; set; }
	public string? Description { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? Whatsapp { get; set; }
	public string? Phone { get; set; }
	public List<ContentExtra>? Extra { get; set; }
}

public sealed class ContentReadParams : BaseReadParams<TagContent> {
	public ContentSelectorArgs SelectorArgs { get; set; } = new();
}
