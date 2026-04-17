namespace SinaMN75U.Data.Entities;

[Table("Tickets")]
public sealed class TicketEntity : BaseEntity<TagTicket, TicketJson> {
	public ICollection<MediaEntity> Media { get; set; } = [];
}

public sealed class TicketJson : BaseJsonData {
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? Whatsapp { get; set; }
	public string? Phone { get; set; }
}