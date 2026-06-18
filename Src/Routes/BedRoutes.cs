namespace SinaMN75U.Routes;

public static class BedRoutes {
	public static void MapBedRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async (BedCreateParams p, IBedService s, CancellationToken c) => (await s.Create(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Read", async (BedReadParams p, IBedService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Produces<UResponse<IEnumerable<BedResponse?>>>();
		r.MapPost("ReadById", async (IdParams<BedSelectorArgs> p, IBedService s, CancellationToken c) => (await s.ReadById(p, c)).ToResult()).Produces<UResponse<BedResponse?>>();
		r.MapPost("Update", async (BedUpdateParams p, IBedService s, CancellationToken c) => (await s.Update(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Delete", async (IdParams p, IBedService s, CancellationToken c) => (await s.Delete(p, c)).ToResult()).Produces<UResponse>();
	}
}