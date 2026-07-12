namespace SinaMN75U.Routes;

public static class PnRoutes {
	public static void MapPnRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Auth", async (PnAuthParams p, IPnService s, CancellationToken c) => (await s.Auth(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("CreateMerchant", async (PnMerchantCreateParams p, IPnService s, CancellationToken c) => (await s.CreateMerchant(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("CreateTerminal", async (PnTerminalCreateParams p, IPnService s, CancellationToken c) => (await s.CreateTerminal(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("UserStatus", async (PnPhoneNumberParams p, IPnService s, CancellationToken c) => (await s.UserStatus(p, c)).ToResult()).Produces<UResponse<PnUserStatusResponse?>>();
		r.MapPost("ReadTerminalSupportPassword", async (PnTerminalSupportPasswordParams p, IPnService s, CancellationToken c) => (await s.ReadTerminalSupportPassword(p, c)).ToResult()).Produces<UResponse<TerminalSupportPasswordResponse?>>();
		r.MapPost("ZipCodeToAddress", async (PnZipCodeParams p, IPnService s, CancellationToken c) => (await s.ZipCodeToAddress(p, c)).ToResult()).Produces<UResponse<ZipCodeToAddressDetailResponse?>>();
	}
}