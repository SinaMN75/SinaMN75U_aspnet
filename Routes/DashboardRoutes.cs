namespace SinaMN75U.Routes;

public static class DashboardRoutes {
	public static void MapDashboardRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("ReadSystemMetrics", async (IDashboardService s) => await s.ReadSystemMetrics()).Produces<SystemMetricsResponse>();
		r.MapPost("Read", async (IDashboardService s, CancellationToken ct) => await s.ReadDashboardData(ct)).Produces<DashboardResponse>();
		r.MapGet("Enums", () => {
			Dictionary<string, IEnumerable<IdTitleParams>> result = new() {
				[nameof(Usc)] = EnumExtensions.GetValues<Usc>(),
				[nameof(TagUser)] = EnumExtensions.GetValues<TagUser>(),
				[nameof(TagCategory)] = EnumExtensions.GetValues<TagCategory>(),
				[nameof(TagMedia)] = EnumExtensions.GetValues<TagMedia>(),
				[nameof(TagProduct)] = EnumExtensions.GetValues<TagProduct>(),
				[nameof(TagComment)] = EnumExtensions.GetValues<TagComment>(),
				[nameof(TagReaction)] = EnumExtensions.GetValues<TagReaction>(),
			};

			return Results.Ok(result);
		});
	}
}