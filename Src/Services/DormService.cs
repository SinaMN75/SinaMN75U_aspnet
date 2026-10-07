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
}

public class DormService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IWalletService ws,
	IOrganizationService os,
	IAccountingService acc,
	IDataSeedService seeds
) : IDormService, IAccountingSource, IUserScope {
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
				Longitude = p.Longitude
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
			JsonData = new DormBedContractJson(),
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
		if (!Core.App.MultiTenant || dorm?.OrganizationId == null) return;

		DateTime now = DateTime.UtcNow;
		bool open = e.Tags.Contains(TagDormBedInvoice.NotPaid);
		bool charged = !removed && (e.DueDate <= now || !open || e.PaidAmount > 0);
		if (!charged && !e.JsonData.Posted) return;

		decimal debt = charged ? Math.Max(0, e.DebtAmount - e.CreditorAmount) : 0;
		bool single = e.Contract!.Tags.Contains(TagDormBedContract.SingleInvoice) && !e.Tags.Contains(TagDormBedInvoice.Rent) && !e.Tags.Contains(TagDormBedInvoice.Service);
		decimal deposit = e.Tags.Contains(TagDormBedInvoice.Deposit) ? debt : single ? Math.Min(e.Contract.Deposit, debt) : 0;
		Dictionary<TagAccount, decimal> target = new() {
			[TagAccount.DepositsHeld] = deposit,
			[e.Tags.Contains(TagDormBedInvoice.Service) ? TagAccount.ServiceIncome : TagAccount.RentIncome] = debt - deposit,
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

		if (Core.App.MultiTenant) {
			List<DormBedInvoiceEntity> dormInvoices = await db.Set<DormBedInvoiceEntity>().AsTracking()
				.Include(x => x.Contract).ThenInclude(x => x!.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm)
				.Where(x => !x.JsonData.Posted && x.DueDate <= now && x.Contract != null && x.Contract.Bed.Room.Dorm.OrganizationId != null)
				.ToListAsync(ct);
			foreach (DormBedInvoiceEntity e in dormInvoices) await SyncDormBedInvoice(e, false, ct);
		}

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
