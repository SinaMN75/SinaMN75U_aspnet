namespace SinaMN75U.Routes;

public static class AddressRoutes {
	public static void MapAddressRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async (AddressCreateParams p, IAddressService s, CancellationToken c) => (await s.Create(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Read", async (AddressReadParams p, IAddressService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Produces<UResponse<IEnumerable<AddressResponse>>>();
		r.MapPost("Update", async (AddressUpdateParams p, IAddressService s, CancellationToken c) => (await s.Update(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Delete", async (IdParams p, IAddressService s, CancellationToken c) => (await s.Delete(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("SoftDelete", async (SoftDeleteParams p, IAddressService s, CancellationToken c) => (await s.SoftDelete(p, c)).ToResult()).Produces<UResponse>();
	}
}