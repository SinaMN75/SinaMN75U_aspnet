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

		// Paginated/filterable table of requests: date range, method, status code, error-only,
		// user, and free-text search across path/exception. Backed by Postgres (see
		// InnerServices/LogService.cs + Data/Entities/ApiRequestLogEntity.cs), not the old
		// per-day JSON files.
		r.MapPost("Logs/Search", async (IApiLogService s, ApiLogSearchParams p, CancellationToken ct) => (await s.Search(p, ct)).ToResult())
			.Produces<UResponse<IEnumerable<ApiLogListItemResponse>?>>();

		// Full detail (bodies, exception, stack trace) for a single request - replaces the old
		// "load the whole day's file to view one entry" Logs/content endpoint.
		r.MapPost("Logs/Detail", async (IApiLogService s, IdParams p, CancellationToken ct) => (await s.ReadById(p, ct)).ToResult())
			.Produces<UResponse<ApiLogDetailResponse?>>();

		// Aggregates for the dashboard's charts: volume/error time series, status code
		// distribution, top slow endpoints, top failing endpoints.
		r.MapPost("Logs/Stats", async (IApiLogService s, ApiLogStatsParams p, CancellationToken ct) => (await s.ReadStats(p, ct)).ToResult())
			.Produces<UResponse<ApiLogStatsResponse?>>();
	}
}
