namespace SinaMN75U.Routes;

public static class TicketRoutes {
	public static void MapTicketRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async (TicketCreateParams d, ITicketService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).Produces<UResponse<TicketResponse>>();
		r.MapPost("Read", async (TicketReadParams p, ITicketService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Produces<UResponse<IEnumerable<TicketResponse>>>();
		r.MapPost("Update", async (TicketUpdateParams d, ITicketService s, CancellationToken c) => (await s.Update(d, c)).ToResult()).Produces<UResponse<TicketResponse>>();
		r.MapPost("Delete", async (IdParams d, ITicketService s, CancellationToken c) => (await s.Delete(d, c)).ToResult()).Produces<UResponse>();
	}
}