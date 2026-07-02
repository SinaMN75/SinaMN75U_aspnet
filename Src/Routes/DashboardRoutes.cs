namespace SinaMN75U.Routes;

public static class DashboardRoutes {
	public static void MapDashboardRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("ReadSystemMetrics", async (IDashboardService s) => await s.ReadSystemMetrics()).Produces<SystemMetricsResponse>();
		r.MapPost("Read", async (IDashboardService s, CancellationToken ct) => await s.ReadDashboardData(ct)).Produces<DashboardResponse>();
		r.MapGet("Enums", () => {
			Dictionary<string, IEnumerable<IdTitleParams>> result = new() {
				[nameof(Usc)] = UExtensions.GetValues<Usc>(),
				[nameof(TagUser)] = UExtensions.GetValues<TagUser>(),
				[nameof(TagCategory)] = UExtensions.GetValues<TagCategory>(),
				[nameof(TagMedia)] = UExtensions.GetValues<TagMedia>(),
				[nameof(TagProduct)] = UExtensions.GetValues<TagProduct>(),
				[nameof(TagComment)] = UExtensions.GetValues<TagComment>(),
				[nameof(TagReaction)] = UExtensions.GetValues<TagReaction>()
			};

			return Results.Ok(result);
		});

		r.MapPost("Logs/Search", async (IApiLogService s, ApiLogSearchParams p, CancellationToken ct) => (await s.Search(p, ct)).ToResult()).Produces<UResponse<IEnumerable<ApiLogListItemResponse>?>>();
		r.MapPost("Logs/Detail", async (IApiLogService s, IdParams p, CancellationToken ct) => (await s.ReadById(p, ct)).ToResult()).Produces<UResponse<ApiLogDetailResponse?>>();
		r.MapPost("Logs/Stats", async (IApiLogService s, ApiLogStatsParams p, CancellationToken ct) => (await s.ReadStats(p, ct)).ToResult()).Produces<UResponse<ApiLogStatsResponse?>>();
		r.MapPost("Logs/Export", async (IApiLogService s, ApiLogSearchParams p, CancellationToken ct) => Results.File(await s.Export(p, ct), "text/csv", $"api-logs-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv"));
	}
}