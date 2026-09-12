namespace SinaMN75U.Routes;

public static class BrokerRoutes {
	public static void MapBrokerRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("CreateBroker", async (BrokerCreateParams p, IBrokerService s, CancellationToken c) => (await s.CreateBroker(p, c)).ToResult()).Produces<UResponse<Guid>>();
		r.MapPost("ReadBroker", async (BrokerReadParams p, IBrokerService s, CancellationToken c) => (await s.ReadBroker(p, c)).ToResult()).Produces<UResponse<IEnumerable<BrokerResponse>>>();
		r.MapPost("UpdateBroker", async (BrokerUpdateParams p, IBrokerService s, CancellationToken c) => (await s.UpdateBroker(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DeleteBroker", async (IdParams p, IBrokerService s, CancellationToken c) => (await s.DeleteBroker(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("CreateBrand", async (TerminalBrandCreateParams p, IBrokerService s, CancellationToken c) => (await s.CreateBrand(p, c)).ToResult()).Produces<UResponse<Guid>>();
		r.MapPost("ReadBrand", async (TerminalBrandReadParams p, IBrokerService s, CancellationToken c) => (await s.ReadBrand(p, c)).ToResult()).Produces<UResponse<IEnumerable<TerminalBrandResponse>>>();
		r.MapPost("UpdateBrand", async (TerminalBrandUpdateParams p, IBrokerService s, CancellationToken c) => (await s.UpdateBrand(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DeleteBrand", async (IdParams p, IBrokerService s, CancellationToken c) => (await s.DeleteBrand(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("CreateAgreementTemplate", async (AgreementTemplateCreateParams p, IBrokerService s, CancellationToken c) => (await s.CreateAgreementTemplate(p, c)).ToResult()).Produces<UResponse<Guid>>();
		r.MapPost("ReadAgreementTemplate", async (AgreementTemplateReadParams p, IBrokerService s, CancellationToken c) => (await s.ReadAgreementTemplate(p, c)).ToResult()).Produces<UResponse<IEnumerable<AgreementTemplateResponse>>>();
		r.MapPost("UpdateAgreementTemplate", async (AgreementTemplateUpdateParams p, IBrokerService s, CancellationToken c) => (await s.UpdateAgreementTemplate(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DeleteAgreementTemplate", async (IdParams p, IBrokerService s, CancellationToken c) => (await s.DeleteAgreementTemplate(p, c)).ToResult()).Produces<UResponse>();
	}
}
