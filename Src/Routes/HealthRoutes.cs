namespace SinaMN75U.Routes;

public static class HealthRoutes {
	public static void MapHealthRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag);
		r.MapGet("Check", async (DbContext db, CancellationToken c) => {
			await db.Database.GetPendingMigrationsAsync(c);
			await db.Database.MigrateAsync(c);
			return Results.Ok(new { status = "Healthy", time = DateTime.UtcNow });
		});
	}
}
