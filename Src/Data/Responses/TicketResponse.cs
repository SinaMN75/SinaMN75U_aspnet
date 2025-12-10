namespace SinaMN75U.Data.Responses;

public class TicketResponse : BaseResponse<TagTicket, TicketJson> {
	public IEnumerable<MediaResponse>? Media { get; set; }

	public UserResponse? User { get; set; }
	public required Guid UserId { get; set; }
}
