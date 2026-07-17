namespace SinaMN75U.Routes;

public static class MoadiRoutes {
	public static void MapMoadiRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async (MoadiCreateParams p, IMoadiService s, CancellationToken c) => (await s.Create(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Read", async (MoadiReadParams p, IMoadiService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Produces<UResponse<IEnumerable<MoadiResponse>>>();
		r.MapPost("ReadById", async (IdParams<MoadiSelectorArgs> p, IMoadiService s, CancellationToken c) => (await s.ReadById(p, c)).ToResult()).Produces<UResponse<MoadiResponse>>();
		r.MapPost("Update", async (MoadiUpdateParams p, IMoadiService s, CancellationToken c) => (await s.Update(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Approve", async (IdParams p, IMoadiService s, CancellationToken c) => (await s.Approve(p, c)).ToResult()).Produces<UResponse<MoadiResponse>>();
		r.MapPost("Reject", async (MoadiRejectParams p, IMoadiService s, CancellationToken c) => (await s.Reject(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Delete", async (IdParams p, IMoadiService s, CancellationToken c) => (await s.Delete(p, c)).ToResult()).Produces<UResponse>();
	}
}
