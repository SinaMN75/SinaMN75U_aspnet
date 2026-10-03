namespace SinaMN75U.Routes;

public static class VenueRoutes {
	public static void MapVenueRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();

		r.MapPost("Venue/Create", async (VenueCreateParams p, IVenueService s, CancellationToken c) => (await s.CreateVenue(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Venue/Read", async (VenueReadParams p, IVenueService s, CancellationToken c) => (await s.ReadVenues(p, c)).ToResult()).Produces<UResponse<IEnumerable<VenueResponse>>>();
		r.MapPost("Venue/ReadById", async (IdParams<VenueSelectorArgs> p, IVenueService s, CancellationToken c) => (await s.ReadVenueById(p, c)).ToResult()).Produces<UResponse<VenueResponse>>();
		r.MapPost("Venue/Update", async (VenueUpdateParams p, IVenueService s, CancellationToken c) => (await s.UpdateVenue(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Venue/Delete", async (IdParams p, IVenueService s, CancellationToken c) => (await s.DeleteVenue(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Venue/Stats", async (VenueStatsParams p, IVenueService s, CancellationToken c) => (await s.ReadVenueStats(p, c)).ToResult()).Produces<UResponse<VenueStatsResponse>>();

		r.MapPost("Court/Create", async (CourtCreateParams p, IVenueService s, CancellationToken c) => (await s.CreateCourt(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Court/Read", async (CourtReadParams p, IVenueService s, CancellationToken c) => (await s.ReadCourts(p, c)).ToResult()).Produces<UResponse<IEnumerable<CourtResponse>>>();
		r.MapPost("Court/Update", async (CourtUpdateParams p, IVenueService s, CancellationToken c) => (await s.UpdateCourt(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Court/Delete", async (IdParams p, IVenueService s, CancellationToken c) => (await s.DeleteCourt(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Court/Availability", async (CourtAvailabilityParams p, IVenueService s, CancellationToken c) => (await s.ReadCourtAvailability(p, c)).ToResult()).Produces<UResponse<IEnumerable<CourtAvailabilityResponse>>>();

		r.MapPost("Booking/Create", async (BookingCreateParams p, IVenueService s, CancellationToken c) => (await s.CreateBooking(p, c)).ToResult()).Produces<UResponse<BookingResponse>>();
		r.MapPost("Booking/Read", async (BookingReadParams p, IVenueService s, CancellationToken c) => (await s.ReadBookings(p, c)).ToResult()).Produces<UResponse<IEnumerable<BookingResponse>>>();
		r.MapPost("Booking/Update", async (BookingUpdateParams p, IVenueService s, CancellationToken c) => (await s.UpdateBooking(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Booking/Cancel", async (BookingCancelParams p, IVenueService s, CancellationToken c) => (await s.CancelBooking(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Booking/PayShare", async (IdParams p, IVenueService s, CancellationToken c) => (await s.PayBookingShare(p, c)).ToResult()).Produces<UResponse>();
	}
}
