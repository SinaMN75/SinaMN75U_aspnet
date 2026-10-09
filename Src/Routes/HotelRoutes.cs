namespace SinaMN75U.Routes;

public static class HotelRoutes {
	public static void MapHotelRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>().AddEndpointFilter<ActivityLogFilter>();

		r.MapPost("Hotel/Create", async (HotelCreateParams p, IHotelService s, CancellationToken c) => (await s.CreateHotel(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Hotel/Read", async (HotelReadParams p, IHotelService s, CancellationToken c) => (await s.ReadHotels(p, c)).ToResult()).Produces<UResponse<IEnumerable<HotelResponse>>>();
		r.MapPost("Hotel/ReadById", async (IdParams<HotelSelectorArgs> p, IHotelService s, CancellationToken c) => (await s.ReadHotelById(p, c)).ToResult()).Produces<UResponse<HotelResponse>>();
		r.MapPost("Hotel/Update", async (HotelUpdateParams p, IHotelService s, CancellationToken c) => (await s.UpdateHotel(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Hotel/Delete", async (IdParams p, IHotelService s, CancellationToken c) => (await s.DeleteHotel(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("HotelRoom/Create", async (HotelRoomCreateParams p, IHotelService s, CancellationToken c) => (await s.CreateHotelRoom(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("HotelRoom/Read", async (HotelRoomReadParams p, IHotelService s, CancellationToken c) => (await s.ReadHotelRooms(p, c)).ToResult()).Produces<UResponse<IEnumerable<HotelRoomResponse>>>();
		r.MapPost("HotelRoom/ReadById", async (IdParams<HotelRoomSelectorArgs> p, IHotelService s, CancellationToken c) => (await s.ReadHotelRoomById(p, c)).ToResult()).Produces<UResponse<HotelRoomResponse>>();
		r.MapPost("HotelRoom/Update", async (HotelRoomUpdateParams p, IHotelService s, CancellationToken c) => (await s.UpdateHotelRoom(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelRoom/Delete", async (IdParams p, IHotelService s, CancellationToken c) => (await s.DeleteHotelRoom(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelRoom/Availability", async (HotelRoomAvailabilityParams p, IHotelService s, CancellationToken c) => (await s.ReadHotelRoomAvailability(p, c)).ToResult()).Produces<UResponse<IEnumerable<HotelRoomAvailabilityResponse>>>();

		r.MapPost("HotelReservation/Create", async (HotelReservationCreateParams p, IHotelService s, CancellationToken c) => (await s.CreateHotelReservation(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("HotelReservation/Read", async (HotelReservationReadParams p, IHotelService s, CancellationToken c) => (await s.ReadHotelReservations(p, c)).ToResult()).Produces<UResponse<IEnumerable<HotelReservationResponse>>>();
		r.MapPost("HotelReservation/ReadById", async (IdParams<HotelReservationSelectorArgs> p, IHotelService s, CancellationToken c) => (await s.ReadHotelReservationById(p, c)).ToResult()).Produces<UResponse<HotelReservationResponse>>();
		r.MapPost("HotelReservation/Update", async (HotelReservationUpdateParams p, IHotelService s, CancellationToken c) => (await s.UpdateHotelReservation(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelReservation/Delete", async (IdParams p, IHotelService s, CancellationToken c) => (await s.DeleteHotelReservation(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelReservation/Confirm", async (IdParams p, IHotelService s, CancellationToken c) => (await s.ConfirmHotelReservation(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelReservation/CheckIn", async (IdParams p, IHotelService s, CancellationToken c) => (await s.CheckInHotelReservation(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelReservation/CheckOut", async (IdParams p, IHotelService s, CancellationToken c) => (await s.CheckOutHotelReservation(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelReservation/Cancel", async (IdParams p, IHotelService s, CancellationToken c) => (await s.CancelHotelReservation(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelReservation/Book", async (HotelReservationBookParams p, IHotelService s, CancellationToken c) => (await s.BookHotelReservation(p, c)).ToResult()).Produces<UResponse<HotelReservationResponse>>();
		r.MapPost("HotelReservation/CancelByUser", async (HotelReservationCancelParams p, IHotelService s, CancellationToken c) => (await s.CancelHotelReservationByUser(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("HotelInvoice/Create", async (HotelInvoiceCreateParams p, IHotelService s, CancellationToken c) => (await s.CreateHotelInvoice(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("HotelInvoice/Read", async (HotelInvoiceReadParams p, IHotelService s, CancellationToken c) => (await s.ReadHotelInvoices(p, c)).ToResult()).Produces<UResponse<IEnumerable<HotelInvoiceResponse>>>();
		r.MapPost("HotelInvoice/Update", async (HotelInvoiceUpdateParams p, IHotelService s, CancellationToken c) => (await s.UpdateHotelInvoice(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelInvoice/Delete", async (IdParams p, IHotelService s, CancellationToken c) => (await s.DeleteHotelInvoice(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelInvoice/Pay", async (IdParams p, IHotelService s, CancellationToken c) => (await s.PayHotelInvoice(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelInvoice/Receive", async (InvoiceReceiveParams p, IHotelService s, CancellationToken c) => (await s.ReceiveHotelInvoice(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("Dashboard/Read", async (DashboardRangeParams p, IHotelService s, CancellationToken ct) => (await s.ReadHotelDashboard(p, ct)).ToResult()).Produces<UResponse<HotelDashboardResponse>>();
		r.MapPost("Seed", async (BaseParams p, IHotelService s, CancellationToken c) => (await s.SeedHotels(p, c)).ToResult()).Produces<UResponse<List<KeyValue>>>();

		r.MapPost("HotelRate/Create", async (HotelRateCreateParams p, IHotelService s, CancellationToken c) => (await s.CreateHotelRate(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("HotelRate/Read", async (HotelRateReadParams p, IHotelService s, CancellationToken c) => (await s.ReadHotelRates(p, c)).ToResult()).Produces<UResponse<IEnumerable<HotelRateResponse>>>();
		r.MapPost("HotelRate/Update", async (HotelRateUpdateParams p, IHotelService s, CancellationToken c) => (await s.UpdateHotelRate(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelRate/Delete", async (IdParams p, IHotelService s, CancellationToken c) => (await s.DeleteHotelRate(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelRoom/Calendar", async (HotelRoomCalendarParams p, IHotelService s, CancellationToken c) => (await s.ReadHotelRoomCalendar(p, c)).ToResult()).Produces<UResponse<IEnumerable<HotelCalendarDay>>>();
		r.MapPost("HotelRoom/Housekeeping", async (HotelHousekeepingParams p, IHotelService s, CancellationToken c) => (await s.SetHotelRoomHousekeeping(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelReservation/CreateGroup", async (HotelReservationGroupParams p, IHotelService s, CancellationToken c) => (await s.CreateHotelReservationGroup(p, c)).ToResult()).Produces<UResponse<List<Guid>>>();
		r.MapPost("HotelReservation/Extend", async (HotelReservationExtendParams p, IHotelService s, CancellationToken c) => (await s.ExtendHotelReservation(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelReservation/ChangeRoom", async (HotelReservationChangeRoomParams p, IHotelService s, CancellationToken c) => (await s.ChangeHotelReservationRoom(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelReservation/GuestExport", async (HotelGuestExportParams p, IHotelService s, CancellationToken c) => (await s.ExportHotelGuests(p, c)).ToResult()).Produces<UResponse<IEnumerable<HotelGuestExportItem>>>();
		r.MapPost("HotelReservation/Print", async (IdParams p, IHotelService s, CancellationToken c) => (await s.PrintHotelReservation(p, c)).ToResult()).Produces<UResponse<string>>();
		r.MapPost("NightAudit/Read", async (HotelNightAuditParams p, IHotelService s, CancellationToken c) => (await s.ReadHotelNightAudit(p, c)).ToResult()).Produces<UResponse<HotelNightAuditResponse>>();
		r.MapPost("NightAudit/Close", async (HotelNightAuditParams p, IHotelService s, CancellationToken c) => (await s.CloseHotelNightAudit(p, c)).ToResult()).Produces<UResponse<HotelNightAuditResponse>>();
	}
}
