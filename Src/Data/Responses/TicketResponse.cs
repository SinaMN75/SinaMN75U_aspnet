namespace SinaMN75U.Data.Responses;

public class TicketResponse : BaseResponse<TagTicket, TicketJson> {
	public IEnumerable<MediaResponse>? Media { get; set; }

	public UserResponse? Creator { get; set; }
	public required Guid CreatorId { get; set; }
}
