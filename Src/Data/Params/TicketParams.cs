namespace SinaMN75U.Data.Params;

public sealed class TicketCreateParams : BaseParams {
	public Guid? UserId { get; set; }
	
	[UValidationRequired("TitleRequired")]
	public required string Title { get; set; }

	[UValidationRequired("DescriptionRequired")]
	public required string Description { get; set; }

	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? Whatsapp { get; set; }
	public string? Phone { get; set; }

	[UValidationMinCollectionLength(1, "TagsRequired")]
	public required List<TagTicket> Tags { get; set; }
	
	public TicketEntity MapToEntity(Guid userId) => new() {
		UserId = UserId ?? userId,
		JsonData = new TicketJson {
			Title = Title,
			Description = Description,
			Instagram = Instagram,
			Telegram = Telegram,
			Whatsapp = Whatsapp,
			Phone = Phone
		},
		Tags = Tags
	};

}

public sealed class TicketUpdateParams : BaseUpdateParams<TagTicket> {
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? Whatsapp { get; set; }
	public string? Phone { get; set; }
	
	public void MapToEntity(TicketEntity e) {
		if (Title != null) e.JsonData.Title = Title;
		if (Description != null) e.JsonData.Description = Description;
		if (Instagram != null) e.JsonData.Instagram = Instagram;
		if (Telegram != null) e.JsonData.Telegram = Telegram;
		if (Whatsapp != null) e.JsonData.Whatsapp = Whatsapp;
		if (Phone != null) e.JsonData.Phone = Phone;
		if (Tags != null) e.Tags = Tags;
	}

}

public sealed class TicketReadParams : BaseReadParams<int> {
	public TicketSelectorArgs SelectorArgs { get; set; } = new();
}