namespace SinaMN75U.Data.Params;

public sealed class TicketCreateParams : BaseParams {
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
}

public sealed class TicketUpdateParams : BaseUpdateParams<TagTicket> {
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? Whatsapp { get; set; }
	public string? Phone { get; set; }
}

public sealed class TicketReadParams : BaseReadParams<int> {
	public TicketSelectorArgs SelectorArgs { get; set; } = new();
}