namespace SinaMN75U.Data.Entities;

[Table("Tickets")]
public class TicketEntity : BaseEntity<TagTicket, TicketJson> {

	public UserEntity User { get; set; } = null!;
	public required Guid UserId { get; set; }

	public ICollection<MediaEntity> Media { get; set; } = [];
	
	public new TicketResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		DeletedAt = DeletedAt,
		JsonData = JsonData,
		Tags = Tags,
		UserId = UserId
	};

}

public class TicketJson {
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? Whatsapp { get; set; }
	public string? Phone { get; set; }
}