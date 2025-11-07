namespace SinaMN75U.Data.Params;

public class ContentCreateParams : BaseParams {
	[UValidationRequired("TitleRequired")]
	public required string Title { get; set; }

	[UValidationRequired("DescriptionRequired")]
	public required string Description { get; set; }

	[UValidationRequired("SubtitleRequired")]
	public required string SubTitle { get; set; }

	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? Whatsapp { get; set; }
	public string? Phone { get; set; }

	[UValidationMinCollectionLength(1, "TagsRequired")]
	public required List<TagContent> Tags { get; set; }
}

public class ContentUpdateParams : BaseUpdateParams<TagContent> {
	public string? Title { get; set; }
	public string? SubTitle { get; set; }
	public string? Description { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? Whatsapp { get; set; }
	public string? Phone { get; set; }
}

public class ContentReadParams : BaseReadParams<int> {
	public bool ShowMedia { get; set; }
}