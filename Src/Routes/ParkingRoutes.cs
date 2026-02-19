namespace SinaMN75U.Routes;

public static class ParkingRoutes {
	public static void MapParkingRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("CreateParking", async (ParkingCreateParams d, IParkingService s, CancellationToken c) => (await s.CreateParking(d, c)).ToResult()).Produces<UResponse<ParkingResponse>>();
		r.MapPost("ReadParking", async (ParkingReadParams p, IParkingService s, CancellationToken c) => (await s.ReadParking(p, c)).ToResult()).Produces<UResponse<IEnumerable<ParkingResponse>>>();
		r.MapPost("UpdateParking", async (ParkingUpdateParams d, IParkingService s, CancellationToken c) => (await s.UpdateParking(d, c)).ToResult()).Produces<UResponse<ParkingResponse>>();
		r.MapPost("DeleteParking", async (IdParams d, IParkingService s, CancellationToken c) => (await s.DeleteParking(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("SoftDeleteParking", async (SoftDeleteParams d, IParkingService s, CancellationToken c) => (await s.SoftDeleteParking(d, c)).ToResult()).Produces<UResponse>();
		
		r.MapPost("CreateParkingReport", async (ParkingReportCreateParams d, IParkingService s, CancellationToken c) => (await s.CreateParkingReport(d, c)).ToResult()).Produces<UResponse<ParkingReportResponse>>();
		r.MapPost("ReadParkingReport", async (ParkingReportReadParams p, IParkingService s, CancellationToken c) => (await s.ReadParkingReport(p, c)).ToResult()).Produces<UResponse<IEnumerable<ParkingReportResponse>>>();
		r.MapPost("UpdateParkingReport", async (ParkingReportUpdateParams d, IParkingService s, CancellationToken c) => (await s.UpdateParkingReport(d, c)).ToResult()).Produces<UResponse<ParkingReportResponse>>();
		r.MapPost("DeleteParkingReport", async (IdParams d, IParkingService s, CancellationToken c) => (await s.DeleteParkingReport(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("SoftDeleteParkingReport", async (SoftDeleteParams d, IParkingService s, CancellationToken c) => (await s.SoftDeleteParkingReport(d, c)).ToResult()).Produces<UResponse>();
	}
}