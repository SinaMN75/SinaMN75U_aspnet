namespace SinaMN75U.Data.Params;

public sealed class TicketCreateParams : BaseParams {
	[UValidationRequired("TitleRequired")]
	public string Title { get; set; } = null!;

	[UValidationRequired("DescriptionRequired")]
	public string Description { get; set; } = null!;
	
	[UValidationMinCollectionLength(1, "TagsRequired")]
	public List<TagTicket> Tags { get; set; } = null!;

	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? Whatsapp { get; set; }
	public string? Phone { get; set; }
}

public sealed class TicketUpdateParams : BaseUpdateParams<TagTicket> {
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? Whatsapp { get; set; }
	public string? Phone { get; set; }
}

public sealed class TicketReadParams : BaseReadParams<TagTicket> {
	public TicketSelectorArgs SelectorArgs { get; set; } = new();
}