namespace SinaMN75U.Routes;

public static class ChargeInternetRoutes {
	public static void MapChargeInternetRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Pin", async (ReserveChargeParams p, IChargeInternetService s, CancellationToken c) => (await s.Pin(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Topup", async (TopupChargeParams p, IChargeInternetService s, CancellationToken c) => (await s.Topup(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("InternetList", async (InternetListParams p, IChargeInternetService s, CancellationToken c) => (await s.InternetList(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("InternetReserve", async (InternetReserveParams p, IChargeInternetService s, CancellationToken c) => (await s.InternetReserve(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Approve", async (ApproveParams p, IChargeInternetService s, CancellationToken c) => (await s.Approve(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("GetStatus", async (GetStatusParams p, IChargeInternetService s, CancellationToken c) => (await s.GetStatus(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("GetBalance", async (ReserveChargeParams p, IChargeInternetService s, CancellationToken c) => (await s.GetBalance(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Echo", async (ReserveChargeParams p, IChargeInternetService s, CancellationToken c) => (await s.Echo(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("MCITopOffer", async (MCITopOfferParams p, IChargeInternetService s, CancellationToken c) => (await s.MCITopOffer(p, c)).ToResult()).Produces<UResponse>();
	}
}