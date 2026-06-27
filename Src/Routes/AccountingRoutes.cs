namespace SinaMN75U.Routes;

public static class AccountingRoutes {
	public static void MapAccountingRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Report", async (AccountingReportParams p, IAccountingService s, CancellationToken c) => (await s.Report(p, c)).ToResult()).Produces<UResponse<AccountingReportResponse?>>();
	}
}
