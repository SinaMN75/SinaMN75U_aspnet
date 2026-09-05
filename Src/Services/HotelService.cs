namespace SinaMN75U.Services;

public interface IHotelService {
	// Hotel
	public Task<UResponse<Guid?>> CreateHotel(HotelCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<HotelResponse>?>> ReadHotels(HotelReadParams p, CancellationToken ct);
	public Task<UResponse<HotelResponse?>> ReadHotelById(IdParams<HotelSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> UpdateHotel(HotelUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteHotel(IdParams p, CancellationToken ct);

	// HotelRoom
	public Task<UResponse<Guid?>> CreateHotelRoom(HotelRoomCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<HotelRoomResponse>?>> ReadHotelRooms(HotelRoomReadParams p, CancellationToken ct);
	public Task<UResponse<HotelRoomResponse?>> ReadHotelRoomById(IdParams<HotelRoomSelectorArgs> p, CancellationToken ct);
	public Task<UResponse<IEnumerable<HotelRoomAvailabilityResponse>?>> ReadHotelRoomAvailability(HotelRoomAvailabilityParams p, CancellationToken ct);
	public Task<UResponse> UpdateHotelRoom(HotelRoomUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteHotelRoom(IdParams p, CancellationToken ct);

	// HotelReservation
	public Task<UResponse<Guid?>> CreateHotelReservation(HotelReservationCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<HotelReservationResponse>?>> ReadHotelReservations(HotelReservationReadParams p, CancellationToken ct);
	public Task<UResponse<HotelReservationResponse?>> ReadHotelReservationById(IdParams<HotelReservationSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> UpdateHotelReservation(HotelReservationUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteHotelReservation(IdParams p, CancellationToken ct);
	public Task<UResponse> ConfirmHotelReservation(IdParams p, CancellationToken ct);
	public Task<UResponse> CheckInHotelReservation(IdParams p, CancellationToken ct);
	public Task<UResponse> CheckOutHotelReservation(IdParams p, CancellationToken ct);
	public Task<UResponse> CancelHotelReservation(IdParams p, CancellationToken ct);
	public Task<UResponse<HotelReservationResponse?>> BookHotelReservation(HotelReservationBookParams p, CancellationToken ct);
	public Task<UResponse> CancelHotelReservationByUser(HotelReservationCancelParams p, CancellationToken ct);

	// HotelInvoice
	public Task<UResponse<Guid?>> CreateHotelInvoice(HotelInvoiceCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<HotelInvoiceResponse>?>> ReadHotelInvoices(HotelInvoiceReadParams p, CancellationToken ct);
	public Task<UResponse> UpdateHotelInvoice(HotelInvoiceUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteHotelInvoice(IdParams p, CancellationToken ct);
	public Task<UResponse> PayHotelInvoice(IdParams p, CancellationToken ct);
	public Task<UResponse> PayHotelInvoiceInternal(HotelInvoicePayParams p, CancellationToken ct);

	// Dorm
	public Task<UResponse<Guid?>> CreateDorm(DormCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<DormResponse>?>> ReadDorms(DormReadParams p, CancellationToken ct);
	public Task<UResponse<DormResponse?>> ReadDormById(IdParams<DormSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> UpdateDorm(DormUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteDorm(IdParams p, CancellationToken ct);

	// DormRoom
	public Task<UResponse<Guid?>> CreateDormRoom(DormRoomCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<DormRoomResponse>?>> ReadDormRooms(DormRoomReadParams p, CancellationToken ct);
	public Task<UResponse<DormRoomResponse?>> ReadDormRoomById(IdParams<DormRoomSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> UpdateDormRoom(DormRoomUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteDormRoom(IdParams p, CancellationToken ct);

	// DormBed
	public Task<UResponse<Guid?>> CreateDormBed(DormBedCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<DormBedResponse>?>> ReadDormBeds(DormBedReadParams p, CancellationToken ct);
	public Task<UResponse<DormBedResponse?>> ReadDormBedById(IdParams<DormBedSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> UpdateDormBed(DormBedUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteDormBed(IdParams p, CancellationToken ct);

	// DormBedContract
	public Task<UResponse<Guid?>> CreateDormBedContract(DormBedContractCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<DormBedContractResponse>?>> ReadDormBedContracts(DormBedContractReadParams p, CancellationToken ct);
	public Task<UResponse> UpdateDormBedContract(DormBedContractUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteDormBedContract(IdParams p, CancellationToken ct);

	// DormBedInvoice
	public Task<UResponse<Guid?>> CreateDormBedInvoice(DormBedInvoiceCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<DormBedInvoiceResponse>?>> ReadDormBedInvoices(DormBedInvoiceReadParams p, CancellationToken ct);
	public Task<UResponse> UpdateDormBedInvoice(DormBedInvoiceUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteDormBedInvoice(IdParams p, CancellationToken ct);
	public Task<UResponse> PayDormBedInvoice(DormBedInvoicePayParams p, CancellationToken ct);
	public Task<UResponse> PayDormBedInvoiceByUser(IdParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<DormBedInvoiceChartResponse>?>> ReadDormBedInvoiceChartData(BaseParams p, CancellationToken ct);
}

public class HotelService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IWalletService ws
) : IHotelService {
	// Managers see what they own; everyone else only sees the public catalogue of active places.
	private static bool IsHotelManager(JwtClaimData? u) => u != null && (u.IsSuperAdmin || u.HasPermission(TagUser.PermissionManageHotels));

	private static bool IsDormManager(JwtClaimData? u) => u != null && (u.IsSuperAdmin || u.HasPermission(TagUser.PermissionManageDorms));

	private static Guid UserIdOf(JwtClaimData? u) => u?.Id ?? Guid.Empty;

	// ===================== Hotel =====================

	public async Task<UResponse<Guid?>> CreateHotel(HotelCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.HasPermission(TagUser.PermissionManageHotels)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		HotelEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new HotelJson {
				Description = p.Description,
				Policies = p.Policies,
				CheckInTime = p.CheckInTime,
				CheckOutTime = p.CheckOutTime,
				Amenities = p.Amenities ?? [],
				Rules = p.Rules ?? [],
				Latitude = p.Latitude,
				Longitude = p.Longitude,
				CancellationFreeHours = p.CancellationFreeHours ?? 24,
				CancellationPenaltyNights = p.CancellationPenaltyNights ?? 1
			},
			Tags = p.Tags,
			Title = p.Title,
			CityCode = p.CityCode,
			Stars = p.Stars,
			Address = p.Address,
			PhoneNumber = p.PhoneNumber,
			Email = p.Email,
			AdminUserIds = p.AdminUserIds ?? []
		};

		await db.Set<HotelEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<HotelResponse>?>> ReadHotels(HotelReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<HotelEntity> q = db.Set<HotelEntity>().ApplyReadParams(p);
		q = IsHotelManager(userData) ? q.ApplyOwnerScope<HotelEntity, TagHotel>(userData) : q.Where(x => x.Tags.Contains(TagHotel.Active));

		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title!));
		if (p.CityCode.IsNotNullOrEmpty()) q = q.Where(x => x.CityCode == p.CityCode);
		if (p.MinStars.HasValue) q = q.Where(x => x.Stars >= p.MinStars);

		IQueryable<HotelResponse> projected = q.Select(Projections.HotelSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<HotelResponse?>> ReadHotelById(IdParams<HotelSelectorArgs> p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<HotelEntity> hotels = db.Set<HotelEntity>();
		hotels = IsHotelManager(userData) ? hotels.ApplyOwnerScope<HotelEntity, TagHotel>(userData) : hotels.Where(x => x.Tags.Contains(TagHotel.Active));
		HotelResponse? e = await hotels.Select(Projections.HotelSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<HotelResponse?>(null, Usc.NotFound, ls.Get("hotelNotFound")) : new UResponse<HotelResponse?>(e);
	}

	public async Task<UResponse> UpdateHotel(HotelUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		HotelEntity? e = await db.Set<HotelEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("hotelNotFound"));

		if (!userData.CanManage(e.CreatorId, e.AdminUserIds) || !userData.HasPermission(TagUser.PermissionManageHotels)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		if (p.CityCode.IsNotNullOrEmpty()) e.CityCode = p.CityCode;
		if (p.Stars.HasValue) e.Stars = p.Stars.Value;
		if (p.Address.IsNotNullOrEmpty()) e.Address = p.Address;
		if (p.PhoneNumber.IsNotNullOrEmpty()) e.PhoneNumber = p.PhoneNumber;
		if (p.Email.IsNotNullOrEmpty()) e.Email = p.Email;
		if (p.Description.IsNotNullOrEmpty()) e.JsonData.Description = p.Description;
		if (p.Policies.IsNotNullOrEmpty()) e.JsonData.Policies = p.Policies;
		if (p.CheckInTime.IsNotNullOrEmpty()) e.JsonData.CheckInTime = p.CheckInTime;
		if (p.CheckOutTime.IsNotNullOrEmpty()) e.JsonData.CheckOutTime = p.CheckOutTime;
		if (p.Amenities != null) e.JsonData.Amenities = p.Amenities;
		if (p.Rules != null) e.JsonData.Rules = p.Rules;
		if (p.Latitude.HasValue) e.JsonData.Latitude = p.Latitude;
		if (p.Longitude.HasValue) e.JsonData.Longitude = p.Longitude;
		if (p.CancellationFreeHours.HasValue) e.JsonData.CancellationFreeHours = p.CancellationFreeHours.Value;
		if (p.CancellationPenaltyNights.HasValue) e.JsonData.CancellationPenaltyNights = p.CancellationPenaltyNights.Value;

		e.ApplyUpdateParam<HotelEntity, TagHotel, HotelJson>(p);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> DeleteHotel(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		HotelEntity? e = await db.Set<HotelEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("hotelNotFound"));

		if (!userData.CanManage(e.CreatorId, e.AdminUserIds) || !userData.HasPermission(TagUser.PermissionDeleteHotels)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		db.Set<HotelEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	// ===================== HotelRoom =====================

	public async Task<UResponse<Guid?>> CreateHotelRoom(HotelRoomCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		HotelEntity? hotel = await db.Set<HotelEntity>().FirstOrDefaultAsync(x => x.Id == p.HotelId, ct);
		if (hotel == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("hotelNotFound"));
		if (!userData.CanManage(hotel.CreatorId, hotel.AdminUserIds) || !userData.HasPermission(TagUser.PermissionManageHotels)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		HotelRoomEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new HotelRoomJson {
				Description = p.Description,
				BedType = p.BedType,
				SizeSquareMeters = p.SizeSquareMeters,
				Floor = p.Floor,
				Amenities = p.Amenities ?? [],
				ExtraGuestCapacity = p.ExtraGuestCapacity,
				ExtraGuestPrice = p.ExtraGuestPrice
			},
			Tags = p.Tags,
			Title = p.Title,
			Capacity = p.Capacity,
			PricePerNight = p.PricePerNight,
			RoomNumber = p.RoomNumber,
			Quantity = p.Quantity,
			IsAvailable = p.IsAvailable,
			HotelId = p.HotelId
		};

		await db.Set<HotelRoomEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<HotelRoomResponse>?>> ReadHotelRooms(HotelRoomReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<HotelRoomEntity> q = db.Set<HotelRoomEntity>().ApplyReadParams(p);
		if (IsHotelManager(userData)) {
			if (userData is not { IsSuperAdmin: true }) {
				Guid uid = UserIdOf(userData);
				q = q.Where(x => x.Hotel.CreatorId == uid || x.Hotel.AdminUserIds.Contains(uid));
			}
		}
		else
			q = q.Where(x => x.Hotel.Tags.Contains(TagHotel.Active) && x.IsAvailable);

		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title!));
		if (p.HotelId.HasValue) q = q.Where(x => x.HotelId == p.HotelId);
		if (p.MinCapacity.HasValue) q = q.Where(x => x.Capacity >= p.MinCapacity);
		if (p.MaxCapacity.HasValue) q = q.Where(x => x.Capacity <= p.MaxCapacity);
		if (p.MinPrice.HasValue) q = q.Where(x => x.PricePerNight >= p.MinPrice);
		if (p.MaxPrice.HasValue) q = q.Where(x => x.PricePerNight <= p.MaxPrice);
		if (p.AvailableOnly == true) q = q.Where(x => x.IsAvailable);

		IQueryable<HotelRoomResponse> projected = q.Select(Projections.HotelRoomSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<HotelRoomResponse?>> ReadHotelRoomById(IdParams<HotelRoomSelectorArgs> p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<HotelRoomEntity> q = db.Set<HotelRoomEntity>();
		if (IsHotelManager(userData)) {
			if (userData is not { IsSuperAdmin: true }) {
				Guid uid = UserIdOf(userData);
				q = q.Where(x => x.Hotel.CreatorId == uid || x.Hotel.AdminUserIds.Contains(uid));
			}
		}
		else
			q = q.Where(x => x.Hotel.Tags.Contains(TagHotel.Active));

		HotelRoomResponse? e = await q.Select(Projections.HotelRoomSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<HotelRoomResponse?>(null, Usc.NotFound, ls.Get("hotelRoomNotFound")) : new UResponse<HotelRoomResponse?>(e);
	}

	public async Task<UResponse> UpdateHotelRoom(HotelRoomUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		HotelRoomEntity? e = await db.Set<HotelRoomEntity>().AsTracking().Include(x => x.Hotel).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("hotelRoomNotFound"));

		if ((!userData.CanManage(e.CreatorId, []) && !userData.CanManage(e.Hotel.CreatorId, e.Hotel.AdminUserIds)) || !userData.HasPermission(TagUser.PermissionManageHotels)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		if (p.Capacity.HasValue) e.Capacity = p.Capacity.Value;
		if (p.PricePerNight.HasValue) e.PricePerNight = p.PricePerNight.Value;
		if (p.HotelId.HasValue) e.HotelId = p.HotelId.Value;
		if (p.RoomNumber.IsNotNullOrEmpty()) e.RoomNumber = p.RoomNumber;
		if (p.Quantity.HasValue) e.Quantity = p.Quantity.Value;
		if (p.IsAvailable.HasValue) e.IsAvailable = p.IsAvailable.Value;
		if (p.Description.IsNotNullOrEmpty()) e.JsonData.Description = p.Description;
		if (p.BedType.IsNotNullOrEmpty()) e.JsonData.BedType = p.BedType;
		if (p.SizeSquareMeters.HasValue) e.JsonData.SizeSquareMeters = p.SizeSquareMeters;
		if (p.Floor.HasValue) e.JsonData.Floor = p.Floor;
		if (p.Amenities != null) e.JsonData.Amenities = p.Amenities;
		if (p.ExtraGuestCapacity.HasValue) e.JsonData.ExtraGuestCapacity = p.ExtraGuestCapacity;
		if (p.ExtraGuestPrice.HasValue) e.JsonData.ExtraGuestPrice = p.ExtraGuestPrice;

		e.ApplyUpdateParam<HotelRoomEntity, TagRoom, HotelRoomJson>(p);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> DeleteHotelRoom(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		HotelRoomEntity? e = await db.Set<HotelRoomEntity>().Include(x => x.Hotel).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("hotelRoomNotFound"));

		if ((!userData.CanManage(e.CreatorId, []) && !userData.CanManage(e.Hotel.CreatorId, e.Hotel.AdminUserIds)) || !userData.HasPermission(TagUser.PermissionDeleteHotels)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		db.Set<HotelRoomEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	// ===================== HotelReservation =====================

	public async Task<UResponse<Guid?>> CreateHotelReservation(HotelReservationCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		HotelRoomEntity? room = await db.Set<HotelRoomEntity>().Include(x => x.Hotel).FirstOrDefaultAsync(x => x.Id == p.RoomId, ct);
		if (room == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("hotelRoomNotFound"));
		if ((!userData.CanManage(room.CreatorId, []) && !userData.CanManage(room.Hotel.CreatorId, room.Hotel.AdminUserIds)) || !userData.HasPermission(TagUser.PermissionManageReservations))
			return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		if (!room.IsAvailable) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("thisRoomIsNotAvailableForBooking"));

		int nights = (p.CheckOutDate.Date - p.CheckInDate.Date).Days;
		if (nights < 1) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("checkOutDateMustBeAfterTheCheckInDate"));

		UserEntity? user = await db.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == p.UserId, ct);
		if (user == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("accountNotFound"));

		// Count overlapping, still-blocking reservations for this room type.
		int overlapping = await db.Set<HotelReservationEntity>().CountAsync(r =>
			r.RoomId == room.Id &&
			r.CheckInDate < p.CheckOutDate &&
			r.CheckOutDate > p.CheckInDate &&
			!r.Tags.Contains(TagHotelReservation.Cancelled) &&
			!r.Tags.Contains(TagHotelReservation.NoShow) &&
			!r.Tags.Contains(TagHotelReservation.CheckedOut), ct);
		if (overlapping >= room.Quantity) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("thisRoomIsAlreadyBookedForTheSelectedDates"));

		decimal total = p.TotalPrice ?? nights * room.PricePerNight;

		Guid reservationId = p.Id ?? Guid.CreateVersion7();
		HotelReservationEntity e = new() {
			Id = reservationId,
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = p.Tags,
			CheckInDate = p.CheckInDate,
			CheckOutDate = p.CheckOutDate,
			GuestCount = p.GuestCount,
			TotalPrice = total,
			UserId = user.Id,
			RoomId = room.Id,
			HotelId = room.HotelId,
			AdminUserIds = p.AdminUserIds ?? [],
			JsonData = new HotelReservationJson {
				GuestName = p.GuestName,
				GuestPhone = p.GuestPhone,
				Notes = p.Notes,
				NightCount = nights,
				ReservationCode = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(),
				Guests = (p.Guests ?? []).Select(g => new ReservationGuestJson {
					FullName = g.FullName,
					NationalCode = g.NationalCode,
					PhoneNumber = g.PhoneNumber
				}).ToList()
			}
		};
		await db.Set<HotelReservationEntity>().AddAsync(e, ct);

		// Generate the reservation's invoice.
		await db.Set<HotelInvoiceEntity>().AddAsync(new HotelInvoiceEntity {
			Id = Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = [TagHotelInvoice.NotPaid, TagHotelInvoice.Full],
			DebtAmount = total,
			CreditorAmount = 0,
			PaidAmount = 0,
			PenaltyAmount = 0,
			ReservationId = reservationId,
			DueDate = p.CheckInDate,
			JsonData = new HotelInvoiceJson { PenaltyPrecentEveryDate = p.PenaltyPrecentEveryDate }
		}, ct);

		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<HotelReservationResponse>?>> ReadHotelReservations(HotelReservationReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<HotelReservationEntity> q = db.Set<HotelReservationEntity>().ApplyReadParams(p);
		if (userData is not { IsSuperAdmin: true }) {
			Guid uid = userData?.Id ?? Guid.Empty;
			q = q.Where(x =>
				x.UserId == uid ||
				x.Hotel.CreatorId == uid ||
				x.Hotel.AdminUserIds.Contains(uid));
		}

		if (p.UserId.IsNotNull()) q = q.Where(x => x.UserId == p.UserId);
		if (p.RoomId.IsNotNull()) q = q.Where(x => x.RoomId == p.RoomId);
		if (p.HotelId.IsNotNull()) q = q.Where(x => x.HotelId == p.HotelId);
		if (p.UserName.IsNotNullOrEmpty()) q = q.Where(x => x.User.UserName.Contains(p.UserName!));
		if (p.CheckInDate.HasValue) q = q.Where(x => x.CheckInDate >= p.CheckInDate);
		if (p.CheckOutDate.HasValue) q = q.Where(x => x.CheckOutDate <= p.CheckOutDate);

		DateTime now = DateTime.UtcNow;
		if (p.ActiveOnly == true) q = q.Where(x => x.CheckInDate <= now && x.CheckOutDate >= now);
		if (p.UpcomingOnly == true) q = q.Where(x => x.CheckInDate > now);
		if (p.PastOnly == true) q = q.Where(x => x.CheckOutDate < now);

		IQueryable<HotelReservationResponse> projected = q.Select(Projections.HotelReservationSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<HotelReservationResponse?>> ReadHotelReservationById(IdParams<HotelReservationSelectorArgs> p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<HotelReservationEntity> q = db.Set<HotelReservationEntity>();
		if (userData is not { IsSuperAdmin: true }) {
			Guid uid = userData?.Id ?? Guid.Empty;
			q = q.Where(x => x.UserId == uid || x.Hotel.CreatorId == uid || x.Hotel.AdminUserIds.Contains(uid));
		}

		HotelReservationResponse? e = await q.Select(Projections.HotelReservationSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<HotelReservationResponse?>(null, Usc.NotFound, ls.Get("reservationNotFound")) : new UResponse<HotelReservationResponse?>(e);
	}

	public async Task<UResponse> UpdateHotelReservation(HotelReservationUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		HotelReservationEntity? e = await db.Set<HotelReservationEntity>().AsTracking().Include(x => x.Hotel).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("reservationNotFound"));
		if ((!userData.CanManage(e.CreatorId, []) && !userData.CanManage(e.Hotel.CreatorId, e.Hotel.AdminUserIds)) || !userData.HasPermission(TagUser.PermissionManageReservations))
			return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		if (p.CheckInDate.HasValue) e.CheckInDate = p.CheckInDate.Value;
		if (p.CheckOutDate.HasValue) e.CheckOutDate = p.CheckOutDate.Value;
		if (p.GuestCount.HasValue) e.GuestCount = p.GuestCount.Value;
		if (p.TotalPrice.HasValue) e.TotalPrice = p.TotalPrice.Value;
		if (p.GuestName.IsNotNullOrEmpty()) e.JsonData.GuestName = p.GuestName;
		if (p.GuestPhone.IsNotNullOrEmpty()) e.JsonData.GuestPhone = p.GuestPhone;
		if (p.Notes.IsNotNullOrEmpty()) e.JsonData.Notes = p.Notes;
		if (p.Guests != null)
			e.JsonData.Guests = p.Guests.Select(g => new ReservationGuestJson {
				FullName = g.FullName,
				NationalCode = g.NationalCode,
				PhoneNumber = g.PhoneNumber
			}).ToList();
		e.JsonData.NightCount = (e.CheckOutDate.Date - e.CheckInDate.Date).Days;

		e.ApplyUpdateParam<HotelReservationEntity, TagHotelReservation, HotelReservationJson>(p);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteHotelReservation(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		HotelReservationEntity? e = await db.Set<HotelReservationEntity>().Include(x => x.Hotel).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("reservationNotFound"));
		if ((!userData.CanManage(e.CreatorId, []) && !userData.CanManage(e.Hotel.CreatorId, e.Hotel.AdminUserIds)) || !userData.HasPermission(TagUser.PermissionDeleteReservations))
			return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		await db.Set<HotelReservationEntity>().Where(x => x.Id == p.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	private async Task<UResponse> TransitionReservation(IdParams p, TagHotelReservation status, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		HotelReservationEntity? e = await db.Set<HotelReservationEntity>().AsTracking().Include(x => x.Hotel).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("reservationNotFound"));
		if ((!userData.CanManage(e.CreatorId, []) && !userData.CanManage(e.Hotel.CreatorId, e.Hotel.AdminUserIds)) || !userData.HasPermission(TagUser.PermissionManageReservations))
			return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		e.Tags = [status];
		db.Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public Task<UResponse> ConfirmHotelReservation(IdParams p, CancellationToken ct) => TransitionReservation(p, TagHotelReservation.Confirmed, ct);
	public Task<UResponse> CheckInHotelReservation(IdParams p, CancellationToken ct) => TransitionReservation(p, TagHotelReservation.CheckedIn, ct);
	public Task<UResponse> CheckOutHotelReservation(IdParams p, CancellationToken ct) => TransitionReservation(p, TagHotelReservation.CheckedOut, ct);
	public Task<UResponse> CancelHotelReservation(IdParams p, CancellationToken ct) => TransitionReservation(p, TagHotelReservation.Cancelled, ct);

	// Availability and pricing use one rule so the quote the user sees equals what booking charges.
	private static decimal ComputeStayPrice(HotelRoomEntity room, int nights, int guestCount) {
		decimal total = nights * room.PricePerNight;
		int extraGuests = Math.Max(0, guestCount - room.Capacity);
		if (extraGuests > 0 && room.JsonData.ExtraGuestPrice.HasValue) total += extraGuests * room.JsonData.ExtraGuestPrice.Value * nights;
		return total;
	}

	private async Task<Dictionary<Guid, int>> ReadBookedCounts(List<Guid> roomIds, DateTime checkIn, DateTime checkOut, CancellationToken ct) =>
		await db.Set<HotelReservationEntity>()
			.Where(r => roomIds.Contains(r.RoomId) &&
			            r.CheckInDate < checkOut &&
			            r.CheckOutDate > checkIn &&
			            !r.Tags.Contains(TagHotelReservation.Cancelled) &&
			            !r.Tags.Contains(TagHotelReservation.NoShow) &&
			            !r.Tags.Contains(TagHotelReservation.CheckedOut))
			.GroupBy(r => r.RoomId)
			.Select(g => new { RoomId = g.Key, Count = g.Count() })
			.ToDictionaryAsync(x => x.RoomId, x => x.Count, ct);

	private async Task AddNotification(Guid userId, TagNotification tag, string title, string body, CancellationToken ct) =>
		await db.Set<NotificationEntity>().AddAsync(new NotificationEntity {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = userId,
			UserId = userId,
			Tags = [tag, TagNotification.Unread],
			JsonData = new BaseJson { Detail1 = title, Detail2 = body }
		}, ct);

	public async Task<UResponse<IEnumerable<HotelRoomAvailabilityResponse>?>> ReadHotelRoomAvailability(HotelRoomAvailabilityParams p, CancellationToken ct) {
		int nights = (p.CheckOutDate.Date - p.CheckInDate.Date).Days;
		if (nights < 1) return new UResponse<IEnumerable<HotelRoomAvailabilityResponse>?>(null, Usc.BadRequest, ls.Get("checkOutDateMustBeAfterTheCheckInDate"));

		IQueryable<HotelRoomEntity> q = db.Set<HotelRoomEntity>()
			.Where(x => x.IsAvailable && x.Hotel.Tags.Contains(TagHotel.Active));
		if (p.HotelId.HasValue) q = q.Where(x => x.HotelId == p.HotelId);
		if (p.RoomId.HasValue) q = q.Where(x => x.Id == p.RoomId);

		List<HotelRoomEntity> rooms = await q.ToListAsync(ct);
		if (rooms.Count == 0) return new UResponse<IEnumerable<HotelRoomAvailabilityResponse>?>([]);

		Dictionary<Guid, int> booked = await ReadBookedCounts(rooms.Select(x => x.Id).ToList(), p.CheckInDate, p.CheckOutDate, ct);
		List<HotelRoomResponse> projected = await q.Select(Projections.HotelRoomSelector(p.SelectorArgs)).ToListAsync(ct);

		List<HotelRoomAvailabilityResponse> result = [];
		foreach (HotelRoomEntity room in rooms) {
			HotelRoomResponse? dto = projected.FirstOrDefault(x => x.Id == room.Id);
			if (dto == null) continue;
			int maxGuests = room.Capacity + (room.JsonData.ExtraGuestCapacity ?? 0);
			result.Add(new HotelRoomAvailabilityResponse {
				Room = dto,
				AvailableQuantity = Math.Max(0, room.Quantity - booked.GetValueOrDefault(room.Id, 0)),
				NightCount = nights,
				TotalPrice = ComputeStayPrice(room, nights, Math.Max(1, p.GuestCount)),
				FitsGuestCount = p.GuestCount <= maxGuests
			});
		}

		return new UResponse<IEnumerable<HotelRoomAvailabilityResponse>?>(result);
	}

	public async Task<UResponse<HotelReservationResponse?>> BookHotelReservation(HotelReservationBookParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<HotelReservationResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<HotelReservationResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		HotelRoomEntity? room = await db.Set<HotelRoomEntity>().Include(x => x.Hotel).FirstOrDefaultAsync(x => x.Id == p.RoomId, ct);
		if (room == null) return new UResponse<HotelReservationResponse?>(null, Usc.NotFound, ls.Get("hotelRoomNotFound"));
		if (!room.Hotel.Tags.Contains(TagHotel.Active)) return new UResponse<HotelReservationResponse?>(null, Usc.Conflict, ls.Get("thisHotelIsCurrentlyNotAcceptingReservations"));
		if (!room.IsAvailable) return new UResponse<HotelReservationResponse?>(null, Usc.Conflict, ls.Get("thisRoomIsNotAvailableForBooking"));

		int nights = (p.CheckOutDate.Date - p.CheckInDate.Date).Days;
		if (nights < 1) return new UResponse<HotelReservationResponse?>(null, Usc.BadRequest, ls.Get("checkOutDateMustBeAfterTheCheckInDate"));
		if (p.CheckInDate.Date < DateTime.UtcNow.Date) return new UResponse<HotelReservationResponse?>(null, Usc.BadRequest, ls.Get("theCheckInDateCannotBeInThePast"));

		int guestCount = Math.Max(1, p.GuestCount);
		if (guestCount > room.Capacity + (room.JsonData.ExtraGuestCapacity ?? 0)) return new UResponse<HotelReservationResponse?>(null, Usc.BadRequest, ls.Get("theNumberOfGuestsIsMoreThanThisRoomCanTake"));

		Dictionary<Guid, int> booked = await ReadBookedCounts([room.Id], p.CheckInDate, p.CheckOutDate, ct);
		if (booked.GetValueOrDefault(room.Id, 0) >= room.Quantity) return new UResponse<HotelReservationResponse?>(null, Usc.Conflict, ls.Get("thisRoomIsAlreadyBookedForTheSelectedDates"));

		decimal total = ComputeStayPrice(room, nights, guestCount);
		Guid reservationId = Guid.CreateVersion7();
		Guid invoiceId = Guid.CreateVersion7();

		HotelReservationEntity reservation = new() {
			Id = reservationId,
			CreatorId = userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = [TagHotelReservation.Pending],
			CheckInDate = p.CheckInDate,
			CheckOutDate = p.CheckOutDate,
			GuestCount = guestCount,
			TotalPrice = total,
			UserId = userData.Id,
			RoomId = room.Id,
			HotelId = room.HotelId,
			AdminUserIds = [],
			JsonData = new HotelReservationJson {
				GuestName = p.GuestName,
				GuestPhone = p.GuestPhone ?? userData.PhoneNumber,
				Notes = p.Notes,
				NightCount = nights,
				ReservationCode = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(),
				Guests = (p.Guests ?? []).Select(g => new ReservationGuestJson {
					FullName = g.FullName,
					NationalCode = g.NationalCode,
					PhoneNumber = g.PhoneNumber
				}).ToList()
			}
		};
		await db.Set<HotelReservationEntity>().AddAsync(reservation, ct);

		await db.Set<HotelInvoiceEntity>().AddAsync(new HotelInvoiceEntity {
			Id = invoiceId,
			CreatorId = userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = [TagHotelInvoice.NotPaid, TagHotelInvoice.Full],
			DebtAmount = total,
			CreditorAmount = 0,
			PaidAmount = 0,
			PenaltyAmount = 0,
			ReservationId = reservationId,
			DueDate = p.CheckInDate,
			JsonData = new HotelInvoiceJson { PenaltyPrecentEveryDate = 0 }
		}, ct);

		await AddNotification(userData.Id, TagNotification.ReservationCreated, ls.Get("reservationRegistered"), room.Hotel.Title, ct);
		await db.SaveChangesAsync(ct);

		if (p.PayFromWallet) {
			UResponse pay = await PayHotelInvoiceInternal(new HotelInvoicePayParams { InvoiceId = invoiceId, UserId = userData.Id }, ct);
			if (pay.Status != Usc.Success) return new UResponse<HotelReservationResponse?>(null, pay.Status, pay.Message);
		}

		HotelReservationResponse? created = await db.Set<HotelReservationEntity>()
			.Select(Projections.HotelReservationSelector(new HotelReservationSelectorArgs {
				Room = new HotelRoomSelectorArgs { Media = new MediaSelectorArgs() },
				Hotel = new HotelSelectorArgs { Media = new MediaSelectorArgs() },
				Invoice = new HotelInvoiceSelectorArgs()
			}))
			.FirstOrDefaultAsync(x => x.Id == reservationId, ct);

		return new UResponse<HotelReservationResponse?>(created, Usc.Created);
	}

	public async Task<UResponse> CancelHotelReservationByUser(HotelReservationCancelParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		HotelReservationEntity? e = await db.Set<HotelReservationEntity>().AsTracking()
			.Include(x => x.Hotel).Include(x => x.Room).Include(x => x.Invoices)
			.FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("reservationNotFound"));

		bool isOwner = e.UserId == userData.Id;
		if (!isOwner && !userData.CanManage(e.Hotel.CreatorId, e.Hotel.AdminUserIds)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (e.Tags.Contains(TagHotelReservation.Cancelled)) return new UResponse(Usc.Conflict, ls.Get("thisReservationHasAlreadyBeenCancelled"));
		if (e.Tags.Contains(TagHotelReservation.CheckedIn) || e.Tags.Contains(TagHotelReservation.CheckedOut)) return new UResponse(Usc.Conflict, ls.Get("aReservationThatHasAlreadyBeenCheckedInCannotBeCancelled"));

		double hoursToCheckIn = (e.CheckInDate - DateTime.UtcNow).TotalHours;
		decimal penalty = hoursToCheckIn >= e.Hotel.JsonData.CancellationFreeHours
			? 0
			: Math.Min(e.TotalPrice, e.Hotel.JsonData.CancellationPenaltyNights * e.Room.PricePerNight);

		decimal paid = e.Invoices.Sum(x => x.PaidAmount);
		decimal refund = Math.Max(0, paid - penalty);

		if (refund > 0) {
			UResponse<WalletTxnResponse?> transfer = await ws.Transfer(new WalletTransferParams {
				SenderId = Core.App.Users.SystemAdmin.Id,
				ReceiverId = e.UserId,
				Amount = refund,
				Detail1 = ls.Get("hotelReservationRefund"),
				TagWalletTxn = [TagWalletTxn.HotelReservationRefund]
			}, ct);
			if (transfer.Result == null) return new UResponse(transfer.Status, transfer.Message);
		}

		foreach (HotelInvoiceEntity invoice in e.Invoices) {
			invoice.PenaltyAmount = penalty;
			invoice.CreditorAmount = refund;
			if (refund > 0) invoice.Tags = [TagHotelInvoice.Refunded];
		}

		e.Tags = [TagHotelReservation.Cancelled];
		e.JsonData.CancelledAt = DateTime.UtcNow;
		e.JsonData.CancelReason = p.Reason;
		e.JsonData.CancellationPenalty = penalty;
		e.JsonData.RefundAmount = refund;

		await AddNotification(e.UserId, TagNotification.ReservationCancelled, ls.Get("reservationCancelled"), e.Hotel.Title, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse(Usc.Success, ls.Get("theReservationWasCancelled"));
	}

	public async Task<UResponse> PayHotelInvoiceInternal(HotelInvoicePayParams p, CancellationToken ct) {
		HotelInvoiceEntity? e = await db.Set<HotelInvoiceEntity>().AsTracking()
			.Include(x => x.Reservation).ThenInclude(x => x!.Hotel)
			.FirstOrDefaultAsync(x => x.Id == p.InvoiceId, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("invoiceNotFound"));
		if (!e.Tags.Contains(TagHotelInvoice.NotPaid)) return new UResponse(Usc.Conflict, ls.Get("thisInvoiceHasAlreadyBeenPaid"));

		decimal amount = e.DebtAmount + e.PenaltyAmount - e.CreditorAmount;
		if (amount > 0) {
			UResponse<WalletTxnResponse?> transfer = await ws.Transfer(new WalletTransferParams {
				SenderId = p.UserId,
				ReceiverId = Core.App.Users.SystemAdmin.Id,
				Amount = amount,
				Detail1 = ls.Get("hotelReservationPayment"),
				TagWalletTxn = [TagWalletTxn.HotelReservation]
			}, ct);
			if (transfer.Result == null) return new UResponse(transfer.Status, transfer.Message);
		}

		e.PaidAmount = amount;
		e.Tags = [TagHotelInvoice.PaidOnline];

		if (e.Reservation != null) {
			HotelReservationEntity? reservation = await db.Set<HotelReservationEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == e.ReservationId, ct);
			if (reservation != null && !reservation.Tags.Contains(TagHotelReservation.Cancelled)) {
				reservation.Tags = [TagHotelReservation.Confirmed];
				await AddNotification(reservation.UserId, TagNotification.ReservationConfirmed, ls.Get("reservationConfirmed"), e.Reservation.Hotel.Title, ct);
			}
		}

		await db.SaveChangesAsync(ct);
		return new UResponse(Usc.Success, ls.Get("paymentCompleted"));
	}

	// ===================== HotelInvoice =====================

	public async Task<UResponse<Guid?>> CreateHotelInvoice(HotelInvoiceCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		HotelReservationEntity? reservation = await db.Set<HotelReservationEntity>().Include(x => x.Hotel).FirstOrDefaultAsync(x => x.Id == p.ReservationId, ct);
		if (reservation == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("reservationNotFound"));
		if ((!userData.CanManage(reservation.CreatorId, []) && !userData.CanManage(reservation.Hotel.CreatorId, reservation.Hotel.AdminUserIds)) || !userData.HasPermission(TagUser.PermissionManageInvoices))
			return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		EntityEntry<HotelInvoiceEntity> e = await db.AddAsync(new HotelInvoiceEntity {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = p.Tags,
			DebtAmount = p.DebtAmount,
			CreditorAmount = p.CreditorAmount,
			PaidAmount = p.PaidAmount,
			PenaltyAmount = p.PenaltyAmount,
			ReservationId = p.ReservationId,
			DueDate = p.DueDate,
			JsonData = new HotelInvoiceJson {
				Detail1 = p.Detail1,
				Detail2 = p.Detail2,
				PenaltyPrecentEveryDate = p.PenaltyPrecentEveryDate
			}
		}, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Entity.Id);
	}

	public async Task<UResponse<IEnumerable<HotelInvoiceResponse>?>> ReadHotelInvoices(HotelInvoiceReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<HotelInvoiceEntity> q = db.Set<HotelInvoiceEntity>().Include(x => x.Reservation).ApplyReadParams(p);
		if (userData is not { IsSuperAdmin: true }) {
			Guid uid = userData?.Id ?? Guid.Empty;
			q = q.Where(x =>
				x.Reservation != null && (
					x.Reservation.UserId == uid ||
					x.Reservation.Hotel.CreatorId == uid ||
					x.Reservation.Hotel.AdminUserIds.Contains(uid)));
		}

		if (p.UserId.IsNotNull()) q = q.Where(x => x.Reservation!.UserId == p.UserId);
		if (p.ReservationId.IsNotNull()) q = q.Where(x => x.ReservationId == p.ReservationId);
		if (p.HotelId.IsNotNull()) q = q.Where(x => x.Reservation != null && x.Reservation.HotelId == p.HotelId);
		if (p.MinDueDate.HasValue) q = q.Where(x => x.DueDate >= p.MinDueDate);
		if (p.MaxDueDate.HasValue) q = q.Where(x => x.DueDate <= p.MaxDueDate);
		if (p.MinDebtAmount.HasValue) q = q.Where(x => x.DebtAmount >= p.MinDebtAmount);
		if (p.MaxDebtAmount.HasValue) q = q.Where(x => x.DebtAmount <= p.MaxDebtAmount);

		DateTime now = DateTime.UtcNow;
		if (p.IsPaid == true) q = q.Where(x => !x.Tags.Contains(TagHotelInvoice.NotPaid));
		if (p.IsPaid == false) q = q.Where(x => x.Tags.Contains(TagHotelInvoice.NotPaid));
		if (p.IsOverdue == true) q = q.Where(x => x.Tags.Contains(TagHotelInvoice.NotPaid) && x.DueDate < now);

		UResponse<IEnumerable<HotelInvoiceResponse>?> response = await q.Select(Projections.HotelInvoiceSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
		List<Guid> ids = response.Result!.Select(x => x.Id).ToList();
		List<HotelInvoiceEntity> entities = await db.Set<HotelInvoiceEntity>().Where(x => ids.Contains(x.Id)).ToListAsync(ct);

		bool anyChanges = false;
		foreach (HotelInvoiceResponse dto in response.Result!) {
			HotelInvoiceEntity? entity = entities.FirstOrDefault(x => x.Id == dto.Id);
			if (entity == null || entity.JsonData.PenaltyPrecentEveryDate <= 0) continue;
			int daysLate = Math.Max(0, (DateTime.UtcNow - entity.DueDate).Days);
			decimal expectedPenalty = entity.DebtAmount * (entity.JsonData.PenaltyPrecentEveryDate / 100m) * daysLate;

			bool needsPenaltyUpdate =
				entity.PaidAmount < entity.DebtAmount + entity.PenaltyAmount &&
				entity.DueDate <= DateTime.UtcNow &&
				entity.PenaltyAmount < expectedPenalty;

			if (needsPenaltyUpdate) {
				entity.PenaltyAmount = expectedPenalty;
				dto.PenaltyAmount = expectedPenalty;
				db.Set<HotelInvoiceEntity>().Update(entity);
				anyChanges = true;
			}
		}

		if (anyChanges) await db.SaveChangesAsync(ct);
		return response;
	}

	public async Task<UResponse> UpdateHotelInvoice(HotelInvoiceUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		HotelInvoiceEntity? e = await db.Set<HotelInvoiceEntity>().AsTracking().Include(x => x.Reservation).ThenInclude(x => x!.Hotel).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("invoiceNotFound"));
		if (e.Reservation != null && (!userData.CanManage(e.CreatorId, []) && !userData.CanManage(e.Reservation.Hotel.CreatorId, e.Reservation.Hotel.AdminUserIds)))
			return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (!userData.HasPermission(TagUser.PermissionManageInvoices)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		if (p.DebtAmount.IsNotNull()) e.DebtAmount = p.DebtAmount.Value;
		if (p.CreditorAmount.IsNotNull()) e.CreditorAmount = p.CreditorAmount.Value;
		if (p.PenaltyAmount.IsNotNull()) e.PenaltyAmount = p.PenaltyAmount.Value;
		if (p.PaidAmount.IsNotNull()) e.PaidAmount = p.PaidAmount.Value;
		if (p.DueDate.HasValue) e.DueDate = p.DueDate.Value;
		if (p.ReservationId.HasValue) e.ReservationId = p.ReservationId.Value;
		if (p.PenaltyPrecentEveryDate.IsNotNull()) e.JsonData.PenaltyPrecentEveryDate = p.PenaltyPrecentEveryDate.Value;

		e.ApplyUpdateParam<HotelInvoiceEntity, TagHotelInvoice, HotelInvoiceJson>(p);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteHotelInvoice(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		HotelInvoiceEntity? e = await db.Set<HotelInvoiceEntity>().Include(x => x.Reservation).ThenInclude(x => x!.Hotel).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("invoiceNotFound"));
		if (e.Reservation != null && (!userData.CanManage(e.CreatorId, []) && !userData.CanManage(e.Reservation.Hotel.CreatorId, e.Reservation.Hotel.AdminUserIds)))
			return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (!userData.HasPermission(TagUser.PermissionDeleteInvoices)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		await db.Set<HotelInvoiceEntity>().Where(x => x.Id == p.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> PayHotelInvoice(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		HotelInvoiceEntity? e = await db.Set<HotelInvoiceEntity>().Include(x => x.Reservation).ThenInclude(x => x!.Hotel).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("invoiceNotFound"));

		bool isOwner = e.Reservation != null && e.Reservation.UserId == userData.Id;
		bool isManager = e.Reservation != null && userData.CanManage(e.Reservation.Hotel.CreatorId, e.Reservation.Hotel.AdminUserIds) && userData.HasPermission(TagUser.PermissionPayInvoices);
		if (!isOwner && !isManager) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		return await PayHotelInvoiceInternal(new HotelInvoicePayParams { InvoiceId = e.Id, UserId = e.Reservation!.UserId }, ct);
	}

	// ===================== Dorm =====================

	public async Task<UResponse<Guid?>> CreateDorm(DormCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.HasPermission(TagUser.PermissionManageDorms)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		DormEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new DormJson {
				Description = p.Description,
				NearbyUniversity = p.NearbyUniversity,
				VisitingHours = p.VisitingHours,
				Amenities = p.Amenities ?? [],
				Rules = p.Rules ?? [],
				RequiredDocuments = p.RequiredDocuments ?? [],
				Latitude = p.Latitude,
				Longitude = p.Longitude
			},
			Tags = p.Tags,
			Title = p.Title,
			CityCode = p.CityCode,
			Address = p.Address,
			PhoneNumber = p.PhoneNumber,
			AdminUserIds = p.AdminUserIds ?? []
		};

		await db.Set<DormEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<DormResponse>?>> ReadDorms(DormReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<DormEntity> q = db.Set<DormEntity>().ApplyReadParams(p);
		if (IsDormManager(userData)) q = q.ApplyOwnerScope<DormEntity, TagDorm>(userData);

		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title!));
		if (p.CityCode.IsNotNullOrEmpty()) q = q.Where(x => x.CityCode.Contains(p.CityCode!));

		IQueryable<DormResponse> projected = q.Select(Projections.DormSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<DormResponse?>> ReadDormById(IdParams<DormSelectorArgs> p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<DormEntity> dorms = db.Set<DormEntity>();
		if (IsDormManager(userData)) dorms = dorms.ApplyOwnerScope<DormEntity, TagDorm>(userData);
		DormResponse? e = await dorms.Select(Projections.DormSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<DormResponse?>(null, Usc.NotFound, ls.Get("dormNotFound")) : new UResponse<DormResponse?>(e);
	}

	public async Task<UResponse> UpdateDorm(DormUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		DormEntity? e = await db.Set<DormEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("dormNotFound"));

		if (!userData.CanManage(e.CreatorId, e.AdminUserIds) || !userData.HasPermission(TagUser.PermissionManageDorms)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		if (p.CityCode.IsNotNullOrEmpty()) e.CityCode = p.CityCode;
		if (p.Address.IsNotNullOrEmpty()) e.Address = p.Address;
		if (p.PhoneNumber.IsNotNullOrEmpty()) e.PhoneNumber = p.PhoneNumber;
		if (p.Description.IsNotNullOrEmpty()) e.JsonData.Description = p.Description;
		if (p.NearbyUniversity.IsNotNullOrEmpty()) e.JsonData.NearbyUniversity = p.NearbyUniversity;
		if (p.VisitingHours.IsNotNullOrEmpty()) e.JsonData.VisitingHours = p.VisitingHours;
		if (p.Amenities != null) e.JsonData.Amenities = p.Amenities;
		if (p.Rules != null) e.JsonData.Rules = p.Rules;
		if (p.RequiredDocuments != null) e.JsonData.RequiredDocuments = p.RequiredDocuments;
		if (p.Latitude.HasValue) e.JsonData.Latitude = p.Latitude;
		if (p.Longitude.HasValue) e.JsonData.Longitude = p.Longitude;

		e.ApplyUpdateParam<DormEntity, TagDorm, DormJson>(p);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> DeleteDorm(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		DormEntity? e = await db.Set<DormEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("dormNotFound"));

		if (!userData.CanManage(e.CreatorId, e.AdminUserIds) || !userData.HasPermission(TagUser.PermissionDeleteDorms)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		db.Set<DormEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	// ===================== DormRoom =====================

	public async Task<UResponse<Guid?>> CreateDormRoom(DormRoomCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		DormEntity? dorm = await db.Set<DormEntity>().FirstOrDefaultAsync(x => x.Id == p.DormId, ct);
		if (dorm == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("dormNotFound"));
		if (!userData.CanManage(dorm.CreatorId, dorm.AdminUserIds) || !userData.HasPermission(TagUser.PermissionManageDorms)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		DormRoomEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new DormRoomJson {
				Description = p.Description,
				Floor = p.Floor,
				SizeSquareMeters = p.SizeSquareMeters,
				Amenities = p.Amenities ?? []
			},
			Tags = p.Tags,
			Title = p.Title,
			Capacity = p.Capacity,
			DormId = p.DormId
		};

		await db.Set<DormRoomEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<DormRoomResponse>?>> ReadDormRooms(DormRoomReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<DormRoomEntity> q = db.Set<DormRoomEntity>().ApplyReadParams(p);
		if (IsDormManager(userData) && userData is not { IsSuperAdmin: true }) {
			Guid uid = UserIdOf(userData);
			q = q.Where(x => x.Dorm.CreatorId == uid || x.Dorm.AdminUserIds.Contains(uid));
		}

		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title!));
		if (p.DormId.HasValue) q = q.Where(x => x.DormId == p.DormId);

		IQueryable<DormRoomResponse> projected = q.Select(Projections.DormRoomSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<DormRoomResponse?>> ReadDormRoomById(IdParams<DormRoomSelectorArgs> p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<DormRoomEntity> q = db.Set<DormRoomEntity>();
		if (IsDormManager(userData) && userData is not { IsSuperAdmin: true }) {
			Guid uid = UserIdOf(userData);
			q = q.Where(x => x.Dorm.CreatorId == uid || x.Dorm.AdminUserIds.Contains(uid));
		}

		DormRoomResponse? e = await q.Select(Projections.DormRoomSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<DormRoomResponse?>(null, Usc.NotFound, ls.Get("dormRoomNotFound")) : new UResponse<DormRoomResponse?>(e);
	}

	public async Task<UResponse> UpdateDormRoom(DormRoomUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		DormRoomEntity? e = await db.Set<DormRoomEntity>().AsTracking().Include(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("dormRoomNotFound"));

		if ((!userData.CanManage(e.CreatorId, []) && !userData.CanManage(e.Dorm.CreatorId, e.Dorm.AdminUserIds)) || !userData.HasPermission(TagUser.PermissionManageDorms)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		if (p.DormId.HasValue) e.DormId = p.DormId.Value;
		if (p.Capacity.HasValue) e.Capacity = p.Capacity.Value;
		if (p.Description.IsNotNullOrEmpty()) e.JsonData.Description = p.Description;
		if (p.Floor.HasValue) e.JsonData.Floor = p.Floor;
		if (p.SizeSquareMeters.HasValue) e.JsonData.SizeSquareMeters = p.SizeSquareMeters;
		if (p.Amenities != null) e.JsonData.Amenities = p.Amenities;

		e.ApplyUpdateParam<DormRoomEntity, TagDormRoom, DormRoomJson>(p);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> DeleteDormRoom(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		DormRoomEntity? e = await db.Set<DormRoomEntity>().Include(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("dormRoomNotFound"));

		if ((!userData.CanManage(e.CreatorId, []) && !userData.CanManage(e.Dorm.CreatorId, e.Dorm.AdminUserIds)) || !userData.HasPermission(TagUser.PermissionDeleteDorms)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		db.Set<DormRoomEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	// ===================== DormBed =====================

	public async Task<UResponse<Guid?>> CreateDormBed(DormBedCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		DormRoomEntity? room = await db.Set<DormRoomEntity>().Include(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.RoomId, ct);
		if (room == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("dormRoomNotFound"));
		if ((!userData.CanManage(room.CreatorId, []) && !userData.CanManage(room.Dorm.CreatorId, room.Dorm.AdminUserIds)) || !userData.HasPermission(TagUser.PermissionManageDorms)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		DormBedEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJson(),
			Tags = p.Tags,
			Title = p.Title,
			Deposit = p.Deposit,
			MonthlyRent = p.MonthlyRent,
			RoomId = p.RoomId
		};

		await db.Set<DormBedEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<DormBedResponse>?>> ReadDormBeds(DormBedReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<DormBedEntity> q = db.Set<DormBedEntity>().ApplyReadParams(p);
		if (IsDormManager(userData) && userData is not { IsSuperAdmin: true }) {
			Guid uid = UserIdOf(userData);
			q = q.Where(x => x.Room.Dorm.CreatorId == uid || x.Room.Dorm.AdminUserIds.Contains(uid));
		}

		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title!));
		if (p.RoomId.HasValue) q = q.Where(x => x.RoomId == p.RoomId);
		if (p.DormId.HasValue) q = q.Where(x => x.Room.DormId == p.DormId);
		if (p.MinDeposit.HasValue) q = q.Where(x => x.Deposit >= p.MinDeposit);
		if (p.MaxDeposit.HasValue) q = q.Where(x => x.Deposit <= p.MaxDeposit);
		if (p.MinMonthlyRent.HasValue) q = q.Where(x => x.MonthlyRent >= p.MinMonthlyRent);
		if (p.MaxMonthlyRent.HasValue) q = q.Where(x => x.MonthlyRent <= p.MaxMonthlyRent);

		IQueryable<DormBedResponse> projected = q.Select(Projections.DormBedSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<DormBedResponse?>> ReadDormBedById(IdParams<DormBedSelectorArgs> p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<DormBedEntity> q = db.Set<DormBedEntity>();
		if (IsDormManager(userData) && userData is not { IsSuperAdmin: true }) {
			Guid uid = UserIdOf(userData);
			q = q.Where(x => x.Room.Dorm.CreatorId == uid || x.Room.Dorm.AdminUserIds.Contains(uid));
		}

		DormBedResponse? e = await q.Select(Projections.DormBedSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<DormBedResponse?>(null, Usc.NotFound, ls.Get("dormBedNotFound")) : new UResponse<DormBedResponse?>(e);
	}

	public async Task<UResponse> UpdateDormBed(DormBedUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		DormBedEntity? e = await db.Set<DormBedEntity>().AsTracking().Include(x => x.Room).ThenInclude(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("dormBedNotFound"));

		if ((!userData.CanManage(e.CreatorId, []) && !userData.CanManage(e.Room.Dorm.CreatorId, e.Room.Dorm.AdminUserIds)) || !userData.HasPermission(TagUser.PermissionManageDorms)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		if (p.Deposit.HasValue) e.Deposit = p.Deposit.Value;
		if (p.MonthlyRent.HasValue) e.MonthlyRent = p.MonthlyRent.Value;
		if (p.RoomId.HasValue) e.RoomId = p.RoomId.Value;

		e.ApplyUpdateParam<DormBedEntity, TagDormBed, BaseJson>(p);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> DeleteDormBed(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		DormBedEntity? e = await db.Set<DormBedEntity>().Include(x => x.Room).ThenInclude(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("dormBedNotFound"));

		if ((!userData.CanManage(e.CreatorId, []) && !userData.CanManage(e.Room.Dorm.CreatorId, e.Room.Dorm.AdminUserIds)) || !userData.HasPermission(TagUser.PermissionDeleteDorms)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		db.Set<DormBedEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	// ===================== DormBedContract =====================

	public async Task<UResponse<Guid?>> CreateDormBedContract(DormBedContractCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		DormBedEntity? bed = await db.Set<DormBedEntity>().Include(x => x.Contracts).Include(x => x.Room).ThenInclude(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.BedId, ct);
		if (bed == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("dormBedNotFound"));
		if ((!userData.CanManage(bed.CreatorId, []) && !userData.CanManage(bed.Room.Dorm.CreatorId, bed.Room.Dorm.AdminUserIds)) || !userData.HasPermission(TagUser.PermissionManageContracts)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (bed.Contracts.Any(y => y.EndDate >= DateTime.UtcNow)) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("thisBedHasAnActiveContract"));

		UserEntity? user = await db.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == p.UserId, ct);
		if (user == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("accountNotFound"));

		Guid contractId = Guid.CreateVersion7();
		DormBedContractEntity e = new() {
			Id = contractId,
			CreatedAt = DateTime.UtcNow,
			StartDate = p.StartDate,
			EndDate = p.EndDate,
			Deposit = p.Deposit ?? bed.Deposit,
			Rent = p.Rent ?? bed.MonthlyRent,
			UserId = user.Id,
			CreatorId = p.CreatorId ?? userData.Id,
			BedId = bed.Id,
			JsonData = new BaseJson(),
			Tags = p.Tags
		};
		await db.Set<DormBedContractEntity>().AddAsync(e, ct);

		if (p.Tags.Contains(TagDormBedContract.SingleInvoice)) {
			await db.Set<DormBedInvoiceEntity>().AddAsync(new DormBedInvoiceEntity {
				Id = Guid.CreateVersion7(),
				CreatorId = p.CreatorId ?? userData.Id,
				CreatedAt = DateTime.UtcNow,
				Tags = [TagDormBedInvoice.NotPaid],
				DebtAmount = e.Deposit + e.Rent,
				CreditorAmount = 0,
				PaidAmount = 0,
				PenaltyAmount = 0,
				ContractId = contractId,
				DueDate = p.StartDate,
				JsonData = new DormBedInvoiceJson { PenaltyPrecentEveryDate = p.PenaltyPrecentEveryDate }
			}, ct);

			await db.SaveChangesAsync(ct);
			return new UResponse<Guid?>(e.Id);
		}

		if (e.Deposit >= 1)
			await db.Set<DormBedInvoiceEntity>().AddAsync(new DormBedInvoiceEntity {
				Id = Guid.CreateVersion7(),
				CreatorId = p.CreatorId ?? userData.Id,
				CreatedAt = DateTime.UtcNow,
				Tags = [TagDormBedInvoice.NotPaid, TagDormBedInvoice.Deposit],
				DebtAmount = e.Deposit,
				CreditorAmount = 0,
				PaidAmount = 0,
				PenaltyAmount = 0,
				ContractId = contractId,
				DueDate = p.StartDate,
				JsonData = new DormBedInvoiceJson { PenaltyPrecentEveryDate = p.PenaltyPrecentEveryDate }
			}, ct);

		PersianDateTime startDate = e.StartDate.ToPersian();
		PersianDateTime endDate = e.EndDate.ToPersian();

		decimal rent = bed.MonthlyRent;

		int totalMonths = (endDate.Year - startDate.Year) * 12 + (endDate.Month - startDate.Month);
		if (endDate.Day < startDate.Day) totalMonths--;

		await db.Set<DormBedInvoiceEntity>().AddAsync(new DormBedInvoiceEntity {
			Id = Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = [TagDormBedInvoice.NotPaid, TagDormBedInvoice.Rent],
			DebtAmount = rent,
			CreditorAmount = 0,
			PaidAmount = 0,
			PenaltyAmount = 0,
			ContractId = contractId,
			DueDate = startDate.ToDateTime(),
			JsonData = new DormBedInvoiceJson { PenaltyPrecentEveryDate = p.PenaltyPrecentEveryDate }
		}, ct);

		if (totalMonths >= 1) {
			int remainingDaysInFirstMonth = PersianDateTime.DaysInMonth(startDate.Year, startDate.Month) - startDate.Day + 1;
			int totalDaysInFirstMonth = PersianDateTime.DaysInMonth(startDate.Year, startDate.Month);
			decimal proportionalPrice = remainingDaysInFirstMonth / (decimal)totalDaysInFirstMonth * rent;

			await db.Set<DormBedInvoiceEntity>().AddAsync(new DormBedInvoiceEntity {
				Id = Guid.CreateVersion7(),
				CreatorId = p.CreatorId ?? userData.Id,
				CreatedAt = DateTime.UtcNow,
				Tags = [TagDormBedInvoice.NotPaid, TagDormBedInvoice.Rent],
				DebtAmount = Math.Round(proportionalPrice, 2),
				CreditorAmount = 0,
				PaidAmount = 0,
				PenaltyAmount = 0,
				ContractId = contractId,
				DueDate = startDate.AddMonths(1).ToDateTime(),
				JsonData = new DormBedInvoiceJson { PenaltyPrecentEveryDate = p.PenaltyPrecentEveryDate }
			}, ct);
		}

		for (int i = 2; i <= totalMonths; i++) {
			PersianDateTime firstOfMonth = startDate.AddMonths(i).StartOfMonth;

			await db.Set<DormBedInvoiceEntity>().AddAsync(new DormBedInvoiceEntity {
				Id = Guid.CreateVersion7(),
				CreatorId = p.CreatorId ?? userData.Id,
				CreatedAt = DateTime.UtcNow,
				Tags = [TagDormBedInvoice.NotPaid, TagDormBedInvoice.Rent],
				DebtAmount = rent,
				CreditorAmount = 0,
				PaidAmount = 0,
				PenaltyAmount = 0,
				ContractId = contractId,
				DueDate = firstOfMonth.ToDateTime(),
				JsonData = new DormBedInvoiceJson { PenaltyPrecentEveryDate = p.PenaltyPrecentEveryDate }
			}, ct);
		}

		await AddNotification(user.Id, TagNotification.InvoiceIssued, ls.Get("newInvoicesIssued"), bed.Room.Dorm.Title, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<DormBedContractResponse>?>> ReadDormBedContracts(DormBedContractReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<DormBedContractEntity> q = db.Set<DormBedContractEntity>().ApplyReadParams(p);
		if (userData is not { IsSuperAdmin: true }) {
			Guid uid = userData?.Id ?? Guid.Empty;
			q = q.Where(x =>
				x.UserId == uid ||
				x.Bed.Room.Dorm.CreatorId == uid ||
				x.Bed.Room.Dorm.AdminUserIds.Contains(uid));
		}

		if (p.UserId.IsNotNull()) q = q.Where(u => u.UserId == p.UserId);
		if (p.BedId.IsNotNull()) q = q.Where(u => u.BedId == p.BedId);
		if (p.DormId.IsNotNull()) q = q.Where(u => u.Bed.Room.DormId == p.DormId);
		if (p.UserName.IsNotNullOrEmpty()) q = q.Include(x => x.User).Where(x => x.User.UserName.Contains(p.UserName));
		if (p.StartDate.HasValue) q = q.Where(u => u.StartDate <= p.StartDate);
		if (p.EndDate.HasValue) q = q.Where(u => u.EndDate >= p.EndDate);

		DateTime nowContract = DateTime.UtcNow;
		if (p.ActiveOnly == true) q = q.Where(u => u.StartDate <= nowContract && u.EndDate >= nowContract);
		if (p.UpcomingOnly == true) q = q.Where(u => u.StartDate > nowContract);
		if (p.ExpiredOnly == true) q = q.Where(u => u.EndDate < nowContract);
		if (p.ExpiringWithinDays.HasValue) {
			DateTime horizon = nowContract.AddDays(p.ExpiringWithinDays.Value);
			q = q.Where(u => u.EndDate >= nowContract && u.EndDate <= horizon);
		}

		IQueryable<DormBedContractResponse> list = q.Select(Projections.DormBedContractSelector(p.SelectorArgs));

		return await list.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateDormBedContract(DormBedContractUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		DormBedContractEntity? e = await db.Set<DormBedContractEntity>().AsTracking().Include(x => x.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("contractNotFound"));

		if ((!userData.CanManage(e.CreatorId, []) && !userData.CanManage(e.Bed.Room.Dorm.CreatorId, e.Bed.Room.Dorm.AdminUserIds)) || !userData.HasPermission(TagUser.PermissionManageContracts)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		if (p.Deposit.HasValue) e.Deposit = p.Deposit.Value;
		if (p.Rent.HasValue) e.Rent = p.Rent.Value;
		if (p.StartDate.HasValue) e.StartDate = p.StartDate.Value;
		if (p.EndDate.HasValue) e.EndDate = p.EndDate.Value;

		e.ApplyUpdateParam<DormBedContractEntity, TagDormBedContract, BaseJson>(p);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> DeleteDormBedContract(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		DormBedContractEntity? e = await db.Set<DormBedContractEntity>().Include(x => x.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("contractNotFound"));

		if ((!userData.CanManage(e.CreatorId, []) && !userData.CanManage(e.Bed.Room.Dorm.CreatorId, e.Bed.Room.Dorm.AdminUserIds)) || !userData.HasPermission(TagUser.PermissionDeleteContracts)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		await db.Set<DormBedContractEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}

	// ===================== DormBedInvoice =====================

	public async Task<UResponse<Guid?>> CreateDormBedInvoice(DormBedInvoiceCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		DormBedContractEntity? contract = await db.Set<DormBedContractEntity>().Include(x => x.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.ContractId, ct);
		if (contract == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("contractNotFound"));
		if ((!userData.CanManage(contract.CreatorId, []) && !userData.CanManage(contract.Bed.Room.Dorm.CreatorId, contract.Bed.Room.Dorm.AdminUserIds)) || !userData.HasPermission(TagUser.PermissionManageInvoices))
			return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		EntityEntry<DormBedInvoiceEntity> e = await db.AddAsync(new DormBedInvoiceEntity {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = p.Tags,
			DebtAmount = p.DebtAmount,
			CreditorAmount = p.CreditorAmount,
			PaidAmount = p.PaidAmount,
			PenaltyAmount = p.PenaltyAmount,
			ContractId = p.ContractId,
			DueDate = p.DueDate,
			JsonData = new DormBedInvoiceJson {
				Detail1 = p.Detail1,
				Detail2 = p.Detail2,
				PenaltyPrecentEveryDate = p.PenaltyPrecentEveryDate
			}
		}, ct);
		await db.SaveChangesAsync(ct);

		return new UResponse<Guid?>(e.Entity.Id);
	}

	public async Task<UResponse<IEnumerable<DormBedInvoiceResponse>?>> ReadDormBedInvoices(DormBedInvoiceReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<DormBedInvoiceEntity> q = db.Set<DormBedInvoiceEntity>().Include(x => x.Contract).ApplyReadParams(p);
		if (userData is not { IsSuperAdmin: true }) {
			Guid uid = userData?.Id ?? Guid.Empty;
			q = q.Where(x =>
				x.Contract != null && (
					x.Contract.UserId == uid ||
					x.Contract.Bed.Room.Dorm.CreatorId == uid ||
					x.Contract.Bed.Room.Dorm.AdminUserIds.Contains(uid)));
		}

		if (p.UserId.IsNotNull()) q = q.Where(x => x.Contract!.UserId == p.UserId);
		if (p.ContractId.IsNotNull()) q = q.Where(x => x.ContractId == p.ContractId);
		if (p.DormId.IsNotNull()) q = q.Where(x => x.Contract != null && x.Contract.Bed.Room.DormId == p.DormId);
		if (p.MinDueDate.HasValue) q = q.Where(x => x.DueDate >= p.MinDueDate);
		if (p.MaxDueDate.HasValue) q = q.Where(x => x.DueDate <= p.MaxDueDate);
		if (p.MinDebtAmount.HasValue) q = q.Where(x => x.DebtAmount >= p.MinDebtAmount);
		if (p.MaxDebtAmount.HasValue) q = q.Where(x => x.DebtAmount <= p.MaxDebtAmount);

		DateTime nowInvoice = DateTime.UtcNow;
		if (p.IsPaid == true) q = q.Where(x => !x.Tags.Contains(TagDormBedInvoice.NotPaid));
		if (p.IsPaid == false) q = q.Where(x => x.Tags.Contains(TagDormBedInvoice.NotPaid));
		if (p.IsOverdue == true) q = q.Where(x => x.Tags.Contains(TagDormBedInvoice.NotPaid) && x.DueDate < nowInvoice);

		UResponse<IEnumerable<DormBedInvoiceResponse>?> response = await q.Select(Projections.DormBedInvoiceSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
		List<Guid> ids = response.Result!.Select(x => x.Id).ToList();
		List<DormBedInvoiceEntity> entities = await db.Set<DormBedInvoiceEntity>().Where(x => ids.Contains(x.Id)).ToListAsync(ct);

		bool anyChanges = false;

		foreach (DormBedInvoiceResponse dto in response.Result!) {
			DormBedInvoiceEntity? entity = entities.FirstOrDefault(x => x.Id == dto.Id);
			if (entity == null || entity.JsonData.PenaltyPrecentEveryDate <= 0) continue;
			int daysLate = Math.Max(0, (DateTime.UtcNow - entity.DueDate).Days);
			decimal expectedPenalty = entity.DebtAmount * (entity.JsonData.PenaltyPrecentEveryDate / 100m) * daysLate;

			bool needsPenaltyUpdate =
				entity.PaidAmount < entity.DebtAmount + entity.PenaltyAmount &&
				entity.DueDate <= DateTime.UtcNow &&
				entity.PenaltyAmount < expectedPenalty;

			if (needsPenaltyUpdate) {
				entity.PenaltyAmount = expectedPenalty;
				dto.PenaltyAmount = expectedPenalty;
				db.Set<DormBedInvoiceEntity>().Update(entity);
				anyChanges = true;
			}
		}

		if (anyChanges) await db.SaveChangesAsync(ct);

		return response;
	}

	public async Task<UResponse> UpdateDormBedInvoice(DormBedInvoiceUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		DormBedInvoiceEntity? e = await db.Set<DormBedInvoiceEntity>().AsTracking().Include(x => x.Contract).ThenInclude(x => x!.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("invoiceNotFound"));
		if (e.Contract != null && (!userData.CanManage(e.CreatorId, []) && !userData.CanManage(e.Contract.Bed.Room.Dorm.CreatorId, e.Contract.Bed.Room.Dorm.AdminUserIds)))
			return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (!userData.HasPermission(TagUser.PermissionManageInvoices)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (p.CreditorAmount.IsNotNull()) e.CreditorAmount = p.CreditorAmount.Value;
		if (p.DebtAmount.IsNotNull()) e.DebtAmount = p.DebtAmount.Value;
		if (p.PenaltyAmount.IsNotNull()) e.PenaltyAmount = p.PenaltyAmount.Value;
		if (p.PaidAmount.IsNotNull()) e.PaidAmount = p.PaidAmount.Value;
		if (p.DueDate.HasValue) e.DueDate = p.DueDate.Value;
		if (p.ContractId.HasValue) e.ContractId = p.ContractId.Value;
		if (p.PenaltyPrecentEveryDate.IsNotNull()) e.JsonData.PenaltyPrecentEveryDate = p.PenaltyPrecentEveryDate.Value;

		e.ApplyUpdateParam<DormBedInvoiceEntity, TagDormBedInvoice, DormBedInvoiceJson>(p);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> DeleteDormBedInvoice(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		DormBedInvoiceEntity? e = await db.Set<DormBedInvoiceEntity>().Include(x => x.Contract).ThenInclude(x => x!.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("invoiceNotFound"));
		if (e.Contract != null && (!userData.CanManage(e.CreatorId, []) && !userData.CanManage(e.Contract.Bed.Room.Dorm.CreatorId, e.Contract.Bed.Room.Dorm.AdminUserIds)))
			return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (!userData.HasPermission(TagUser.PermissionDeleteInvoices)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		await db.Set<DormBedInvoiceEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> PayDormBedInvoice(DormBedInvoicePayParams p, CancellationToken ct) {
		DormBedInvoiceEntity? e = await db.Set<DormBedInvoiceEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.InvoiceId, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("invoiceNotFound"));
		if (!e.Tags.Contains(TagDormBedInvoice.NotPaid)) return new UResponse(Usc.Conflict, ls.Get("thisInvoiceHasAlreadyBeenPaid"));

		decimal amount = e.DebtAmount + e.PenaltyAmount - e.CreditorAmount;
		if (amount > 0) {
			UResponse<WalletTxnResponse?> transfer = await ws.Transfer(new WalletTransferParams {
				SenderId = p.UserId,
				ReceiverId = Core.App.Users.SystemAdmin.Id,
				Amount = amount,
				Detail1 = ls.Get("dormInvoicePayment"),
				TagWalletTxn = [TagWalletTxn.DormBedInvoice]
			}, ct);
			if (transfer.Result == null) return new UResponse(transfer.Status, transfer.Message);
		}

		e.PaidAmount = amount;
		e.Tags = [TagDormBedInvoice.PaidOnline];
		await AddNotification(p.UserId, TagNotification.InvoicePaid, ls.Get("invoicePaid"), ls.Get("dormInvoicePayment"), ct);
		await db.SaveChangesAsync(ct);

		return new UResponse(Usc.Success, ls.Get("paymentCompleted"));
	}

	public async Task<UResponse> PayDormBedInvoiceByUser(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		DormBedInvoiceEntity? e = await db.Set<DormBedInvoiceEntity>()
			.Include(x => x.Contract).ThenInclude(x => x!.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm)
			.FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e?.Contract == null) return new UResponse(Usc.NotFound, ls.Get("invoiceNotFound"));

		bool isOwner = e.Contract.UserId == userData.Id;
		bool isManager = userData.CanManage(e.Contract.Bed.Room.Dorm.CreatorId, e.Contract.Bed.Room.Dorm.AdminUserIds) && userData.HasPermission(TagUser.PermissionPayInvoices);
		if (!isOwner && !isManager) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		return await PayDormBedInvoice(new DormBedInvoicePayParams { InvoiceId = e.Id, UserId = e.Contract.UserId }, ct);
	}

	public async Task<UResponse<IEnumerable<DormBedInvoiceChartResponse>?>> ReadDormBedInvoiceChartData(BaseParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<DormBedInvoiceChartResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<DormBedInvoiceChartResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		IQueryable<DormBedInvoiceEntity> invoiceQuery = db.Set<DormBedInvoiceEntity>();
		if (!userData.IsSuperAdmin) {
			Guid uid = userData.Id;
			invoiceQuery = invoiceQuery.Where(x =>
				x.Contract != null && (
					x.Contract.UserId == uid ||
					x.Contract.Bed.Room.Dorm.CreatorId == uid ||
					x.Contract.Bed.Room.Dorm.AdminUserIds.Contains(uid)));
		}

		var rawData = await invoiceQuery
			.GroupBy(x => x.CreatedAt.Month)
			.Select(g => new {
				MonthNumber = g.Key,
				TotalDebt = g.Sum(x => x.DebtAmount),
				TotalPaid = g.Sum(x => x.PaidAmount),
				TotalPenalty = g.Sum(x => x.PenaltyAmount),
				TotalRemaining = g.Sum(x => x.DebtAmount - x.PaidAmount),
				InvoiceCount = g.Count()
			})
			.OrderBy(x => x.MonthNumber)
			.ToListAsync(ct);

		List<DormBedInvoiceChartResponse> chartData = rawData.Select(item => new DormBedInvoiceChartResponse {
			Month = new DateTime(1, item.MonthNumber, 1).ToString("MMM"),
			TotalDebt = item.TotalDebt,
			TotalPaid = item.TotalPaid,
			TotalPenalty = item.TotalPenalty,
			TotalRemaining = item.TotalRemaining,
			InvoiceCount = item.InvoiceCount
		}).ToList();

		return new UResponse<IEnumerable<DormBedInvoiceChartResponse>?>(chartData);
	}
}