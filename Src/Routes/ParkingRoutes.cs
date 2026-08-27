namespace SinaMN75U.Routes;

public static class ParkingRoutes {
	public static void MapParkingRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("CreateParking", async (ParkingCreateParams d, IParkingService s, CancellationToken c) => (await s.CreateParking(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("ReadParking", async (ParkingReadParams p, IParkingService s, CancellationToken c) => (await s.ReadParking(p, c)).ToResult()).Produces<UResponse<IEnumerable<ParkingResponse>>>();
		r.MapPost("UpdateParking", async (ParkingUpdateParams d, IParkingService s, CancellationToken c) => (await s.UpdateParking(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DeleteParking", async (IdParams d, IParkingService s, CancellationToken c) => (await s.DeleteParking(d, c)).ToResult()).Produces<UResponse>();

		r.MapPost("CreateParkingUser", async (ParkingUserCreateParams d, IParkingService s, CancellationToken c) => (await s.CreateParkingUser(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("ReadParkingUsers", async (ParkingUserReadParams d, IParkingService s, CancellationToken c) => (await s.ReadParkingUsers(d, c)).ToResult()).Produces<UResponse<IEnumerable<UserResponse>>>();
		r.MapPost("RemoveParkingUser", async (ParkingUserDeleteParams d, IParkingService s, CancellationToken c) => (await s.RemoveParkingUser(d, c)).ToResult()).Produces<UResponse>();

		r.MapPost("CreateParkingReport", async (ParkingReportCreateParams d, IParkingService s, CancellationToken c) => (await s.CreateParkingReport(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("ReadParkingReport", async (ParkingReportReadParams p, IParkingService s, CancellationToken c) => (await s.ReadParkingReport(p, c)).ToResult()).Produces<UResponse<IEnumerable<ParkingReportResponse>>>();
		r.MapPost("UpdateParkingReport", async (ParkingReportUpdateParams d, IParkingService s, CancellationToken c) => (await s.UpdateParkingReport(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DeleteParkingReport", async (IdParams d, IParkingService s, CancellationToken c) => (await s.DeleteParkingReport(d, c)).ToResult()).Produces<UResponse>();

		r.MapPost("CreateParkingTariff", async (ParkingTariffCreateParams d, IParkingService s, CancellationToken c) => (await s.CreateParkingTariff(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("ReadParkingTariff", async (ParkingTariffReadParams d, IParkingService s, CancellationToken c) => (await s.ReadParkingTariff(d, c)).ToResult()).Produces<UResponse<IEnumerable<ParkingTariffResponse>>>();
		r.MapPost("UpdateParkingTariff", async (ParkingTariffUpdateParams d, IParkingService s, CancellationToken c) => (await s.UpdateParkingTariff(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DeleteParkingTariff", async (IdParams d, IParkingService s, CancellationToken c) => (await s.DeleteParkingTariff(d, c)).ToResult()).Produces<UResponse>();

		r.MapPost("CreateParkingSubscription", async (ParkingSubscriptionCreateParams d, IParkingService s, CancellationToken c) => (await s.CreateParkingSubscription(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("ReadParkingSubscription", async (ParkingSubscriptionReadParams d, IParkingService s, CancellationToken c) => (await s.ReadParkingSubscription(d, c)).ToResult()).Produces<UResponse<IEnumerable<ParkingSubscriptionResponse>>>();
		r.MapPost("UpdateParkingSubscription", async (ParkingSubscriptionUpdateParams d, IParkingService s, CancellationToken c) => (await s.UpdateParkingSubscription(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DeleteParkingSubscription", async (IdParams d, IParkingService s, CancellationToken c) => (await s.DeleteParkingSubscription(d, c)).ToResult()).Produces<UResponse>();

		r.MapPost("CreateParkingPlateFlag", async (ParkingPlateFlagCreateParams d, IParkingService s, CancellationToken c) => (await s.CreateParkingPlateFlag(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("ReadParkingPlateFlag", async (ParkingPlateFlagReadParams d, IParkingService s, CancellationToken c) => (await s.ReadParkingPlateFlag(d, c)).ToResult()).Produces<UResponse<IEnumerable<ParkingPlateFlagResponse>>>();
		r.MapPost("UpdateParkingPlateFlag", async (ParkingPlateFlagUpdateParams d, IParkingService s, CancellationToken c) => (await s.UpdateParkingPlateFlag(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DeleteParkingPlateFlag", async (IdParams d, IParkingService s, CancellationToken c) => (await s.DeleteParkingPlateFlag(d, c)).ToResult()).Produces<UResponse>();

		r.MapPost("CreateParkingStaff", async (ParkingStaffCreateParams d, IParkingService s, CancellationToken c) => (await s.CreateParkingStaff(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("ReadParkingStaff", async (ParkingStaffReadParams d, IParkingService s, CancellationToken c) => (await s.ReadParkingStaff(d, c)).ToResult()).Produces<UResponse<IEnumerable<ParkingStaffResponse>>>();
		r.MapPost("UpdateParkingStaff", async (ParkingStaffUpdateParams d, IParkingService s, CancellationToken c) => (await s.UpdateParkingStaff(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DeleteParkingStaff", async (IdParams d, IParkingService s, CancellationToken c) => (await s.DeleteParkingStaff(d, c)).ToResult()).Produces<UResponse>();

		r.MapPost("OpenParkingShift", async (ParkingShiftOpenParams d, IParkingService s, CancellationToken c) => (await s.OpenParkingShift(d, c)).ToResult()).Produces<UResponse<ParkingShiftResponse>>();
		r.MapPost("ReadParkingShift", async (ParkingShiftReadParams d, IParkingService s, CancellationToken c) => (await s.ReadParkingShift(d, c)).ToResult()).Produces<UResponse<IEnumerable<ParkingShiftResponse>>>();
		r.MapPost("CloseParkingShift", async (ParkingShiftCloseParams d, IParkingService s, CancellationToken c) => (await s.CloseParkingShift(d, c)).ToResult()).Produces<UResponse<ParkingShiftResponse>>();

		r.MapPost("ReadParkingPlateStatus", async (ParkingPlateStatusParams d, IParkingService s, CancellationToken c) => (await s.ReadParkingPlateStatus(d, c)).ToResult()).Produces<UResponse<ParkingPlateStatusResponse>>();
		r.MapPost("RegisterParkingEntry", async (ParkingEntryParams d, IParkingService s, CancellationToken c) => (await s.RegisterParkingEntry(d, c)).ToResult()).Produces<UResponse<ParkingReportResponse>>();
		r.MapPost("CalculateParkingExit", async (ParkingExitCalculateParams d, IParkingService s, CancellationToken c) => (await s.CalculateParkingExit(d, c)).ToResult()).Produces<UResponse<ParkingBillResponse>>();
		r.MapPost("RegisterParkingExit", async (ParkingExitParams d, IParkingService s, CancellationToken c) => (await s.RegisterParkingExit(d, c)).ToResult()).Produces<UResponse<ParkingReportResponse>>();
		r.MapPost("ReadParkingDashboard", async (ParkingDashboardParams d, IParkingService s, CancellationToken c) => (await s.ReadParkingDashboard(d, c)).ToResult()).Produces<UResponse<ParkingDashboardResponse>>();
		r.MapPost("ReadParkingInsideVehicles", async (ParkingInsideVehiclesParams d, IParkingService s, CancellationToken c) => (await s.ReadParkingInsideVehicles(d, c)).ToResult()).Produces<UResponse<IEnumerable<ParkingInsideVehicleResponse>>>();
	}
}