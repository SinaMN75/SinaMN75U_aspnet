namespace SinaMN75U.Routes;

public static class TerminalRoutes {
	public static void MapTerminalRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async (TerminalCreateParams d, ITerminalService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Read", async (TerminalReadParams p, ITerminalService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Produces<UResponse<IEnumerable<TerminalResponse>>>();
		r.MapPost("Update", async (TerminalUpdateParams d, ITerminalService s, CancellationToken c) => (await s.Update(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Delete", async (IdParams d, ITerminalService s, CancellationToken c) => (await s.Delete(d, c)).ToResult()).Produces<UResponse>();
	}
}