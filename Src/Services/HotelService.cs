namespace SinaMN75U.Services;

public interface IHotelService {
	public Task<UResponse<Guid?>> CreateHotel(HotelCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<HotelResponse>?>> ReadHotels(HotelReadParams p, CancellationToken ct);
	public Task<UResponse<HotelResponse?>> ReadHotelById(IdParams<HotelSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> UpdateHotel(HotelUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteHotel(IdParams p, CancellationToken ct);
	public Task<UResponse<Guid?>> CreateHotelRoom(HotelRoomCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<HotelRoomResponse>?>> ReadHotelRooms(HotelRoomReadParams p, CancellationToken ct);
	public Task<UResponse<HotelRoomResponse?>> ReadHotelRoomById(IdParams<HotelRoomSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> UpdateHotelRoom(HotelRoomUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteHotelRoom(IdParams p, CancellationToken ct);
	public Task<UResponse<Guid?>> CreateHotelReservation(HotelReservationCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<HotelReservationResponse>?>> ReadHotelReservations(HotelReservationReadParams p, CancellationToken ct);
	public Task<UResponse<HotelReservationResponse?>> ReadHotelReservationById(IdParams<HotelReservationSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> UpdateHotelReservation(HotelReservationUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteHotelReservation(IdParams p, CancellationToken ct);
	public Task<UResponse> ConfirmHotelReservation(IdParams p, CancellationToken ct);
	public Task<UResponse> CheckInHotelReservation(IdParams p, CancellationToken ct);
	public Task<UResponse> CheckOutHotelReservation(IdParams p, CancellationToken ct);
	public Task<UResponse> CancelHotelReservation(IdParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<HotelRoomAvailabilityResponse>?>> ReadHotelRoomAvailability(HotelRoomAvailabilityParams p, CancellationToken ct);
	public Task<UResponse<HotelReservationResponse?>> BookHotelReservation(HotelReservationBookParams p, CancellationToken ct);
	public Task<UResponse> CancelHotelReservationByUser(HotelReservationCancelParams p, CancellationToken ct);
	public Task<UResponse> PayHotelInvoiceInternal(HotelInvoicePayParams p, CancellationToken ct);
	public Task<UResponse<Guid?>> CreateHotelInvoice(HotelInvoiceCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<HotelInvoiceResponse>?>> ReadHotelInvoices(HotelInvoiceReadParams p, CancellationToken ct);
	public Task<UResponse> UpdateHotelInvoice(HotelInvoiceUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteHotelInvoice(IdParams p, CancellationToken ct);
	public Task<UResponse> PayHotelInvoice(IdParams p, CancellationToken ct);
	public Task<UResponse> ReceiveHotelInvoice(InvoiceReceiveParams p, CancellationToken ct);
	public Task<bool?> CanActOnPlaceOf(JwtClaimData u, Guid? hotelId, Guid? hotelRoomId, CancellationToken ct);
	public IQueryable<Guid> RelatedUserIds(Guid userId);
	public Task ReopenPayment(Guid sourceId, decimal amount, CancellationToken ct);
	public Task<List<AccountingDue>> Outstanding(Guid organizationId, DateTime now, CancellationToken ct);
	public Task PostDueInvoices(CancellationToken ct);
	public Task<UResponse<HotelDashboardResponse?>> ReadHotelDashboard(DashboardRangeParams p, CancellationToken ct);
	public Task<UResponse<List<KeyValue>?>> SeedHotels(BaseParams p, CancellationToken ct);
	public Task<bool> IsResidentOf(Guid userId, Guid placeId, CancellationToken ct);
	public Task<UResponse<Guid?>> CreateHotelRate(HotelRateCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<HotelRateResponse>?>> ReadHotelRates(HotelRateReadParams p, CancellationToken ct);
	public Task<UResponse> UpdateHotelRate(HotelRateUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteHotelRate(IdParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<HotelCalendarDay>?>> ReadHotelRoomCalendar(HotelRoomCalendarParams p, CancellationToken ct);
	public Task<UResponse> SetHotelRoomHousekeeping(HotelHousekeepingParams p, CancellationToken ct);
	public Task<UResponse<List<Guid>?>> CreateHotelReservationGroup(HotelReservationGroupParams p, CancellationToken ct);
	public Task<UResponse> ExtendHotelReservation(HotelReservationExtendParams p, CancellationToken ct);
	public Task<UResponse> ChangeHotelReservationRoom(HotelReservationChangeRoomParams p, CancellationToken ct);
	public Task<UResponse<HotelNightAuditResponse?>> ReadHotelNightAudit(HotelNightAuditParams p, CancellationToken ct);
	public Task<UResponse<HotelNightAuditResponse?>> CloseHotelNightAudit(HotelNightAuditParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<HotelGuestExportItem>?>> ExportHotelGuests(HotelGuestExportParams p, CancellationToken ct);
	public Task<UResponse<string?>> PrintHotelReservation(IdParams p, CancellationToken ct);
}

public class HotelService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IWalletService ws,
	IOrganizationService os,
	IAccountingService acc,
	IDataSeedService seeds
) : IHotelService, IAccountingSource, IUserScope, IPlaceResidency {
	private static Guid UserIdOf(JwtClaimData? u) => u?.Id ?? Guid.Empty;

	private static ReservationGuestJson Guest(ReservationGuestParams g) => new() {
		FullName = g.FullName,
		NationalCode = g.NationalCode,
		PhoneNumber = g.PhoneNumber,
		Nationality = g.Nationality,
		PassportNumber = g.PassportNumber,
		BirthDate = g.BirthDate,
		FatherName = g.FatherName,
		Gender = g.Gender
	};

	private static decimal PenaltyOf(decimal debt, int percentPerDay, DateTime dueDate, DateTime now) =>
		percentPerDay <= 0 || dueDate > now ? 0 : debt * (percentPerDay / 100m) * Math.Max(0, (now - dueDate).Days);

	private async Task<bool> CanAct(JwtClaimData u, HotelEntity place, TagUser permission, CancellationToken ct) =>
		await os.HasModule(u, place.OrganizationId, TagModule.Hotel, ct) && await os.CanActOnPlace(u, place.OrganizationId, place.AdminUserIds, permission, ct);

	private async Task AddNotification(Guid userId, TagNotification tag, string title, string body, CancellationToken ct) =>
		await db.Set<NotificationEntity>().AddAsync(new NotificationEntity {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = userId,
			UserId = userId,
			Tags = [tag, TagNotification.Unread],
			JsonData = new NotificationJson { Detail1 = title, Detail2 = body }
		}, ct);

	private static HotelSelectorArgs Safe(HotelSelectorArgs a, bool people) => new() {
		Rooms = a.Rooms == null ? null : Safe(a.Rooms, people),
		Reservations = people && a.Reservations != null ? Safe(a.Reservations) : null,
		Comments = a.Comments,
		Media = a.Media
	};

	private static HotelRoomSelectorArgs Safe(HotelRoomSelectorArgs a, bool people) => new() {
		Hotel = a.Hotel == null ? null : Safe(a.Hotel, people),
		Reservations = people && a.Reservations != null ? Safe(a.Reservations) : null,
		Media = a.Media
	};

	private static HotelReservationSelectorArgs Safe(HotelReservationSelectorArgs a) => new() {
		User = a.User == null ? null : new UserSelectorArgs(),
		Room = a.Room == null ? null : Safe(a.Room, false),
		Hotel = a.Hotel == null ? null : Safe(a.Hotel, false),
		Invoice = a.Invoice == null ? null : new HotelInvoiceSelectorArgs()
	};

	private static HotelInvoiceSelectorArgs Safe(HotelInvoiceSelectorArgs a) => new() { Reservation = a.Reservation == null ? null : Safe(a.Reservation) };

	public async Task<UResponse<Guid?>> CreateHotel(HotelCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!await os.CanCreatePlace(userData, p.OrganizationId, TagUser.PermissionManageHotels, ct)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		string? planError = await os.PlanError(userData, p.OrganizationId, TagPlanLimit.Places, () => os.PlaceCount(p.OrganizationId!.Value, ct), ct);
		if (planError != null) return new UResponse<Guid?>(null, Usc.Forbidden, planError);

		HotelEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = OrganizationService.IsFull(userData) ? p.CreatorId ?? userData.Id : userData.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new HotelJson {
				Description = p.Description,
				Policies = p.Policies,
				CheckInTime = p.CheckInTime,
				CheckOutTime = p.CheckOutTime,
				Highlights = p.Highlights ?? [],
				Rules = p.Rules ?? [],
				HowToGetThere = p.HowToGetThere,
				Nearby = p.Nearby ?? [],
				Faqs = p.Faqs ?? [],
				Website = p.Website,
				Whatsapp = p.Whatsapp,
				Instagram = p.Instagram,
				Telegram = p.Telegram,
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
			OrganizationId = p.OrganizationId,
			AdminUserIds = await os.PlaceAdmins(p.OrganizationId, OrganizationService.IsFull(userData) ? p.AdminUserIds ?? [] : [userData.Id], ct)
		};

		await db.Set<HotelEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<HotelResponse>?>> ReadHotels(HotelReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<HotelEntity> q = db.Set<HotelEntity>().ApplyReadParams(p);
		Guid uid = UserIdOf(userData);
		if (OrganizationService.IsScopedAdmin(userData)) q = q.Where(x => x.AdminUserIds.Contains(uid));
		else if (!OrganizationService.IsFull(userData)) q = q.Where(x => x.Tags.Contains(TagHotel.Active));
		if (!OrganizationService.IsFull(userData)) p.SelectorArgs = Safe(p.SelectorArgs, OrganizationService.IsScopedAdmin(userData));

		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title!));
		if (p.CityCode.IsNotNullOrEmpty()) q = q.Where(x => x.CityCode == p.CityCode);
		if (p.MinStars.HasValue) q = q.Where(x => x.Stars >= p.MinStars);
		if (p.OrganizationId.HasValue) q = q.Where(x => x.OrganizationId == p.OrganizationId);
		if (p.MinPrice.HasValue || p.MaxPrice.HasValue) q = q.Where(x => x.Rooms.Any(r => (p.MinPrice == null || r.PricePerNight >= p.MinPrice) && (p.MaxPrice == null || r.PricePerNight <= p.MaxPrice)));
		if (p.MinScore.HasValue) q = q.Where(x => x.Comments.Count > 0 && x.Comments.Average(c => c.Score) >= p.MinScore);

		IQueryable<HotelResponse> projected = q.Select(Projections.HotelSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<HotelResponse?>> ReadHotelById(IdParams<HotelSelectorArgs> p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<HotelEntity> hotels = db.Set<HotelEntity>();
		Guid uid = UserIdOf(userData);
		if (OrganizationService.IsScopedAdmin(userData)) hotels = hotels.Where(x => x.AdminUserIds.Contains(uid));
		else if (!OrganizationService.IsFull(userData)) hotels = hotels.Where(x => x.Tags.Contains(TagHotel.Active));
		if (!OrganizationService.IsFull(userData)) p.SelectorArgs = Safe(p.SelectorArgs, OrganizationService.IsScopedAdmin(userData));
		HotelResponse? e = await hotels.Select(Projections.HotelSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<HotelResponse?>(null, Usc.NotFound, ls.Get("hotelNotFound")) : new UResponse<HotelResponse?>(e);
	}

	public async Task<UResponse> UpdateHotel(HotelUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		HotelEntity? e = await db.Set<HotelEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("hotelNotFound"));

		if (!await CanAct(userData, e, TagUser.PermissionManageHotels, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (OrganizationService.TouchesAdminUserIds(p) && !OrganizationService.IsFull(userData) && !(Core.App.MultiTenant && await os.IsOwner(userData, e.OrganizationId, ct))) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (p.OrganizationId.HasValue && p.OrganizationId != e.OrganizationId) {
			if (!OrganizationService.IsFull(userData)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
			e.OrganizationId = p.OrganizationId;
		}

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		if (p.CityCode.IsNotNullOrEmpty()) e.CityCode = p.CityCode;
		if (p.Stars.HasValue) e.Stars = p.Stars.Value;
		if (p.Address.IsNotNull()) e.Address = p.Address;
		if (p.PhoneNumber.IsNotNull()) e.PhoneNumber = p.PhoneNumber;
		if (p.Email.IsNotNull()) e.Email = p.Email;
		if (p.Description.IsNotNull()) e.JsonData.Description = p.Description;
		if (p.Policies.IsNotNull()) e.JsonData.Policies = p.Policies;
		if (p.CheckInTime.IsNotNull()) e.JsonData.CheckInTime = p.CheckInTime;
		if (p.CheckOutTime.IsNotNull()) e.JsonData.CheckOutTime = p.CheckOutTime;
		if (p.Highlights.IsNotNull()) e.JsonData.Highlights = p.Highlights;
		if (p.Rules.IsNotNull()) e.JsonData.Rules = p.Rules;
		if (p.HowToGetThere.IsNotNull()) e.JsonData.HowToGetThere = p.HowToGetThere;
		if (p.Nearby.IsNotNull()) e.JsonData.Nearby = p.Nearby;
		if (p.Faqs.IsNotNull()) e.JsonData.Faqs = p.Faqs;
		if (p.Website.IsNotNull()) e.JsonData.Website = p.Website;
		if (p.Whatsapp.IsNotNull()) e.JsonData.Whatsapp = p.Whatsapp;
		if (p.Instagram.IsNotNull()) e.JsonData.Instagram = p.Instagram;
		if (p.Telegram.IsNotNull()) e.JsonData.Telegram = p.Telegram;
		if (p.Latitude.HasValue) e.JsonData.Latitude = p.Latitude;
		if (p.Longitude.HasValue) e.JsonData.Longitude = p.Longitude;
		if (p.CancellationFreeHours.HasValue) e.JsonData.CancellationFreeHours = p.CancellationFreeHours.Value;
		if (p.CancellationPenaltyNights.HasValue) e.JsonData.CancellationPenaltyNights = p.CancellationPenaltyNights.Value;
		e.ApplyUpdateParam<HotelEntity, TagHotel, HotelJson>(p);
		if (Core.App.MultiTenant) e.AdminUserIds = await os.PlaceAdmins(e.OrganizationId, e.AdminUserIds, ct);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> DeleteHotel(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		HotelEntity? e = await db.Set<HotelEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("hotelNotFound"));

		if (!await CanAct(userData, e, TagUser.PermissionDeleteHotels, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		db.Set<HotelEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateHotelRoom(HotelRoomCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		HotelEntity? hotel = await db.Set<HotelEntity>().FirstOrDefaultAsync(x => x.Id == p.HotelId, ct);
		if (hotel == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("hotelNotFound"));
		if (!await CanAct(userData, hotel, TagUser.PermissionManageHotels, ct)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		Guid? organizationId = hotel.OrganizationId;
		string? planError = await os.PlanError(userData, organizationId, TagPlanLimit.Rooms,
			async () => (await db.Set<HotelRoomEntity>().Where(x => x.Hotel.OrganizationId == organizationId).SumAsync(x => (int?)x.Quantity, ct) ?? 0) + Math.Max(1, p.Quantity) - 1, ct);
		if (planError != null) return new UResponse<Guid?>(null, Usc.Forbidden, planError);

		HotelRoomEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new HotelRoomJson {
				Description = p.Description,
				BedType = p.BedType,
				SizeSquareMeters = p.SizeSquareMeters,
				Floor = p.Floor,
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
		Guid uid = UserIdOf(userData);
		if (OrganizationService.IsScopedAdmin(userData)) q = q.Where(x => x.Hotel.AdminUserIds.Contains(uid));
		else if (!OrganizationService.IsFull(userData)) q = q.Where(x => x.Hotel.Tags.Contains(TagHotel.Active) && x.IsAvailable);
		if (!OrganizationService.IsFull(userData)) p.SelectorArgs = Safe(p.SelectorArgs, OrganizationService.IsScopedAdmin(userData));

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
		Guid uid = UserIdOf(userData);
		if (OrganizationService.IsScopedAdmin(userData)) q = q.Where(x => x.Hotel.AdminUserIds.Contains(uid));
		else if (!OrganizationService.IsFull(userData)) q = q.Where(x => x.Hotel.Tags.Contains(TagHotel.Active));
		if (!OrganizationService.IsFull(userData)) p.SelectorArgs = Safe(p.SelectorArgs, OrganizationService.IsScopedAdmin(userData));

		HotelRoomResponse? e = await q.Select(Projections.HotelRoomSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<HotelRoomResponse?>(null, Usc.NotFound, ls.Get("hotelRoomNotFound")) : new UResponse<HotelRoomResponse?>(e);
	}

	public async Task<UResponse> UpdateHotelRoom(HotelRoomUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		HotelRoomEntity? e = await db.Set<HotelRoomEntity>().AsTracking().Include(x => x.Hotel).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("hotelRoomNotFound"));

		if (!await CanAct(userData, e.Hotel, TagUser.PermissionManageHotels, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (p.HotelId.HasValue && p.HotelId != e.HotelId) {
			HotelEntity? to = await db.Set<HotelEntity>().FirstOrDefaultAsync(x => x.Id == p.HotelId, ct);
			if (to == null || !await CanAct(userData, to, TagUser.PermissionManageHotels, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		}

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		if (p.Capacity.HasValue) e.Capacity = p.Capacity.Value;
		if (p.PricePerNight.HasValue) e.PricePerNight = p.PricePerNight.Value;
		if (p.HotelId.HasValue) e.HotelId = p.HotelId.Value;
		if (p.RoomNumber.IsNotNull()) e.RoomNumber = p.RoomNumber;
		if (p.Quantity.HasValue) e.Quantity = p.Quantity.Value;
		if (p.IsAvailable.HasValue) e.IsAvailable = p.IsAvailable.Value;
		if (p.Description.IsNotNull()) e.JsonData.Description = p.Description;
		if (p.BedType.IsNotNull()) e.JsonData.BedType = p.BedType;
		if (p.SizeSquareMeters.HasValue) e.JsonData.SizeSquareMeters = p.SizeSquareMeters;
		if (p.Floor.HasValue) e.JsonData.Floor = p.Floor;
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

		if (!await CanAct(userData, e.Hotel, TagUser.PermissionDeleteHotels, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		db.Set<HotelRoomEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateHotelReservation(HotelReservationCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		HotelRoomEntity? room = await db.Set<HotelRoomEntity>().Include(x => x.Hotel).FirstOrDefaultAsync(x => x.Id == p.RoomId, ct);
		if (room == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("hotelRoomNotFound"));
		if (!await CanAct(userData, room.Hotel, TagUser.PermissionManageReservations, ct))
			return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		if (!room.IsAvailable) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("thisRoomIsNotAvailableForBooking"));

		int nights = (p.CheckOutDate.Date - p.CheckInDate.Date).Days;
		if (nights < 1) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("checkOutDateMustBeAfterTheCheckInDate"));

		int guestCount = Math.Max(1, p.GuestCount);
		if (guestCount > room.Capacity + (room.JsonData.ExtraGuestCapacity ?? 0)) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("theNumberOfGuestsIsMoreThanThisRoomCanTake"));

		UserEntity? user = await db.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == p.UserId, ct);
		if (user == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("accountNotFound"));

		int overlapping = await db.Set<HotelReservationEntity>().CountAsync(r =>
			r.RoomId == room.Id &&
			r.CheckInDate < p.CheckOutDate &&
			r.CheckOutDate > p.CheckInDate &&
			!r.Tags.Contains(TagHotelReservation.Cancelled) &&
			!r.Tags.Contains(TagHotelReservation.NoShow) &&
			!r.Tags.Contains(TagHotelReservation.CheckedOut), ct);
		if (overlapping >= room.Quantity) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("thisRoomIsAlreadyBookedForTheSelectedDates"));

		decimal total = p.TotalPrice ?? (await StayPrice(room, p.CheckInDate, p.CheckOutDate, guestCount, ct)).Total;

		Guid reservationId = p.Id ?? Guid.CreateVersion7();
		HotelReservationEntity e = new() {
			Id = reservationId,
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = p.Tags,
			CheckInDate = p.CheckInDate,
			CheckOutDate = p.CheckOutDate,
			GuestCount = guestCount,
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
				Guests = (p.Guests ?? []).Select(Guest).ToList(),
				RoomNumber = p.RoomNumber,
				GroupCode = p.GroupCode,
				GroupName = p.GroupName
			}
		};
		await db.Set<HotelReservationEntity>().AddAsync(e, ct);

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
		if (!OrganizationService.IsFull(userData)) {
			Guid uid = UserIdOf(userData);
			bool scoped = OrganizationService.IsScopedAdmin(userData);
			q = q.Where(x => x.UserId == uid || scoped && x.Hotel.AdminUserIds.Contains(uid));
			p.SelectorArgs = Safe(p.SelectorArgs);
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
		if (!OrganizationService.IsFull(userData)) {
			Guid uid = UserIdOf(userData);
			bool scoped = OrganizationService.IsScopedAdmin(userData);
			q = q.Where(x => x.UserId == uid || scoped && x.Hotel.AdminUserIds.Contains(uid));
			p.SelectorArgs = Safe(p.SelectorArgs);
		}

		HotelReservationResponse? e = await q.Select(Projections.HotelReservationSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<HotelReservationResponse?>(null, Usc.NotFound, ls.Get("reservationNotFound")) : new UResponse<HotelReservationResponse?>(e);
	}

	public async Task<UResponse> UpdateHotelReservation(HotelReservationUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		HotelReservationEntity? e = await ReservationForChange(p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("reservationNotFound"));
		if (!await CanAct(userData, e.Hotel, TagUser.PermissionManageReservations, ct))
			return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		DateTime checkIn = p.CheckInDate ?? e.CheckInDate;
		DateTime checkOut = p.CheckOutDate ?? e.CheckOutDate;
		if ((checkOut.Date - checkIn.Date).Days < 1) return new UResponse(Usc.BadRequest, ls.Get("checkOutDateMustBeAfterTheCheckInDate"));
		if ((checkIn != e.CheckInDate || checkOut != e.CheckOutDate) && Active(e)) {
			int overlapping = await db.Set<HotelReservationEntity>().CountAsync(r => r.Id != e.Id && r.RoomId == e.RoomId && r.CheckInDate < checkOut && r.CheckOutDate > checkIn &&
			                                                                         !r.Tags.Contains(TagHotelReservation.Cancelled) && !r.Tags.Contains(TagHotelReservation.NoShow) && !r.Tags.Contains(TagHotelReservation.CheckedOut), ct);
			if (overlapping >= e.Room.Quantity) return new UResponse(Usc.Conflict, ls.Get("thisRoomIsAlreadyBookedForTheSelectedDates"));
		}

		if (p.TotalPrice.HasValue && p.TotalPrice != e.TotalPrice) {
			HotelInvoiceEntity? main = e.Invoices.Where(x => x.Tags.Contains(TagHotelInvoice.Full) && x.Tags.Contains(TagHotelInvoice.NotPaid) && x.PaidAmount == 0).OrderBy(x => x.CreatedAt).FirstOrDefault();
			if (main != null) {
				main.DebtAmount = Math.Max(0, main.DebtAmount + p.TotalPrice.Value - e.TotalPrice);
				await SyncHotelInvoice(main, false, ct);
			}
		}

		if (p.CheckInDate.HasValue) e.CheckInDate = p.CheckInDate.Value;
		if (p.CheckOutDate.HasValue) e.CheckOutDate = p.CheckOutDate.Value;
		if (p.GuestCount.HasValue) e.GuestCount = p.GuestCount.Value;
		if (p.TotalPrice.HasValue) e.TotalPrice = p.TotalPrice.Value;
		if (p.GuestName.IsNotNullOrEmpty()) e.JsonData.GuestName = p.GuestName;
		if (p.GuestPhone.IsNotNullOrEmpty()) e.JsonData.GuestPhone = p.GuestPhone;
		if (p.Notes.IsNotNullOrEmpty()) e.JsonData.Notes = p.Notes;
		if (p.Guests != null)
			e.JsonData.Guests = p.Guests.Select(Guest).ToList();
		if (p.RoomNumber != null) e.JsonData.RoomNumber = p.RoomNumber.NullIfEmpty();
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
		if (!await CanAct(userData, e.Hotel, TagUser.PermissionDeleteReservations, ct))
			return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		foreach (HotelInvoiceEntity invoice in await db.Set<HotelInvoiceEntity>().Include(x => x.Reservation).ThenInclude(x => x!.Hotel).Where(x => x.ReservationId == e.Id && x.JsonData.Posted).ToListAsync(ct))
			await SyncHotelInvoice(invoice, true, ct);
		await db.SaveChangesAsync(ct);
		await db.Set<HotelReservationEntity>().Where(x => x.Id == p.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	private async Task<UResponse> TransitionReservation(IdParams p, TagHotelReservation status, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		HotelReservationEntity? e = await db.Set<HotelReservationEntity>().AsTracking().Include(x => x.Hotel).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("reservationNotFound"));
		if (!await CanAct(userData, e.Hotel, TagUser.PermissionManageReservations, ct))
			return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		if (e.Tags.Contains(TagHotelReservation.Cancelled) || e.Tags.Contains(TagHotelReservation.CheckedOut)) return new UResponse(Usc.Conflict, ls.Get("thisReservationCannotBeChanged"));
		e.Tags = [status];
		if (status == TagHotelReservation.CheckedOut && e.JsonData.RoomNumber != null) await SetUnit(e.RoomId, e.JsonData.RoomNumber, TagHousekeeping.Dirty, null, userData.Id, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public Task<UResponse> ConfirmHotelReservation(IdParams p, CancellationToken ct) => TransitionReservation(p, TagHotelReservation.Confirmed, ct);

	public Task<UResponse> CheckInHotelReservation(IdParams p, CancellationToken ct) => TransitionReservation(p, TagHotelReservation.CheckedIn, ct);

	public Task<UResponse> CheckOutHotelReservation(IdParams p, CancellationToken ct) => TransitionReservation(p, TagHotelReservation.CheckedOut, ct);

	public Task<UResponse> CancelHotelReservation(IdParams p, CancellationToken ct) => CancelHotelReservationByUser(new HotelReservationCancelParams { Token = p.Token, ApiKey = p.ApiKey, Id = p.Id }, ct);

	private Task<List<HotelRateEntity>> RatesFor(Guid hotelId, DateTime from, DateTime to, CancellationToken ct) =>
		db.Set<HotelRateEntity>().Where(x => x.HotelId == hotelId && x.StartDate <= to && x.EndDate >= from).ToListAsync(ct);

	private static (decimal Price, bool Closed, int? MinNights) NightOf(HotelRoomEntity room, DateTime night, List<HotelRateEntity> rates) {
		List<HotelRateEntity> matching = rates.Where(r => (r.RoomId == null || r.RoomId == room.Id) &&
		                                                  r.StartDate.Date <= night.Date && r.EndDate.Date >= night.Date &&
		                                                  (r.JsonData.Weekdays.Count == 0 || r.JsonData.Weekdays.Contains((int)night.DayOfWeek))).ToList();
		HotelRateEntity? best = matching
			.Where(r => !r.Tags.Contains(TagHotelRate.Closed) && (r.Price != null || r.Percent != null))
			.OrderByDescending(r => r.RoomId != null)
			.ThenByDescending(r => r.Tags.Contains(TagHotelRate.Holiday))
			.ThenByDescending(r => r.CreatedAt)
			.FirstOrDefault();
		decimal price = best?.Price ?? (best?.Percent != null ? Math.Round(room.PricePerNight * (1 + best.Percent.Value / 100)) : room.PricePerNight);
		return (price, matching.Any(r => r.Tags.Contains(TagHotelRate.Closed)), matching.Max(r => r.JsonData.MinNights));
	}

	private async Task<(decimal Total, string? Error)> StayPrice(HotelRoomEntity room, DateTime checkIn, DateTime checkOut, int guestCount, CancellationToken ct) {
		int nights = (checkOut.Date - checkIn.Date).Days;
		List<HotelRateEntity> rates = await RatesFor(room.HotelId, checkIn, checkOut, ct);
		decimal total = 0;
		string? error = null;
		for (int i = 0; i < nights; i++) {
			(decimal price, bool closed, int? minNights) = NightOf(room, checkIn.Date.AddDays(i), rates);
			if (closed) error ??= ls.Get("thisRoomIsClosedOnTheSelectedDates");
			if (i == 0 && minNights > nights) error ??= ls.Get("minimumStayIsNotMet");
			total += price;
		}

		int extraGuests = Math.Max(0, guestCount - room.Capacity);
		if (extraGuests > 0 && room.JsonData.ExtraGuestPrice.HasValue) total += extraGuests * room.JsonData.ExtraGuestPrice.Value * nights;
		return (total, error);
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

	public async Task<UResponse<IEnumerable<HotelRoomAvailabilityResponse>?>> ReadHotelRoomAvailability(HotelRoomAvailabilityParams p, CancellationToken ct) {
		int nights = (p.CheckOutDate.Date - p.CheckInDate.Date).Days;
		if (nights < 1) return new UResponse<IEnumerable<HotelRoomAvailabilityResponse>?>(null, Usc.BadRequest, ls.Get("checkOutDateMustBeAfterTheCheckInDate"));

		p.SelectorArgs = Safe(p.SelectorArgs, false);
		IQueryable<HotelRoomEntity> q = db.Set<HotelRoomEntity>()
			.Where(x => x.IsAvailable && x.Hotel.Tags.Contains(TagHotel.Active));
		if (p.HotelId.HasValue) q = q.Where(x => x.HotelId == p.HotelId);
		if (p.RoomId.HasValue) q = q.Where(x => x.Id == p.RoomId);

		List<HotelRoomEntity> rooms = await q.ToListAsync(ct);
		if (rooms.Count == 0) return new UResponse<IEnumerable<HotelRoomAvailabilityResponse>?>([]);

		Dictionary<Guid, int> booked = await ReadBookedCounts(rooms.Select(x => x.Id).ToList(), p.CheckInDate, p.CheckOutDate, ct);
		Dictionary<Guid, HotelRoomResponse> projected = await q.Select(Projections.HotelRoomSelector(p.SelectorArgs)).ToDictionaryAsync(x => x.Id, ct);

		List<HotelRoomAvailabilityResponse> result = [];
		foreach (HotelRoomEntity room in rooms) {
			HotelRoomResponse? dto = projected.GetValueOrDefault(room.Id);
			if (dto == null) continue;
			int maxGuests = room.Capacity + (room.JsonData.ExtraGuestCapacity ?? 0);
			(decimal stay, string? closed) = await StayPrice(room, p.CheckInDate, p.CheckOutDate, Math.Max(1, p.GuestCount), ct);
			result.Add(new HotelRoomAvailabilityResponse {
				Room = dto,
				AvailableQuantity = closed != null ? 0 : Math.Max(0, room.Quantity - booked.GetValueOrDefault(room.Id, 0)),
				NightCount = nights,
				TotalPrice = stay,
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

		if (!await os.HasModule(null, room.Hotel.OrganizationId, TagModule.Hotel, ct)) return new UResponse<HotelReservationResponse?>(null, Usc.Conflict, ls.Get("thisHotelIsCurrentlyNotAcceptingReservations"));
		if (await os.IsBlacklisted(room.Hotel.OrganizationId, userData.Id, ct)) return new UResponse<HotelReservationResponse?>(null, Usc.Forbidden, ls.Get("youCannotBookThisPlace"));
		(decimal total, string? priceError) = await StayPrice(room, p.CheckInDate, p.CheckOutDate, guestCount, ct);
		if (priceError != null) return new UResponse<HotelReservationResponse?>(null, Usc.Conflict, priceError);
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
				Guests = (p.Guests ?? []).Select(Guest).ToList()
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
		if (!isOwner && !await CanAct(userData, e.Hotel, TagUser.PermissionManageReservations, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (e.Tags.Contains(TagHotelReservation.Cancelled)) return new UResponse(Usc.Conflict, ls.Get("thisReservationHasAlreadyBeenCancelled"));
		if (e.Tags.Contains(TagHotelReservation.CheckedIn) || e.Tags.Contains(TagHotelReservation.CheckedOut)) return new UResponse(Usc.Conflict, ls.Get("aReservationThatHasAlreadyBeenCheckedInCannotBeCancelled"));

		double hoursToCheckIn = (e.CheckInDate - DateTime.UtcNow).TotalHours;
		decimal penalty = hoursToCheckIn >= e.Hotel.JsonData.CancellationFreeHours
			? 0
			: Math.Min(e.TotalPrice, e.Hotel.JsonData.CancellationPenaltyNights * e.Room.PricePerNight);

		decimal paid = e.Invoices.Sum(x => x.PaidAmount);
		decimal refund = Math.Max(0, paid - penalty);

		List<TagHotelReservation> previousTags = e.Tags.ToList();
		List<TagHotelReservation> cancelledTags = [TagHotelReservation.Cancelled];
		int claimed = await db.Set<HotelReservationEntity>()
			.Where(x => x.Id == e.Id && !x.Tags.Contains(TagHotelReservation.Cancelled) && !x.Tags.Contains(TagHotelReservation.CheckedIn) && !x.Tags.Contains(TagHotelReservation.CheckedOut))
			.ExecuteUpdateAsync(u => u.SetProperty(x => x.Tags, cancelledTags), ct);
		if (claimed == 0) return new UResponse(Usc.Conflict, ls.Get("thisReservationHasAlreadyBeenCancelled"));

		if (refund > 0) {
			UResponse<WalletTxnResponse?> transfer = await ws.Transfer(new WalletTransferParams {
				SenderId = OrganizationService.WalletOf(e.Hotel.OrganizationId),
				ReceiverId = e.UserId,
				Amount = refund,
				Detail1 = ls.Get("hotelReservationRefund"),
				KeyValues = [
					new KeyValue { Key = ULocalizedConstants.Hotel, Value = e.Hotel.Title },
					new KeyValue { Key = ULocalizedConstants.Room, Value = e.Room.Title },
					new KeyValue { Key = ULocalizedConstants.CheckInDate, Value = e.CheckInDate.ToString("O") },
					new KeyValue { Key = ULocalizedConstants.CheckOutDate, Value = e.CheckOutDate.ToString("O") },
					new KeyValue { Key = ULocalizedConstants.Penalty, Value = penalty.ToIntString() },
					new KeyValue { Key = ULocalizedConstants.RefundAmount, Value = refund.ToIntString() },
					new KeyValue { Key = ULocalizedConstants.ReservationId, Value = e.Id.ToString() }
				],
				TagWalletTxn = [TagWalletTxn.HotelReservationRefund]
			}, ct);
			if (transfer.Result == null) {
				await db.Set<HotelReservationEntity>().Where(x => x.Id == e.Id).ExecuteUpdateAsync(u => u.SetProperty(x => x.Tags, previousTags), ct);
				return new UResponse(transfer.Status, transfer.Message);
			}
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
		foreach (HotelInvoiceEntity invoice in e.Invoices) await SyncHotelInvoice(invoice, false, ct);
		await acc.Post(e.Hotel.OrganizationId, TagVoucher.Refund, e.Id, e.HotelId, e.UserId, $"{ls.Get("hotelReservationRefund", "fa")} - {e.Hotel.Title}", DateTime.UtcNow, ct,
			new AccountingLeg(TagAccount.Receivable, refund), new AccountingLeg(TagAccount.Wallet, -refund));

		await AddNotification(e.UserId, TagNotification.ReservationCancelled, ls.Get("reservationCancelled"), e.Hotel.Title, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse(Usc.Success, ls.Get("theReservationWasCancelled"));
	}

	private static List<KeyValue> HotelInvoiceKeyValues(HotelInvoiceEntity e) => e.Reservation == null
		? [new KeyValue { Key = ULocalizedConstants.InvoiceId, Value = e.Id.ToString() }]
		: [
			new KeyValue { Key = ULocalizedConstants.Hotel, Value = e.Reservation.Hotel.Title },
			new KeyValue { Key = ULocalizedConstants.Room, Value = e.Reservation.Room.Title },
			new KeyValue { Key = ULocalizedConstants.CheckInDate, Value = e.Reservation.CheckInDate.ToString("O") },
			new KeyValue { Key = ULocalizedConstants.CheckOutDate, Value = e.Reservation.CheckOutDate.ToString("O") },
			new KeyValue { Key = ULocalizedConstants.NumberOfNights, Value = e.Reservation.JsonData.NightCount.ToString() },
			new KeyValue { Key = ULocalizedConstants.ReservationId, Value = e.Reservation.Id.ToString() },
			new KeyValue { Key = ULocalizedConstants.InvoiceId, Value = e.Id.ToString() }
		];

	public async Task<UResponse> PayHotelInvoiceInternal(HotelInvoicePayParams p, CancellationToken ct) {
		HotelInvoiceEntity? e = await db.Set<HotelInvoiceEntity>().AsTracking()
			.Include(x => x.Reservation).ThenInclude(x => x!.Hotel)
			.Include(x => x.Reservation).ThenInclude(x => x!.Room)
			.FirstOrDefaultAsync(x => x.Id == p.InvoiceId, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("invoiceNotFound"));
		if (!e.Tags.Contains(TagHotelInvoice.NotPaid)) return new UResponse(Usc.Conflict, ls.Get("thisInvoiceHasAlreadyBeenPaid"));

		List<TagHotelInvoice> unpaidTags = e.Tags.ToList();
		List<TagHotelInvoice> paidTags = [TagHotelInvoice.PaidOnline];
		int claimed = await db.Set<HotelInvoiceEntity>().Where(x => x.Id == e.Id && x.Tags.Contains(TagHotelInvoice.NotPaid)).ExecuteUpdateAsync(u => u.SetProperty(x => x.Tags, paidTags), ct);
		if (claimed == 0) return new UResponse(Usc.Conflict, ls.Get("thisInvoiceHasAlreadyBeenPaid"));

		decimal amount = e.DebtAmount + e.PenaltyAmount - e.CreditorAmount - e.PaidAmount;
		if (amount > 0) {
			UResponse<WalletTxnResponse?> transfer = await ws.Transfer(new WalletTransferParams {
				SenderId = p.UserId,
				ReceiverId = OrganizationService.WalletOf(e.Reservation?.Hotel.OrganizationId),
				Amount = amount,
				Detail1 = ls.Get("hotelReservationPayment"),
				KeyValues = HotelInvoiceKeyValues(e),
				TagWalletTxn = [TagWalletTxn.HotelReservation]
			}, ct);
			if (transfer.Result == null) {
				await db.Set<HotelInvoiceEntity>().Where(x => x.Id == e.Id).ExecuteUpdateAsync(u => u.SetProperty(x => x.Tags, unpaidTags), ct);
				return new UResponse(transfer.Status, transfer.Message);
			}
			await acc.TakeCommission(e.Reservation?.Hotel.OrganizationId, amount, ls.Get("hotelReservationPayment"), HotelInvoiceKeyValues(e), e.Id, e.Reservation?.HotelId, ct);
		}

		e.PaidAmount += amount;
		e.Tags = [TagHotelInvoice.PaidOnline];
		await SyncHotelInvoice(e, false, ct);
		await acc.Post(e.Reservation?.Hotel.OrganizationId, TagVoucher.Payment, e.Id, e.Reservation?.HotelId, e.Reservation?.UserId, $"{ls.Get("hotelReservationPayment", "fa")} - {e.Reservation?.Hotel.Title}", DateTime.UtcNow, ct,
			new AccountingLeg(TagAccount.Wallet, amount), new AccountingLeg(TagAccount.Receivable, -amount));

		if (e.Reservation != null) {
			HotelReservationEntity? reservation = await db.Set<HotelReservationEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == e.ReservationId, ct);
			if (reservation != null && reservation.Tags.Contains(TagHotelReservation.Pending)) {
				reservation.Tags = [TagHotelReservation.Confirmed];
				await AddNotification(reservation.UserId, TagNotification.ReservationConfirmed, ls.Get("reservationConfirmed"), e.Reservation.Hotel.Title, ct);
			}
		}

		await db.SaveChangesAsync(ct);
		return new UResponse(Usc.Success, ls.Get("paymentCompleted"));
	}

	public async Task<UResponse<Guid?>> CreateHotelInvoice(HotelInvoiceCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		HotelReservationEntity? reservation = await db.Set<HotelReservationEntity>().Include(x => x.Hotel).FirstOrDefaultAsync(x => x.Id == p.ReservationId, ct);
		if (reservation == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("reservationNotFound"));
		if (!await CanAct(userData, reservation.Hotel, TagUser.PermissionManageInvoices, ct))
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
		if (!OrganizationService.IsFull(userData)) {
			Guid uid = UserIdOf(userData);
			bool scoped = OrganizationService.IsScopedAdmin(userData);
			q = q.Where(x => x.Reservation != null && (x.Reservation.UserId == uid || scoped && x.Reservation.Hotel.AdminUserIds.Contains(uid)));
			p.SelectorArgs = Safe(p.SelectorArgs);
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
		Dictionary<Guid, HotelInvoiceEntity> entities = await db.Set<HotelInvoiceEntity>().AsTracking().Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);

		bool anyChanges = false;
		foreach (HotelInvoiceResponse dto in response.Result!) {
			HotelInvoiceEntity? entity = entities.GetValueOrDefault(dto.Id);
			if (entity == null || entity.JsonData.PenaltyPrecentEveryDate <= 0) continue;
			decimal expectedPenalty = PenaltyOf(entity.DebtAmount, entity.JsonData.PenaltyPrecentEveryDate, entity.DueDate, DateTime.UtcNow);

			bool needsPenaltyUpdate =
				entity.PaidAmount < entity.DebtAmount + entity.PenaltyAmount &&
				entity.DueDate <= DateTime.UtcNow &&
				entity.PenaltyAmount < expectedPenalty;

			if (needsPenaltyUpdate) {
				entity.PenaltyAmount = expectedPenalty;
				dto.PenaltyAmount = expectedPenalty;
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
		if (!(e.Reservation == null ? OrganizationService.IsFull(userData) : await CanAct(userData, e.Reservation.Hotel, TagUser.PermissionManageInvoices, ct)))
			return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (p.ReservationId.HasValue && p.ReservationId != e.ReservationId) {
			HotelReservationEntity? to = await db.Set<HotelReservationEntity>().Include(x => x.Hotel).FirstOrDefaultAsync(x => x.Id == p.ReservationId, ct);
			if (to == null || !await CanAct(userData, to.Hotel, TagUser.PermissionManageInvoices, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		}

		if (p.DebtAmount.IsNotNull()) e.DebtAmount = p.DebtAmount.Value;
		if (p.CreditorAmount.IsNotNull()) e.CreditorAmount = p.CreditorAmount.Value;
		if (p.PenaltyAmount.IsNotNull()) e.PenaltyAmount = p.PenaltyAmount.Value;
		if (p.PaidAmount.IsNotNull()) e.PaidAmount = p.PaidAmount.Value;
		if (p.DueDate.HasValue) e.DueDate = p.DueDate.Value;
		if (p.ReservationId.HasValue) e.ReservationId = p.ReservationId.Value;
		if (p.PenaltyPrecentEveryDate.IsNotNull()) e.JsonData.PenaltyPrecentEveryDate = p.PenaltyPrecentEveryDate.Value;

		e.ApplyUpdateParam<HotelInvoiceEntity, TagHotelInvoice, HotelInvoiceJson>(p);
		await SyncHotelInvoice(e, false, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteHotelInvoice(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		HotelInvoiceEntity? e = await db.Set<HotelInvoiceEntity>().Include(x => x.Reservation).ThenInclude(x => x!.Hotel).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("invoiceNotFound"));
		if (!(e.Reservation == null ? OrganizationService.IsFull(userData) : await CanAct(userData, e.Reservation.Hotel, TagUser.PermissionDeleteInvoices, ct)))
			return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		await SyncHotelInvoice(e, true, ct);
		await db.SaveChangesAsync(ct);
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
		bool isManager = e.Reservation != null && await CanAct(userData, e.Reservation.Hotel, TagUser.PermissionPayInvoices, ct);
		if (!isOwner && !isManager) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		return await PayHotelInvoiceInternal(new HotelInvoicePayParams { InvoiceId = e.Id, UserId = e.Reservation!.UserId }, ct);
	}

	private async Task SyncHotelInvoice(HotelInvoiceEntity e, bool removed, CancellationToken ct) {
		HotelReservationEntity? r = e.Reservation;
		if (r?.Hotel.OrganizationId == null) return;

		DateTime now = DateTime.UtcNow;
		bool charged = !removed && (e.DueDate <= now || !e.Tags.Contains(TagHotelInvoice.NotPaid) || e.PaidAmount > 0);
		if (!charged && !e.JsonData.Posted) return;

		decimal amount = !charged ? 0
			: r.Tags.Contains(TagHotelReservation.Cancelled) ? Math.Max(0, e.PaidAmount - e.CreditorAmount)
			: Math.Max(0, e.DebtAmount + e.PenaltyAmount - e.CreditorAmount);
		e.JsonData.VatPercent ??= e.JsonData.Posted ? 0 : await acc.VatPercentOf(r.Hotel.OrganizationId.Value, ct);
		await acc.SyncCharge(r.Hotel.OrganizationId.Value, e.Id, r.HotelId, r.UserId, !e.JsonData.Posted && e.DueDate < now ? e.DueDate : now, $"{ls.Get("hotelInvoiceIssued", "fa")} - {r.Hotel.Title}", new Dictionary<TagAccount, decimal> { [e.Tags.Contains(TagHotelInvoice.Extra) ? TagAccount.ServiceIncome : TagAccount.HotelIncome] = amount }, ct, e.JsonData.VatPercent.Value);
		e.JsonData.Posted = charged;
	}

	public async Task<UResponse> ReceiveHotelInvoice(InvoiceReceiveParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		HotelInvoiceEntity? e = await db.Set<HotelInvoiceEntity>().AsTracking()
			.Include(x => x.Reservation).ThenInclude(x => x!.Hotel)
			.FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e?.Reservation == null) return new UResponse(Usc.NotFound, ls.Get("invoiceNotFound"));
		HotelReservationEntity r = e.Reservation;
		if (!await CanAct(userData, r.Hotel, TagUser.PermissionPayInvoices, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (r.Tags.Contains(TagHotelReservation.Cancelled)) return new UResponse(Usc.Conflict, ls.Get("thisReservationHasAlreadyBeenCancelled"));
		if (!e.Tags.Contains(TagHotelInvoice.NotPaid)) return new UResponse(Usc.Conflict, ls.Get("thisInvoiceHasAlreadyBeenPaid"));

		decimal due = e.DebtAmount + e.PenaltyAmount - e.CreditorAmount - e.PaidAmount;
		decimal amount = p.Amount ?? due;
		if (amount <= 0 || amount > due) return new UResponse(Usc.BadRequest, ls.Get("amountIsNotValid"));

		e.PaidAmount += amount;
		if (amount == due) {
			e.Tags = [TagHotelInvoice.PaidManual];
			if (r.Tags.Contains(TagHotelReservation.Pending)) r.Tags = [TagHotelReservation.Confirmed];
		}
		await SyncHotelInvoice(e, false, ct);
		UResponse? error = await acc.PostReceipt(r.Hotel.OrganizationId, e.Id, r.HotelId, r.UserId, null, amount, p, $"{ls.Get("invoiceReceipt", "fa")} - {r.Hotel.Title}", userData.Id, ct);
		if (error != null) return error;

		await AddNotification(r.UserId, TagNotification.InvoicePaid, ls.Get("invoicePaid"), r.Hotel.Title, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse(Usc.Success, ls.Get("paymentCompleted"));
	}

	public async Task<bool?> CanActOnPlaceOf(JwtClaimData u, Guid? hotelId, Guid? hotelRoomId, CancellationToken ct) {
		if (hotelId == null && hotelRoomId == null) return null;
		var hotel = hotelId != null
			? await db.Set<HotelEntity>().Where(x => x.Id == hotelId).Select(x => new { x.AdminUserIds, x.OrganizationId }).FirstOrDefaultAsync(ct)
			: await db.Set<HotelRoomEntity>().Where(x => x.Id == hotelRoomId).Select(x => new { x.Hotel.AdminUserIds, x.Hotel.OrganizationId }).FirstOrDefaultAsync(ct);
		return await os.HasModule(u, hotel?.OrganizationId, TagModule.Hotel, ct) && await os.CanActOnPlace(u, hotel?.OrganizationId, hotel?.AdminUserIds ?? [], TagUser.PermissionManageHotels, ct);
	}

	public IQueryable<Guid> RelatedUserIds(Guid userId) => db.Set<HotelReservationEntity>().Where(r => r.Hotel.AdminUserIds.Contains(userId)).Select(r => r.UserId);

	public async Task ReopenPayment(Guid sourceId, decimal amount, CancellationToken ct) {
		HotelInvoiceEntity? h = await db.Set<HotelInvoiceEntity>().AsTracking().Include(x => x.Reservation).ThenInclude(x => x!.Hotel).FirstOrDefaultAsync(x => x.Id == sourceId, ct);
		if (h == null) return;
		h.PaidAmount = Math.Max(0, h.PaidAmount - amount);
		if (!h.Tags.Contains(TagHotelInvoice.NotPaid)) h.Tags = [..h.Tags.Where(x => (int)x < 200), TagHotelInvoice.NotPaid];
		await SyncHotelInvoice(h, false, ct);
	}

	public async Task<List<AccountingDue>> Outstanding(Guid organizationId, DateTime now, CancellationToken ct) =>
		(await db.Set<HotelInvoiceEntity>()
			.Where(x => x.Tags.Contains(TagHotelInvoice.NotPaid) && x.DueDate <= now && x.Reservation != null && !x.Reservation.Tags.Contains(TagHotelReservation.Cancelled) && x.Reservation.Hotel.OrganizationId == organizationId)
			.Select(x => new { PersonId = x.Reservation!.UserId, x.DueDate, Amount = x.DebtAmount + x.PenaltyAmount - x.CreditorAmount - x.PaidAmount })
			.ToListAsync(ct))
		.Select(x => new AccountingDue(x.PersonId, x.DueDate, x.Amount)).ToList();

	public async Task PostDueInvoices(CancellationToken ct) {
		DateTime now = DateTime.UtcNow;
		List<HotelInvoiceEntity> invoices = await db.Set<HotelInvoiceEntity>().AsTracking()
			.Include(x => x.Reservation).ThenInclude(x => x!.Hotel)
			.Where(x => !x.JsonData.Posted && x.DueDate <= now && x.Reservation != null && x.Reservation.Hotel.OrganizationId != null)
			.ToListAsync(ct);
		foreach (HotelInvoiceEntity e in invoices) await SyncHotelInvoice(e, false, ct);
		await db.SaveChangesAsync(ct);
	}

	public async Task<UResponse<HotelDashboardResponse?>> ReadHotelDashboard(DashboardRangeParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<HotelDashboardResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<HotelDashboardResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		bool tenant = Core.App.MultiTenant && !userData.IsSystemAdmin;
		if (tenant ? !await os.HasOrganizationPermission(userData, null, TagUser.PermissionViewDashboard, ct) : !userData.IsSuperAdmin)
			return new UResponse<HotelDashboardResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		Guid uid = userData.Id;
		IQueryable<HotelEntity> hotels = db.Set<HotelEntity>().Where(x => !tenant || x.AdminUserIds.Contains(uid));
		IQueryable<HotelRoomEntity> hotelRooms = db.Set<HotelRoomEntity>().Where(x => !tenant || x.Hotel.AdminUserIds.Contains(uid));
		IQueryable<HotelReservationEntity> reservations = db.Set<HotelReservationEntity>().Where(x => !tenant || x.Hotel.AdminUserIds.Contains(uid));
		IQueryable<UserEntity> guests = db.Set<UserEntity>().Where(x => reservations.Any(r => r.UserId == x.Id));

		DateTime now = DateTime.UtcNow;
		DateTime to = p.ToDate ?? now;
		DateTime from = p.FromDate ?? to.AddDays(-30);

		Dictionary<Guid, int> quantities = await hotelRooms.ToDictionaryAsync(x => x.Id, x => Math.Max(1, x.Quantity), ct);
		int hotelRoomsCount = quantities.Values.Sum();
		int hotelRoomsOccupiedCount = (await reservations
				.Where(x => x.CheckInDate <= now && x.CheckOutDate > now)
				.Where(x => !x.Tags.Contains(TagHotelReservation.Cancelled) && !x.Tags.Contains(TagHotelReservation.NoShow) && !x.Tags.Contains(TagHotelReservation.CheckedOut))
				.GroupBy(x => x.RoomId).Select(g => new { g.Key, Count = g.Count() }).ToListAsync(ct))
			.Sum(x => Math.Min(x.Count, quantities.GetValueOrDefault(x.Key, x.Count)));

		List<UserEntity> recentGuestEntities = await guests.OrderByDescending(x => x.CreatedAt).Take(5).ToListAsync(ct);

		return new UResponse<HotelDashboardResponse?>(new HotelDashboardResponse {
			GeneratedAt = DateTime.UtcNow,
			GuestsCount = await guests.CountAsync(ct),
			NewGuestsCount = await guests.CountAsync(x => x.CreatedAt >= from && x.CreatedAt <= to, ct),
			HotelsCount = await hotels.CountAsync(ct),
			HotelRoomsCount = hotelRoomsCount,
			HotelRoomsAvailableCount = hotelRoomsCount - hotelRoomsOccupiedCount,
			HotelRoomsOccupiedCount = hotelRoomsOccupiedCount,
			HotelOccupancyRate = hotelRoomsCount == 0 ? 0 : Math.Round(hotelRoomsOccupiedCount * 100.0 / hotelRoomsCount, 1),
			ReservationsCount = await reservations.CountAsync(ct),
			RecentGuests = recentGuestEntities.Select(x => new RecentUserItem {
				Id = x.Id, DisplayName = $"{x.FirstName} {x.LastName}".Trim() is { Length: > 0 } n ? n : x.UserName,
				UserName = x.UserName, PhoneNumber = x.PhoneNumber, CreatedAt = x.CreatedAt
			}).ToList(),
			HotelsByCity = await hotels
				.GroupBy(x => x.CityCode)
				.Select(g => new HotelCityItem { Name = g.Key, Count = g.Count() })
				.OrderByDescending(x => x.Count).Take(10).ToListAsync(ct)
		});
	}

	public async Task<UResponse<List<KeyValue>?>> SeedHotels(BaseParams p, CancellationToken ct) {
		if (ts.ExtractClaims(p.Token) is not { IsSystemAdmin: true }) return new UResponse<List<KeyValue>?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		Guid adminId = Core.App.Users.SystemAdmin.Id;
		DateTime now = DateTime.UtcNow;
		DateTime today = now.Date;

		if (!await db.Set<UserEntity>().AnyAsync(x => x.Id == adminId, ct)) return new UResponse<List<KeyValue>?>(null, Usc.BadRequest, "Run DataSeeder/Users first.");
		if (await db.Set<HotelEntity>().AnyAsync(x => x.Title == "هتل سنتی عباسی اصفهان", ct)) return new UResponse<List<KeyValue>?>(null, Usc.Conflict, "Hotel demo data already exists.");

		(List<UserEntity> users, List<Guid> userIds) = await seeds.DemoUsers(ct);
		string[] firstNames = DataSeedService.DemoFirstNames;
		string[] lastNames = DataSeedService.DemoLastNames;
		Guid UserAt(int i) => userIds[i % userIds.Count];

		List<HotelEntity> hotels = [];
		List<HotelRoomEntity> rooms = [];
		List<HotelReservationEntity> reservations = [];
		List<HotelInvoiceEntity> hotelInvoices = [];
		List<CommentEntity> comments = [];

		CommentEntity Review(Guid? hotelId, Guid? dormId, int userIndex, decimal score, string text, TagComment tag, int daysAgo) => new() {
			Id = Guid.CreateVersion7(),
			CreatedAt = now.AddDays(-daysAgo),
			CreatorId = UserAt(userIndex),
			UserId = UserAt(userIndex),
			HotelId = hotelId,
			DormId = dormId,
			Score = score,
			Description = text,
			Tags = [tag],
			JsonData = new CommentJson()
		};

		(string Title, string City, int Stars, string Address, string Phone, List<TagHotel> Tags, HotelJson Json, (string Title, int Capacity, decimal Price, int Quantity, string Bed, double Size, int Floor, List<TagRoom> Tags)[] Rooms)[] hotelSeeds = [
			("هتل سنتی عباسی اصفهان", "104005", 4, "اصفهان، خیابان چهارباغ عباسی، کوچه ملک", "03132200100",
				[
					TagHotel.Traditional, TagHotel.Active, TagHotel.Featured, TagHotel.Approved,
					TagHotel.ChildrenAllowed, TagHotel.ExtraBedAvailable, TagHotel.PriceIncludesTax,
					TagHotel.Wifi, TagHotel.Parking, TagHotel.Reception24, TagHotel.LuggageStorage, TagHotel.Cafe, TagHotel.Garden, TagHotel.Cctv,
					TagHotel.Breakfast, TagHotel.HalfBoard
				],
				new HotelJson {
					Highlights = ["حیاط مرکزی با حوض و چهار باغچه", "ده دقیقه پیاده تا میدان نقش جهان", "صبحانه سنتی هر روز", "بنای مرمت‌شده‌ی دوره قاجار"],
					Website = "https://khabroom.com", Whatsapp = "989120000001", Instagram = "khabroom", Telegram = "khabroom",
					HowToGetThere = "از خیابان چهارباغ عباسی وارد کوچه‌ی ملک شوید؛ هتل سمت راست، روبه‌روی نانوایی سنتی است.",
					Nearby = [
						new PlaceNearby { Title = "میدان نقش جهان", DistanceMeters = 800, Minutes = 10 }, new PlaceNearby { Title = "سی‌وسه‌پل", DistanceMeters = 1200, Minutes = 15 }, new PlaceNearby { Title = "بازار قیصریه", DistanceMeters = 900, Minutes = 11 },
						new PlaceNearby { Title = "ایستگاه مترو", DistanceMeters = 1500, Minutes = 5 }
					],
					Faqs = [new PlaceFaq { Question = "آیا پارکینگ دارید؟", Answer = "بله، پارکینگ اختصاصی و رایگان برای مهمانان وجود دارد." }, new PlaceFaq { Question = "امکان تحویل زودتر اتاق هست؟", Answer = "با هماهنگی قبلی و بسته به ظرفیت، ورود از ساعت ۱۲ ممکن است." }, new PlaceFaq { Question = "صبحانه شامل چه چیزهایی است؟", Answer = "نان سنگک، پنیر، تخم‌مرغ محلی، مربا، عسل و چای سماوری." }],
					Description = "خانه‌ی قاجاری مرمت‌شده با حیاط مرکزی، حوض و چهار باغچه؛ ده دقیقه پیاده تا میدان نقش جهان. اتاق‌ها دور حیاط چیده شده‌اند و شب‌ها سکوت کامل است.",
					Policies = "ورود از ساعت ۱۴ و خروج تا ساعت ۱۲ ظهر. پرداخت بیعانه هنگام رزرو الزامی است. کودکان زیر ۶ سال با والدین رایگان اقامت می‌کنند.",
					CheckInTime = "14:00", CheckOutTime = "12:00",
					Rules = ["استعمال دخانیات در اتاق‌ها ممنوع است", "ورود حیوان خانگی ممنوع است", "سکوت پس از ساعت ۲۳ رعایت شود"],
					Latitude = 32.6607, Longitude = 51.6693, CancellationFreeHours = 48, CancellationPenaltyNights = 1
				},
				[
					("اتاق دو تخته سنتی", 2, 4_200_000m, 12, "دو تخت", 24, 1, [TagRoom.Double, TagRoom.BreakfastIncluded, TagRoom.CourtyardView, TagRoom.Tv, TagRoom.Minibar, TagRoom.AirConditioning, TagRoom.Wardrobe, TagRoom.PrivateBathroom]),
					("اتاق سه تخته", 3, 5_400_000m, 8, "سه تخت", 32, 1, [TagRoom.Triple, TagRoom.BreakfastIncluded, TagRoom.CourtyardView, TagRoom.Tv, TagRoom.Minibar, TagRoom.AirConditioning, TagRoom.Kettle, TagRoom.PrivateBathroom]),
					("سوئیت خانوادگی", 4, 7_800_000m, 4, "یک دو نفره و دو تک", 48, 2, [TagRoom.Family, TagRoom.BreakfastIncluded, TagRoom.GardenView, TagRoom.Tv, TagRoom.Minibar, TagRoom.AirConditioning, TagRoom.Fridge, TagRoom.Kettle, TagRoom.Balcony, TagRoom.Bathtub])
				]),

			("هتل پارسیان ولیعصر", "108012", 5, "تهران، بلوار ولیعصر، بالاتر از پارک ملت", "02122000200",
				[
					TagHotel.Hotel, TagHotel.Active, TagHotel.Featured, TagHotel.Approved,
					TagHotel.ChildrenAllowed, TagHotel.ExtraBedAvailable,
					TagHotel.Wifi, TagHotel.Parking, TagHotel.Elevator, TagHotel.Reception24, TagHotel.LuggageStorage, TagHotel.Laundry, TagHotel.AirportShuttle, TagHotel.Restaurant, TagHotel.Cafe,
					TagHotel.RoomService, TagHotel.Pool, TagHotel.Gym, TagHotel.Sauna, TagHotel.Spa, TagHotel.MeetingRoom, TagHotel.Wheelchair, TagHotel.Cctv,
					TagHotel.RoomOnly, TagHotel.Breakfast, TagHotel.HalfBoard, TagHotel.FullBoard
				],
				new HotelJson {
					Highlights = ["استخر و سونای اختصاصی مهمانان", "ترانسفر رایگان فرودگاه امام", "سالن همایش تا ۳۰۰ نفر", "ده دقیقه تا مترو ولیعصر"],
					Website = "https://example.com/parsian", Whatsapp = "989120000002", Instagram = "parsian_valiasr",
					HowToGetThere = "ورودی اصلی از بلوار ولیعصر است؛ پارکینگ طبقات منفی از خیابان فرعی شرقی.",
					Nearby = [
						new PlaceNearby { Title = "مترو ولیعصر", DistanceMeters = 600, Minutes = 8 }, new PlaceNearby { Title = "پارک ملت", DistanceMeters = 400, Minutes = 5 }, new PlaceNearby { Title = "بیمارستان آرش", DistanceMeters = 1800, Minutes = 6 },
						new PlaceNearby { Title = "فرودگاه امام خمینی", DistanceMeters = 48000, Minutes = 50 }
					],
					Faqs = [new PlaceFaq { Question = "آیا سالن همایش دارید؟", Answer = "بله، سه سالن با ظرفیت ۵۰ تا ۳۰۰ نفر، با تجهیزات کامل صوتی و تصویری." }, new PlaceFaq { Question = "ساعت کار استخر چیست؟", Answer = "هر روز از ۷ صبح تا ۲۲ شب، ویژه‌ی مهمانان هتل." }],
					Description = "هتل پنج‌ستاره‌ی مدرن در قلب تهران با استخر سرپوشیده، اسپا و سالن‌های همایش؛ مناسب سفرهای کاری و خانوادگی.",
					Policies = "ورود از ساعت ۱۴ و خروج تا ۱۲. کارت ملی یا گذرنامه هنگام ورود الزامی است. کودکان زیر ۱۲ سال با استفاده از تخت موجود رایگان هستند.",
					CheckInTime = "14:00", CheckOutTime = "12:00",
					Rules = ["ساعت سکوت ۲۳ تا ۷ صبح", "میهمان‌پذیری در لابی تا ساعت ۲۲ مجاز است"],
					Latitude = 35.7580, Longitude = 51.4090, CancellationFreeHours = 24, CancellationPenaltyNights = 1
				},
				[
					("اتاق استاندارد", 2, 6_500_000m, 30, "دو تخت", 28, 5, [TagRoom.Double, TagRoom.BreakfastIncluded, TagRoom.CityView, TagRoom.Tv, TagRoom.Minibar, TagRoom.SafeBox, TagRoom.AirConditioning, TagRoom.HairDryer, TagRoom.PrivateBathroom]),
					("اتاق دلوکس", 3, 8_900_000m, 20, "کینگ + کاناپه", 38, 10, [TagRoom.Deluxe, TagRoom.BreakfastIncluded, TagRoom.CityView, TagRoom.Tv, TagRoom.Minibar, TagRoom.SafeBox, TagRoom.AirConditioning, TagRoom.Desk, TagRoom.Kettle, TagRoom.Bathtub]),
					("سوئیت رویال", 4, 16_500_000m, 6, "کینگ", 75, 17, [TagRoom.Suite, TagRoom.MountainView, TagRoom.Tv, TagRoom.Minibar, TagRoom.SafeBox, TagRoom.AirConditioning, TagRoom.Desk, TagRoom.Kettle, TagRoom.Bathtub, TagRoom.Balcony, TagRoom.Fridge])
				]),

			("مهمان‌پذیر باغ‌نو شیراز", "117044", 3, "شیراز، خیابان لطفعلی‌خان زند، کوچه‌ی باغ‌نو", "07132300300",
				[
					TagHotel.Guesthouse, TagHotel.Active, TagHotel.Approved,
					TagHotel.PetsAllowed, TagHotel.ChildrenAllowed, TagHotel.PriceIncludesTax,
					TagHotel.Wifi, TagHotel.Parking, TagHotel.Garden,
					TagHotel.Breakfast
				],
				new HotelJson {
					Highlights = ["باغچه‌ی نارنج و سایه‌بان", "صبحانه‌ی خانگی", "پنج دقیقه تا ارگ کریم‌خان"],
					Whatsapp = "989120000003", Instagram = "baghnow_shiraz",
					HowToGetThere = "از میدان شهدا به سمت لطفعلی‌خان زند؛ کوچه‌ی باغ‌نو دومین کوچه‌ی سمت چپ.",
					Nearby = [new PlaceNearby { Title = "ارگ کریم‌خان", DistanceMeters = 450, Minutes = 6 }, new PlaceNearby { Title = "بازار وکیل", DistanceMeters = 600, Minutes = 8 }],
					Faqs = [new PlaceFaq { Question = "آیا حیوان خانگی مجاز است؟", Answer = "بله، سگ و گربه‌ی کوچک با هماهنگی قبلی." }],
					Description = "اقامتگاه صمیمی و خانوادگی در بافت تاریخی شیراز، با باغچه‌ی نارنج و صبحانه‌ی خانگی؛ پنج دقیقه تا ارگ کریم‌خان.",
					Policies = "ورود از ساعت ۱۳ و خروج تا ۱۱:۳۰.",
					CheckInTime = "13:00", CheckOutTime = "11:30",
					Rules = ["ورود پس از ساعت ۲۳ با هماهنگی"],
					Latitude = 29.6100, Longitude = 52.5420, CancellationFreeHours = 24, CancellationPenaltyNights = 1
				},
				[
					("اتاق دو نفره", 2, 2_800_000m, 5, "دو تخت", 20, 1, [TagRoom.Double, TagRoom.BreakfastIncluded, TagRoom.GardenView, TagRoom.AirConditioning, TagRoom.Tv, TagRoom.PrivateBathroom]),
					("اتاق خانوادگی", 4, 4_100_000m, 3, "چهار تخت", 34, 1, [TagRoom.Family, TagRoom.BreakfastIncluded, TagRoom.CourtyardView, TagRoom.AirConditioning, TagRoom.Tv, TagRoom.Fridge, TagRoom.PrivateBathroom])
				]),

			("هتل‌آپارتمان زائر مشهد", "111062", 4, "مشهد، خیابان امام رضا، نبش امام رضا ۲۱", "05132400400",
				[
					TagHotel.Apartment, TagHotel.Active, TagHotel.PendingApproval,
					TagHotel.ChildrenAllowed, TagHotel.ExtraBedAvailable, TagHotel.PriceIncludesTax,
					TagHotel.Wifi, TagHotel.Elevator, TagHotel.Parking, TagHotel.Reception24, TagHotel.LuggageStorage, TagHotel.PrayerRoom, TagHotel.Laundry,
					TagHotel.RoomOnly, TagHotel.Breakfast
				],
				new HotelJson {
					Highlights = ["آشپزخانه‌ی کامل در هر واحد", "پنج دقیقه پیاده تا حرم", "نمازخانه و انبار چمدان"],
					Whatsapp = "989120000004",
					HowToGetThere = "ورودی از خیابان امام رضا ۲۱؛ ایستگاه مترو حرم رضوی ۳ دقیقه پیاده.",
					Nearby = [new PlaceNearby { Title = "حرم مطهر امام رضا (ع)", DistanceMeters = 400, Minutes = 5 }, new PlaceNearby { Title = "مترو شهدا", DistanceMeters = 250, Minutes = 3 }, new PlaceNearby { Title = "مرکز خرید رضوی", DistanceMeters = 700, Minutes = 9 }],
					Faqs = [new PlaceFaq { Question = "آیا لوازم آشپزخانه در واحدها موجود است؟", Answer = "بله، ظروف، اجاق و یخچال کامل است." }],
					Description = "واحدهای مبله با آشپزخانه‌ی کامل، ۵ دقیقه پیاده تا حرم مطهر؛ مناسب اقامت‌های چندشبه‌ی خانوادگی.",
					Policies = "حداقل اقامت ۲ شب. ورود ۱۴ و خروج ۱۲.",
					CheckInTime = "14:00", CheckOutTime = "12:00",
					Rules = ["حداقل اقامت دو شب", "تعداد مهمان بیش از ظرفیت واحد مجاز نیست"],
					Latitude = 36.2880, Longitude = 59.6170, CancellationFreeHours = 72, CancellationPenaltyNights = 1
				},
				[
					("واحد یک‌خوابه", 3, 3_600_000m, 10, "یک دو نفره + مبل تخت‌شو", 45, 3, [TagRoom.Triple, TagRoom.CityView, TagRoom.Kitchenette, TagRoom.Fridge, TagRoom.Tv, TagRoom.AirConditioning, TagRoom.PrivateBathroom]),
					("واحد دوخوابه", 5, 5_200_000m, 8, "دو دو نفره + مبل تخت‌شو", 70, 5, [TagRoom.Family, TagRoom.NonRefundable, TagRoom.CityView, TagRoom.Kitchenette, TagRoom.Fridge, TagRoom.Tv, TagRoom.AirConditioning, TagRoom.Balcony, TagRoom.PrivateBathroom])
				])
		];

		int hotelIndex = 0;
		foreach (var h in hotelSeeds) {
			Guid hotelId = Guid.CreateVersion7();
			h.Json.Detail1 = "";
			hotels.Add(new HotelEntity {
				Id = hotelId, CreatedAt = now.AddDays(-80), CreatorId = adminId, Tags = h.Tags, JsonData = h.Json,
				Title = h.Title, CityCode = h.City, Stars = h.Stars, Address = h.Address, PhoneNumber = h.Phone, Email = $"info{hotelIndex + 1}@example.com"
			});

			List<HotelRoomEntity> hotelRooms = [];
			foreach (var r in h.Rooms) {
				HotelRoomEntity room = new() {
					Id = Guid.CreateVersion7(), CreatedAt = now.AddDays(-79), CreatorId = adminId, HotelId = hotelId,
					Tags = [TagRoom.Available, .. r.Tags],
					Title = r.Title, Capacity = r.Capacity, PricePerNight = r.Price, Quantity = r.Quantity, IsAvailable = true, RoomNumber = $"{100 * (hotelRooms.Count + 1)}",
					JsonData = new HotelRoomJson {
						Description = $"{r.Title} با امکانات کامل و پاکیزگی روزانه.", BedType = r.Bed, SizeSquareMeters = r.Size, Floor = r.Floor,
						ExtraGuestCapacity = 1, ExtraGuestPrice = 10000
					}
				};
				hotelRooms.Add(room);
			}

			rooms.AddRange(hotelRooms);

			(TagHotelReservation Status, int StartOffset, int Nights)[] plan = [
				(TagHotelReservation.CheckedOut, -20, 3), (TagHotelReservation.CheckedIn, -1, 3), (TagHotelReservation.Confirmed, 5, 2),
				(TagHotelReservation.Pending, 12, 2), (TagHotelReservation.Cancelled, 3, 2), (TagHotelReservation.CheckedOut, -45, 4)
			];
			for (int i = 0; i < plan.Length; i++) {
				HotelRoomEntity room = hotelRooms[i % hotelRooms.Count];
				Guid userId = UserAt(hotelIndex * 3 + i);
				DateTime checkIn = today.AddDays(plan[i].StartOffset);
				DateTime checkOut = checkIn.AddDays(plan[i].Nights);
				decimal total = room.PricePerNight * plan[i].Nights;
				bool cancelled = plan[i].Status == TagHotelReservation.Cancelled;
				Guid reservationId = Guid.CreateVersion7();
				reservations.Add(new HotelReservationEntity {
					Id = reservationId, CreatedAt = checkIn.AddDays(-10), CreatorId = userId, Tags = [plan[i].Status],
					CheckInDate = checkIn, CheckOutDate = checkOut, GuestCount = Math.Min(room.Capacity, 2 + i % 2), TotalPrice = total,
					UserId = userId, RoomId = room.Id, HotelId = hotelId,
					JsonData = new HotelReservationJson {
						GuestName = $"{firstNames[(hotelIndex * 3 + i) % firstNames.Length]} {lastNames[(hotelIndex * 3 + i) % lastNames.Length]}",
						GuestPhone = $"0912000{(hotelIndex * 3 + i) % 12 + 1:0000}", Notes = i % 2 == 0 ? "لطفاً اتاق طبقه‌ی بالا باشد." : null,
						NightCount = plan[i].Nights, ReservationCode = $"KH{hotelIndex}{i}{Random.Shared.Next(1000, 9999)}",
						Guests = [new ReservationGuestJson { FullName = $"{firstNames[(hotelIndex * 3 + i) % firstNames.Length]} {lastNames[(hotelIndex * 3 + i) % lastNames.Length]}", PhoneNumber = $"0912000{(hotelIndex * 3 + i) % 12 + 1:0000}" }],
						CancelledAt = cancelled ? checkIn.AddDays(-4) : null, CancelReason = cancelled ? "تغییر برنامه‌ی سفر" : null,
						CancellationPenalty = cancelled ? room.PricePerNight : null, RefundAmount = cancelled ? total - room.PricePerNight : null
					}
				});
				bool paid = plan[i].Status is TagHotelReservation.CheckedOut or TagHotelReservation.CheckedIn or TagHotelReservation.Confirmed;
				hotelInvoices.Add(new HotelInvoiceEntity {
					Id = Guid.CreateVersion7(), CreatedAt = checkIn.AddDays(-10), CreatorId = userId,
					Tags = cancelled ? [TagHotelInvoice.Refunded, TagHotelInvoice.Full] : paid ? [TagHotelInvoice.Paid, i % 2 == 0 ? TagHotelInvoice.PaidOnline : TagHotelInvoice.PaidManual, TagHotelInvoice.Full] : [TagHotelInvoice.NotPaid, TagHotelInvoice.Full],
					DebtAmount = total, CreditorAmount = cancelled ? total - room.PricePerNight : 0, PaidAmount = paid || cancelled ? total : 0, PenaltyAmount = cancelled ? room.PricePerNight : 0,
					ReservationId = reservationId, DueDate = checkIn, JsonData = new HotelInvoiceJson { PenaltyPrecentEveryDate = 0 }
				});
			}

			string[] good = ["اقامتی بسیار دلنشین؛ پرسنل مهربان و اتاق‌ها تمیز بودند.", "موقعیت عالی و صبحانه‌ی خوشمزه. حتماً دوباره می‌آیم.", "قیمت مناسب نسبت به امکانات. تحویل اتاق سریع بود.", "برای سفر خانوادگی گزینه‌ی خوبی است؛ فضای آرام و امن."];
			for (int i = 0; i < good.Length; i++) comments.Add(Review(hotelId, null, hotelIndex * 4 + i, i == 2 ? 4m : i == 3 ? 4.5m : 5m, good[i], TagComment.Released, 5 + i * 9));
			comments.Add(Review(hotelId, null, hotelIndex + 6, 3m, "نظر در صف بررسی: سرویس بهداشتی می‌توانست بهتر باشد.", TagComment.InQueue, 2));
			hotelIndex++;
		}

		await db.Set<UserEntity>().AddRangeAsync(users, ct);
		await db.Set<HotelEntity>().AddRangeAsync(hotels, ct);
		await db.Set<HotelRoomEntity>().AddRangeAsync(rooms, ct);
		await db.Set<HotelReservationEntity>().AddRangeAsync(reservations, ct);
		await db.Set<HotelInvoiceEntity>().AddRangeAsync(hotelInvoices, ct);
		await db.Set<CommentEntity>().AddRangeAsync(comments, ct);
		await db.SaveChangesAsync(ct);

		return new UResponse<List<KeyValue>?>([
			new KeyValue { Key = "users", Value = users.Count.ToString() },
			new KeyValue { Key = "hotels", Value = hotels.Count.ToString() },
			new KeyValue { Key = "hotelRooms", Value = rooms.Count.ToString() },
			new KeyValue { Key = "hotelReservations", Value = reservations.Count.ToString() },
			new KeyValue { Key = "hotelInvoices", Value = hotelInvoices.Count.ToString() },
			new KeyValue { Key = "reviews", Value = comments.Count.ToString() },
			new KeyValue { Key = "demoUsersPassword", Value = "Demo1234 (usernames demo01 ... demo12)" }
		], Usc.Created);
	}

	public async Task<bool> IsResidentOf(Guid userId, Guid placeId, CancellationToken ct) {
		DateTime now = DateTime.UtcNow;
		return await db.Set<HotelReservationEntity>().AnyAsync(x => x.UserId == userId && x.HotelId == placeId && x.CheckOutDate >= now &&
		                                                         (x.Tags.Contains(TagHotelReservation.CheckedIn) || x.Tags.Contains(TagHotelReservation.Confirmed)), ct);
	}

	private static bool Active(HotelReservationEntity r) => !r.Tags.Contains(TagHotelReservation.Cancelled) && !r.Tags.Contains(TagHotelReservation.NoShow) && !r.Tags.Contains(TagHotelReservation.CheckedOut);

	private async Task SetUnit(Guid roomId, string number, TagHousekeeping status, string? note, Guid by, CancellationToken ct) {
		HotelRoomEntity? room = await db.Set<HotelRoomEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == roomId, ct);
		if (room == null) return;
		HotelRoomUnit? unit = room.JsonData.Units.FirstOrDefault(x => x.Number == number);
		if (unit == null) {
			unit = new HotelRoomUnit { Number = number };
			room.JsonData.Units = [..room.JsonData.Units, unit];
		}

		unit.Status = status;
		unit.Note = note;
		unit.UpdatedAt = DateTime.UtcNow;
		unit.UpdatedBy = by;
	}

	private async Task<(JwtClaimData? User, HotelEntity? Hotel, UResponse? Error)> HotelManager(string? token, Guid hotelId, TagUser permission, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(token);
		if (u == null) return (null, null, new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue")));
		if (u.IsExpired) return (null, null, new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired")));
		HotelEntity? hotel = await db.Set<HotelEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == hotelId, ct);
		if (hotel == null) return (u, null, new UResponse(Usc.NotFound, ls.Get("hotelNotFound")));
		return await CanAct(u, hotel, permission, ct) ? (u, hotel, null) : (u, hotel, new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction")));
	}

	public async Task<UResponse<Guid?>> CreateHotelRate(HotelRateCreateParams p, CancellationToken ct) {
		(_, HotelEntity? hotel, UResponse? error) = await HotelManager(p.Token, p.HotelId, TagUser.PermissionManageHotels, ct);
		if (error != null) return new UResponse<Guid?>(null, error.Status, error.Message);
		if (p.EndDate < p.StartDate) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("endDateMustBeAfterStartDate"));
		if (p.RoomId != null && !await db.Set<HotelRoomEntity>().AnyAsync(x => x.Id == p.RoomId && x.HotelId == hotel!.Id, ct)) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("hotelRoomNotFound"));

		Guid id = Guid.CreateVersion7();
		await db.Set<HotelRateEntity>().AddAsync(new HotelRateEntity {
			Id = id,
			CreatorId = ts.ExtractClaims(p.Token)!.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = p.Tags,
			StartDate = p.StartDate.Date,
			EndDate = p.EndDate.Date,
			Price = p.Price,
			Percent = p.Percent,
			HotelId = hotel!.Id,
			RoomId = p.RoomId,
			JsonData = new HotelRateJson { Detail1 = p.Detail1, Detail2 = p.Detail2, Weekdays = p.Weekdays ?? [], MinNights = p.MinNights }
		}, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<HotelRateResponse>?>> ReadHotelRates(HotelRateReadParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u == null) return new UResponse<IEnumerable<HotelRateResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		IQueryable<HotelRateEntity> q = db.Set<HotelRateEntity>().ApplyReadParams(p);
		if (!OrganizationService.IsFull(u)) {
			Guid uid = u.Id;
			q = q.Where(x => x.Hotel.AdminUserIds.Contains(uid));
		}

		if (p.HotelId != null) q = q.Where(x => x.HotelId == p.HotelId);
		if (p.RoomId != null) q = q.Where(x => x.RoomId == p.RoomId || x.RoomId == null);
		if (p.FromDate != null) q = q.Where(x => x.EndDate >= p.FromDate);
		if (p.ToDate != null) q = q.Where(x => x.StartDate <= p.ToDate);
		return await q.OrderBy(x => x.StartDate).Select(x => new HotelRateResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Tags = x.Tags,
			JsonData = x.JsonData,
			StartDate = x.StartDate,
			EndDate = x.EndDate,
			Price = x.Price,
			Percent = x.Percent,
			HotelId = x.HotelId,
			RoomId = x.RoomId,
			RoomTitle = x.Room == null ? null : x.Room.Title
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateHotelRate(HotelRateUpdateParams p, CancellationToken ct) {
		HotelRateEntity? e = await db.Set<HotelRateEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		(_, _, UResponse? error) = await HotelManager(p.Token, e.HotelId, TagUser.PermissionManageHotels, ct);
		if (error != null) return error;
		if (p.StartDate != null) e.StartDate = p.StartDate.Value.Date;
		if (p.EndDate != null) e.EndDate = p.EndDate.Value.Date;
		if (p.Price != null) e.Price = p.Price;
		if (p.Percent != null) e.Percent = p.Percent;
		if (p.Weekdays != null) e.JsonData.Weekdays = p.Weekdays;
		if (p.MinNights != null) e.JsonData.MinNights = p.MinNights;
		e.ApplyUpdateParam<HotelRateEntity, TagHotelRate, HotelRateJson>(p);
		if (e.EndDate < e.StartDate) return new UResponse(Usc.BadRequest, ls.Get("endDateMustBeAfterStartDate"));
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteHotelRate(IdParams p, CancellationToken ct) {
		HotelRateEntity? e = await db.Set<HotelRateEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		(_, _, UResponse? error) = await HotelManager(p.Token, e.HotelId, TagUser.PermissionManageHotels, ct);
		if (error != null) return error;
		db.Set<HotelRateEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<IEnumerable<HotelCalendarDay>?>> ReadHotelRoomCalendar(HotelRoomCalendarParams p, CancellationToken ct) {
		HotelRoomEntity? room = await db.Set<HotelRoomEntity>().FirstOrDefaultAsync(x => x.Id == p.RoomId, ct);
		if (room == null) return new UResponse<IEnumerable<HotelCalendarDay>?>(null, Usc.NotFound, ls.Get("hotelRoomNotFound"));
		DateTime from = (p.FromDate == default ? DateTime.UtcNow : p.FromDate).Date;
		DateTime to = from.AddDays(Math.Clamp(p.Days, 1, 92));
		List<HotelRateEntity> rates = await RatesFor(room.HotelId, from, to, ct);
		List<HotelReservationEntity> reservations = await db.Set<HotelReservationEntity>()
			.Where(r => r.RoomId == room.Id && r.CheckInDate < to && r.CheckOutDate > from &&
			            !r.Tags.Contains(TagHotelReservation.Cancelled) && !r.Tags.Contains(TagHotelReservation.NoShow) && !r.Tags.Contains(TagHotelReservation.CheckedOut))
			.ToListAsync(ct);

		List<HotelCalendarDay> days = [];
		for (DateTime d = from; d < to; d = d.AddDays(1)) {
			(decimal price, bool closed, _) = NightOf(room, d, rates);
			int booked = reservations.Count(r => r.CheckInDate.Date <= d && r.CheckOutDate.Date > d);
			days.Add(new HotelCalendarDay {
				Date = d,
				Price = price,
				Booked = booked,
				Closed = closed || !room.IsAvailable,
				Available = closed || !room.IsAvailable ? 0 : Math.Max(0, room.Quantity - booked)
			});
		}

		return new UResponse<IEnumerable<HotelCalendarDay>?>(days);
	}

	public async Task<UResponse> SetHotelRoomHousekeeping(HotelHousekeepingParams p, CancellationToken ct) {
		HotelRoomEntity? room = await db.Set<HotelRoomEntity>().Include(x => x.Hotel).FirstOrDefaultAsync(x => x.Id == p.RoomId, ct);
		if (room == null) return new UResponse(Usc.NotFound, ls.Get("hotelRoomNotFound"));
		(JwtClaimData? u, _, UResponse? error) = await HotelManager(p.Token, room.HotelId, TagUser.PermissionManageReservations, ct);
		if (error != null) return error;
		await SetUnit(room.Id, p.Number.Trim(), p.Status, p.Note, u!.Id, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<List<Guid>?>> CreateHotelReservationGroup(HotelReservationGroupParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u == null) return new UResponse<List<Guid>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		List<Guid> roomIds = p.Rooms.Select(x => x.RoomId).Distinct().ToList();
		List<HotelRoomEntity> rooms = await db.Set<HotelRoomEntity>().Include(x => x.Hotel).Where(x => roomIds.Contains(x.Id)).ToListAsync(ct);
		if (rooms.Count != roomIds.Count || rooms.Select(x => x.HotelId).Distinct().Count() != 1) return new UResponse<List<Guid>?>(null, Usc.BadRequest, ls.Get("hotelRoomNotFound"));
		if (!await CanAct(u, rooms[0].Hotel, TagUser.PermissionManageReservations, ct)) return new UResponse<List<Guid>?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		Dictionary<Guid, int> booked = await ReadBookedCounts(roomIds, p.CheckInDate, p.CheckOutDate, ct);
		foreach (IGrouping<Guid, HotelGroupRoomParams> g in p.Rooms.GroupBy(x => x.RoomId)) {
			HotelRoomEntity room = rooms.First(x => x.Id == g.Key);
			if (!room.IsAvailable || booked.GetValueOrDefault(room.Id) + g.Sum(x => Math.Max(1, x.Count)) > room.Quantity)
				return new UResponse<List<Guid>?>(null, Usc.Conflict, $"{ls.Get("thisRoomIsAlreadyBookedForTheSelectedDates")} - {room.Title}");
		}

		string code = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
		List<Guid> ids = [];
		foreach (HotelGroupRoomParams r in p.Rooms)
			for (int i = 0; i < Math.Max(1, r.Count); i++) {
				UResponse<Guid?> created = await CreateHotelReservation(new HotelReservationCreateParams {
					Token = p.Token,
					ApiKey = p.ApiKey,
					Tags = [TagHotelReservation.Confirmed],
					CheckInDate = p.CheckInDate,
					CheckOutDate = p.CheckOutDate,
					GuestCount = Math.Max(1, r.GuestCount),
					UserId = p.UserId,
					RoomId = r.RoomId,
					GuestName = p.GroupName,
					GuestPhone = p.GuestPhone,
					Notes = p.Notes,
					PenaltyPrecentEveryDate = p.PenaltyPrecentEveryDate,
					GroupCode = code,
					GroupName = p.GroupName
				}, ct);
				if (created.Result == null) return new UResponse<List<Guid>?>(ids, created.Status, created.Message);
				ids.Add(created.Result.Value);
			}

		return new UResponse<List<Guid>?>(ids, Usc.Created);
	}

	private async Task<HotelReservationEntity?> ReservationForChange(Guid id, CancellationToken ct) =>
		await db.Set<HotelReservationEntity>().AsTracking().Include(x => x.Hotel).Include(x => x.Room).Include(x => x.Invoices).FirstOrDefaultAsync(x => x.Id == id, ct);

	private HotelInvoiceEntity NewHotelInvoice(HotelReservationEntity r, Guid creatorId, ICollection<TagHotelInvoice> tags, decimal amount, DateTime dueDate, string? detail) => new() {
		Id = Guid.CreateVersion7(),
		CreatorId = creatorId,
		CreatedAt = DateTime.UtcNow,
		Tags = tags,
		DebtAmount = amount,
		CreditorAmount = 0,
		PaidAmount = 0,
		PenaltyAmount = 0,
		ReservationId = r.Id,
		Reservation = r,
		DueDate = dueDate,
		JsonData = new HotelInvoiceJson { Detail1 = detail ?? "" }
	};

	public async Task<UResponse> ExtendHotelReservation(HotelReservationExtendParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		HotelReservationEntity? e = await ReservationForChange(p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("reservationNotFound"));
		if (!await CanAct(u, e.Hotel, TagUser.PermissionManageReservations, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (!Active(e)) return new UResponse(Usc.Conflict, ls.Get("thisReservationCannotBeChanged"));
		if (p.CheckOutDate.Date <= e.CheckOutDate.Date) return new UResponse(Usc.BadRequest, ls.Get("checkOutDateMustBeAfterTheCheckInDate"));

		Dictionary<Guid, int> booked = await ReadBookedCounts([e.RoomId], e.CheckOutDate, p.CheckOutDate, ct);
		if (booked.GetValueOrDefault(e.RoomId) >= e.Room.Quantity) return new UResponse(Usc.Conflict, ls.Get("thisRoomIsAlreadyBookedForTheSelectedDates"));

		(decimal extra, _) = await StayPrice(e.Room, e.CheckOutDate, p.CheckOutDate, e.GuestCount, ct);
		HotelInvoiceEntity invoice = NewHotelInvoice(e, u.Id, [TagHotelInvoice.NotPaid, TagHotelInvoice.Full], extra, e.CheckOutDate, ls.Get("reservationExtension", "fa"));
		await db.Set<HotelInvoiceEntity>().AddAsync(invoice, ct);
		e.CheckOutDate = p.CheckOutDate;
		e.TotalPrice += extra;
		e.JsonData.NightCount = (e.CheckOutDate.Date - e.CheckInDate.Date).Days;
		await SyncHotelInvoice(invoice, false, ct);
		await AddNotification(e.UserId, TagNotification.InvoiceIssued, ls.Get("reservationExtended"), e.Hotel.Title, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> ChangeHotelReservationRoom(HotelReservationChangeRoomParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		HotelReservationEntity? e = await ReservationForChange(p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("reservationNotFound"));
		if (!await CanAct(u, e.Hotel, TagUser.PermissionManageReservations, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (!Active(e)) return new UResponse(Usc.Conflict, ls.Get("thisReservationCannotBeChanged"));
		HotelRoomEntity? room = await db.Set<HotelRoomEntity>().FirstOrDefaultAsync(x => x.Id == p.RoomId && x.HotelId == e.HotelId, ct);
		if (room == null) return new UResponse(Usc.NotFound, ls.Get("hotelRoomNotFound"));

		DateTime from = e.CheckInDate.Date > DateTime.UtcNow.Date ? e.CheckInDate : DateTime.UtcNow.Date;
		if (room.Id != e.RoomId) {
			Dictionary<Guid, int> booked = await ReadBookedCounts([room.Id], from, e.CheckOutDate, ct);
			if (!room.IsAvailable || booked.GetValueOrDefault(room.Id) >= room.Quantity) return new UResponse(Usc.Conflict, ls.Get("thisRoomIsAlreadyBookedForTheSelectedDates"));
		}

		if (!p.KeepPrice && room.Id != e.RoomId && from < e.CheckOutDate) {
			decimal oldPrice = (await StayPrice(e.Room, from, e.CheckOutDate, e.GuestCount, ct)).Total;
			decimal newPrice = (await StayPrice(room, from, e.CheckOutDate, e.GuestCount, ct)).Total;
			decimal difference = newPrice - oldPrice;
			if (difference > 0) {
				HotelInvoiceEntity invoice = NewHotelInvoice(e, u.Id, [TagHotelInvoice.NotPaid, TagHotelInvoice.Full], difference, from, ls.Get("roomChangeDifference", "fa"));
				await db.Set<HotelInvoiceEntity>().AddAsync(invoice, ct);
				await SyncHotelInvoice(invoice, false, ct);
				e.TotalPrice += difference;
			}
		}

		if (e.JsonData.RoomNumber != null && e.Tags.Contains(TagHotelReservation.CheckedIn)) await SetUnit(e.RoomId, e.JsonData.RoomNumber, TagHousekeeping.Dirty, null, u.Id, ct);
		e.RoomId = room.Id;
		e.JsonData.RoomNumber = p.RoomNumber.NullIfEmpty();
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	private async Task<HotelNightAuditResponse> BuildNightAudit(HotelEntity hotel, DateTime day, CancellationToken ct) {
		DateTime next = day.AddDays(1);
		List<HotelReservationEntity> list = await db.Set<HotelReservationEntity>()
			.Include(x => x.Room).Include(x => x.User).Include(x => x.Invoices)
			.Where(x => x.HotelId == hotel.Id && x.CheckInDate < next && x.CheckOutDate >= day && !x.Tags.Contains(TagHotelReservation.Cancelled))
			.ToListAsync(ct);
		List<HotelRoomEntity> rooms = await db.Set<HotelRoomEntity>().Where(x => x.HotelId == hotel.Id).ToListAsync(ct);

		HotelAuditItem Item(HotelReservationEntity r) => new() {
			ReservationId = r.Id,
			ReservationCode = r.JsonData.ReservationCode,
			GuestName = r.JsonData.GuestName.NullIfEmpty() ?? $"{r.User.FirstName} {r.User.LastName}",
			RoomTitle = r.Room.Title,
			RoomNumber = r.JsonData.RoomNumber,
			CheckInDate = r.CheckInDate,
			CheckOutDate = r.CheckOutDate,
			Balance = r.Invoices.Where(i => i.Tags.Contains(TagHotelInvoice.NotPaid)).Sum(i => i.DebtAmount + i.PenaltyAmount - i.CreditorAmount - i.PaidAmount)
		};

		List<HotelReservationEntity> inHouse = list.Where(x => x.Tags.Contains(TagHotelReservation.CheckedIn) && x.CheckInDate.Date <= day && x.CheckOutDate.Date > day).ToList();
		int total = rooms.Where(x => x.IsAvailable).Sum(x => x.Quantity);
		return new HotelNightAuditResponse {
			Date = day,
			LastAuditDate = hotel.JsonData.LastAuditDate,
			TotalRooms = total,
			OccupiedRooms = inHouse.Count,
			Occupancy = total == 0 ? 0 : Math.Round(inHouse.Count * 100.0 / total, 1),
			RoomRevenue = Math.Round(inHouse.Sum(x => x.TotalPrice / Math.Max(1, x.JsonData.NightCount)), 0),
			OpenBalance = inHouse.Sum(x => Item(x).Balance),
			Arrivals = list.Where(x => x.CheckInDate.Date == day && !x.Tags.Contains(TagHotelReservation.NoShow)).Select(Item).ToList(),
			Departures = list.Where(x => x.CheckOutDate.Date == day && !x.Tags.Contains(TagHotelReservation.NoShow)).Select(Item).ToList(),
			InHouse = inHouse.Select(Item).ToList(),
			NoShows = list.Where(x => (x.Tags.Contains(TagHotelReservation.Pending) || x.Tags.Contains(TagHotelReservation.Confirmed) || x.Tags.Contains(TagHotelReservation.NoShow)) && x.CheckInDate.Date <= day).Select(Item).ToList(),
			DirtyUnits = rooms.SelectMany(r => r.JsonData.Units.Where(x => x.Status is TagHousekeeping.Dirty or TagHousekeeping.OutOfOrder)
				.Select(x => new HotelRoomUnitItem { RoomId = r.Id, RoomTitle = r.Title, Number = x.Number, Status = x.Status })).ToList()
		};
	}

	public async Task<UResponse<HotelNightAuditResponse?>> ReadHotelNightAudit(HotelNightAuditParams p, CancellationToken ct) {
		(_, HotelEntity? hotel, UResponse? error) = await HotelManager(p.Token, p.HotelId, TagUser.PermissionManageReservations, ct);
		if (error != null) return new UResponse<HotelNightAuditResponse?>(null, error.Status, error.Message);
		return new UResponse<HotelNightAuditResponse?>(await BuildNightAudit(hotel!, (p.Date ?? DateTime.UtcNow).Date, ct));
	}

	public async Task<UResponse<HotelNightAuditResponse?>> CloseHotelNightAudit(HotelNightAuditParams p, CancellationToken ct) {
		(_, HotelEntity? hotel, UResponse? error) = await HotelManager(p.Token, p.HotelId, TagUser.PermissionManageReservations, ct);
		if (error != null) return new UResponse<HotelNightAuditResponse?>(null, error.Status, error.Message);
		DateTime day = (p.Date ?? DateTime.UtcNow).Date;
		DateTime next = day.AddDays(1);
		List<HotelReservationEntity> missed = await db.Set<HotelReservationEntity>().AsTracking()
			.Where(x => x.HotelId == hotel!.Id && x.CheckInDate < next && (x.Tags.Contains(TagHotelReservation.Pending) || x.Tags.Contains(TagHotelReservation.Confirmed)))
			.ToListAsync(ct);
		foreach (HotelReservationEntity r in missed) r.Tags = [TagHotelReservation.NoShow];
		hotel!.JsonData.LastAuditDate = day;
		await db.SaveChangesAsync(ct);
		return new UResponse<HotelNightAuditResponse?>(await BuildNightAudit(hotel, day, ct));
	}

	public async Task<UResponse<IEnumerable<HotelGuestExportItem>?>> ExportHotelGuests(HotelGuestExportParams p, CancellationToken ct) {
		(_, HotelEntity? hotel, UResponse? error) = await HotelManager(p.Token, p.HotelId, TagUser.PermissionManageReservations, ct);
		if (error != null) return new UResponse<IEnumerable<HotelGuestExportItem>?>(null, error.Status, error.Message);
		DateTime to = p.ToDate == default ? DateTime.UtcNow : p.ToDate;
		List<HotelReservationEntity> list = await db.Set<HotelReservationEntity>()
			.Include(x => x.User).Include(x => x.Room)
			.Where(x => x.HotelId == hotel!.Id && x.CheckInDate <= to && x.CheckOutDate >= p.FromDate && !x.Tags.Contains(TagHotelReservation.Cancelled) && !x.Tags.Contains(TagHotelReservation.NoShow))
			.OrderBy(x => x.CheckInDate)
			.ToListAsync(ct);

		List<HotelGuestExportItem> rows = [];
		foreach (HotelReservationEntity r in list) {
			HotelGuestExportItem Row(string name) => new() {
				ReservationCode = r.JsonData.ReservationCode,
				CheckInDate = r.CheckInDate,
				CheckOutDate = r.CheckOutDate,
				RoomTitle = r.Room.Title,
				RoomNumber = r.JsonData.RoomNumber,
				FullName = name
			};

			HotelGuestExportItem main = Row($"{r.User.FirstName} {r.User.LastName}".Trim());
			main.NationalCode = r.User.NationalCode;
			main.PhoneNumber = r.User.PhoneNumber ?? r.JsonData.GuestPhone;
			main.BirthDate = r.User.Birthdate;
			rows.Add(main);
			foreach (ReservationGuestJson g in r.JsonData.Guests) {
				HotelGuestExportItem row = Row(g.FullName);
				row.NationalCode = g.NationalCode;
				row.PhoneNumber = g.PhoneNumber;
				row.Nationality = g.Nationality;
				row.PassportNumber = g.PassportNumber;
				row.BirthDate = g.BirthDate;
				row.FatherName = g.FatherName;
				row.Gender = g.Gender;
				rows.Add(row);
			}
		}

		return new UResponse<IEnumerable<HotelGuestExportItem>?>(rows);
	}

	public async Task<UResponse<string?>> PrintHotelReservation(IdParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u == null) return new UResponse<string?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		HotelReservationEntity? e = await db.Set<HotelReservationEntity>().Include(x => x.Hotel).Include(x => x.Room).Include(x => x.User).Include(x => x.Invoices).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<string?>(null, Usc.NotFound, ls.Get("reservationNotFound"));
		if (e.UserId != u.Id && !await CanAct(u, e.Hotel, TagUser.PermissionManageReservations, ct)) return new UResponse<string?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		OrganizationEntity? organization = await os.ReadOrganization(e.Hotel.OrganizationId, ct);
		decimal due = e.Invoices.Where(i => i.Tags.Contains(TagHotelInvoice.NotPaid)).Sum(i => i.DebtAmount + i.PenaltyAmount - i.CreditorAmount - i.PaidAmount);
		decimal vat = e.Invoices.Sum(i => {
			decimal v = i.JsonData.VatPercent ?? organization?.JsonData.VatPercent ?? 0;
			return Math.Round((i.DebtAmount + i.PenaltyAmount - i.CreditorAmount) * v / (100 + v));
		});
		string html = PrintTemplate.Build(organization, e.Hotel.Title, ls.Get("guestFolio", "fa"), [
				(ls.Get("guest", "fa"), e.JsonData.GuestName.NullIfEmpty() ?? $"{e.User.FirstName} {e.User.LastName}"),
				(ls.Get("phoneNumber", "fa"), e.JsonData.GuestPhone ?? e.User.PhoneNumber),
				(ls.Get("reservationCode", "fa"), e.JsonData.ReservationCode),
				(ls.Get("hotel", "fa"), e.Hotel.Title),
				(ls.Get("room", "fa"), e.JsonData.RoomNumber == null ? e.Room.Title : $"{e.Room.Title} - {e.JsonData.RoomNumber}"),
				(ls.Get("checkIn", "fa"), PrintTemplate.Date(e.CheckInDate)),
				(ls.Get("checkOut", "fa"), PrintTemplate.Date(e.CheckOutDate)),
				(ls.Get("nights", "fa"), e.JsonData.NightCount.ToString()),
				(ls.Get("guests", "fa"), e.GuestCount.ToString()),
				(ls.Get("totalPrice", "fa"), PrintTemplate.Money(e.TotalPrice)),
				(ls.Get("vatIncluded", "fa"), vat > 0 ? PrintTemplate.Money(vat) : null),
				(ls.Get("remaining", "fa"), PrintTemplate.Money(due))
			],
			[ls.Get("description", "fa"), ls.Get("dueDate", "fa"), ls.Get("amount", "fa"), ls.Get("paidAmount", "fa"), ls.Get("status", "fa")],
			e.Invoices.OrderBy(x => x.CreatedAt).Select(i => (IReadOnlyList<string>)[
				i.JsonData.Detail1.NullIfEmpty() ?? ls.Get(i.Tags.Contains(TagHotelInvoice.Extra) ? "extraCharge" : "roomCharge", "fa"),
				PrintTemplate.Date(i.DueDate),
				PrintTemplate.Money(i.DebtAmount + i.PenaltyAmount - i.CreditorAmount),
				PrintTemplate.Money(i.PaidAmount),
				ls.Get(i.Tags.Contains(TagHotelInvoice.NotPaid) ? "notPaid" : "paid", "fa")
			]),
			[e.Hotel.JsonData.Policies ?? ""],
			[ls.Get("guestSignature", "fa"), ls.Get("organizationSignature", "fa")]);
		return new UResponse<string?>(html);
	}
}

public sealed class HotelReminderService(IServiceScopeFactory scopeFactory) : BackgroundService {
	private bool _failureLogged;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
		using PeriodicTimer timer = new(TimeSpan.FromHours(1));
		try {
			do {
				try {
					using IServiceScope scope = scopeFactory.CreateScope();
					await scope.ServiceProvider.GetRequiredService<IHotelService>().PostDueInvoices(stoppingToken);
				}
				catch (OperationCanceledException) {
					throw;
				}
				catch (Exception e) {
					if (!_failureLogged) ULog.Error(e, "Hotel invoice posting is off (are the hotel tables migrated?)");
					_failureLogged = true;
				}
			} while (await timer.WaitForNextTickAsync(stoppingToken));
		}
		catch (OperationCanceledException) {
		}
	}
}
