namespace SinaMN75U.Routes;

public static class AppSettingsRoutes {
	public static void MapAppSettingsRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Read", (BaseParams _) => new UResponse<AppSettingsResponse>(
			new AppSettingsResponse {
				ApiCallCosts = Core.App.ApiCallCosts,
				ChargeInternet = Core.App.ChargeInternet
			}
		).ToResult()).Produces<UResponse<AppSettingsResponse>>();
	}
}