namespace SinaMN75U.Routes;

public static class VehicleRoutes {
	public static void MapVehicleRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async (VehicleCreateParams d, IVehicleService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).Produces<UResponse<VehicleResponse>>();
		r.MapPost("Read", async (VehicleReadParams p, IVehicleService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Produces<UResponse<IEnumerable<VehicleResponse>>>();
		r.MapPost("Update", async (VehicleUpdateParams d, IVehicleService s, CancellationToken c) => (await s.Update(d, c)).ToResult()).Produces<UResponse<VehicleResponse>>();
		r.MapPost("Delete", async (IdParams d, IVehicleService s, CancellationToken c) => (await s.Delete(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("SoftDelete", async (SoftDeleteParams d, IVehicleService s, CancellationToken c) => (await s.SoftDelete(d, c)).ToResult()).Produces<UResponse>();
	}
}