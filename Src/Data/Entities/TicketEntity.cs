namespace SinaMN75U.Data.Entities;

[Table("Tickets")]
public sealed class TicketEntity : BaseEntity<TagTicket, TicketJson> {
	public required Guid CreatorId { get; set; }
	public UserEntity Creator { get; set; } = null!;

	public ICollection<MediaEntity> Media { get; set; } = [];

	public TicketResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		DeletedAt = DeletedAt,
		JsonData = JsonData,
		Tags = Tags,
		CreatorId = CreatorId
	};
}

public sealed class TicketJson {
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? Whatsapp { get; set; }
	public string? Phone { get; set; }
}