namespace SinaMN75U.Routes;

public static class LogRoutes {
	public static void MapLogRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag);

		r.MapGet("Read", (string? level, int? take) => {
			IEnumerable<ULogEntry> logs = ULog.GetLogs();
			if (level.IsNotNullOrEmpty()) logs = logs.Where(x => string.Equals(x.Level, level, StringComparison.OrdinalIgnoreCase));
			if (take is > 0) logs = logs.Take(take.Value);
			return Results.Ok(new UResponse<IEnumerable<ULogEntry>>(logs.ToList()));
		}).Produces<UResponse<IEnumerable<ULogEntry>>>();

		r.MapDelete("Clear", () => {
			ULog.ClearLogs();
			return Results.Ok(new UResponse());
		}).Produces<UResponse>();
	}
}
