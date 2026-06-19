namespace SinaMN75U.Routes;

public static class HotelRoutes {
	public static void MapHotelRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();

		// Hotel
		r.MapPost("Hotel/Create", async (HotelCreateParams p, IHotelService s, CancellationToken c) => (await s.CreateHotel(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Hotel/Read", async (HotelReadParams p, IHotelService s, CancellationToken c) => (await s.ReadHotels(p, c)).ToResult()).Produces<UResponse<IEnumerable<HotelResponse>>>();
		r.MapPost("Hotel/ReadById", async (IdParams<HotelSelectorArgs> p, IHotelService s, CancellationToken c) => (await s.ReadHotelById(p, c)).ToResult()).Produces<UResponse<HotelResponse>>();
		r.MapPost("Hotel/Update", async (HotelUpdateParams p, IHotelService s, CancellationToken c) => (await s.UpdateHotel(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Hotel/Delete", async (IdParams p, IHotelService s, CancellationToken c) => (await s.DeleteHotel(p, c)).ToResult()).Produces<UResponse>();

		// HotelRoom
		r.MapPost("HotelRoom/Create", async (HotelRoomCreateParams p, IHotelService s, CancellationToken c) => (await s.CreateHotelRoom(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("HotelRoom/Read", async (HotelRoomReadParams p, IHotelService s, CancellationToken c) => (await s.ReadHotelRooms(p, c)).ToResult()).Produces<UResponse<IEnumerable<HotelRoomResponse>>>();
		r.MapPost("HotelRoom/ReadById", async (IdParams<HotelRoomSelectorArgs> p, IHotelService s, CancellationToken c) => (await s.ReadHotelRoomById(p, c)).ToResult()).Produces<UResponse<HotelRoomResponse>>();
		r.MapPost("HotelRoom/Update", async (HotelRoomUpdateParams p, IHotelService s, CancellationToken c) => (await s.UpdateHotelRoom(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelRoom/Delete", async (IdParams p, IHotelService s, CancellationToken c) => (await s.DeleteHotelRoom(p, c)).ToResult()).Produces<UResponse>();

		// Dorm
		r.MapPost("Dorm/Create", async (DormCreateParams p, IHotelService s, CancellationToken c) => (await s.CreateDorm(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Dorm/Read", async (DormReadParams p, IHotelService s, CancellationToken c) => (await s.ReadDorms(p, c)).ToResult()).Produces<UResponse<IEnumerable<DormResponse>>>();
		r.MapPost("Dorm/ReadById", async (IdParams<DormSelectorArgs> p, IHotelService s, CancellationToken c) => (await s.ReadDormById(p, c)).ToResult()).Produces<UResponse<DormResponse>>();
		r.MapPost("Dorm/Update", async (DormUpdateParams p, IHotelService s, CancellationToken c) => (await s.UpdateDorm(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Dorm/Delete", async (IdParams p, IHotelService s, CancellationToken c) => (await s.DeleteDorm(p, c)).ToResult()).Produces<UResponse>();

		// DormRoom
		r.MapPost("DormRoom/Create", async (DormRoomCreateParams p, IHotelService s, CancellationToken c) => (await s.CreateDormRoom(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("DormRoom/Read", async (DormRoomReadParams p, IHotelService s, CancellationToken c) => (await s.ReadDormRooms(p, c)).ToResult()).Produces<UResponse<IEnumerable<DormRoomResponse>>>();
		r.MapPost("DormRoom/ReadById", async (IdParams<DormRoomSelectorArgs> p, IHotelService s, CancellationToken c) => (await s.ReadDormRoomById(p, c)).ToResult()).Produces<UResponse<DormRoomResponse>>();
		r.MapPost("DormRoom/Update", async (DormRoomUpdateParams p, IHotelService s, CancellationToken c) => (await s.UpdateDormRoom(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormRoom/Delete", async (IdParams p, IHotelService s, CancellationToken c) => (await s.DeleteDormRoom(p, c)).ToResult()).Produces<UResponse>();

		// DormBed
		r.MapPost("DormBed/Create", async (DormBedCreateParams p, IHotelService s, CancellationToken c) => (await s.CreateDormBed(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("DormBed/Read", async (DormBedReadParams p, IHotelService s, CancellationToken c) => (await s.ReadDormBeds(p, c)).ToResult()).Produces<UResponse<IEnumerable<DormBedResponse>>>();
		r.MapPost("DormBed/ReadById", async (IdParams<DormBedSelectorArgs> p, IHotelService s, CancellationToken c) => (await s.ReadDormBedById(p, c)).ToResult()).Produces<UResponse<DormBedResponse>>();
		r.MapPost("DormBed/Update", async (DormBedUpdateParams p, IHotelService s, CancellationToken c) => (await s.UpdateDormBed(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormBed/Delete", async (IdParams p, IHotelService s, CancellationToken c) => (await s.DeleteDormBed(p, c)).ToResult()).Produces<UResponse>();
	}
}