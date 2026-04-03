namespace SinaMN75U.Routes;

public static class SimCardRoutes {
	public static void MapSimCardRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async (SimCardCreateParams p, ISimCardService s, CancellationToken c) => (await s.Create(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Read", async (SimCardReadParams p, ISimCardService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Produces<UResponse<IEnumerable<SimCardResponse>>>();
		r.MapPost("Update", async (SimCardUpdateParams p, ISimCardService s, CancellationToken c) => (await s.Update(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Delete", async (IdParams p, ISimCardService s, CancellationToken c) => (await s.Delete(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("SoftDelete", async (SoftDeleteParams p, ISimCardService s, CancellationToken c) => (await s.SoftDelete(p, c)).ToResult()).Produces<UResponse>();
	}
}