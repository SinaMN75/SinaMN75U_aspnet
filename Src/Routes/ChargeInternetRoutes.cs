namespace SinaMN75U.Routes;

public static class ChargeInternetRoutes {
	public static void MapChargeInternetRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Reserve", async (ReserveChargeParams p, IChargeInternetService s, CancellationToken c) => (await s.Reserve(p, c)).ToResult()).Produces<UResponse>();
	}
}