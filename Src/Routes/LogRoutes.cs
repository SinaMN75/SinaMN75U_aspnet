namespace SinaMN75U.Routes;

public static class LogRoutes {
	public static void MapLogRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag);

		r.MapGet("Read", () => Results.Ok(new UResponse<IReadOnlyList<string>>(ULog.GetLogs()))).Produces<UResponse<IReadOnlyList<string>>>();

		r.MapDelete("Clear", () => {
			ULog.ClearLogs();
			return Results.Ok(new UResponse());
		}).Produces<UResponse>();
	}
}
