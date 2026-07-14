namespace SinaMN75U.Routes;

public static class DashboardRoutes {
	public static void MapDashboardRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("ReadSystemMetrics", async (IDashboardService s) => await s.ReadSystemMetrics()).Produces<SystemMetricsResponse>();
		r.MapPost("Read", async (IDashboardService s, CancellationToken ct) => await s.ReadDashboardData(ct)).Produces<DashboardResponse>();
		r.MapPost("ReadFinancialOpsDashboard", async (DashboardRangeParams p, IDashboardService s, CancellationToken ct) => (await s.ReadFinancialOpsDashboard(p, ct)).ToResult()).Produces<UResponse<FinancialOpsDashboardResponse>>();
		r.MapPost("ReadPropertyDashboard", async (DashboardRangeParams p, IDashboardService s, CancellationToken ct) => (await s.ReadPropertyDashboard(p, ct)).ToResult()).Produces<UResponse<PropertyDashboardResponse>>();
		r.MapPost("ReadOsMetrics", async (BaseParams p, IDashboardService s, CancellationToken ct) => (await s.ReadOsMetrics(p, ct)).ToResult()).Produces<UResponse<OsMetricsResponse>>();
		r.MapPost("ReadApiLogs", async (ApiLogReadParams p, IDashboardService s, CancellationToken ct) => (await s.ReadApiLogs(p, ct)).ToResult()).Produces<UResponse<IEnumerable<ApiLogResponse>>>();
		r.MapPost("ApiLogStats", async (ApiLogStatsParams p, IDashboardService s, CancellationToken ct) => (await s.ApiLogStats(p, ct)).ToResult()).Produces<UResponse<ApiLogStatsResponse>>();
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
	}
}