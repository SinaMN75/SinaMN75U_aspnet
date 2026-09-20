namespace SinaMN75U.Routes;

public static class DataSeedRoutes {
	public static void MapDataSeedRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Users", async (IDataSeedService s) => (await s.SeedUsers()).ToResult());
		r.MapPost("Categories", async (IDataSeedService s) => (await s.SeedCategories()).ToResult());
		r.MapPost("Contents", async (IDataSeedService s) => (await s.SeedContents()).ToResult());
		r.MapPost("HotelsAndDorms", async (IDataSeedService s, CancellationToken c) => (await s.SeedHotelsAndDorms(c)).ToResult()).Produces<UResponse<List<KeyValue>>>();
		r.MapPost("Parking", async (ParkingSeedParams d, IParkingSeedService s, CancellationToken c) => (await s.SeedParking(d, c)).ToResult()).Produces<UResponse<ParkingSeedResponse>>();
	}
}