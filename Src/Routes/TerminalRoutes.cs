namespace SinaMN75U.Routes;

public static class TerminalRoutes {
	public static void MapTerminalRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async (TerminalCreateParams p, ITerminalService s, CancellationToken c) => (await s.Create(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("BulkCreate", async (TerminalBulkCreateParams p, ITerminalService s, CancellationToken c) => (await s.BulkCreate(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Read", async (TerminalReadParams p, ITerminalService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Produces<UResponse<IEnumerable<TerminalResponse>>>();
		r.MapPost("Update", async (TerminalUpdateParams p, ITerminalService s, CancellationToken c) => (await s.Update(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Delete", async (IdParams p, ITerminalService s, CancellationToken c) => (await s.Delete(p, c)).ToResult()).Produces<UResponse>();
	}
}