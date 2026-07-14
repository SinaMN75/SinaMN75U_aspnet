namespace SinaMN75U.Routes;

public static class ApiLogRoutes {
	public static void MapApiLogRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Read", async (ApiLogReadParams p, IApiLogService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Produces<UResponse<IEnumerable<ApiLogResponse>>>();
		r.MapPost("Stats", async (ApiLogStatsParams p, IApiLogService s, CancellationToken c) => (await s.Stats(p, c)).ToResult()).Produces<UResponse<ApiLogStatsResponse>>();
	}
}
