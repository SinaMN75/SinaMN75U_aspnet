namespace SinaMN75U.Routes;

public static class IpgRoutes {
	public static void MapIpgRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Pay", async (IpgSaleParams p, IIpgService s, CancellationToken c) => (await s.GetSaleIpgLink(p, c)).ToResult()).Produces<UResponse>();

		r.MapGet("CallBack", () => Results.Content(
				"HTML CONTENT",
				"text/html"
			)
		);
	}
}