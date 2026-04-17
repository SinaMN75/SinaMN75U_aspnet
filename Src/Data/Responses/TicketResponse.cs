namespace SinaMN75U.Data.Responses;

public sealed class TicketResponse : BaseResponse<TagTicket, TicketJson> {
	public IEnumerable<MediaResponse>? Media { get; set; }
}