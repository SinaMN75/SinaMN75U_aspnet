namespace SinaMN75U.Routes;

public static class AppSettingsRoutes {
	public static void MapAppSettingsRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Read", (BaseParams _) => new UResponse<AppSettingsResponse>(new AppSettingsResponse { ApiCallCosts = Core.App.ApiCallCosts, ChargeInternet = Core.App.ChargeInternet }).ToResult()).Produces<UResponse<AppSettingsResponse>>();
		r.MapPost("ReadAll", (BaseParams p, ITokenService ts, ILocalizationService ls) => !IsSystemAdmin(p.Token, ts) ? Forbidden(ls) : new UResponse<AppSettings>(Core.App).ToResult()).Produces<UResponse<AppSettings>>();
		r.MapPost("Update", (AppSettingsUpdateParams p, ITokenService ts, ILocalizationService ls) => {
			if (!IsSystemAdmin(p.Token, ts)) return Forbidden(ls);
			Core.App = p.Settings;
			return new UResponse(Usc.Success, ls.Get("Updated")).ToResult();
		}).Produces<UResponse>();
	}

	private static bool IsSystemAdmin(string? token, ITokenService ts) {
		JwtClaimData? u = ts.ExtractClaims(token);
		return u != null && u.Tags.Contains(TagUser.SystemAdmin);
	}

	private static IResult Forbidden(ILocalizationService ls) => new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction")).ToResult();
}
