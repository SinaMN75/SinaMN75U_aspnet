namespace SinaMN75U.Routes;

public static class HealthRoutes {
	public static void MapHealthRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag);
		r.MapGet("Check", () => Results.Ok(new { status = "Healthy", time = DateTime.UtcNow }));
	}
}
