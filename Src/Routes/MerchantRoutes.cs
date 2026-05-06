namespace SinaMN75U.Routes;

public static class MerchantRoutes {
	public static void MapMerchantRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async (MerchantCreateParams p, IMerchantService s, CancellationToken c) => (await s.Create(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Read", async (MerchantReadParams p, IMerchantService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Produces<UResponse<IEnumerable<MerchantResponse>>>();
		r.MapPost("ReadById", async (IdParams<MerchantSelectorArgs> p, IMerchantService s, CancellationToken c) => (await s.ReadById(p, c)).ToResult()).Produces<UResponse<MerchantResponse>>();
		r.MapPost("Update", async (MerchantUpdateParams p, IMerchantService s, CancellationToken c) => (await s.Update(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Delete", async (IdParams p, IMerchantService s, CancellationToken c) => (await s.Delete(p, c)).ToResult()).Produces<UResponse>();
	}
}