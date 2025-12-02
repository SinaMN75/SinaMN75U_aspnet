namespace SinaMN75U.Data.Params;

public sealed class ContentCreateParams : BaseParams {
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
	
	public ContentEntity MapToEntity() => new() {
		JsonData = new ContentJson {
			Title = Title,
			Description = Description,
			SubTitle = SubTitle,
			Instagram = Instagram,
			Telegram = Telegram,
			Whatsapp = Whatsapp,
			Phone = Phone
		},
		Tags = Tags
	};

}

public sealed class ContentUpdateParams : BaseUpdateParams<TagContent> {
	public string? Title { get; set; }
	public string? SubTitle { get; set; }
	public string? Description { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? Whatsapp { get; set; }
	public string? Phone { get; set; }
	
	public void MapToEntity(ContentEntity e) {
		if (Title != null) e.JsonData.Title = Title;
		if (SubTitle != null) e.JsonData.SubTitle = SubTitle;
		if (Description != null) e.JsonData.Description = Description;
		if (Instagram != null) e.JsonData.Instagram = Instagram;
		if (Telegram != null) e.JsonData.Telegram = Telegram;
		if (Whatsapp != null) e.JsonData.Whatsapp = Whatsapp;
		if (Phone != null) e.JsonData.Phone = Phone;
		if (Tags != null) e.Tags = Tags;
	}

}

public sealed class ContentReadParams : BaseReadParams<int> {
	public bool ShowMedia { get; set; }
}