namespace SinaMN75U.Services;

public interface IDormService {
	public Task<UResponse<Guid?>> CreateDorm(DormCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<DormResponse>?>> ReadDorms(DormReadParams p, CancellationToken ct);
	public Task<UResponse<DormResponse?>> ReadDormById(IdParams<DormSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> UpdateDorm(DormUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteDorm(IdParams p, CancellationToken ct);
	public Task<UResponse<Guid?>> CreateDormRoom(DormRoomCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<DormRoomResponse>?>> ReadDormRooms(DormRoomReadParams p, CancellationToken ct);
	public Task<UResponse<DormRoomResponse?>> ReadDormRoomById(IdParams<DormRoomSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> UpdateDormRoom(DormRoomUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteDormRoom(IdParams p, CancellationToken ct);
	public Task<UResponse<Guid?>> CreateDormBed(DormBedCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<DormBedResponse>?>> ReadDormBeds(DormBedReadParams p, CancellationToken ct);
	public Task<UResponse<DormBedResponse?>> ReadDormBedById(IdParams<DormBedSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> UpdateDormBed(DormBedUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteDormBed(IdParams p, CancellationToken ct);
	public Task<UResponse<Guid?>> CreateDormBedContract(DormBedContractCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<DormBedContractResponse>?>> ReadDormBedContracts(DormBedContractReadParams p, CancellationToken ct);
	public Task<UResponse> UpdateDormBedContract(DormBedContractUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteDormBedContract(IdParams p, CancellationToken ct);
	public Task<UResponse<Guid?>> CreateDormBedInvoice(DormBedInvoiceCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<DormBedInvoiceResponse>?>> ReadDormBedInvoices(DormBedInvoiceReadParams p, CancellationToken ct);
	public Task<UResponse> UpdateDormBedInvoice(DormBedInvoiceUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteDormBedInvoice(IdParams p, CancellationToken ct);
	public Task<UResponse> PayDormBedInvoice(DormBedInvoicePayParams p, CancellationToken ct);
	public Task<UResponse> PayDormBedInvoiceByUser(IdParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<DormBedInvoiceChartResponse>?>> ReadDormBedInvoiceChartData(BaseParams p, CancellationToken ct);
	public Task<UResponse> SettleDormBedContract(DormBedContractSettleParams p, CancellationToken ct);
	public Task<UResponse> RenewDormBedContract(DormBedContractRenewParams p, CancellationToken ct);
	public Task<UResponse> TransferDormBedContract(DormBedContractTransferParams p, CancellationToken ct);
	public Task<UResponse> SplitDormBedInvoice(DormBedInvoiceSplitParams p, CancellationToken ct);
	public Task<UResponse> ReceiveDormBedInvoice(InvoiceReceiveParams p, CancellationToken ct);
	public Task ProcessDueInvoices(CancellationToken ct);
	public Task<bool?> CanActOnPlaceOf(JwtClaimData u, Guid? dormId, Guid? dormRoomId, Guid? dormBedId, CancellationToken ct);
	public IQueryable<Guid> RelatedUserIds(Guid userId);
	public Task ReopenPayment(Guid sourceId, decimal amount, CancellationToken ct);
	public Task<List<AccountingDue>> Outstanding(Guid organizationId, DateTime now, CancellationToken ct);
	public Task<UResponse<DormDashboardResponse?>> ReadDormDashboard(DashboardRangeParams p, CancellationToken ct);
	public Task<UResponse<List<KeyValue>?>> SeedDorms(CancellationToken ct = default);
	public Task<UResponse> SetDormBedContractChecklist(DormBedContractChecklistParams p, CancellationToken ct);
	public Task<UResponse<Guid?>> CreateDormApplication(DormApplicationCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<DormApplicationResponse>?>> ReadDormApplications(DormApplicationReadParams p, CancellationToken ct);
	public Task<UResponse> ReviewDormApplication(DormApplicationReviewParams p, CancellationToken ct);
	public Task<UResponse> DeleteDormApplication(IdParams p, CancellationToken ct);
	public Task<UResponse<Guid?>> CreateDormRecord(DormRecordCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<DormRecordResponse>?>> ReadDormRecords(DormRecordReadParams p, CancellationToken ct);
	public Task<UResponse> UpdateDormRecord(DormRecordUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteDormRecord(IdParams p, CancellationToken ct);
	public Task<UResponse<Guid?>> CreateDormMeal(DormMealCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<DormMealResponse>?>> ReadDormMeals(DormMealReadParams p, CancellationToken ct);
	public Task<UResponse> UpdateDormMeal(DormMealUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteDormMeal(IdParams p, CancellationToken ct);
	public Task<UResponse<Guid?>> CreateDormBooking(DormBookingCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<DormBookingResponse>?>> ReadDormBookings(DormBookingReadParams p, CancellationToken ct);
	public Task<UResponse> CancelDormBooking(IdParams p, CancellationToken ct);
	public Task<UResponse<string?>> PrintDormBedContract(IdParams p, CancellationToken ct);
	public Task<UResponse<string?>> PrintDormBedInvoice(IdParams p, CancellationToken ct);
	public Task<bool> IsResidentOf(Guid userId, Guid placeId, CancellationToken ct);
}

public class DormService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IWalletService ws,
	IOrganizationService os,
	IAccountingService acc,
	IDataSeedService seeds
) : IDormService, IAccountingSource, IUserScope, IPlaceResidency {
	private static Guid UserIdOf(JwtClaimData? u) => u?.Id ?? Guid.Empty;

	private Task<bool> CanAct(JwtClaimData u, DormEntity place, TagUser permission, CancellationToken ct) => os.CanActOnPlace(u, place.OrganizationId, place.AdminUserIds, permission, ct);

	private async Task AddNotification(Guid userId, TagNotification tag, string title, string body, CancellationToken ct) =>
		await db.Set<NotificationEntity>().AddAsync(new NotificationEntity {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = userId,
			UserId = userId,
			Tags = [tag, TagNotification.Unread],
			JsonData = new NotificationJson { Detail1 = title, Detail2 = body }
		}, ct);

	private static DormSelectorArgs Safe(DormSelectorArgs a, bool people) => new() {
		Rooms = a.Rooms == null ? null : Safe(a.Rooms, people),
		Beds = a.Beds == null ? null : Safe(a.Beds, people),
		Comments = a.Comments,
		Media = a.Media
	};

	private static DormRoomSelectorArgs Safe(DormRoomSelectorArgs a, bool people) => new() {
		Dorm = a.Dorm == null ? null : Safe(a.Dorm, people),
		Beds = a.Beds == null ? null : Safe(a.Beds, people),
		Media = a.Media
	};

	private static DormBedSelectorArgs Safe(DormBedSelectorArgs a, bool people) => new() {
		Room = a.Room == null ? null : Safe(a.Room, people),
		Contract = people && a.Contract != null ? Safe(a.Contract) : null,
		Media = a.Media
	};

	private static DormBedContractSelectorArgs Safe(DormBedContractSelectorArgs a) => new() {
		User = a.User == null ? null : new UserSelectorArgs(),
		Bed = a.Bed == null ? null : Safe(a.Bed, false),
		Invoice = a.Invoice == null ? null : new DormBedInvoiceSelectorArgs()
	};

	private static DormBedInvoiceSelectorArgs Safe(DormBedInvoiceSelectorArgs a) => new() { Contract = a.Contract == null ? null : Safe(a.Contract) };

	public async Task<UResponse<Guid?>> CreateDorm(DormCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!await os.CanCreatePlace(userData, p.OrganizationId, TagUser.PermissionManageDorms, ct)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		string? planError = await os.PlanError(userData, p.OrganizationId, x => x.MaxPlaces, () => os.PlaceCount(p.OrganizationId!.Value, ct), ct);
		if (planError != null) return new UResponse<Guid?>(null, Usc.Forbidden, planError);

		DormEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = OrganizationService.IsFull(userData) ? p.CreatorId ?? userData.Id : userData.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new DormJson {
				Description = p.Description,
				Policies = p.Policies,
				Highlights = p.Highlights ?? [],
				Rules = p.Rules ?? [],
				RequiredDocuments = p.RequiredDocuments ?? [],
				NearbyUniversity = p.NearbyUniversity,
				UniversityWalkMinutes = p.UniversityWalkMinutes,
				VisitingHours = p.VisitingHours,
				CurfewTime = p.CurfewTime,
				MinimumStayMonths = p.MinimumStayMonths,
				HowToGetThere = p.HowToGetThere,
				Nearby = p.Nearby ?? [],
				Faqs = p.Faqs ?? [],
				Website = p.Website,
				Whatsapp = p.Whatsapp,
				Instagram = p.Instagram,
				Telegram = p.Telegram,
				Latitude = p.Latitude,
				Longitude = p.Longitude,
				LaundryMachines = p.LaundryMachines ?? [],
				LaundrySlotMinutes = p.LaundrySlotMinutes
			},
			Tags = p.Tags,
			Title = p.Title,
			CityCode = p.CityCode,
			Address = p.Address,
			PhoneNumber = p.PhoneNumber,
			OrganizationId = p.OrganizationId,
			AdminUserIds = await os.PlaceAdmins(p.OrganizationId, OrganizationService.IsFull(userData) ? p.AdminUserIds ?? [] : [userData.Id], ct)
		};

		await db.Set<DormEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<DormResponse>?>> ReadDorms(DormReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<DormEntity> q = db.Set<DormEntity>().ApplyReadParams(p);
		Guid uid = UserIdOf(userData);
		if (OrganizationService.IsScopedAdmin(userData)) q = q.Where(x => x.AdminUserIds.Contains(uid));
		else if (!OrganizationService.IsFull(userData)) q = q.Where(x => x.Tags.Contains(TagDorm.Active));
		if (!OrganizationService.IsFull(userData)) p.SelectorArgs = Safe(p.SelectorArgs, OrganizationService.IsScopedAdmin(userData));

		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title!));
		if (p.CityCode.IsNotNullOrEmpty()) q = q.Where(x => x.CityCode.Contains(p.CityCode!));
		if (p.OrganizationId.HasValue) q = q.Where(x => x.OrganizationId == p.OrganizationId);
		if (p.MinRent.HasValue) q = q.Where(x => x.Rooms.SelectMany(r => r.Beds).Any(b => b.MonthlyRent >= p.MinRent));
		if (p.MaxRent.HasValue) q = q.Where(x => x.Rooms.SelectMany(r => r.Beds).Any(b => b.MonthlyRent <= p.MaxRent));
		if (p.AvailableOnly == true) q = q.Where(x => x.Rooms.SelectMany(r => r.Beds).Any(b => !b.Contracts.Any(c => c.StartDate <= DateTime.UtcNow && c.EndDate >= DateTime.UtcNow)));

		IQueryable<DormResponse> projected = q.Select(Projections.DormSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<DormResponse?>> ReadDormById(IdParams<DormSelectorArgs> p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<DormEntity> dorms = db.Set<DormEntity>();
		Guid uid = UserIdOf(userData);
		if (OrganizationService.IsScopedAdmin(userData)) dorms = dorms.Where(x => x.AdminUserIds.Contains(uid));
		else if (!OrganizationService.IsFull(userData)) dorms = dorms.Where(x => x.Tags.Contains(TagDorm.Active));
		if (!OrganizationService.IsFull(userData)) p.SelectorArgs = Safe(p.SelectorArgs, OrganizationService.IsScopedAdmin(userData));
		DormResponse? e = await dorms.Select(Projections.DormSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<DormResponse?>(null, Usc.NotFound, ls.Get("dormNotFound")) : new UResponse<DormResponse?>(e);
	}

	public async Task<UResponse> UpdateDorm(DormUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		DormEntity? e = await db.Set<DormEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("dormNotFound"));

		if (!await CanAct(userData, e, TagUser.PermissionManageDorms, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (OrganizationService.TouchesAdminUserIds(p) && !OrganizationService.IsFull(userData) && !(Core.App.MultiTenant && await os.IsOwner(userData, e.OrganizationId, ct))) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (p.OrganizationId.HasValue && p.OrganizationId != e.OrganizationId) {
			if (!OrganizationService.IsFull(userData)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
			e.OrganizationId = p.OrganizationId;
		}

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		if (p.CityCode.IsNotNullOrEmpty()) e.CityCode = p.CityCode;
		if (p.Address.IsNotNull()) e.Address = p.Address;
		if (p.PhoneNumber.IsNotNull()) e.PhoneNumber = p.PhoneNumber;
		if (p.Description.IsNotNull()) e.JsonData.Description = p.Description;
		if (p.Policies.IsNotNull()) e.JsonData.Policies = p.Policies;
		if (p.Highlights.IsNotNull()) e.JsonData.Highlights = p.Highlights;
		if (p.Rules.IsNotNull()) e.JsonData.Rules = p.Rules;
		if (p.RequiredDocuments.IsNotNull()) e.JsonData.RequiredDocuments = p.RequiredDocuments;
		if (p.NearbyUniversity.IsNotNull()) e.JsonData.NearbyUniversity = p.NearbyUniversity;
		if (p.UniversityWalkMinutes.HasValue) e.JsonData.UniversityWalkMinutes = p.UniversityWalkMinutes;
		if (p.VisitingHours.IsNotNull()) e.JsonData.VisitingHours = p.VisitingHours;
		if (p.CurfewTime.IsNotNull()) e.JsonData.CurfewTime = p.CurfewTime;
		if (p.MinimumStayMonths.HasValue) e.JsonData.MinimumStayMonths = p.MinimumStayMonths;
		if (p.HowToGetThere.IsNotNull()) e.JsonData.HowToGetThere = p.HowToGetThere;
		if (p.Nearby.IsNotNull()) e.JsonData.Nearby = p.Nearby;
		if (p.Faqs.IsNotNull()) e.JsonData.Faqs = p.Faqs;
		if (p.Website.IsNotNull()) e.JsonData.Website = p.Website;
		if (p.Whatsapp.IsNotNull()) e.JsonData.Whatsapp = p.Whatsapp;
		if (p.Instagram.IsNotNull()) e.JsonData.Instagram = p.Instagram;
		if (p.Telegram.IsNotNull()) e.JsonData.Telegram = p.Telegram;
		if (p.Latitude.HasValue) e.JsonData.Latitude = p.Latitude;
		if (p.Longitude.HasValue) e.JsonData.Longitude = p.Longitude;
		if (p.LaundryMachines != null) e.JsonData.LaundryMachines = p.LaundryMachines;
		if (p.LaundrySlotMinutes.HasValue) e.JsonData.LaundrySlotMinutes = p.LaundrySlotMinutes;
		e.ApplyUpdateParam<DormEntity, TagDorm, DormJson>(p);
		if (Core.App.MultiTenant) e.AdminUserIds = await os.PlaceAdmins(e.OrganizationId, e.AdminUserIds, ct);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> DeleteDorm(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		DormEntity? e = await db.Set<DormEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("dormNotFound"));

		if (!await CanAct(userData, e, TagUser.PermissionDeleteDorms, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		db.Set<DormEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateDormRoom(DormRoomCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		DormEntity? dorm = await db.Set<DormEntity>().FirstOrDefaultAsync(x => x.Id == p.DormId, ct);
		if (dorm == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("dormNotFound"));
		if (!await CanAct(userData, dorm, TagUser.PermissionManageDorms, ct)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		DormRoomEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new DormRoomJson {
				Description = p.Description,
				Floor = p.Floor,
				SizeSquareMeters = p.SizeSquareMeters
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
		Guid uid = UserIdOf(userData);
		if (OrganizationService.IsScopedAdmin(userData)) q = q.Where(x => x.Dorm.AdminUserIds.Contains(uid));
		else if (!OrganizationService.IsFull(userData)) q = q.Where(x => x.Dorm.Tags.Contains(TagDorm.Active));
		if (!OrganizationService.IsFull(userData)) p.SelectorArgs = Safe(p.SelectorArgs, OrganizationService.IsScopedAdmin(userData));

		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title!));
		if (p.DormId.HasValue) q = q.Where(x => x.DormId == p.DormId);

		IQueryable<DormRoomResponse> projected = q.Select(Projections.DormRoomSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<DormRoomResponse?>> ReadDormRoomById(IdParams<DormRoomSelectorArgs> p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<DormRoomEntity> q = db.Set<DormRoomEntity>();
		Guid uid = UserIdOf(userData);
		if (OrganizationService.IsScopedAdmin(userData)) q = q.Where(x => x.Dorm.AdminUserIds.Contains(uid));
		else if (!OrganizationService.IsFull(userData)) q = q.Where(x => x.Dorm.Tags.Contains(TagDorm.Active));
		if (!OrganizationService.IsFull(userData)) p.SelectorArgs = Safe(p.SelectorArgs, OrganizationService.IsScopedAdmin(userData));

		DormRoomResponse? e = await q.Select(Projections.DormRoomSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<DormRoomResponse?>(null, Usc.NotFound, ls.Get("dormRoomNotFound")) : new UResponse<DormRoomResponse?>(e);
	}

	public async Task<UResponse> UpdateDormRoom(DormRoomUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		DormRoomEntity? e = await db.Set<DormRoomEntity>().AsTracking().Include(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("dormRoomNotFound"));

		if (!await CanAct(userData, e.Dorm, TagUser.PermissionManageDorms, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (p.DormId.HasValue && p.DormId != e.DormId) {
			DormEntity? to = await db.Set<DormEntity>().FirstOrDefaultAsync(x => x.Id == p.DormId, ct);
			if (to == null || !await CanAct(userData, to, TagUser.PermissionManageDorms, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		}

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		if (p.DormId.HasValue) e.DormId = p.DormId.Value;
		if (p.Capacity.HasValue) e.Capacity = p.Capacity.Value;
		if (p.Description.IsNotNull()) e.JsonData.Description = p.Description;
		if (p.Floor.HasValue) e.JsonData.Floor = p.Floor;
		if (p.SizeSquareMeters.HasValue) e.JsonData.SizeSquareMeters = p.SizeSquareMeters;
		e.ApplyUpdateParam<DormRoomEntity, TagDormRoom, DormRoomJson>(p);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> DeleteDormRoom(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		DormRoomEntity? e = await db.Set<DormRoomEntity>().Include(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("dormRoomNotFound"));

		if (!await CanAct(userData, e.Dorm, TagUser.PermissionDeleteDorms, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		db.Set<DormRoomEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateDormBed(DormBedCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		DormRoomEntity? room = await db.Set<DormRoomEntity>().Include(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.RoomId, ct);
		if (room == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("dormRoomNotFound"));
		if (!await CanAct(userData, room.Dorm, TagUser.PermissionManageDorms, ct)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		Guid? organizationId = room.Dorm.OrganizationId;
		string? planError = await os.PlanError(userData, organizationId, x => x.MaxBeds, () => db.Set<DormBedEntity>().CountAsync(x => x.Room.Dorm.OrganizationId == organizationId, ct), ct);
		if (planError != null) return new UResponse<Guid?>(null, Usc.Forbidden, planError);

		DormBedEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new DormBedJson {
				Description = p.Description
			},
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
		Guid uid = UserIdOf(userData);
		if (OrganizationService.IsScopedAdmin(userData)) q = q.Where(x => x.Room.Dorm.AdminUserIds.Contains(uid));
		else if (!OrganizationService.IsFull(userData)) q = q.Where(x => x.Room.Dorm.Tags.Contains(TagDorm.Active));
		if (!OrganizationService.IsFull(userData)) p.SelectorArgs = Safe(p.SelectorArgs, OrganizationService.IsScopedAdmin(userData));

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
		Guid uid = UserIdOf(userData);
		if (OrganizationService.IsScopedAdmin(userData)) q = q.Where(x => x.Room.Dorm.AdminUserIds.Contains(uid));
		else if (!OrganizationService.IsFull(userData)) q = q.Where(x => x.Room.Dorm.Tags.Contains(TagDorm.Active));
		if (!OrganizationService.IsFull(userData)) p.SelectorArgs = Safe(p.SelectorArgs, OrganizationService.IsScopedAdmin(userData));

		DormBedResponse? e = await q.Select(Projections.DormBedSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<DormBedResponse?>(null, Usc.NotFound, ls.Get("dormBedNotFound")) : new UResponse<DormBedResponse?>(e);
	}

	public async Task<UResponse> UpdateDormBed(DormBedUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		DormBedEntity? e = await db.Set<DormBedEntity>().AsTracking().Include(x => x.Room).ThenInclude(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("dormBedNotFound"));

		if (!await CanAct(userData, e.Room.Dorm, TagUser.PermissionManageDorms, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (p.RoomId.HasValue && p.RoomId != e.RoomId) {
			DormRoomEntity? to = await db.Set<DormRoomEntity>().Include(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.RoomId, ct);
			if (to == null || !await CanAct(userData, to.Dorm, TagUser.PermissionManageDorms, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		}

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		if (p.Deposit.HasValue) e.Deposit = p.Deposit.Value;
		if (p.MonthlyRent.HasValue) e.MonthlyRent = p.MonthlyRent.Value;
		if (p.RoomId.HasValue) e.RoomId = p.RoomId.Value;
		if (p.Description.IsNotNull()) e.JsonData.Description = p.Description;
		e.ApplyUpdateParam<DormBedEntity, TagDormBed, DormBedJson>(p);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> DeleteDormBed(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		DormBedEntity? e = await db.Set<DormBedEntity>().Include(x => x.Room).ThenInclude(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("dormBedNotFound"));

		if (!await CanAct(userData, e.Room.Dorm, TagUser.PermissionDeleteDorms, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		db.Set<DormBedEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateDormBedContract(DormBedContractCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		DormBedEntity? bed = await db.Set<DormBedEntity>().Include(x => x.Contracts).Include(x => x.Room).ThenInclude(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.BedId, ct);
		if (bed == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("dormBedNotFound"));
		if (!await CanAct(userData, bed.Room.Dorm, TagUser.PermissionManageContracts, ct)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
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
			JsonData = new DormBedContractJson {
				GuardianName = p.GuardianName,
				GuardianPhone = p.GuardianPhone,
				EmergencyName = p.EmergencyName,
				EmergencyPhone = p.EmergencyPhone,
				EmergencyRelation = p.EmergencyRelation,
				ApplicationId = p.ApplicationId
			},
			Tags = p.Tags
		};
		await db.Set<DormBedContractEntity>().AddAsync(e, ct);
		if (p.ApplicationId != null) {
			DormApplicationEntity? application = await db.Set<DormApplicationEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.ApplicationId && x.UserId == user.Id, ct);
			if (application != null) {
				application.Tags = [TagDormApplication.Converted];
				application.JsonData.ContractId = contractId;
				application.JsonData.BedId = bed.Id;
			}
		}

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
		if (!OrganizationService.IsFull(userData)) {
			Guid uid = UserIdOf(userData);
			bool scoped = OrganizationService.IsScopedAdmin(userData);
			q = q.Where(x => x.UserId == uid || scoped && x.Bed.Room.Dorm.AdminUserIds.Contains(uid));
			p.SelectorArgs = Safe(p.SelectorArgs);
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

		if (!await CanAct(userData, e.Bed.Room.Dorm, TagUser.PermissionManageContracts, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		if (p.Deposit.HasValue) e.Deposit = p.Deposit.Value;
		if (p.Rent.HasValue) e.Rent = p.Rent.Value;
		if (p.StartDate.HasValue) e.StartDate = p.StartDate.Value;
		if (p.EndDate.HasValue) e.EndDate = p.EndDate.Value;
		if (p.GuardianName != null) e.JsonData.GuardianName = p.GuardianName;
		if (p.GuardianPhone != null) e.JsonData.GuardianPhone = p.GuardianPhone;
		if (p.EmergencyName != null) e.JsonData.EmergencyName = p.EmergencyName;
		if (p.EmergencyPhone != null) e.JsonData.EmergencyPhone = p.EmergencyPhone;
		if (p.EmergencyRelation != null) e.JsonData.EmergencyRelation = p.EmergencyRelation;

		e.ApplyUpdateParam<DormBedContractEntity, TagDormBedContract, DormBedContractJson>(p);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> DeleteDormBedContract(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		DormBedContractEntity? e = await db.Set<DormBedContractEntity>().Include(x => x.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("contractNotFound"));

		if (!await CanAct(userData, e.Bed.Room.Dorm, TagUser.PermissionDeleteContracts, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		foreach (DormBedInvoiceEntity invoice in await db.Set<DormBedInvoiceEntity>().Include(x => x.Contract).ThenInclude(x => x!.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm).Where(x => x.ContractId == e.Id && x.JsonData.Posted).ToListAsync(ct))
			await SyncDormBedInvoice(invoice, true, ct);
		await db.SaveChangesAsync(ct);
		await db.Set<DormBedContractEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateDormBedInvoice(DormBedInvoiceCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		DormBedContractEntity? contract = await db.Set<DormBedContractEntity>().Include(x => x.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.ContractId, ct);
		if (contract == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("contractNotFound"));
		if (!await CanAct(userData, contract.Bed.Room.Dorm, TagUser.PermissionManageInvoices, ct))
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
		if (!OrganizationService.IsFull(userData)) {
			Guid uid = UserIdOf(userData);
			bool scoped = OrganizationService.IsScopedAdmin(userData);
			q = q.Where(x => x.Contract != null && (x.Contract.UserId == uid || scoped && x.Contract.Bed.Room.Dorm.AdminUserIds.Contains(uid)));
			p.SelectorArgs = Safe(p.SelectorArgs);
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
		Dictionary<Guid, DormBedInvoiceEntity> entities = await db.Set<DormBedInvoiceEntity>().AsTracking().Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);

		bool anyChanges = false;

		foreach (DormBedInvoiceResponse dto in response.Result!) {
			DormBedInvoiceEntity? entity = entities.GetValueOrDefault(dto.Id);
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

	public async Task<UResponse> UpdateDormBedInvoice(DormBedInvoiceUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		DormBedInvoiceEntity? e = await db.Set<DormBedInvoiceEntity>().AsTracking().Include(x => x.Contract).ThenInclude(x => x!.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("invoiceNotFound"));
		if (!(e.Contract == null ? OrganizationService.IsFull(userData) : await CanAct(userData, e.Contract.Bed.Room.Dorm, TagUser.PermissionManageInvoices, ct)))
			return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (p.ContractId.HasValue && p.ContractId != e.ContractId) {
			DormBedContractEntity? to = await db.Set<DormBedContractEntity>().Include(x => x.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.ContractId, ct);
			if (to == null || !await CanAct(userData, to.Bed.Room.Dorm, TagUser.PermissionManageInvoices, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		}
		if (p.CreditorAmount.IsNotNull()) e.CreditorAmount = p.CreditorAmount.Value;
		if (p.DebtAmount.IsNotNull()) e.DebtAmount = p.DebtAmount.Value;
		if (p.PenaltyAmount.IsNotNull()) e.PenaltyAmount = p.PenaltyAmount.Value;
		if (p.PaidAmount.IsNotNull()) e.PaidAmount = p.PaidAmount.Value;
		if (p.DueDate.HasValue) e.DueDate = p.DueDate.Value;
		if (p.ContractId.HasValue) e.ContractId = p.ContractId.Value;
		if (p.PenaltyPrecentEveryDate.IsNotNull()) e.JsonData.PenaltyPrecentEveryDate = p.PenaltyPrecentEveryDate.Value;

		e.ApplyUpdateParam<DormBedInvoiceEntity, TagDormBedInvoice, DormBedInvoiceJson>(p);
		await SyncDormBedInvoice(e, false, ct);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> DeleteDormBedInvoice(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		DormBedInvoiceEntity? e = await db.Set<DormBedInvoiceEntity>().Include(x => x.Contract).ThenInclude(x => x!.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("invoiceNotFound"));
		if (!(e.Contract == null ? OrganizationService.IsFull(userData) : await CanAct(userData, e.Contract.Bed.Room.Dorm, TagUser.PermissionDeleteInvoices, ct)))
			return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		await SyncDormBedInvoice(e, true, ct);
		await db.SaveChangesAsync(ct);
		await db.Set<DormBedInvoiceEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}

	private static List<KeyValue> DormBedInvoiceKeyValues(DormBedInvoiceEntity e) => e.Contract == null
		? [new KeyValue { Key = ULocalizedConstants.InvoiceId, Value = e.Id.ToString() }]
		: [
			new KeyValue { Key = ULocalizedConstants.Dorm, Value = e.Contract.Bed.Room.Dorm.Title },
			new KeyValue { Key = ULocalizedConstants.Room, Value = e.Contract.Bed.Room.Title },
			new KeyValue { Key = ULocalizedConstants.Bed, Value = e.Contract.Bed.Title },
			new KeyValue { Key = ULocalizedConstants.Period, Value = $"{e.Contract.StartDate:O}|{e.Contract.EndDate:O}" },
			new KeyValue { Key = ULocalizedConstants.Contract, Value = e.Contract.Id.ToString() },
			new KeyValue { Key = ULocalizedConstants.InvoiceId, Value = e.Id.ToString() }
		];

	public async Task<UResponse> PayDormBedInvoice(DormBedInvoicePayParams p, CancellationToken ct) {
		DormBedInvoiceEntity? e = await db.Set<DormBedInvoiceEntity>().AsTracking()
			.Include(x => x.Contract).ThenInclude(x => x!.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm)
			.FirstOrDefaultAsync(x => x.Id == p.InvoiceId, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("invoiceNotFound"));
		if (!e.Tags.Contains(TagDormBedInvoice.NotPaid)) return new UResponse(Usc.Conflict, ls.Get("thisInvoiceHasAlreadyBeenPaid"));

		// Claim NotPaid→PaidOnline atomically first so concurrent payments of the same invoice can't charge twice.
		List<TagDormBedInvoice> unpaidTags = e.Tags.ToList();
		List<TagDormBedInvoice> paidTags = [TagDormBedInvoice.PaidOnline];
		int claimed = await db.Set<DormBedInvoiceEntity>().Where(x => x.Id == e.Id && x.Tags.Contains(TagDormBedInvoice.NotPaid)).ExecuteUpdateAsync(u => u.SetProperty(x => x.Tags, paidTags), ct);
		if (claimed == 0) return new UResponse(Usc.Conflict, ls.Get("thisInvoiceHasAlreadyBeenPaid"));

		decimal amount = DueOf(e);
		if (amount > 0) {
			UResponse<WalletTxnResponse?> transfer = await ws.Transfer(new WalletTransferParams {
				SenderId = p.UserId,
				ReceiverId = OrganizationService.WalletOf(e.Contract?.Bed.Room.Dorm.OrganizationId),
				Amount = amount,
				Detail1 = ls.Get("dormInvoicePayment"),
				KeyValues = DormBedInvoiceKeyValues(e),
				TagWalletTxn = [TagWalletTxn.DormBedInvoice]
			}, ct);
			if (transfer.Result == null) {
				await db.Set<DormBedInvoiceEntity>().Where(x => x.Id == e.Id).ExecuteUpdateAsync(u => u.SetProperty(x => x.Tags, unpaidTags), ct);
				return new UResponse(transfer.Status, transfer.Message);
			}
			decimal commissionBase = e.Tags.Contains(TagDormBedInvoice.Deposit) ? 0 : e.Contract != null && e.Contract.Tags.Contains(TagDormBedContract.SingleInvoice) ? Math.Max(0, amount - e.Contract.Deposit) : amount;
			await acc.TakeCommission(e.Contract?.Bed.Room.Dorm.OrganizationId, commissionBase, ls.Get("dormInvoicePayment"), DormBedInvoiceKeyValues(e), e.Id, e.Contract?.Bed.Room.DormId, ct);
		}

		e.PaidAmount += amount;
		e.Tags = [..e.Tags.Where(x => (int)x < 200), TagDormBedInvoice.PaidOnline];
		await SyncDormBedInvoice(e, false, ct);
		await acc.Post(e.Contract?.Bed.Room.Dorm.OrganizationId, TagVoucher.Payment, e.Id, e.Contract?.Bed.Room.DormId, e.Contract?.UserId, $"{ls.Get("dormInvoicePayment", "fa")} - {e.Contract?.Bed.Room.Dorm.Title} - {e.Contract?.Bed.Title}", DateTime.UtcNow, ct,
			new AccountingLeg(TagAccount.Wallet, amount), new AccountingLeg(TagAccount.Receivable, -amount));
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
		bool isManager = await CanAct(userData, e.Contract.Bed.Room.Dorm, TagUser.PermissionPayInvoices, ct);
		if (!isOwner && !isManager) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		return await PayDormBedInvoice(new DormBedInvoicePayParams { InvoiceId = e.Id, UserId = e.Contract.UserId }, ct);
	}

	public async Task<UResponse<IEnumerable<DormBedInvoiceChartResponse>?>> ReadDormBedInvoiceChartData(BaseParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<DormBedInvoiceChartResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<DormBedInvoiceChartResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		IQueryable<DormBedInvoiceEntity> invoiceQuery = db.Set<DormBedInvoiceEntity>();
		if (!OrganizationService.IsFull(userData)) {
			Guid uid = userData.Id;
			bool scoped = OrganizationService.IsScopedAdmin(userData);
			invoiceQuery = invoiceQuery.Where(x => x.Contract != null && (x.Contract.UserId == uid || scoped && x.Contract.Bed.Room.Dorm.AdminUserIds.Contains(uid)));
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

	private static decimal PenaltyOf(decimal debt, int percentPerDay, DateTime dueDate, DateTime now) =>
		percentPerDay <= 0 || dueDate > now ? 0 : debt * (percentPerDay / 100m) * Math.Max(0, (now - dueDate).Days);

	private static decimal DueOf(DormBedInvoiceEntity i) => i.DebtAmount + i.PenaltyAmount - i.CreditorAmount - i.PaidAmount;

	private static DormBedInvoiceEntity NewDormBedInvoice(DormBedContractEntity contract, Guid creatorId, ICollection<TagDormBedInvoice> tags, decimal amount, DateTime dueDate, int penaltyPercent) => new() {
		Id = Guid.CreateVersion7(),
		CreatorId = creatorId,
		CreatedAt = DateTime.UtcNow,
		Tags = tags,
		DebtAmount = amount,
		CreditorAmount = 0,
		PaidAmount = 0,
		PenaltyAmount = 0,
		ContractId = contract.Id,
		DueDate = dueDate,
		JsonData = new DormBedInvoiceJson { PenaltyPrecentEveryDate = penaltyPercent }
	};

	private async Task<DormBedContractEntity?> ContractForChange(Guid id, CancellationToken ct) =>
		await db.Set<DormBedContractEntity>().AsTracking()
			.Include(x => x.Invoices)
			.Include(x => x.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm)
			.FirstOrDefaultAsync(x => x.Id == id, ct);

	public async Task<UResponse> SettleDormBedContract(DormBedContractSettleParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		DormBedContractEntity? e = await ContractForChange(p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("contractNotFound"));
		if (!await CanAct(userData, e.Bed.Room.Dorm, TagUser.PermissionManageContracts, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (e.Tags.Contains(TagDormBedContract.Settled)) return new UResponse(Usc.Conflict, ls.Get("thisContractIsAlreadySettled"));

		DateTime end = p.EndDate ?? DateTime.UtcNow;
		if (end > e.EndDate) end = e.EndDate;
		if (end < e.StartDate) end = e.StartDate;

		decimal unusedCredit = 0;
		List<DormBedInvoiceEntity> rents = e.Invoices.Where(x => x.Tags.Contains(TagDormBedInvoice.Rent)).OrderBy(x => x.DueDate).ToList();
		foreach (DormBedInvoiceEntity r in rents.Where(x => x.DueDate > end)) {
			if (r.Tags.Contains(TagDormBedInvoice.NotPaid)) {
				await SyncDormBedInvoice(r, true, ct);
				db.Set<DormBedInvoiceEntity>().Remove(r);
			}
			else unusedCredit += r.PaidAmount;
		}

		foreach (DormBedInvoiceEntity d in e.Invoices.Where(x => x.Tags.Contains(TagDormBedInvoice.Deposit) && x.Tags.Contains(TagDormBedInvoice.NotPaid) && x.PaidAmount == 0).ToList()) {
			await SyncDormBedInvoice(d, true, ct);
			db.Set<DormBedInvoiceEntity>().Remove(d);
		}

		DormBedInvoiceEntity? current = rents.LastOrDefault(x => x.DueDate <= end);
		if (current != null) {
			PersianDateTime start = current.DueDate.ToPersian();
			decimal fraction = Math.Min(1, ((end.Date - current.DueDate.Date).Days + 1) / (decimal)PersianDateTime.DaysInMonth(start.Year, start.Month));
			if (current.Tags.Contains(TagDormBedInvoice.NotPaid)) {
				current.DebtAmount = Math.Round(current.DebtAmount * fraction, 2);
				await SyncDormBedInvoice(current, false, ct);
			}
			else unusedCredit += current.PaidAmount * (1 - fraction);
		}

		decimal depositPaid = e.Tags.Contains(TagDormBedContract.SingleInvoice)
			? e.Invoices.Any(x => !x.Tags.Contains(TagDormBedInvoice.NotPaid)) ? e.Deposit : 0
			: e.Invoices.Where(x => x.Tags.Contains(TagDormBedInvoice.Deposit) && !x.Tags.Contains(TagDormBedInvoice.NotPaid)).Sum(x => x.PaidAmount);
		decimal pool = depositPaid + Math.Round(unusedCredit, 2) - p.Deductions;

		decimal applied = 0;
		foreach (DormBedInvoiceEntity u in e.Invoices.Where(x => x.Tags.Contains(TagDormBedInvoice.NotPaid) && db.Entry(x).State != EntityState.Deleted).OrderBy(x => x.DueDate).ToList()) {
			decimal due = DueOf(u);
			if (due <= 0 || pool < due) continue;
			u.PaidAmount += due;
			u.Tags = [..u.Tags.Where(x => x != TagDormBedInvoice.NotPaid), TagDormBedInvoice.Paid];
			pool -= due;
			applied += due;
			await SyncDormBedInvoice(u, false, ct);
		}

		if (pool < 0) {
			DormBedInvoiceEntity extra = NewDormBedInvoice(e, userData.Id, [TagDormBedInvoice.NotPaid, TagDormBedInvoice.Service], -pool, DateTime.UtcNow, 0);
			extra.Contract = e;
			await db.Set<DormBedInvoiceEntity>().AddAsync(extra, ct);
			await SyncDormBedInvoice(extra, false, ct);
		}

		decimal refund = Math.Max(0, pool);
		if (refund > 0) {
			UResponse<WalletTxnResponse?> transfer = await ws.Transfer(new WalletTransferParams {
				SenderId = OrganizationService.WalletOf(e.Bed.Room.Dorm.OrganizationId),
				ReceiverId = e.UserId,
				Amount = refund,
				Detail1 = ls.Get("dormDepositRefund"),
				KeyValues = [
					new KeyValue { Key = ULocalizedConstants.Dorm, Value = e.Bed.Room.Dorm.Title },
					new KeyValue { Key = ULocalizedConstants.Bed, Value = e.Bed.Title },
					new KeyValue { Key = ULocalizedConstants.Contract, Value = e.Id.ToString() }
				],
				TagWalletTxn = [TagWalletTxn.DormDepositRefund]
			}, ct);
			if (transfer.Result == null) return new UResponse(transfer.Status, transfer.Message);
		}

		decimal held = depositPaid + Math.Round(unusedCredit, 2);
		await acc.Post(e.Bed.Room.Dorm.OrganizationId, TagVoucher.Settlement, e.Id, e.Bed.Room.DormId, e.UserId, $"{ls.Get("contractSettled", "fa")} - {e.Bed.Room.Dorm.Title} - {e.Bed.Title}", DateTime.UtcNow, ct,
			new AccountingLeg(TagAccount.DepositsHeld, depositPaid), new AccountingLeg(TagAccount.RentIncome, Math.Round(unusedCredit, 2)), new AccountingLeg(TagAccount.Receivable, -applied),
			new AccountingLeg(TagAccount.DamageIncome, -(held - applied - refund)), new AccountingLeg(TagAccount.Wallet, -refund));

		e.EndDate = end;
		e.Tags = [..e.Tags, TagDormBedContract.Settled];
		e.JsonData.SettledAt = DateTime.UtcNow;
		e.JsonData.Deductions = p.Deductions;
		e.JsonData.DeductionReason = p.DeductionReason;
		e.JsonData.DepositRefund = refund;
		await AddNotification(e.UserId, TagNotification.General, ls.Get("contractSettled"), e.Bed.Room.Dorm.Title, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> RenewDormBedContract(DormBedContractRenewParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		DormBedContractEntity? e = await ContractForChange(p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("contractNotFound"));
		if (!await CanAct(userData, e.Bed.Room.Dorm, TagUser.PermissionManageContracts, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (e.Tags.Contains(TagDormBedContract.Settled)) return new UResponse(Usc.Conflict, ls.Get("thisContractIsAlreadySettled"));
		if (p.EndDate.Date <= e.EndDate.Date) return new UResponse(Usc.BadRequest, ls.Get("theNewEndDateMustBeAfterTheCurrentOne"));

		decimal rent = p.Rent ?? e.Rent;
		int penaltyPercent = e.Invoices.OrderBy(x => x.CreatedAt).LastOrDefault()?.JsonData.PenaltyPrecentEveryDate ?? 0;
		PersianDateTime cursor = e.EndDate.Date.AddDays(1).ToPersian();
		while (cursor.ToDateTime() <= p.EndDate.Date) {
			PersianDateTime next = cursor.AddMonths(1).StartOfMonth;
			DateTime periodEnd = next.ToDateTime().AddDays(-1) < p.EndDate.Date ? next.ToDateTime().AddDays(-1) : p.EndDate.Date;
			int days = (periodEnd - cursor.ToDateTime()).Days + 1;
			int monthDays = PersianDateTime.DaysInMonth(cursor.Year, cursor.Month);
			decimal amount = days >= monthDays ? rent : Math.Round(rent * days / monthDays, 2);
			await db.Set<DormBedInvoiceEntity>().AddAsync(NewDormBedInvoice(e, userData.Id, [TagDormBedInvoice.NotPaid, TagDormBedInvoice.Rent], amount, cursor.ToDateTime(), penaltyPercent), ct);
			cursor = next;
		}

		e.EndDate = p.EndDate;
		e.Rent = rent;
		await AddNotification(e.UserId, TagNotification.InvoiceIssued, ls.Get("newInvoicesIssued"), e.Bed.Room.Dorm.Title, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> TransferDormBedContract(DormBedContractTransferParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		DormBedContractEntity? e = await ContractForChange(p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("contractNotFound"));
		if (!await CanAct(userData, e.Bed.Room.Dorm, TagUser.PermissionManageContracts, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (e.Tags.Contains(TagDormBedContract.Settled)) return new UResponse(Usc.Conflict, ls.Get("thisContractIsAlreadySettled"));

		DateTime now = DateTime.UtcNow;
		DormBedEntity? to = await db.Set<DormBedEntity>().Include(x => x.Contracts).Include(x => x.Room).ThenInclude(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.BedId, ct);
		if (to == null) return new UResponse(Usc.NotFound, ls.Get("dormBedNotFound"));
		if (!await CanAct(userData, to.Room.Dorm, TagUser.PermissionManageContracts, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (to.Room.Dorm.OrganizationId != e.Bed.Room.Dorm.OrganizationId) return new UResponse(Usc.Conflict, ls.Get("bedsOfAnotherOrganization"));
		if (to.Contracts.Any(x => x.Id != e.Id && x.EndDate >= now && !x.Tags.Contains(TagDormBedContract.Settled))) return new UResponse(Usc.Conflict, ls.Get("thisBedHasAnActiveContract"));

		DateTime date = p.Date ?? now;
		e.JsonData.BedHistory = [..e.JsonData.BedHistory, new ContractBedChange { BedId = e.BedId, From = e.JsonData.BedHistory.LastOrDefault()?.To ?? e.StartDate, To = date }];
		e.BedId = to.Id;
		if (p.Rent.HasValue && p.Rent != e.Rent) {
			foreach (DormBedInvoiceEntity i in e.Invoices.Where(x => x.Tags.Contains(TagDormBedInvoice.Rent) && x.Tags.Contains(TagDormBedInvoice.NotPaid) && x.DueDate >= date && x.DebtAmount == e.Rent))
				i.DebtAmount = p.Rent.Value;
			e.Rent = p.Rent.Value;
		}

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> SplitDormBedInvoice(DormBedInvoiceSplitParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (p.Count is < 2 or > 12) return new UResponse(Usc.BadRequest, ls.Get("amountIsNotValid"));

		DormBedInvoiceEntity? e = await db.Set<DormBedInvoiceEntity>().AsTracking().Include(x => x.Contract).ThenInclude(x => x!.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e?.Contract == null) return new UResponse(Usc.NotFound, ls.Get("invoiceNotFound"));
		if (!await CanAct(userData, e.Contract.Bed.Room.Dorm, TagUser.PermissionManageInvoices, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (!e.Tags.Contains(TagDormBedInvoice.NotPaid) || e.PaidAmount > 0) return new UResponse(Usc.Conflict, ls.Get("onlyUnpaidInvoicesCanBeSplit"));

		decimal total = DueOf(e);
		decimal part = Math.Floor(total / p.Count);
		PersianDateTime due = e.DueDate.ToPersian();
		for (int i = 0; i < p.Count; i++) {
			DormBedInvoiceEntity x = NewDormBedInvoice(e.Contract, userData.Id, e.Tags.ToList(), i == p.Count - 1 ? total - part * (p.Count - 1) : part, due.AddMonths(i).ToDateTime(), e.JsonData.PenaltyPrecentEveryDate);
			x.JsonData.Detail1 = e.JsonData.Detail1;
			await db.Set<DormBedInvoiceEntity>().AddAsync(x, ct);
		}

		await SyncDormBedInvoice(e, true, ct);
		db.Set<DormBedInvoiceEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	private async Task SyncDormBedInvoice(DormBedInvoiceEntity e, bool removed, CancellationToken ct) {
		DormEntity? dorm = e.Contract?.Bed.Room.Dorm;
		if (dorm?.OrganizationId == null) return;

		DateTime now = DateTime.UtcNow;
		bool open = e.Tags.Contains(TagDormBedInvoice.NotPaid);
		bool charged = !removed && (e.DueDate <= now || !open || e.PaidAmount > 0);
		if (!charged && !e.JsonData.Posted) return;

		decimal debt = charged ? Math.Max(0, e.DebtAmount - e.CreditorAmount) : 0;
		bool single = e.Contract!.Tags.Contains(TagDormBedContract.SingleInvoice) && !e.Tags.Contains(TagDormBedInvoice.Rent) && !e.Tags.Contains(TagDormBedInvoice.Service) && !e.Tags.Contains(TagDormBedInvoice.Damage);
		decimal deposit = e.Tags.Contains(TagDormBedInvoice.Deposit) ? debt : single ? Math.Min(e.Contract.Deposit, debt) : 0;
		Dictionary<TagAccount, decimal> target = new() {
			[TagAccount.DepositsHeld] = deposit,
			[e.Tags.Contains(TagDormBedInvoice.Damage) ? TagAccount.DamageIncome : e.Tags.Contains(TagDormBedInvoice.Service) ? TagAccount.ServiceIncome : TagAccount.RentIncome] = debt - deposit,
			[TagAccount.PenaltyIncome] = charged && !open ? e.PenaltyAmount : 0
		};
		await acc.SyncCharge(dorm.OrganizationId.Value, e.Id, dorm.Id, e.Contract.UserId, !e.JsonData.Posted && e.DueDate < now ? e.DueDate : now, $"{ls.Get("dormBedInvoiceIssued", "fa")} - {dorm.Title} - {e.Contract.Bed.Title}", target, ct);
		e.JsonData.Posted = charged;
	}

	public async Task<UResponse> ReceiveDormBedInvoice(InvoiceReceiveParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		DormBedInvoiceEntity? e = await db.Set<DormBedInvoiceEntity>().AsTracking()
			.Include(x => x.Contract).ThenInclude(x => x!.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm)
			.FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e?.Contract == null) return new UResponse(Usc.NotFound, ls.Get("invoiceNotFound"));
		DormEntity dorm = e.Contract.Bed.Room.Dorm;
		if (!await CanAct(userData, dorm, TagUser.PermissionPayInvoices, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (!e.Tags.Contains(TagDormBedInvoice.NotPaid)) return new UResponse(Usc.Conflict, ls.Get("thisInvoiceHasAlreadyBeenPaid"));

		decimal due = DueOf(e);
		decimal amount = p.Amount ?? due;
		if (amount <= 0 || amount > due) return new UResponse(Usc.BadRequest, ls.Get("amountIsNotValid"));

		e.PaidAmount += amount;
		if (amount == due) e.Tags = [..e.Tags.Where(x => x != TagDormBedInvoice.NotPaid), TagDormBedInvoice.PaidManual];
		await SyncDormBedInvoice(e, false, ct);
		UResponse? error = await acc.PostReceipt(dorm.OrganizationId, e.Id, dorm.Id, e.Contract.UserId, e.Contract.Id, amount, p, $"{ls.Get("invoiceReceipt", "fa")} - {dorm.Title} - {e.Contract.Bed.Title}", userData.Id, ct);
		if (error != null) return error;

		await AddNotification(e.Contract.UserId, TagNotification.InvoicePaid, ls.Get("invoicePaid"), ls.Get("dormInvoicePayment"), ct);
		await db.SaveChangesAsync(ct);
		return new UResponse(Usc.Success, ls.Get("paymentCompleted"));
	}

	public async Task ProcessDueInvoices(CancellationToken ct) {
		DateTime now = DateTime.UtcNow, soon = now.AddDays(3);
		List<DormBedInvoiceEntity> list = await db.Set<DormBedInvoiceEntity>().AsTracking()
			.Include(x => x.Contract).ThenInclude(x => x!.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm)
			.Where(x => x.Contract != null && x.Tags.Contains(TagDormBedInvoice.NotPaid) && x.DueDate <= soon)
			.ToListAsync(ct);

		foreach (DormBedInvoiceEntity e in list) {
			decimal penalty = PenaltyOf(e.DebtAmount, e.JsonData.PenaltyPrecentEveryDate, e.DueDate, now);
			if (penalty > e.PenaltyAmount) e.PenaltyAmount = penalty;
			if (e.DueDate < now && !e.JsonData.OverdueReminded) {
				await AddNotification(e.Contract!.UserId, TagNotification.InvoiceOverdue, ls.Get("invoiceIsOverdue", "fa"), e.Contract.Bed.Room.Dorm.Title, ct);
				e.JsonData.OverdueReminded = true;
				e.JsonData.DueReminded = true;
			}
			else if (e.DueDate >= now && !e.JsonData.DueReminded) {
				await AddNotification(e.Contract!.UserId, TagNotification.InvoiceDue, ls.Get("invoiceDueSoon", "fa"), e.Contract.Bed.Room.Dorm.Title, ct);
				e.JsonData.DueReminded = true;
			}
		}

		List<DormBedInvoiceEntity> dormInvoices = await db.Set<DormBedInvoiceEntity>().AsTracking()
			.Include(x => x.Contract).ThenInclude(x => x!.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm)
			.Where(x => !x.JsonData.Posted && x.DueDate <= now && x.Contract != null && x.Contract.Bed.Room.Dorm.OrganizationId != null)
			.ToListAsync(ct);
		foreach (DormBedInvoiceEntity e in dormInvoices) await SyncDormBedInvoice(e, false, ct);

		await db.SaveChangesAsync(ct);
	}

	public async Task<bool?> CanActOnPlaceOf(JwtClaimData u, Guid? dormId, Guid? dormRoomId, Guid? dormBedId, CancellationToken ct) {
		if (dormId == null && dormRoomId == null && dormBedId == null) return null;
		var dorm = dormId != null
			? await db.Set<DormEntity>().Where(x => x.Id == dormId).Select(x => new { x.AdminUserIds, x.OrganizationId }).FirstOrDefaultAsync(ct)
			: dormRoomId != null
				? await db.Set<DormRoomEntity>().Where(x => x.Id == dormRoomId).Select(x => new { x.Dorm.AdminUserIds, x.Dorm.OrganizationId }).FirstOrDefaultAsync(ct)
				: await db.Set<DormBedEntity>().Where(x => x.Id == dormBedId).Select(x => new { x.Room.Dorm.AdminUserIds, x.Room.Dorm.OrganizationId }).FirstOrDefaultAsync(ct);
		return await os.CanActOnPlace(u, dorm?.OrganizationId, dorm?.AdminUserIds ?? [], TagUser.PermissionManageDorms, ct);
	}

	public IQueryable<Guid> RelatedUserIds(Guid userId) => db.Set<DormBedContractEntity>().Where(c => c.Bed.Room.Dorm.AdminUserIds.Contains(userId)).Select(c => c.UserId);

	public async Task ReopenPayment(Guid sourceId, decimal amount, CancellationToken ct) {
		DormBedInvoiceEntity? d = await db.Set<DormBedInvoiceEntity>().AsTracking()
			.Include(x => x.Contract).ThenInclude(x => x!.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm)
			.FirstOrDefaultAsync(x => x.Id == sourceId, ct);
		if (d == null) return;
		d.PaidAmount = Math.Max(0, d.PaidAmount - amount);
		if (!d.Tags.Contains(TagDormBedInvoice.NotPaid)) d.Tags = [..d.Tags.Where(x => (int)x < 200), TagDormBedInvoice.NotPaid];
		await SyncDormBedInvoice(d, false, ct);
	}

	public async Task<List<AccountingDue>> Outstanding(Guid organizationId, DateTime now, CancellationToken ct) =>
		(await db.Set<DormBedInvoiceEntity>()
			.Where(x => x.Tags.Contains(TagDormBedInvoice.NotPaid) && x.DueDate <= now && x.Contract != null && x.Contract.Bed.Room.Dorm.OrganizationId == organizationId)
			.Select(x => new { PersonId = x.Contract!.UserId, x.DueDate, Amount = x.DebtAmount + x.PenaltyAmount - x.CreditorAmount - x.PaidAmount })
			.ToListAsync(ct))
		.Select(x => new AccountingDue(x.PersonId, x.DueDate, x.Amount)).ToList();

	public async Task<UResponse<DormDashboardResponse?>> ReadDormDashboard(DashboardRangeParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<DormDashboardResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<DormDashboardResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		bool tenant = Core.App.MultiTenant && !userData.IsSystemAdmin;
		if (tenant ? !await os.HasOrganizationPermission(userData, null, TagUser.PermissionViewDashboard, ct) : !userData.IsSuperAdmin)
			return new UResponse<DormDashboardResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		Guid uid = userData.Id;
		IQueryable<DormEntity> dorms = db.Set<DormEntity>().Where(x => !tenant || x.AdminUserIds.Contains(uid));
		IQueryable<DormRoomEntity> dormRooms = db.Set<DormRoomEntity>().Where(x => !tenant || x.Dorm.AdminUserIds.Contains(uid));
		IQueryable<DormBedEntity> dormBeds = db.Set<DormBedEntity>().Where(x => !tenant || x.Room.Dorm.AdminUserIds.Contains(uid));
		IQueryable<DormBedContractEntity> contracts = db.Set<DormBedContractEntity>().Where(x => !tenant || x.Bed.Room.Dorm.AdminUserIds.Contains(uid));
		IQueryable<DormBedInvoiceEntity> invoices = db.Set<DormBedInvoiceEntity>().Where(x => !tenant || x.Contract != null && x.Contract.Bed.Room.Dorm.AdminUserIds.Contains(uid));
		IQueryable<UserEntity> users = db.Set<UserEntity>().Where(x => contracts.Any(c => c.UserId == x.Id));

		DateTime now = DateTime.UtcNow;
		DateTime to = p.ToDate ?? now;
		DateTime from = p.FromDate ?? to.AddDays(-30);
		DateTime soon = now.AddDays(30);

		int usersCount = await users.CountAsync(ct);
		int newUsersCount = await users.CountAsync(x => x.CreatedAt >= from && x.CreatedAt <= to, ct);

		int dormsCount = await dorms.CountAsync(ct);
		int dormRoomsCount = await dormRooms.CountAsync(ct);
		int dormBedsCount = await dormBeds.CountAsync(ct);
		int dormBedsOccupiedCount = await contracts.Where(x => x.StartDate <= now && x.EndDate >= now).Select(x => x.BedId).Distinct().CountAsync(ct);
		int dormBedsAvailableCount = dormBedsCount - dormBedsOccupiedCount;

		int contractsCount = await contracts.CountAsync(ct);
		int activeContractsCount = await contracts.CountAsync(x => x.StartDate <= now && x.EndDate >= now, ct);
		int upcomingContractsCount = await contracts.CountAsync(x => x.StartDate > now, ct);
		int expiredContractsCount = await contracts.CountAsync(x => x.EndDate < now, ct);
		int expiringSoonContractsCount = await contracts.CountAsync(x => x.EndDate >= now && x.EndDate <= soon, ct);

		int invoicesCount = await invoices.CountAsync(ct);
		int unpaidInvoicesCount = await invoices.CountAsync(x => x.Tags.Contains(TagDormBedInvoice.NotPaid), ct);
		int paidInvoicesCount = invoicesCount - unpaidInvoicesCount;
		int overdueInvoicesCount = await invoices.CountAsync(x => x.Tags.Contains(TagDormBedInvoice.NotPaid) && x.DueDate < now, ct);

		decimal totalDebt = await invoices.SumAsync(x => (decimal?)x.DebtAmount, ct) ?? 0;
		decimal totalPaid = await invoices.SumAsync(x => (decimal?)x.PaidAmount, ct) ?? 0;
		decimal totalPenalty = await invoices.SumAsync(x => (decimal?)x.PenaltyAmount, ct) ?? 0;

		List<DormBedInvoiceEntity> recentInvoices = await invoices
			.Where(x => x.CreatedAt >= now.AddMonths(-12))
			.ToListAsync(ct);
		List<DormBedInvoiceChartResponse> monthlyRevenue = recentInvoices
			.GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
			.Select(g => new DormBedInvoiceChartResponse {
				Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
				TotalDebt = g.Sum(x => x.DebtAmount),
				TotalPaid = g.Sum(x => x.PaidAmount),
				TotalPenalty = g.Sum(x => x.PenaltyAmount),
				TotalRemaining = g.Sum(x => x.DebtAmount - x.PaidAmount),
				InvoiceCount = g.Count()
			})
			.OrderBy(x => x.Month).ToList();

		List<DormBedContractEntity> expiringEntities = await contracts
			.Include(x => x.User).Include(x => x.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm)
			.Where(x => x.EndDate >= now && x.EndDate <= soon)
			.OrderBy(x => x.EndDate).Take(10).ToListAsync(ct);
		List<ExpiringContractItem> expiringContracts = expiringEntities.Select(x => new ExpiringContractItem {
			Id = x.Id, UserName = x.User.UserName, BedTitle = x.Bed.Title, DormTitle = x.Bed.Room.Dorm.Title, EndDate = x.EndDate, Rent = x.Rent
		}).ToList();

		List<DormBedInvoiceEntity> overdueEntities = await invoices
			.Include(x => x.Contract).ThenInclude(x => x!.User)
			.Where(x => x.Tags.Contains(TagDormBedInvoice.NotPaid) && x.DueDate < now)
			.OrderBy(x => x.DueDate).Take(10).ToListAsync(ct);
		List<OverdueInvoiceItem> overdueInvoices = overdueEntities.Select(x => new OverdueInvoiceItem {
			Id = x.Id, UserName = x.Contract?.User.UserName, DebtAmount = x.DebtAmount, PaidAmount = x.PaidAmount,
			PenaltyAmount = x.PenaltyAmount, DueDate = x.DueDate, DaysOverdue = Math.Max(0, (now - x.DueDate).Days)
		}).ToList();

		List<DormBedContractEntity> recentContractEntities = await contracts
			.Include(x => x.User).Include(x => x.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm)
			.OrderByDescending(x => x.CreatedAt).Take(5).ToListAsync(ct);
		List<RecentContractItem> recentContracts = recentContractEntities.Select(x => new RecentContractItem {
			Id = x.Id, UserName = x.User.UserName, BedTitle = x.Bed.Title, DormTitle = x.Bed.Room.Dorm.Title,
			StartDate = x.StartDate, EndDate = x.EndDate, Rent = x.Rent, CreatedAt = x.CreatedAt
		}).ToList();

		List<UserEntity> recentUserEntities = await users
			.OrderByDescending(x => x.CreatedAt).Take(5).ToListAsync(ct);
		List<RecentUserItem> recentUsers = recentUserEntities.Select(x => new RecentUserItem {
			Id = x.Id, DisplayName = $"{x.FirstName} {x.LastName}".Trim() is { Length: > 0 } n ? n : x.UserName,
			UserName = x.UserName, PhoneNumber = x.PhoneNumber, CreatedAt = x.CreatedAt
		}).ToList();

		List<DormCityItem> dormsByCity = await dorms
			.GroupBy(x => x.CityCode)
			.Select(g => new DormCityItem { Name = g.Key, Count = g.Count() })
			.OrderByDescending(x => x.Count).Take(10).ToListAsync(ct);

		return new UResponse<DormDashboardResponse?>(new DormDashboardResponse {
			GeneratedAt = DateTime.UtcNow,
			ResidentsCount = usersCount,
			NewResidentsCount = newUsersCount,
			DormsCount = dormsCount,
			DormRoomsCount = dormRoomsCount,
			DormBedsCount = dormBedsCount,
			DormBedsAvailableCount = dormBedsAvailableCount,
			DormBedsOccupiedCount = dormBedsOccupiedCount,
			DormOccupancyRate = dormBedsCount == 0 ? 0 : Math.Round(dormBedsOccupiedCount * 100.0 / dormBedsCount, 1),
			ContractsCount = contractsCount,
			ActiveContractsCount = activeContractsCount,
			UpcomingContractsCount = upcomingContractsCount,
			ExpiredContractsCount = expiredContractsCount,
			ExpiringSoonContractsCount = expiringSoonContractsCount,
			InvoicesCount = invoicesCount,
			PaidInvoicesCount = paidInvoicesCount,
			UnpaidInvoicesCount = unpaidInvoicesCount,
			OverdueInvoicesCount = overdueInvoicesCount,
			TotalDebt = totalDebt,
			TotalPaid = totalPaid,
			TotalPenalty = totalPenalty,
			TotalOutstanding = totalDebt + totalPenalty - totalPaid,
			MonthlyRevenue = monthlyRevenue,
			ExpiringContracts = expiringContracts,
			OverdueInvoices = overdueInvoices,
			RecentContracts = recentContracts,
			RecentResidents = recentUsers,
			DormsByCity = dormsByCity
		});
	}

	public async Task<UResponse<List<KeyValue>?>> SeedDorms(CancellationToken ct = default) {
		Guid adminId = Core.App.Users.SystemAdmin.Id;
		DateTime now = DateTime.UtcNow;
		DateTime today = now.Date;

		if (!await db.Set<UserEntity>().AnyAsync(x => x.Id == adminId, ct)) return new UResponse<List<KeyValue>?>(null, Usc.BadRequest, "Run DataSeeder/Users first.");
		if (await db.Set<DormEntity>().AnyAsync(x => x.Title == "خوابگاه دخترانه‌ی نگین", ct)) return new UResponse<List<KeyValue>?>(null, Usc.Conflict, "Dorm demo data already exists.");

		(List<UserEntity> users, List<Guid> userIds) = await seeds.DemoUsers(ct);
		string[] firstNames = DataSeedService.DemoFirstNames;
		string[] lastNames = DataSeedService.DemoLastNames;
		Guid UserAt(int i) => userIds[i % userIds.Count];

		List<CommentEntity> comments = [];
		List<DormEntity> dorms = [];
		List<DormRoomEntity> dormRooms = [];
		List<DormBedEntity> beds = [];
		List<DormBedContractEntity> contracts = [];
		List<DormBedInvoiceEntity> dormInvoices = [];

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

		// ---------------------------------------------------------------- dorms
		// Residents, amenities, meals and what the rent includes are tags; the Json only keeps texts and numbers.
		(string Title, string City, string Address, string Phone, List<TagDorm> Tags, DormJson Json, (string Title, int Beds, decimal Rent, decimal Deposit, double Size, int Floor, List<TagDormRoom> Tags)[] Rooms)[] dormSeeds = [
			("خوابگاه دخترانه‌ی نگین", "108012", "تهران، امیرآباد شمالی، خیابان چهارم", "02166001000",
				[
					TagDorm.Girls, TagDorm.Active, TagDorm.Featured, TagDorm.Approved,
					TagDorm.Bachelor, TagDorm.Master, TagDorm.Phd,
					TagDorm.Wifi, TagDorm.SharedKitchen, TagDorm.StudyRoom, TagDorm.Laundry, TagDorm.Supervisor, TagDorm.Cctv, TagDorm.SecurityGuard, TagDorm.Lockers, TagDorm.Lounge,
					TagDorm.Breakfast, TagDorm.Dinner,
					TagDorm.InternetIncluded, TagDorm.UtilitiesIncluded, TagDorm.CleaningIncluded
				],
				new DormJson {
					Highlights = ["هفت دقیقه پیاده تا دانشگاه تهران", "اتاق مطالعه‌ی شبانه‌روزی", "اینترنت فیبر ۱۰۰ مگابیت"],
					Website = "https://example.com/negin", Whatsapp = "989120000011", Instagram = "negin_dorm", CurfewTime = "23:00", MinimumStayMonths = 6,
					Policies = "ودیعه هنگام عقد قرارداد و اجاره‌ی ماهانه تا پنجم هر ماه پرداخت می‌شود. ودیعه پس از تسویه و تحویل اتاق حداکثر ظرف ۷ روز کاری مسترد می‌شود. فسخ زودهنگام با معرفی جایگزین بدون جریمه است.",
					UniversityWalkMinutes = 7, HowToGetThere = "از ایستگاه مترو دانشگاه تهران با تاکسی یا ۱۰ دقیقه پیاده تا خیابان چهارم امیرآباد.",
					Nearby = [new PlaceNearby { Title = "دانشگاه تهران", DistanceMeters = 450, Minutes = 7 }, new PlaceNearby { Title = "مترو دانشگاه تهران", DistanceMeters = 900, Minutes = 12 }, new PlaceNearby { Title = "داروخانه‌ی شبانه‌روزی", DistanceMeters = 200, Minutes = 3 }],
					Faqs = [new PlaceFaq { Question = "ساعت آخرین ورود چه زمانی است؟", Answer = "ساعت ۲۳؛ برای ورود دیرتر باید از پیش با سرپرست هماهنگ کنید." }, new PlaceFaq { Question = "آیا امکان آشپزی وجود دارد؟", Answer = "بله، در هر طبقه یک آشپزخانه‌ی مشترک با یخچال جداگانه برای هر اتاق هست." }],
					Description = "هفت دقیقه پیاده تا درِ اصلی دانشگاه تهران؛ اتاق‌های مبله‌ی دو و چهارنفره، اتاق مطالعه‌ی شبانه‌روزی و سرپرست مقیم.",
					NearbyUniversity = "دانشگاه تهران", VisitingHours = "۱۶ تا ۲۰",
					Rules = ["رعایت سکوت از ساعت ۲۲", "استعمال دخانیات ممنوع", "ورود مهمان تنها در لابی"], RequiredDocuments = ["کارت ملی", "گواهی اشتغال به تحصیل", "دو قطعه عکس ۳×۴"],
					Latitude = 35.7300, Longitude = 51.3900
				},
				[
					("اتاق دو نفره‌ی A", 2, 3_200_000m, 15_000_000m, 18, 2, [TagDormRoom.Double, TagDormRoom.Furnished, TagDormRoom.PrivateBathroom, TagDormRoom.Desk, TagDormRoom.Wardrobe, TagDormRoom.AirConditioning]),
					("اتاق چهارنفره‌ی B", 4, 2_100_000m, 10_000_000m, 28, 3, [TagDormRoom.Dorm, TagDormRoom.Furnished, TagDormRoom.Desk, TagDormRoom.Wardrobe, TagDormRoom.Heating]),
					("اتاق سه نفره‌ی C", 3, 2_600_000m, 12_000_000m, 22, 4, [TagDormRoom.Dorm, TagDormRoom.Furnished, TagDormRoom.PrivateBathroom, TagDormRoom.Desk, TagDormRoom.AirConditioning, TagDormRoom.Balcony])
				]),

			("خوابگاه پسرانه‌ی آرمان", "108012", "تهران، انقلاب، خیابان فخر رازی", "02166002000",
				[
					TagDorm.Boys, TagDorm.Active, TagDorm.Approved,
					TagDorm.Bachelor, TagDorm.Master,
					TagDorm.Wifi, TagDorm.SharedKitchen, TagDorm.BikeParking, TagDorm.Laundry, TagDorm.Cctv, TagDorm.Lockers, TagDorm.Lounge,
					TagDorm.Dinner,
					TagDorm.InternetIncluded, TagDorm.UtilitiesIncluded
				],
				new DormJson {
					Highlights = ["نزدیک مترو انقلاب", "آشپزخانه‌ی مرکزی", "پارکینگ دوچرخه و موتور"], Whatsapp = "989120000012", MinimumStayMonths = 4,
					Policies = "اجاره‌ی ماهانه. ودیعه معادل دو ماه اجاره است. ورود مهمان ممنوع.",
					UniversityWalkMinutes = 15, HowToGetThere = "خروجی ۳ مترو انقلاب، ۱۰ دقیقه پیاده به سمت خیابان فخر رازی.",
					Nearby = [new PlaceNearby { Title = "مترو انقلاب", DistanceMeters = 800, Minutes = 10 }, new PlaceNearby { Title = "کتابفروشی‌های انقلاب", DistanceMeters = 300, Minutes = 4 }],
					Faqs = [new PlaceFaq { Question = "قرارداد ترمی است یا ماهانه؟", Answer = "هر دو ممکن است؛ حداقل مدت ۴ ماه." }],
					Description = "ساختمان بازسازی‌شده‌ی چهارطبقه با آشپزخانه‌ی مرکزی؛ ۱۰ دقیقه تا مترو انقلاب.", NearbyUniversity = "دانشگاه تهران", VisitingHours = "۱۷ تا ۲۰",
					Rules = ["ورود تا ساعت ۲۴", "ورود مهمان ممنوع"], RequiredDocuments = ["کارت ملی", "گواهی اشتغال به تحصیل"], Latitude = 35.7010, Longitude = 51.3950
				},
				[
					("اتاق دو نفره", 2, 2_800_000m, 12_000_000m, 16, 1, [TagDormRoom.Double, TagDormRoom.Furnished, TagDormRoom.Desk, TagDormRoom.Heating]),
					("اتاق چهارنفره", 4, 1_800_000m, 8_000_000m, 26, 2, [TagDormRoom.Dorm, TagDormRoom.Desk, TagDormRoom.Wardrobe])
				]),

			("خوابگاه دخترانه‌ی نسیم شیراز", "117044", "شیراز، بلوار ارم، خیابان دانشجو", "07132500500",
				[
					TagDorm.Girls, TagDorm.Active, TagDorm.Approved,
					TagDorm.Bachelor, TagDorm.Master,
					TagDorm.Wifi, TagDorm.Shuttle, TagDorm.SelfService, TagDorm.Garden, TagDorm.Supervisor, TagDorm.Cctv, TagDorm.SecurityGuard, TagDorm.SharedKitchen, TagDorm.StudyRoom,
					TagDorm.Lunch, TagDorm.Dinner,
					TagDorm.InternetIncluded, TagDorm.UtilitiesIncluded, TagDorm.CleaningIncluded
				],
				new DormJson {
					Highlights = ["سرویس رفت‌وآمد رایگان", "سلف‌سرویس ناهار و شام", "حیاط و فضای سبز"], Instagram = "nasim_dorm", CurfewTime = "22:30", MinimumStayMonths = 6,
					Policies = "ودیعه + اجاره‌ی ماهانه. ودیعه پس از تسویه مسترد می‌شود. فسخ پیش از پایان ترم با معرفی جایگزین. ورود خانواده در ساعات ۱۶ تا ۱۹.",
					UniversityWalkMinutes = 20, HowToGetThere = "ایستگاه اتوبوس دانشگاه شیراز مقابل درب ورودی است.",
					Nearby = [new PlaceNearby { Title = "دانشگاه شیراز", DistanceMeters = 1500, Minutes = 20 }, new PlaceNearby { Title = "ایستگاه اتوبوس", DistanceMeters = 50, Minutes = 1 }],
					Faqs = [new PlaceFaq { Question = "سرویس رفت‌وآمد چند بار در روز است؟", Answer = "دو بار: صبح و عصر؛ رایگان برای ساکنین." }],
					Description = "ویژه‌ی خواهران با ورودی مستقل، حیاط، سلف‌سرویس و سرویس رفت‌وآمد رایگان تا دانشگاه.", NearbyUniversity = "دانشگاه شیراز", VisitingHours = "۱۶ تا ۱۹",
					Rules = ["رعایت پوشش و شئونات", "ورود تا ساعت ۲۲:۳۰"], RequiredDocuments = ["کارت ملی", "گواهی اشتغال به تحصیل", "معرفی‌نامه‌ی دانشگاه"], Latitude = 29.6400, Longitude = 52.5250
				},
				[
					("اتاق سه نفره", 3, 1_900_000m, 10_000_000m, 22, 1, [TagDormRoom.Dorm, TagDormRoom.Furnished, TagDormRoom.PrivateBathroom, TagDormRoom.Desk, TagDormRoom.Wardrobe]),
					("اتاق دو نفره", 2, 2_400_000m, 10_000_000m, 16, 2, [TagDormRoom.Double, TagDormRoom.Furnished, TagDormRoom.PrivateBathroom, TagDormRoom.Desk, TagDormRoom.AirConditioning])
				]),

			("خوابگاه پسرانه‌ی دانا", "101013", "تبریز، خیابان دانشگاه، کوچه‌ی سوم", "04133600600",
				[
					TagDorm.Boys, TagDorm.Active, TagDorm.PendingApproval,
					TagDorm.Bachelor,
					TagDorm.Wifi, TagDorm.SharedKitchen, TagDorm.BikeParking, TagDorm.Laundry, TagDorm.Lockers,
					TagDorm.InternetIncluded, TagDorm.UtilitiesIncluded
				],
				new DormJson {
					Highlights = ["هزینه‌ی شارژ و اینترنت در اجاره", "آشپزخانه‌ی بزرگ مرکزی"], MinimumStayMonths = 3,
					Policies = "اجاره‌ی ماهانه. ودیعه‌ی ۸ میلیون تومان.", UniversityWalkMinutes = 12,
					Nearby = [new PlaceNearby { Title = "دانشگاه تبریز", DistanceMeters = 900, Minutes = 12 }],
					Description = "ارزان‌ترین گزینه‌ی ما با آشپزخانه‌ی بزرگ و پارکینگ دوچرخه؛ شارژ و اینترنت در اجاره لحاظ شده است.", NearbyUniversity = "دانشگاه تبریز", VisitingHours = "۱۷ تا ۲۰",
					Rules = ["سکوت پس از ۲۳"], RequiredDocuments = ["کارت ملی"], Latitude = 38.0800, Longitude = 46.3200
				},
				[
					("اتاق چهارنفره", 4, 1_500_000m, 8_000_000m, 26, 1, [TagDormRoom.Dorm, TagDormRoom.Desk]),
					("اتاق دو نفره", 2, 2_000_000m, 8_000_000m, 15, 2, [TagDormRoom.Double, TagDormRoom.Desk, TagDormRoom.Heating])
				]),

			// Built without the Active tag to show the "hidden" state: the site and the app do not list it.
			("خوابگاه دخترانه‌ی آرام کرج", "105009", "کرج، گوهردشت، فاز ۳", "02634700700",
				[TagDorm.Girls, TagDorm.Inactive, TagDorm.Bachelor, TagDorm.Wifi, TagDorm.Garden, TagDorm.SharedKitchen],
				new DormJson { Highlights = ["سی تخت", "حیاط بزرگ"], Description = "کوچک‌ترین مجموعه‌ی ما؛ سی تخت، حیاط و سکوت.", NearbyUniversity = "دانشگاه آزاد کرج", Latitude = 35.83, Longitude = 50.93 },
				[("اتاق دو نفره", 2, 1_600_000m, 7_000_000m, 18, 1, [TagDormRoom.Double, TagDormRoom.Desk])])
		];

		int dormIndex = 0;
		int residentIndex = 0;
		foreach (var d in dormSeeds) {
			Guid dormId = Guid.CreateVersion7();
			dorms.Add(new DormEntity {
				Id = dormId, CreatedAt = now.AddDays(-85), CreatorId = adminId, Tags = d.Tags, JsonData = d.Json,
				Title = d.Title, CityCode = d.City, Address = d.Address, PhoneNumber = d.Phone
			});

			int roomNumber = 0;
			foreach (var r in d.Rooms) {
				roomNumber++;
				Guid roomId = Guid.CreateVersion7();
				dormRooms.Add(new DormRoomEntity {
					Id = roomId, CreatedAt = now.AddDays(-84), CreatorId = adminId, DormId = dormId, Title = r.Title, Capacity = r.Beds,
					Tags = r.Tags,
					JsonData = new DormRoomJson { Description = $"{r.Title}؛ با میز مطالعه و کمد شخصی برای هر نفر.", Floor = r.Floor, SizeSquareMeters = r.Size }
				});

				for (int b = 0; b < r.Beds; b++) {
					Guid bedId = Guid.CreateVersion7();
					beds.Add(new DormBedEntity {
						Id = bedId, CreatedAt = now.AddDays(-83), CreatorId = adminId, RoomId = roomId, Title = $"{(char)('A' + roomNumber - 1)}{b + 1}", Deposit = r.Deposit, MonthlyRent = r.Rent,
						Tags = r.Beds > 2
							? [TagDormBed.Single, b % 2 == 0 ? TagDormBed.BunkBottom : TagDormBed.BunkTop, TagDormBed.Locker, TagDormBed.ReadingLamp, TagDormBed.PowerOutlet]
							: [TagDormBed.Single, TagDormBed.Desk, TagDormBed.Locker, TagDormBed.PrivacyCurtain, TagDormBed.PowerOutlet],
						JsonData = new DormBedJson { Description = "تخت با تشک طبی." }
					});

					// pattern per bed: 0 = active contract, 1 = expired contract, 2 = free (no contract); inactive dorms have none
					int pattern = (b + roomNumber + dormIndex) % 3;
					if (d.Tags.Contains(TagDorm.Inactive) || pattern == 2) continue;

					bool active = pattern == 0;
					DateTime start = active ? today.AddMonths(-2).AddDays(-b) : today.AddMonths(-10);
					DateTime end = active ? today.AddMonths(4) : today.AddMonths(-4);
					Guid userId = UserAt(6 + residentIndex++);
					Guid contractId = Guid.CreateVersion7();
					contracts.Add(new DormBedContractEntity {
						Id = contractId, CreatedAt = start.AddDays(-5), CreatorId = adminId, Tags = [TagDormBedContract.Monthly],
						StartDate = start, EndDate = end, Deposit = r.Deposit, Rent = r.Rent, UserId = userId, BedId = bedId, JsonData = new DormBedContractJson()
					});
					dormInvoices.Add(new DormBedInvoiceEntity {
						Id = Guid.CreateVersion7(), CreatedAt = start.AddDays(-5), CreatorId = adminId, Tags = [TagDormBedInvoice.Deposit, TagDormBedInvoice.Paid, TagDormBedInvoice.PaidOnline],
						DebtAmount = r.Deposit, CreditorAmount = 0, PaidAmount = r.Deposit, PenaltyAmount = 0, ContractId = contractId, DueDate = start, JsonData = new DormBedInvoiceJson { PenaltyPrecentEveryDate = 1 }
					});
					bool lateResident = active && b == 0; // the first active resident of every room is late with the last rent
					for (int m = 0; start.AddMonths(m) < end; m++) {
						DateTime due = start.AddMonths(m);
						bool inPast = due < today;
						bool late = lateResident && inPast && due.AddMonths(1) >= today;
						bool paid = inPast && !late;
						decimal penalty = late ? 10000 : 0;
						dormInvoices.Add(new DormBedInvoiceEntity {
							Id = Guid.CreateVersion7(), CreatedAt = due.AddDays(-7), CreatorId = adminId,
							Tags = paid ? [TagDormBedInvoice.Rent, TagDormBedInvoice.Paid, m % 2 == 0 ? TagDormBedInvoice.PaidOnline : TagDormBedInvoice.PaidManual] : [TagDormBedInvoice.Rent, TagDormBedInvoice.NotPaid],
							DebtAmount = r.Rent, CreditorAmount = 0, PaidAmount = paid ? r.Rent : 0, PenaltyAmount = penalty, ContractId = contractId, DueDate = due,
							JsonData = new DormBedInvoiceJson { PenaltyPrecentEveryDate = 1 }
						});
					}
				}
			}

			if (!d.Tags.Contains(TagDorm.Inactive)) {
				string[] texts = ["نزدیک دانشگاه و امن؛ سرپرست خوبی دارد.", "اینترنت پایدار و اتاق مطالعه‌ی عالی، قیمت منطقی.", "تمیز و آرام است؛ فقط آشپزخانه در ساعات اوج شلوغ می‌شود."];
				for (int i = 0; i < texts.Length; i++) comments.Add(Review(null, dormId, dormIndex * 3 + i + 2, i == 2 ? 4m : 5m - i * 0.5m, texts[i], TagComment.Released, 8 + i * 12));
			}

			dormIndex++;
		}

		await db.Set<UserEntity>().AddRangeAsync(users, ct);
		await db.Set<DormEntity>().AddRangeAsync(dorms, ct);
		await db.Set<DormRoomEntity>().AddRangeAsync(dormRooms, ct);
		await db.Set<DormBedEntity>().AddRangeAsync(beds, ct);
		await db.Set<DormBedContractEntity>().AddRangeAsync(contracts, ct);
		await db.Set<DormBedInvoiceEntity>().AddRangeAsync(dormInvoices, ct);
		await db.Set<CommentEntity>().AddRangeAsync(comments, ct);
		await db.SaveChangesAsync(ct);

		return new UResponse<List<KeyValue>?>([
			new KeyValue { Key = "users", Value = users.Count.ToString() },
			new KeyValue { Key = "dorms", Value = dorms.Count.ToString() },
			new KeyValue { Key = "dormRooms", Value = dormRooms.Count.ToString() },
			new KeyValue { Key = "dormBeds", Value = beds.Count.ToString() },
			new KeyValue { Key = "dormContracts", Value = contracts.Count.ToString() },
			new KeyValue { Key = "dormInvoices", Value = dormInvoices.Count.ToString() },
			new KeyValue { Key = "reviews", Value = comments.Count.ToString() },
			new KeyValue { Key = "demoUsersPassword", Value = "Demo1234 (usernames demo01 ... demo12)" }
		], Usc.Created);
	}

	public async Task<bool> IsResidentOf(Guid userId, Guid placeId, CancellationToken ct) {
		DateTime now = DateTime.UtcNow;
		return await db.Set<DormBedContractEntity>().AnyAsync(x => x.UserId == userId && x.Bed.Room.DormId == placeId && x.EndDate >= now && !x.Tags.Contains(TagDormBedContract.Settled), ct);
	}

	private Task<DormBedContractEntity?> ActiveContract(Guid userId, Guid dormId, DateTime date, CancellationToken ct) =>
		db.Set<DormBedContractEntity>().AsTracking()
			.Include(x => x.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm)
			.Where(x => x.UserId == userId && x.Bed.Room.DormId == dormId && x.StartDate <= date && x.EndDate >= date && !x.Tags.Contains(TagDormBedContract.Settled))
			.OrderByDescending(x => x.StartDate)
			.FirstOrDefaultAsync(ct);

	private static List<HandoverItem> Handover(IEnumerable<HandoverItemParams>? items) => (items ?? []).Select(x => new HandoverItem {
		Title = x.Title,
		Ok = x.Ok,
		Note = x.Note,
		Damage = Math.Max(0, x.Damage),
		PhotoUrls = x.PhotoUrls
	}).ToList();

	private async Task<(JwtClaimData? User, DormEntity? Dorm, bool Manager, UResponse? Error)> DormAccess(string? token, Guid dormId, TagUser permission, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(token);
		if (u == null) return (null, null, false, new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue")));
		if (u.IsExpired) return (null, null, false, new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired")));
		DormEntity? dorm = await db.Set<DormEntity>().FirstOrDefaultAsync(x => x.Id == dormId, ct);
		if (dorm == null) return (u, null, false, new UResponse(Usc.NotFound, ls.Get("dormNotFound")));
		return (u, dorm, await CanAct(u, dorm, permission, ct), null);
	}

	private IQueryable<T> ScopeToDorms<T>(IQueryable<T> q, JwtClaimData? u, Expression<Func<T, bool>> mine, Expression<Func<T, ICollection<Guid>>> admins) {
		if (OrganizationService.IsFull(u)) return q;
		Guid uid = UserIdOf(u);
		bool scoped = OrganizationService.IsScopedAdmin(u);
		ParameterExpression x = mine.Parameters[0];
		Expression adminCheck = Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), [typeof(Guid)], new ParameterReplacer(admins.Parameters[0], x).Visit(admins.Body), Expression.Constant(uid));
		Expression body = scoped ? Expression.OrElse(mine.Body, adminCheck) : mine.Body;
		return q.Where(Expression.Lambda<Func<T, bool>>(body, x));
	}

	private sealed class ParameterReplacer(ParameterExpression from, ParameterExpression to) : ExpressionVisitor {
		protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : base.VisitParameter(node);
	}

	public async Task<UResponse> SetDormBedContractChecklist(DormBedContractChecklistParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		DormBedContractEntity? e = await ContractForChange(p.ContractId, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("contractNotFound"));
		if (!await CanAct(userData, e.Bed.Room.Dorm, TagUser.PermissionManageContracts, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		List<HandoverItem> items = Handover(p.Items);
		if (!p.CheckOut) e.JsonData.CheckInChecklist = items;
		else {
			e.JsonData.CheckOutChecklist = items;
			decimal damage = items.Sum(x => x.Damage);
			DormBedInvoiceEntity? invoice = e.Invoices.FirstOrDefault(x => x.Id == e.JsonData.DamageInvoiceId);
			if (invoice == null && damage > 0) {
				invoice = NewDormBedInvoice(e, userData.Id, [TagDormBedInvoice.NotPaid, TagDormBedInvoice.Damage], damage, DateTime.UtcNow, 0);
				invoice.JsonData.Detail1 = ls.Get("handoverDamage", "fa");
				invoice.Contract = e;
				await db.Set<DormBedInvoiceEntity>().AddAsync(invoice, ct);
				e.JsonData.DamageInvoiceId = invoice.Id;
				await SyncDormBedInvoice(invoice, false, ct);
				await AddNotification(e.UserId, TagNotification.InvoiceIssued, ls.Get("newInvoicesIssued"), e.Bed.Room.Dorm.Title, ct);
			}
			else if (invoice != null && invoice.Tags.Contains(TagDormBedInvoice.NotPaid) && invoice.PaidAmount == 0) {
				invoice.DebtAmount = damage;
				await SyncDormBedInvoice(invoice, damage == 0, ct);
				if (damage == 0) {
					db.Set<DormBedInvoiceEntity>().Remove(invoice);
					e.JsonData.DamageInvoiceId = null;
				}
			}
		}

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateDormApplication(DormApplicationCreateParams p, CancellationToken ct) {
		(JwtClaimData? u, DormEntity? dorm, bool manager, UResponse? error) = await DormAccess(p.Token, p.DormId, TagUser.PermissionManageContracts, ct);
		if (error != null) return new UResponse<Guid?>(null, error.Status, error.Message);
		Guid userId = p.UserId ?? u!.Id;
		if (userId != u!.Id && !manager) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (!await db.Set<UserEntity>().AnyAsync(x => x.Id == userId, ct)) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("accountNotFound"));
		if (await os.IsBlacklisted(dorm!.OrganizationId, userId, ct)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youCannotApplyToThisPlace"));
		if (await db.Set<DormApplicationEntity>().AnyAsync(x => x.DormId == dorm.Id && x.UserId == userId && (x.Tags.Contains(TagDormApplication.Pending) || x.Tags.Contains(TagDormApplication.Waitlisted)), ct))
			return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("youAlreadyHaveAnOpenApplication"));

		Guid id = Guid.CreateVersion7();
		await db.Set<DormApplicationEntity>().AddAsync(new DormApplicationEntity {
			Id = id,
			CreatorId = userId,
			CreatedAt = DateTime.UtcNow,
			Tags = [TagDormApplication.Pending],
			DesiredStartDate = p.DesiredStartDate,
			DesiredEndDate = p.DesiredEndDate,
			DormId = dorm.Id,
			UserId = userId,
			JsonData = new DormApplicationJson {
				Detail1 = p.Detail1,
				Documents = p.Documents.Select(x => new DormApplicationDocument { Title = x.Title, Url = x.Url }).ToList()
			}
		}, ct);
		foreach (Guid admin in dorm.AdminUserIds) await AddNotification(admin, TagNotification.General, ls.Get("newDormApplication"), dorm.Title, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<DormApplicationResponse>?>> ReadDormApplications(DormApplicationReadParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u == null) return new UResponse<IEnumerable<DormApplicationResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		Guid uid = u.Id;
		IQueryable<DormApplicationEntity> q = db.Set<DormApplicationEntity>().ApplyReadParams(p);
		q = p.Mine ? q.Where(x => x.UserId == uid) : ScopeToDorms(q, u, x => x.UserId == uid, x => x.Dorm.AdminUserIds);
		if (p.DormId != null) q = q.Where(x => x.DormId == p.DormId);
		if (p.UserId != null) q = q.Where(x => x.UserId == p.UserId);

		return await q.Select(x => new DormApplicationResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Tags = x.Tags,
			JsonData = x.JsonData,
			DesiredStartDate = x.DesiredStartDate,
			DesiredEndDate = x.DesiredEndDate,
			DormId = x.DormId,
			DormTitle = x.Dorm.Title,
			UserId = x.UserId,
			UserName = x.User.FirstName + " " + x.User.LastName,
			PhoneNumber = x.User.PhoneNumber,
			WaitlistPosition = x.Tags.Contains(TagDormApplication.Waitlisted)
				? db.Set<DormApplicationEntity>().Count(y => y.DormId == x.DormId && y.Tags.Contains(TagDormApplication.Waitlisted) && y.CreatedAt < x.CreatedAt) + 1
				: null
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> ReviewDormApplication(DormApplicationReviewParams p, CancellationToken ct) {
		DormApplicationEntity? e = await db.Set<DormApplicationEntity>().AsTracking().Include(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		(JwtClaimData? u, _, bool manager, UResponse? error) = await DormAccess(p.Token, e.DormId, TagUser.PermissionManageContracts, ct);
		if (error != null) return error;
		if (!manager) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (p.Status is not (TagDormApplication.Approved or TagDormApplication.Rejected or TagDormApplication.Waitlisted or TagDormApplication.Pending))
			return new UResponse(Usc.BadRequest, ls.Get("tagsIsRequired"));

		e.Tags = [p.Status];
		e.JsonData.ReviewNote = p.ReviewNote;
		e.JsonData.ReviewedBy = u!.Id;
		e.JsonData.ReviewedAt = DateTime.UtcNow;
		if (p.BedId != null) e.JsonData.BedId = p.BedId;
		if (p.DocumentApprovals != null)
			for (int i = 0; i < Math.Min(p.DocumentApprovals.Count, e.JsonData.Documents.Count); i++)
				e.JsonData.Documents[i].Approved = p.DocumentApprovals[i];
		e.JsonData.Documents = e.JsonData.Documents.ToList();

		string key = p.Status switch {
			TagDormApplication.Approved => "dormApplicationApproved",
			TagDormApplication.Rejected => "dormApplicationRejected",
			TagDormApplication.Waitlisted => "dormApplicationWaitlisted",
			_ => "dormApplicationUpdated"
		};
		await AddNotification(e.UserId, TagNotification.General, ls.Get(key), e.Dorm.Title, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteDormApplication(IdParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		DormApplicationEntity? e = await db.Set<DormApplicationEntity>().Include(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		if (!(e.UserId == u.Id && e.Tags.Contains(TagDormApplication.Pending)) && !await CanAct(u, e.Dorm, TagUser.PermissionManageContracts, ct))
			return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		db.Set<DormApplicationEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateDormRecord(DormRecordCreateParams p, CancellationToken ct) {
		(JwtClaimData? u, DormEntity? dorm, bool manager, UResponse? error) = await DormAccess(p.Token, p.DormId, TagUser.PermissionManageContracts, ct);
		if (error != null) return new UResponse<Guid?>(null, error.Status, error.Message);

		TagDormRecord kind = p.Tags.FirstOrDefault(x => (int)x < 300, TagDormRecord.Announcement);
		Guid? userId = p.UserId;
		List<TagDormRecord> tags;
		if (manager) tags = [kind, ..p.Tags.Where(x => (int)x >= 300).Take(1)];
		else {
			if (kind != TagDormRecord.NightLeave && kind != TagDormRecord.Visitor) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
			if (!await IsResidentOf(u!.Id, dorm!.Id, ct)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
			userId = u.Id;
			tags = [kind, TagDormRecord.Pending];
		}

		if (kind is TagDormRecord.CheckIn or TagDormRecord.CheckOut or TagDormRecord.NightLeave or TagDormRecord.Violation or TagDormRecord.Warning && userId == null)
			return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("userIsRequired"));

		DateTime date = p.Date ?? DateTime.UtcNow;
		DormRecordEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatorId = u!.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = tags,
			Title = p.Title,
			Date = date,
			EndDate = p.EndDate,
			DormId = dorm!.Id,
			UserId = userId,
			JsonData = new DormRecordJson {
				Detail1 = p.Detail1,
				Detail2 = p.Detail2,
				Body = p.Body,
				VisitorName = p.VisitorName,
				VisitorPhone = p.VisitorPhone,
				VisitorNationalCode = p.VisitorNationalCode,
				Relation = p.Relation,
				RoomId = p.RoomId,
				Penalty = manager ? p.Penalty : null,
				Items = Handover(p.Items)
			}
		};
		await db.Set<DormRecordEntity>().AddAsync(e, ct);

		if (manager && kind == TagDormRecord.Announcement) {
			DateTime now = DateTime.UtcNow;
			List<Guid> residents = await db.Set<DormBedContractEntity>()
				.Where(x => x.Bed.Room.DormId == dorm.Id && x.StartDate <= now && x.EndDate >= now && !x.Tags.Contains(TagDormBedContract.Settled))
				.Select(x => x.UserId).Distinct().ToListAsync(ct);
			foreach (Guid r in residents) await AddNotification(r, TagNotification.General, p.Title, dorm.Title, ct);
		}

		if (manager && userId != null && kind is TagDormRecord.Violation or TagDormRecord.Warning) {
			await AddNotification(userId.Value, TagNotification.General, $"{ls.Get(kind == TagDormRecord.Violation ? "violationRecorded" : "warningRecorded")}: {p.Title}", dorm.Title, ct);
			if (p.Penalty > 0) {
				DormBedContractEntity? contract = await ActiveContract(userId.Value, dorm.Id, date, ct);
				if (contract != null) {
					DormBedInvoiceEntity invoice = NewDormBedInvoice(contract, u.Id, [TagDormBedInvoice.NotPaid, TagDormBedInvoice.Damage], p.Penalty.Value, date, 0);
					invoice.JsonData.Detail1 = p.Title;
					invoice.Contract = contract;
					await db.Set<DormBedInvoiceEntity>().AddAsync(invoice, ct);
					await SyncDormBedInvoice(invoice, false, ct);
					e.JsonData.InvoiceId = invoice.Id;
				}
			}
		}

		if (!manager)
			foreach (Guid admin in dorm.AdminUserIds) await AddNotification(admin, TagNotification.General, ls.Get(kind == TagDormRecord.NightLeave ? "newLeaveRequest" : "newVisitorRequest"), dorm.Title, ct);

		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<DormRecordResponse>?>> ReadDormRecords(DormRecordReadParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u == null) return new UResponse<IEnumerable<DormRecordResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		Guid uid = u.Id;
		DateTime now = DateTime.UtcNow;
		IQueryable<DormRecordEntity> q = db.Set<DormRecordEntity>().ApplyReadParams(p);
		Expression<Func<DormRecordEntity, bool>> mine = x => x.UserId == uid || x.Tags.Contains(TagDormRecord.Announcement) && db.Set<DormBedContractEntity>().Any(c => c.UserId == uid && c.Bed.Room.DormId == x.DormId && c.EndDate >= now);
		q = p.Mine ? q.Where(mine) : ScopeToDorms(q, u, mine, x => x.Dorm.AdminUserIds);
		if (p.DormId != null) q = q.Where(x => x.DormId == p.DormId);
		if (p.UserId != null) q = q.Where(x => x.UserId == p.UserId);
		if (p.FromDate != null) q = q.Where(x => x.Date >= p.FromDate);
		if (p.ToDate != null) q = q.Where(x => x.Date <= p.ToDate);

		return await q.OrderByDescending(x => x.Date).Select(x => new DormRecordResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Title = x.Title,
			Date = x.Date,
			EndDate = x.EndDate,
			DormId = x.DormId,
			DormTitle = x.Dorm.Title,
			UserId = x.UserId,
			UserName = x.User == null ? null : x.User.FirstName + " " + x.User.LastName
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateDormRecord(DormRecordUpdateParams p, CancellationToken ct) {
		DormRecordEntity? e = await db.Set<DormRecordEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		(JwtClaimData? u, DormEntity? dorm, bool manager, UResponse? error) = await DormAccess(p.Token, e.DormId, TagUser.PermissionManageContracts, ct);
		if (error != null) return error;
		if (!manager) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		bool wasPending = e.Tags.Contains(TagDormRecord.Pending);
		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title!;
		if (p.Date != null) e.Date = p.Date.Value;
		if (p.EndDate != null) e.EndDate = p.EndDate;
		if (p.Body != null) e.JsonData.Body = p.Body;
		if (p.VisitorName != null) e.JsonData.VisitorName = p.VisitorName;
		if (p.VisitorPhone != null) e.JsonData.VisitorPhone = p.VisitorPhone;
		if (p.Items != null) e.JsonData.Items = Handover(p.Items);
		List<TagDormRecord> status = [..p.Tags ?? p.AddTags ?? []];
		if (status.Any(x => (int)x >= 300)) {
			e.Tags = [..e.Tags.Where(x => (int)x < 300), status.First(x => (int)x >= 300)];
			e.JsonData.ReviewedBy = u!.Id;
		}

		if (wasPending && e.UserId != null && !e.Tags.Contains(TagDormRecord.Pending))
			await AddNotification(e.UserId.Value, TagNotification.General, ls.Get(e.Tags.Contains(TagDormRecord.Approved) ? "yourRequestIsApproved" : "yourRequestIsRejected"), dorm!.Title, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteDormRecord(IdParams p, CancellationToken ct) {
		DormRecordEntity? e = await db.Set<DormRecordEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		(JwtClaimData? u, _, bool manager, UResponse? error) = await DormAccess(p.Token, e.DormId, TagUser.PermissionManageContracts, ct);
		if (error != null) return error;
		if (!manager && !(e.UserId == u!.Id && e.Tags.Contains(TagDormRecord.Pending))) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		db.Set<DormRecordEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateDormMeal(DormMealCreateParams p, CancellationToken ct) {
		(_, DormEntity? dorm, bool manager, UResponse? error) = await DormAccess(p.Token, p.DormId, TagUser.PermissionManageDorms, ct);
		if (error != null) return new UResponse<Guid?>(null, error.Status, error.Message);
		if (!manager) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		Guid id = Guid.CreateVersion7();
		await db.Set<DormMealEntity>().AddAsync(new DormMealEntity {
			Id = id,
			CreatorId = p.CreatorId ?? ts.ExtractClaims(p.Token)!.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = p.Tags,
			Title = p.Title,
			Date = p.Date,
			Price = Math.Max(0, p.Price),
			Capacity = p.Capacity,
			DormId = dorm!.Id,
			JsonData = new BaseJson { Detail1 = p.Detail1, Detail2 = p.Detail2 }
		}, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<DormMealResponse>?>> ReadDormMeals(DormMealReadParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u == null) return new UResponse<IEnumerable<DormMealResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		Guid uid = u.Id;
		IQueryable<DormMealEntity> q = db.Set<DormMealEntity>().ApplyReadParams(p);
		if (p.DormId != null) q = q.Where(x => x.DormId == p.DormId);
		if (p.FromDate != null) q = q.Where(x => x.Date >= p.FromDate);
		if (p.ToDate != null) q = q.Where(x => x.Date <= p.ToDate);
		return await q.OrderBy(x => x.Date).Select(x => new DormMealResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Title = x.Title,
			Date = x.Date,
			Price = x.Price,
			Capacity = x.Capacity,
			ReservedCount = x.Bookings.Count(b => b.Tags.Contains(TagDormBooking.Reserved)),
			ReservedByMe = x.Bookings.Any(b => b.UserId == uid && b.Tags.Contains(TagDormBooking.Reserved)),
			DormId = x.DormId
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateDormMeal(DormMealUpdateParams p, CancellationToken ct) {
		DormMealEntity? e = await db.Set<DormMealEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		(_, _, bool manager, UResponse? error) = await DormAccess(p.Token, e.DormId, TagUser.PermissionManageDorms, ct);
		if (error != null) return error;
		if (!manager) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title!;
		if (p.Date != null) e.Date = p.Date.Value;
		if (p.Price != null) e.Price = Math.Max(0, p.Price.Value);
		if (p.Capacity != null) e.Capacity = p.Capacity;
		e.ApplyUpdateParam<DormMealEntity, TagDormMeal, BaseJson>(p);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteDormMeal(IdParams p, CancellationToken ct) {
		DormMealEntity? e = await db.Set<DormMealEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		(_, _, bool manager, UResponse? error) = await DormAccess(p.Token, e.DormId, TagUser.PermissionManageDorms, ct);
		if (error != null) return error;
		if (!manager) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (await db.Set<DormBookingEntity>().AnyAsync(x => x.MealId == e.Id && x.Tags.Contains(TagDormBooking.Reserved), ct)) return new UResponse(Usc.Conflict, ls.Get("itemIsInUse"));
		db.Set<DormMealEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateDormBooking(DormBookingCreateParams p, CancellationToken ct) {
		(JwtClaimData? u, DormEntity? dorm, bool manager, UResponse? error) = await DormAccess(p.Token, p.DormId, TagUser.PermissionManageContracts, ct);
		if (error != null) return new UResponse<Guid?>(null, error.Status, error.Message);
		Guid userId = p.UserId ?? u!.Id;
		if (userId != u!.Id && !manager) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		bool meal = p.MealId != null || p.Tags.Contains(TagDormBooking.Meal);
		DormBookingEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatorId = userId,
			CreatedAt = DateTime.UtcNow,
			Tags = [meal ? TagDormBooking.Meal : TagDormBooking.Laundry, TagDormBooking.Reserved],
			StartAt = p.StartAt ?? DateTime.UtcNow,
			EndAt = p.EndAt,
			Resource = p.Resource,
			DormId = dorm!.Id,
			UserId = userId,
			MealId = p.MealId,
			JsonData = new DormBookingJson { Detail1 = p.Detail1, Detail2 = p.Detail2 }
		};

		if (meal) {
			DormMealEntity? m = await db.Set<DormMealEntity>().FirstOrDefaultAsync(x => x.Id == p.MealId && x.DormId == dorm.Id, ct);
			if (m == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("itemNotFound"));
			if (!manager && m.Date.Date < DateTime.UtcNow.Date) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("thisMealIsPast"));
			List<DormBookingEntity> booked = await db.Set<DormBookingEntity>().Where(x => x.MealId == m.Id && x.Tags.Contains(TagDormBooking.Reserved)).ToListAsync(ct);
			if (booked.Any(x => x.UserId == userId)) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("youAlreadyReservedThis"));
			if (m.Capacity != null && booked.Count >= m.Capacity) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("capacityIsFull"));
			DormBedContractEntity? contract = await ActiveContract(userId, dorm.Id, m.Date, ct);
			if (contract == null) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youHaveNoActiveContractHere"));
			e.StartAt = m.Date;
			e.Price = m.Price;
			if (m.Price > 0) {
				DormBedInvoiceEntity invoice = NewDormBedInvoice(contract, u.Id, [TagDormBedInvoice.NotPaid, TagDormBedInvoice.Service], m.Price, m.Date, 0);
				invoice.JsonData.Detail1 = m.Title;
				invoice.Contract = contract;
				await db.Set<DormBedInvoiceEntity>().AddAsync(invoice, ct);
				await SyncDormBedInvoice(invoice, false, ct);
				e.JsonData.InvoiceId = invoice.Id;
			}
		}
		else {
			if (p.StartAt == null) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("dateIsRequired"));
			if (!manager && !await IsResidentOf(userId, dorm.Id, ct)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youHaveNoActiveContractHere"));
			if (dorm.JsonData.LaundryMachines.Count != 0 && !dorm.JsonData.LaundryMachines.Contains(p.Resource ?? "")) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("selectALaundryMachine"));
			e.EndAt = p.EndAt ?? e.StartAt.AddMinutes(dorm.JsonData.LaundrySlotMinutes ?? 60);
			if (e.EndAt <= e.StartAt) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("endDateMustBeAfterStartDate"));
			DateTime start = e.StartAt;
			DateTime end = e.EndAt.Value;
			if (await db.Set<DormBookingEntity>().AnyAsync(x => x.DormId == dorm.Id && x.Tags.Contains(TagDormBooking.Laundry) && x.Tags.Contains(TagDormBooking.Reserved) && x.Resource == p.Resource && x.StartAt < end && x.EndAt > start, ct))
				return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("thisTimeIsAlreadyBooked"));
		}

		await db.Set<DormBookingEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<DormBookingResponse>?>> ReadDormBookings(DormBookingReadParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u == null) return new UResponse<IEnumerable<DormBookingResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		Guid uid = u.Id;
		IQueryable<DormBookingEntity> q = db.Set<DormBookingEntity>().ApplyReadParams(p);
		q = p.Mine ? q.Where(x => x.UserId == uid) : ScopeToDorms(q, u, x => x.UserId == uid, x => x.Dorm.AdminUserIds);
		if (p.DormId != null) q = q.Where(x => x.DormId == p.DormId);
		if (p.UserId != null) q = q.Where(x => x.UserId == p.UserId);
		if (p.MealId != null) q = q.Where(x => x.MealId == p.MealId);
		if (p.FromDate != null) q = q.Where(x => x.StartAt >= p.FromDate);
		if (p.ToDate != null) q = q.Where(x => x.StartAt <= p.ToDate);
		return await q.OrderBy(x => x.StartAt).Select(x => new DormBookingResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Tags = x.Tags,
			JsonData = x.JsonData,
			StartAt = x.StartAt,
			EndAt = x.EndAt,
			Resource = x.Resource,
			Price = x.Price,
			DormId = x.DormId,
			UserId = x.UserId,
			UserName = x.User.FirstName + " " + x.User.LastName,
			MealId = x.MealId,
			MealTitle = x.Meal == null ? null : x.Meal.Title
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> CancelDormBooking(IdParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		DormBookingEntity? e = await db.Set<DormBookingEntity>().AsTracking().Include(x => x.Dorm).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		bool manager = await CanAct(u, e.Dorm, TagUser.PermissionManageContracts, ct);
		if (!manager && (e.UserId != u.Id || e.StartAt <= DateTime.UtcNow)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (!e.Tags.Contains(TagDormBooking.Reserved)) return new UResponse(Usc.Conflict, ls.Get("thisBookingCannotBeCancelled"));

		e.Tags = [..e.Tags.Where(x => (int)x < 200), TagDormBooking.Cancelled];
		if (e.JsonData.InvoiceId != null) {
			DormBedInvoiceEntity? invoice = await db.Set<DormBedInvoiceEntity>().AsTracking()
				.Include(x => x.Contract).ThenInclude(x => x!.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm)
				.FirstOrDefaultAsync(x => x.Id == e.JsonData.InvoiceId, ct);
			if (invoice != null && invoice.Tags.Contains(TagDormBedInvoice.NotPaid) && invoice.PaidAmount == 0) {
				await SyncDormBedInvoice(invoice, true, ct);
				db.Set<DormBedInvoiceEntity>().Remove(invoice);
			}
		}

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<string?>> PrintDormBedContract(IdParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u == null) return new UResponse<string?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		DormBedContractEntity? e = await db.Set<DormBedContractEntity>()
			.Include(x => x.User).Include(x => x.Invoices)
			.Include(x => x.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm)
			.FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<string?>(null, Usc.NotFound, ls.Get("contractNotFound"));
		if (e.UserId != u.Id && !await CanAct(u, e.Bed.Room.Dorm, TagUser.PermissionManageContracts, ct)) return new UResponse<string?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		DormEntity dorm = e.Bed.Room.Dorm;
		OrganizationEntity? organization = await os.ReadOrganization(dorm.OrganizationId, ct);
		string html = PrintTemplate.Build(organization, dorm.Title, ls.Get("dormContractTitle", "fa"), [
				(ls.Get("resident", "fa"), $"{e.User.FirstName} {e.User.LastName}"),
				(ls.Get("nationalCode", "fa"), e.User.NationalCode),
				(ls.Get("phoneNumber", "fa"), e.User.PhoneNumber),
				(ls.Get("dorm", "fa"), dorm.Title),
				(ls.Get("address", "fa"), dorm.Address),
				(ls.Get("roomAndBed", "fa"), $"{e.Bed.Room.Title} / {e.Bed.Title}"),
				(ls.Get("startDate", "fa"), PrintTemplate.Date(e.StartDate)),
				(ls.Get("endDate", "fa"), PrintTemplate.Date(e.EndDate)),
				(ls.Get("monthlyRent", "fa"), PrintTemplate.Money(e.Rent)),
				(ls.Get("deposit", "fa"), PrintTemplate.Money(e.Deposit)),
				(ls.Get("guardian", "fa"), string.Join(" - ", new[] { e.JsonData.GuardianName, e.JsonData.GuardianPhone }.Where(x => x.IsNotNullOrEmpty()))),
				(ls.Get("emergencyContact", "fa"), string.Join(" - ", new[] { e.JsonData.EmergencyName, e.JsonData.EmergencyRelation, e.JsonData.EmergencyPhone }.Where(x => x.IsNotNullOrEmpty())))
			],
			[ls.Get("dueDate", "fa"), ls.Get("amount", "fa"), ls.Get("status", "fa")],
			e.Invoices.OrderBy(x => x.DueDate).Select(i => (IReadOnlyList<string>)[PrintTemplate.Date(i.DueDate), PrintTemplate.Money(i.DebtAmount + i.PenaltyAmount - i.CreditorAmount), ls.Get(i.Tags.Contains(TagDormBedInvoice.NotPaid) ? "notPaid" : "paid", "fa")]),
			[..dorm.JsonData.Rules, dorm.JsonData.Policies ?? ""],
			[ls.Get("residentSignature", "fa"), ls.Get("organizationSignature", "fa")]);
		return new UResponse<string?>(html);
	}

	public async Task<UResponse<string?>> PrintDormBedInvoice(IdParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u == null) return new UResponse<string?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		DormBedInvoiceEntity? e = await db.Set<DormBedInvoiceEntity>()
			.Include(x => x.Contract).ThenInclude(x => x!.User)
			.Include(x => x.Contract).ThenInclude(x => x!.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm)
			.FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e?.Contract == null) return new UResponse<string?>(null, Usc.NotFound, ls.Get("invoiceNotFound"));
		if (e.Contract.UserId != u.Id && !await CanAct(u, e.Contract.Bed.Room.Dorm, TagUser.PermissionManageInvoices, ct)) return new UResponse<string?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		DormEntity dorm = e.Contract.Bed.Room.Dorm;
		OrganizationEntity? organization = await os.ReadOrganization(dorm.OrganizationId, ct);
		bool paid = !e.Tags.Contains(TagDormBedInvoice.NotPaid);
		string html = PrintTemplate.Build(organization, dorm.Title, ls.Get(paid ? "paymentReceipt" : "invoice", "fa"), [
			(ls.Get("resident", "fa"), $"{e.Contract.User.FirstName} {e.Contract.User.LastName}"),
			(ls.Get("roomAndBed", "fa"), $"{e.Contract.Bed.Room.Title} / {e.Contract.Bed.Title}"),
			(ls.Get("description", "fa"), e.JsonData.Detail1),
			(ls.Get("dueDate", "fa"), PrintTemplate.Date(e.DueDate)),
			(ls.Get("amount", "fa"), PrintTemplate.Money(e.DebtAmount)),
			(ls.Get("discount", "fa"), e.CreditorAmount > 0 ? PrintTemplate.Money(e.CreditorAmount) : null),
			(ls.Get("penalty", "fa"), e.PenaltyAmount > 0 ? PrintTemplate.Money(e.PenaltyAmount) : null),
			(ls.Get("paidAmount", "fa"), PrintTemplate.Money(e.PaidAmount)),
			(ls.Get("remaining", "fa"), PrintTemplate.Money(Math.Max(0, DueOf(e)))),
			(ls.Get("status", "fa"), ls.Get(paid ? "paid" : "notPaid", "fa"))
		], signatures: [ls.Get("organizationSignature", "fa")]);
		return new UResponse<string?>(html);
	}
}

public sealed class DormReminderService(IServiceScopeFactory scopeFactory) : BackgroundService {
	private bool _failureLogged;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
		using PeriodicTimer timer = new(TimeSpan.FromHours(1));
		try {
			do {
				try {
					using IServiceScope scope = scopeFactory.CreateScope();
					await scope.ServiceProvider.GetRequiredService<IDormService>().ProcessDueInvoices(stoppingToken);
				}
				catch (OperationCanceledException) {
					throw;
				}
				catch (Exception e) {
					if (!_failureLogged) ULog.Error(e, "Dorm invoice reminders are off (are the dorm tables migrated?)");
					_failureLogged = true;
				}
			} while (await timer.WaitForNextTickAsync(stoppingToken));
		}
		catch (OperationCanceledException) {
		}
	}
}
