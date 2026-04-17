namespace SinaMN75U.Routes;

public static class AgreementRoutes {
	public static void MapAgreementRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("GenerateAgreement", async (GenerateAgreementParams p, IAgreementService s, CancellationToken c) => (await s.GenerateAgreement(p, c)).ToResult()).Produces<UResponse<AgreementResponse>>();
	}
}