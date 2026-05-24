namespace SinaMN75U.Routes;

public static class DataSeedRoutes {
	public static void MapDataSeedRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Users", async (IDataSeedService s) => (await s.SeedUsers()).ToResult());
		r.MapPost("Processes", async (IDataSeedService s) => (await s.SeedVerificationProcess()).ToResult());
	}
}