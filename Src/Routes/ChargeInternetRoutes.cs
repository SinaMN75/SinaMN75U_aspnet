namespace SinaMN75U.Routes;

public static class ChargeInternetRoutes {
	public static void MapChargeInternetRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Pin", async (ReserveChargeParams p, IChargeInternetService s, CancellationToken c) => (await s.Pin(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Topup", async (TopupChargeParams p, IChargeInternetService s, CancellationToken c) => (await s.Topup(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("InternetList", async (InternetListParams p, IChargeInternetService s, CancellationToken c) => (await s.InternetList(p, c)).ToResult()).Produces<UResponse>();
	}
}