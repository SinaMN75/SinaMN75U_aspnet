namespace SinaMN75U.Routes;

public static class AppSettingsRoutes {
	public static void MapAppSettingsRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Read", async (BaseParams _, DbContext db, CancellationToken c) => new UResponse<AppSettingsResponse>(new AppSettingsResponse {
			ApiCallCosts = Core.App.ApiCallCosts,
			ChargeInternet = Core.App.ChargeInternet,
			ChargeInternetTaxPercent = Core.App.ChargeInternetTaxPercent,
			AppVersions = await db.Set<AppVersionEntity>().Select(x => new AppVersionResponse {
				Id = x.Id,
				CreatedAt = x.CreatedAt,
				Tags = x.Tags,
				JsonData = x.JsonData,
				LatestBuildNumber = x.LatestBuildNumber,
				MinBuildNumber = x.MinBuildNumber
			}).ToListAsync(c)
		}).ToResult()).Produces<UResponse<AppSettingsResponse>>();
		r.MapPost("ReadAll", (BaseParams p, ITokenService ts, ILocalizationService ls) => !IsSystemAdmin(p.Token, ts) ? Forbidden(ls) : new UResponse<AppSettings>(Core.App).ToResult()).Produces<UResponse<AppSettings>>();
		r.MapPost("Update", (AppSettingsUpdateParams p, ITokenService ts, ILocalizationService ls) => {
			if (!IsSystemAdmin(p.Token, ts)) return Forbidden(ls);
			Core.App = p.Settings;
			return new UResponse(Usc.Success, ls.Get("updatedSuccessfully")).ToResult();
		}).Produces<UResponse>();
		r.MapPost("UpdateAppVersion", async (AppVersionUpdateParams p, DbContext db, ITokenService ts, ILocalizationService ls, CancellationToken c) => {
			if (!IsSystemAdmin(p.Token, ts)) return Forbidden(ls);
			AppVersionEntity? e = await db.Set<AppVersionEntity>().AsTracking().FirstOrDefaultAsync(x => x.Tags.Contains(p.Platform), c);
			if (e == null) {
				e = new AppVersionEntity { Id = Guid.CreateVersion7(), CreatedAt = DateTime.UtcNow, CreatorId = ts.ExtractClaims(p.Token)!.Id, Tags = [p.Platform], LatestBuildNumber = 0, MinBuildNumber = 0, JsonData = new AppVersionJson() };
				await db.AddAsync(e, c);
			}
			e.LatestBuildNumber = p.LatestBuildNumber;
			e.MinBuildNumber = p.MinBuildNumber;
			e.JsonData.LatestVersionName = p.LatestVersionName;
			e.JsonData.Description = p.Description;
			e.JsonData.Links = p.Links;
			await db.SaveChangesAsync(c);
			return new UResponse(Usc.Success, ls.Get("updatedSuccessfully")).ToResult();
		}).Produces<UResponse>();
	}

	private static bool IsSystemAdmin(string? token, ITokenService ts) {
		JwtClaimData? u = ts.ExtractClaims(token);
		return u != null && u.Tags.Contains(TagUser.SystemAdmin);
	}

	private static IResult Forbidden(ILocalizationService ls) => new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction")).ToResult();
}
