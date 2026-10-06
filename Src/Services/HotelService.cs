namespace SinaMN75U.Services;

public interface IHotelService {
	public Task<bool> HasOrganizationPermission(JwtClaimData u, Guid? organizationId, TagUser permission, CancellationToken ct);
	public Task<bool?> CanActOnPlaceOf(JwtClaimData u, Guid? hotelId, Guid? hotelRoomId, Guid? dormId, Guid? dormRoomId, Guid? dormBedId, CancellationToken ct);
	public IQueryable<UserEntity> RelatedUsers(IQueryable<UserEntity> q, Guid userId);
	public Task<UResponse<List<KeyValue>?>> SeedHotelsAndDorms(CancellationToken ct = default);
	public Task<UResponse<Guid?>> CreateOrganization(OrganizationCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<OrganizationResponse>?>> ReadOrganizations(OrganizationReadParams p, CancellationToken ct);
	public Task<UResponse> UpdateOrganization(OrganizationUpdateParams p, CancellationToken ct);
	public Task<UResponse> SetOrganizationMember(OrganizationMemberParams p, CancellationToken ct);
	public Task<UResponse> RemoveOrganizationMember(OrganizationMemberParams p, CancellationToken ct);

	public Task<UResponse<Guid?>> CreateHotel(HotelCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<HotelResponse>?>> ReadHotels(HotelReadParams p, CancellationToken ct);
	public Task<UResponse<HotelResponse?>> ReadHotelById(IdParams<HotelSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> UpdateHotel(HotelUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteHotel(IdParams p, CancellationToken ct);

	public Task<UResponse<Guid?>> CreateHotelRoom(HotelRoomCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<HotelRoomResponse>?>> ReadHotelRooms(HotelRoomReadParams p, CancellationToken ct);
	public Task<UResponse<HotelRoomResponse?>> ReadHotelRoomById(IdParams<HotelRoomSelectorArgs> p, CancellationToken ct);
	public Task<UResponse<IEnumerable<HotelRoomAvailabilityResponse>?>> ReadHotelRoomAvailability(HotelRoomAvailabilityParams p, CancellationToken ct);
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
	public Task<UResponse<HotelReservationResponse?>> BookHotelReservation(HotelReservationBookParams p, CancellationToken ct);
	public Task<UResponse> CancelHotelReservationByUser(HotelReservationCancelParams p, CancellationToken ct);

	public Task<UResponse<Guid?>> CreateHotelInvoice(HotelInvoiceCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<HotelInvoiceResponse>?>> ReadHotelInvoices(HotelInvoiceReadParams p, CancellationToken ct);
	public Task<UResponse> UpdateHotelInvoice(HotelInvoiceUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteHotelInvoice(IdParams p, CancellationToken ct);
	public Task<UResponse> PayHotelInvoice(IdParams p, CancellationToken ct);
	public Task<UResponse> PayHotelInvoiceInternal(HotelInvoicePayParams p, CancellationToken ct);

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
	public Task<UResponse<PropertyDashboardResponse?>> ReadPropertyDashboard(DashboardRangeParams p, CancellationToken ct);
	public Task<UResponse> SettleDormBedContract(DormBedContractSettleParams p, CancellationToken ct);
	public Task<UResponse> RenewDormBedContract(DormBedContractRenewParams p, CancellationToken ct);
	public Task<UResponse> TransferDormBedContract(DormBedContractTransferParams p, CancellationToken ct);
	public Task<UResponse> SplitDormBedInvoice(DormBedInvoiceSplitParams p, CancellationToken ct);
	public Task<UResponse> RequestOrganizationSettlement(OrganizationSettlementRequestParams p, CancellationToken ct);
	public Task<UResponse> ProcessOrganizationSettlement(OrganizationSettlementProcessParams p, CancellationToken ct);
	public Task ProcessDueInvoices(CancellationToken ct);
	public Task<UResponse<Guid?>> CreateAccount(AccountCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<AccountResponse>?>> ReadAccounts(AccountReadParams p, CancellationToken ct);
	public Task<UResponse> UpdateAccount(AccountUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteAccount(IdParams p, CancellationToken ct);
	public Task<UResponse<Guid?>> CreateVoucher(VoucherCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<VoucherResponse>?>> ReadVouchers(VoucherReadParams p, CancellationToken ct);
	public Task<UResponse> DeleteVoucher(IdParams p, CancellationToken ct);
	public Task<UResponse<LedgerResponse?>> ReadLedger(LedgerReadParams p, CancellationToken ct);
	public Task<UResponse<LedgerReportResponse?>> ReadLedgerReport(LedgerReportParams p, CancellationToken ct);
	public Task<UResponse<Guid?>> CreateCheck(CheckCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<CheckResponse>?>> ReadChecks(CheckReadParams p, CancellationToken ct);
	public Task<UResponse> SetCheckStatus(CheckStatusParams p, CancellationToken ct);
	public Task<UResponse> ReceiveDormBedInvoice(InvoiceReceiveParams p, CancellationToken ct);
	public Task<UResponse> ReceiveHotelInvoice(InvoiceReceiveParams p, CancellationToken ct);
}

public class HotelService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IWalletService ws
) : IHotelService {
	// ---- Who sees and changes what ----
	// SuperAdmin/SystemAdmin: everything.
	// SubAdmin: only the hotels/dorms whose AdminUserIds holds his id (a SuperAdmin puts it there) and everything under them;
	//           what he may change there is decided by his Permission tags.
	// Everybody else: the public view (active places, no people data) and his own reservations/contracts/invoices.
	private static bool IsFull(JwtClaimData? u) => u is { IsSuperAdmin: true };

	private static bool IsScopedAdmin(JwtClaimData? u) => !IsFull(u) && (u is { IsSubAdmin: true } || Core.App.MultiTenant && u?.Tags.Contains(TagUser.SuperAdmin) == true);

	private Task<bool> CanAct(JwtClaimData u, HotelEntity place, TagUser permission, CancellationToken ct) => CanActOnPlace(u, place.OrganizationId, place.AdminUserIds, permission, ct);

	private Task<bool> CanAct(JwtClaimData u, DormEntity place, TagUser permission, CancellationToken ct) => CanActOnPlace(u, place.OrganizationId, place.AdminUserIds, permission, ct);

	private async Task<bool> CanCreatePlace(JwtClaimData u, Guid? organizationId, TagUser permission, CancellationToken ct) =>
		Core.App.MultiTenant ? u.IsSystemAdmin || organizationId != null && await HasOrganizationPermission(u, organizationId, permission, ct) : u.HasPermission(permission);

	private async Task<bool> IsOwner(JwtClaimData u, Guid? organizationId, CancellationToken ct) =>
		organizationId != null && await db.Set<OrganizationEntity>().AnyAsync(x => x.Id == organizationId && x.OwnerId == u.Id, ct);

	private async Task<List<Guid>> PlaceAdmins(Guid? organizationId, ICollection<Guid> ids, CancellationToken ct) {
		if (!Core.App.MultiTenant || organizationId == null) return ids.ToList();
		OrganizationEntity? o = await db.Set<OrganizationEntity>().FirstOrDefaultAsync(x => x.Id == organizationId, ct);
		return o == null ? [] : ids.Where(o.AdminUserIds.Contains).Append(o.OwnerId).Distinct().ToList();
	}

	private static Guid MoneyAccountOf(Guid? organizationId) => Core.App.MultiTenant && organizationId != null ? organizationId.Value : Core.App.Users.SystemAdmin.Id;

	private async Task TakeCommission(Guid? organizationId, decimal amount, string detail, List<KeyValue> keyValues, Guid sourceId, Guid? placeId, CancellationToken ct) {
		if (!Core.App.MultiTenant || organizationId == null) return;
		decimal percent = await db.Set<OrganizationEntity>().Where(x => x.Id == organizationId).Select(x => x.JsonData.CommissionPercent).FirstOrDefaultAsync(ct);
		decimal commission = Math.Round(amount * percent / 100);
		if (commission <= 0) return;
		await ws.Transfer(new WalletTransferParams {
			SenderId = organizationId.Value,
			ReceiverId = Core.App.Users.SystemAdmin.Id,
			Amount = commission,
			Detail1 = detail,
			KeyValues = keyValues,
			TagWalletTxn = [TagWalletTxn.PlatformCommission],
			AllowOverdraft = true
		}, ct);
		await Post(organizationId, TagVoucher.Commission, sourceId, placeId, null, ls.Get("platformCommission", "fa"), DateTime.UtcNow, ct, new Leg(TagAccount.CommissionExpense, commission), new Leg(TagAccount.Wallet, -commission));
	}

	private static Guid UserIdOf(JwtClaimData? u) => u?.Id ?? Guid.Empty;

	private static bool TouchesAdminUserIds<T>(BaseUpdateParams<T> p) => p.AdminUserIds.IsNotNullOrEmpty() || p.AddAdminUserIds.IsNotNullOrEmpty() || p.RemoveAdminUserIds.IsNotNullOrEmpty();

	// Selector args are cleaned for everyone but full admins: never Creator (admin accounts), users only with their own fields,
	// and reservations/contracts only for a place admin (people = true) whose rows are already limited to his places.
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

	public async Task<UResponse<Guid?>> CreateOrganization(OrganizationCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!Core.App.MultiTenant || !userData.IsSystemAdmin) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		UserEntity? owner = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.OwnerId, ct);
		if (owner == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("accountNotFound"));
		if (!SetFirstAdminPassword(owner, p.OwnerPassword)) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("pleaseEnterAPassword"));
		if (!owner.Tags.Contains(TagUser.SuperAdmin)) owner.Tags = [..owner.Tags, TagUser.SuperAdmin];

		Guid id = p.Id ?? Guid.CreateVersion7();
		DateTime now = DateTime.UtcNow;
		await db.Set<UserEntity>().AddAsync(new UserEntity {
			Id = id,
			CreatorId = userData.Id,
			CreatedAt = now,
			UserName = "organization_" + id.ToString("N"),
			Password = UPasswordHasher.Hash(Guid.NewGuid().ToString()),
			RefreshToken = "",
			FirstName = p.Title,
			JsonData = new UserJson(),
			Tags = [TagUser.Organization],
			Wallets = [new WalletEntity { Id = id, CreatorId = id, CreatedAt = now, JsonData = new WalletJson(), Tags = [TagWallet.Primary], Balance = 0 }]
		}, ct);
		await db.Set<OrganizationEntity>().AddAsync(new OrganizationEntity {
			Id = id,
			CreatorId = userData.Id,
			CreatedAt = now,
			Title = p.Title,
			OwnerId = owner.Id,
			Tags = p.Tags,
			JsonData = new OrganizationJson { Detail1 = p.Detail1, Detail2 = p.Detail2, CommissionPercent = p.CommissionPercent }
		}, ct);
		await AccountsOf(id, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<OrganizationResponse>?>> ReadOrganizations(OrganizationReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<OrganizationResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<OrganizationResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		Guid uid = userData.Id;
		IQueryable<OrganizationEntity> q = db.Set<OrganizationEntity>().ApplyReadParams(p);
		if (!IsFull(userData)) q = q.Where(x => x.OwnerId == uid || x.AdminUserIds.Contains(uid));
		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title!));

		return await q.Select(x => new OrganizationResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Tags = x.Tags,
			JsonData = x.JsonData,
			AdminUserIds = x.AdminUserIds,
			Title = x.Title,
			OwnerId = x.OwnerId,
			Balance = db.Set<WalletEntity>().Where(w => w.CreatorId == x.Id).Sum(w => (decimal?)w.Balance) ?? 0
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateOrganization(OrganizationUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (!Core.App.MultiTenant || TouchesAdminUserIds(p)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		OrganizationEntity? e = await db.Set<OrganizationEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("organizationNotFound"));

		bool platformChange = p.OwnerId.HasValue && p.OwnerId != e.OwnerId || p.CommissionPercent.HasValue || p.Tags != null || p.AddTags != null || p.RemoveTags != null;
		if (!userData.IsSystemAdmin && (e.OwnerId != userData.Id || platformChange)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		if (p.OwnerId.HasValue && p.OwnerId != e.OwnerId) {
			UserEntity? owner = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.OwnerId, ct);
			if (owner == null) return new UResponse(Usc.NotFound, ls.Get("accountNotFound"));
			if (!SetFirstAdminPassword(owner, p.OwnerPassword)) return new UResponse(Usc.BadRequest, ls.Get("pleaseEnterAPassword"));
			if (!owner.Tags.Contains(TagUser.SuperAdmin)) owner.Tags = [..owner.Tags, TagUser.SuperAdmin];

			Guid oldOwnerId = e.OwnerId;
			e.OwnerId = owner.Id;
			await ReplacePlaceAdmin(e.Id, oldOwnerId, owner.Id, ct);
			UserEntity? oldOwner = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == oldOwnerId, ct);
			if (oldOwner != null && !await db.Set<OrganizationEntity>().AnyAsync(x => x.Id != e.Id && x.OwnerId == oldOwnerId, ct))
				oldOwner.Tags = oldOwner.Tags.Where(x => x != TagUser.SuperAdmin).ToList();
		}

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		if (p.CommissionPercent.HasValue) e.JsonData.CommissionPercent = p.CommissionPercent.Value;
		e.ApplyUpdateParam<OrganizationEntity, TagOrganization, OrganizationJson>(p);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> SetOrganizationMember(OrganizationMemberParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!Core.App.MultiTenant) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		OrganizationEntity? e = await db.Set<OrganizationEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.OrganizationId, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("organizationNotFound"));
		if (!userData.IsSystemAdmin && e.OwnerId != userData.Id || p.UserId == e.OwnerId) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		UserEntity? user = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.UserId, ct);
		if (user == null) return new UResponse(Usc.NotFound, ls.Get("accountNotFound"));
		if (user.Tags.Contains(TagUser.SystemAdmin) || user.Tags.Contains(TagUser.Organization)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (!SetFirstAdminPassword(user, p.Password)) return new UResponse(Usc.BadRequest, ls.Get("pleaseEnterAPassword"));

		OrganizationMember member = new() { UserId = user.Id, Permissions = p.Permissions.Where(x => (int)x is >= 600 and < 700).Distinct().ToList() };
		e.JsonData.Members = e.JsonData.Members.Where(x => x.UserId != user.Id).Append(member).ToList();
		if (!e.AdminUserIds.Contains(user.Id)) e.AdminUserIds = [..e.AdminUserIds, user.Id];
		await SyncMemberTags(user, e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> RemoveOrganizationMember(OrganizationMemberParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (!Core.App.MultiTenant) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		OrganizationEntity? e = await db.Set<OrganizationEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.OrganizationId, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("organizationNotFound"));
		if (!userData.IsSystemAdmin && e.OwnerId != userData.Id || p.UserId == e.OwnerId) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		e.JsonData.Members = e.JsonData.Members.Where(x => x.UserId != p.UserId).ToList();
		e.AdminUserIds = e.AdminUserIds.Where(x => x != p.UserId).ToList();
		await ReplacePlaceAdmin(e.Id, p.UserId, null, ct);
		UserEntity? user = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.UserId, ct);
		if (user != null) await SyncMemberTags(user, e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	private static bool SetFirstAdminPassword(UserEntity user, string? password) {
		if (JwtClaimData.Rank(user.Tags) > 0) return true;
		if (password.IsNullOrEmpty()) return false;
		user.Password = UPasswordHasher.Hash(password);
		return true;
	}

	private static List<Guid> SwapId(ICollection<Guid> ids, Guid oldId, Guid? newId) {
		List<Guid> list = ids.Where(x => x != oldId).ToList();
		if (newId != null && !list.Contains(newId.Value)) list.Add(newId.Value);
		return list;
	}

	private async Task ReplacePlaceAdmin(Guid organizationId, Guid oldId, Guid? newId, CancellationToken ct) {
		foreach (HotelEntity x in await db.Set<HotelEntity>().AsTracking().Where(x => x.OrganizationId == organizationId).ToListAsync(ct)) x.AdminUserIds = SwapId(x.AdminUserIds, oldId, newId);
		foreach (DormEntity x in await db.Set<DormEntity>().AsTracking().Where(x => x.OrganizationId == organizationId).ToListAsync(ct)) x.AdminUserIds = SwapId(x.AdminUserIds, oldId, newId);
	}

	private async Task SyncMemberTags(UserEntity user, OrganizationEntity changed, CancellationToken ct) {
		List<OrganizationEntity> organizations = await db.Set<OrganizationEntity>().Where(x => x.Id != changed.Id && x.AdminUserIds.Contains(user.Id)).ToListAsync(ct);
		if (changed.AdminUserIds.Contains(user.Id)) organizations.Add(changed);
		List<TagUser> tags = user.Tags.Where(x => x != TagUser.SubAdmin && (int)x is < 600 or >= 700).ToList();
		if (organizations.Count > 0) tags = [..tags, TagUser.SubAdmin, ..organizations.SelectMany(x => x.JsonData.Members.Where(m => m.UserId == user.Id).SelectMany(m => m.Permissions)).Distinct()];
		user.Tags = tags;
	}

	public async Task<UResponse<Guid?>> CreateHotel(HotelCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!await CanCreatePlace(userData, p.OrganizationId, TagUser.PermissionManageHotels, ct)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		HotelEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = IsFull(userData) ? p.CreatorId ?? userData.Id : userData.Id,
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
			AdminUserIds = await PlaceAdmins(p.OrganizationId, IsFull(userData) ? p.AdminUserIds ?? [] : [userData.Id], ct)
		};

		await db.Set<HotelEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<HotelResponse>?>> ReadHotels(HotelReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<HotelEntity> q = db.Set<HotelEntity>().ApplyReadParams(p);
		Guid uid = UserIdOf(userData);
		if (IsScopedAdmin(userData)) q = q.Where(x => x.AdminUserIds.Contains(uid));
		else if (!IsFull(userData)) q = q.Where(x => x.Tags.Contains(TagHotel.Active));
		if (!IsFull(userData)) p.SelectorArgs = Safe(p.SelectorArgs, IsScopedAdmin(userData));

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
		if (IsScopedAdmin(userData)) hotels = hotels.Where(x => x.AdminUserIds.Contains(uid));
		else if (!IsFull(userData)) hotels = hotels.Where(x => x.Tags.Contains(TagHotel.Active));
		if (!IsFull(userData)) p.SelectorArgs = Safe(p.SelectorArgs, IsScopedAdmin(userData));
		HotelResponse? e = await hotels.Select(Projections.HotelSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<HotelResponse?>(null, Usc.NotFound, ls.Get("hotelNotFound")) : new UResponse<HotelResponse?>(e);
	}

	public async Task<UResponse> UpdateHotel(HotelUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		HotelEntity? e = await db.Set<HotelEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("hotelNotFound"));

		if (!await CanAct(userData, e, TagUser.PermissionManageHotels, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (TouchesAdminUserIds(p) && !IsFull(userData) && !(Core.App.MultiTenant && await IsOwner(userData, e.OrganizationId, ct))) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (p.OrganizationId.HasValue && p.OrganizationId != e.OrganizationId) {
			if (!IsFull(userData)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
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
		if (Core.App.MultiTenant) e.AdminUserIds = await PlaceAdmins(e.OrganizationId, e.AdminUserIds, ct);
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
		if (IsScopedAdmin(userData)) q = q.Where(x => x.Hotel.AdminUserIds.Contains(uid));
		else if (!IsFull(userData)) q = q.Where(x => x.Hotel.Tags.Contains(TagHotel.Active) && x.IsAvailable);
		if (!IsFull(userData)) p.SelectorArgs = Safe(p.SelectorArgs, IsScopedAdmin(userData));

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
		if (IsScopedAdmin(userData)) q = q.Where(x => x.Hotel.AdminUserIds.Contains(uid));
		else if (!IsFull(userData)) q = q.Where(x => x.Hotel.Tags.Contains(TagHotel.Active));
		if (!IsFull(userData)) p.SelectorArgs = Safe(p.SelectorArgs, IsScopedAdmin(userData));

		HotelRoomResponse? e = await q.Select(Projections.HotelRoomSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<HotelRoomResponse?>(null, Usc.NotFound, ls.Get("hotelRoomNotFound")) : new UResponse<HotelRoomResponse?>(e);
	}

	public async Task<UResponse> UpdateHotelRoom(HotelRoomUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		HotelRoomEntity? e = await db.Set<HotelRoomEntity>().AsTracking().Include(x => x.Hotel).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("hotelRoomNotFound"));

		if (!await CanAct(userData, e.Hotel, TagUser.PermissionManageHotels, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		// Moving it under another hotel/dorm needs the same right there.
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
		if (!IsFull(userData)) {
			Guid uid = UserIdOf(userData);
			bool scoped = IsScopedAdmin(userData);
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
		if (!IsFull(userData)) {
			Guid uid = UserIdOf(userData);
			bool scoped = IsScopedAdmin(userData);
			q = q.Where(x => x.UserId == uid || scoped && x.Hotel.AdminUserIds.Contains(uid));
			p.SelectorArgs = Safe(p.SelectorArgs);
		}

		HotelReservationResponse? e = await q.Select(Projections.HotelReservationSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<HotelReservationResponse?>(null, Usc.NotFound, ls.Get("reservationNotFound")) : new UResponse<HotelReservationResponse?>(e);
	}

	public async Task<UResponse> UpdateHotelReservation(HotelReservationUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		HotelReservationEntity? e = await db.Set<HotelReservationEntity>().AsTracking().Include(x => x.Hotel).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("reservationNotFound"));
		if (!await CanAct(userData, e.Hotel, TagUser.PermissionManageReservations, ct))
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

		e.Tags = [status];
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public Task<UResponse> ConfirmHotelReservation(IdParams p, CancellationToken ct) => TransitionReservation(p, TagHotelReservation.Confirmed, ct);
	public Task<UResponse> CheckInHotelReservation(IdParams p, CancellationToken ct) => TransitionReservation(p, TagHotelReservation.CheckedIn, ct);
	public Task<UResponse> CheckOutHotelReservation(IdParams p, CancellationToken ct) => TransitionReservation(p, TagHotelReservation.CheckedOut, ct);
	public Task<UResponse> CancelHotelReservation(IdParams p, CancellationToken ct) => TransitionReservation(p, TagHotelReservation.Cancelled, ct);

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
			JsonData = new NotificationJson { Detail1 = title, Detail2 = body }
		}, ct);

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
		if (!isOwner && !await CanAct(userData, e.Hotel, TagUser.PermissionManageReservations, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (e.Tags.Contains(TagHotelReservation.Cancelled)) return new UResponse(Usc.Conflict, ls.Get("thisReservationHasAlreadyBeenCancelled"));
		if (e.Tags.Contains(TagHotelReservation.CheckedIn) || e.Tags.Contains(TagHotelReservation.CheckedOut)) return new UResponse(Usc.Conflict, ls.Get("aReservationThatHasAlreadyBeenCheckedInCannotBeCancelled"));

		double hoursToCheckIn = (e.CheckInDate - DateTime.UtcNow).TotalHours;
		decimal penalty = hoursToCheckIn >= e.Hotel.JsonData.CancellationFreeHours
			? 0
			: Math.Min(e.TotalPrice, e.Hotel.JsonData.CancellationPenaltyNights * e.Room.PricePerNight);

		decimal paid = e.Invoices.Sum(x => x.PaidAmount);
		decimal refund = Math.Max(0, paid - penalty);

		// Claim the cancellation atomically first so two concurrent cancel requests can't both refund.
		List<TagHotelReservation> previousTags = e.Tags.ToList();
		List<TagHotelReservation> cancelledTags = [TagHotelReservation.Cancelled];
		int claimed = await db.Set<HotelReservationEntity>()
			.Where(x => x.Id == e.Id && !x.Tags.Contains(TagHotelReservation.Cancelled) && !x.Tags.Contains(TagHotelReservation.CheckedIn) && !x.Tags.Contains(TagHotelReservation.CheckedOut))
			.ExecuteUpdateAsync(u => u.SetProperty(x => x.Tags, cancelledTags), ct);
		if (claimed == 0) return new UResponse(Usc.Conflict, ls.Get("thisReservationHasAlreadyBeenCancelled"));

		if (refund > 0) {
			UResponse<WalletTxnResponse?> transfer = await ws.Transfer(new WalletTransferParams {
				SenderId = MoneyAccountOf(e.Hotel.OrganizationId),
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
		await Post(e.Hotel.OrganizationId, TagVoucher.Refund, e.Id, e.HotelId, e.UserId, $"{ls.Get("hotelReservationRefund", "fa")} - {e.Hotel.Title}", DateTime.UtcNow, ct,
			new Leg(TagAccount.Receivable, refund), new Leg(TagAccount.Wallet, -refund));

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

	public async Task<UResponse> PayHotelInvoiceInternal(HotelInvoicePayParams p, CancellationToken ct) {
		HotelInvoiceEntity? e = await db.Set<HotelInvoiceEntity>().AsTracking()
			.Include(x => x.Reservation).ThenInclude(x => x!.Hotel)
			.Include(x => x.Reservation).ThenInclude(x => x!.Room)
			.FirstOrDefaultAsync(x => x.Id == p.InvoiceId, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("invoiceNotFound"));
		if (!e.Tags.Contains(TagHotelInvoice.NotPaid)) return new UResponse(Usc.Conflict, ls.Get("thisInvoiceHasAlreadyBeenPaid"));

		// Claim NotPaid→PaidOnline atomically first so concurrent payments of the same invoice can't charge twice.
		List<TagHotelInvoice> unpaidTags = e.Tags.ToList();
		List<TagHotelInvoice> paidTags = [TagHotelInvoice.PaidOnline];
		int claimed = await db.Set<HotelInvoiceEntity>().Where(x => x.Id == e.Id && x.Tags.Contains(TagHotelInvoice.NotPaid)).ExecuteUpdateAsync(u => u.SetProperty(x => x.Tags, paidTags), ct);
		if (claimed == 0) return new UResponse(Usc.Conflict, ls.Get("thisInvoiceHasAlreadyBeenPaid"));

		decimal amount = e.DebtAmount + e.PenaltyAmount - e.CreditorAmount - e.PaidAmount;
		if (amount > 0) {
			UResponse<WalletTxnResponse?> transfer = await ws.Transfer(new WalletTransferParams {
				SenderId = p.UserId,
				ReceiverId = MoneyAccountOf(e.Reservation?.Hotel.OrganizationId),
				Amount = amount,
				Detail1 = ls.Get("hotelReservationPayment"),
				KeyValues = HotelInvoiceKeyValues(e),
				TagWalletTxn = [TagWalletTxn.HotelReservation]
			}, ct);
			if (transfer.Result == null) {
				await db.Set<HotelInvoiceEntity>().Where(x => x.Id == e.Id).ExecuteUpdateAsync(u => u.SetProperty(x => x.Tags, unpaidTags), ct);
				return new UResponse(transfer.Status, transfer.Message);
			}
			await TakeCommission(e.Reservation?.Hotel.OrganizationId, amount, ls.Get("hotelReservationPayment"), HotelInvoiceKeyValues(e), e.Id, e.Reservation?.HotelId, ct);
		}

		e.PaidAmount += amount;
		e.Tags = [TagHotelInvoice.PaidOnline];
		await SyncHotelInvoice(e, false, ct);
		await Post(e.Reservation?.Hotel.OrganizationId, TagVoucher.Payment, e.Id, e.Reservation?.HotelId, e.Reservation?.UserId, $"{ls.Get("hotelReservationPayment", "fa")} - {e.Reservation?.Hotel.Title}", DateTime.UtcNow, ct,
			new Leg(TagAccount.Wallet, amount), new Leg(TagAccount.Receivable, -amount));

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
		if (!IsFull(userData)) {
			Guid uid = UserIdOf(userData);
			bool scoped = IsScopedAdmin(userData);
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
		if (!(e.Reservation == null ? IsFull(userData) : await CanAct(userData, e.Reservation.Hotel, TagUser.PermissionManageInvoices, ct)))
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
		if (!(e.Reservation == null ? IsFull(userData) : await CanAct(userData, e.Reservation.Hotel, TagUser.PermissionDeleteInvoices, ct)))
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
	
	public async Task<UResponse<Guid?>> CreateDorm(DormCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!await CanCreatePlace(userData, p.OrganizationId, TagUser.PermissionManageDorms, ct)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		DormEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = IsFull(userData) ? p.CreatorId ?? userData.Id : userData.Id,
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
			AdminUserIds = await PlaceAdmins(p.OrganizationId, IsFull(userData) ? p.AdminUserIds ?? [] : [userData.Id], ct)
		};

		await db.Set<DormEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<DormResponse>?>> ReadDorms(DormReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<DormEntity> q = db.Set<DormEntity>().ApplyReadParams(p);
		Guid uid = UserIdOf(userData);
		if (IsScopedAdmin(userData)) q = q.Where(x => x.AdminUserIds.Contains(uid));
		else if (!IsFull(userData)) q = q.Where(x => x.Tags.Contains(TagDorm.Active));
		if (!IsFull(userData)) p.SelectorArgs = Safe(p.SelectorArgs, IsScopedAdmin(userData));

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
		if (IsScopedAdmin(userData)) dorms = dorms.Where(x => x.AdminUserIds.Contains(uid));
		else if (!IsFull(userData)) dorms = dorms.Where(x => x.Tags.Contains(TagDorm.Active));
		if (!IsFull(userData)) p.SelectorArgs = Safe(p.SelectorArgs, IsScopedAdmin(userData));
		DormResponse? e = await dorms.Select(Projections.DormSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<DormResponse?>(null, Usc.NotFound, ls.Get("dormNotFound")) : new UResponse<DormResponse?>(e);
	}

	public async Task<UResponse> UpdateDorm(DormUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		DormEntity? e = await db.Set<DormEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("dormNotFound"));

		if (!await CanAct(userData, e, TagUser.PermissionManageDorms, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (TouchesAdminUserIds(p) && !IsFull(userData) && !(Core.App.MultiTenant && await IsOwner(userData, e.OrganizationId, ct))) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (p.OrganizationId.HasValue && p.OrganizationId != e.OrganizationId) {
			if (!IsFull(userData)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
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
		if (Core.App.MultiTenant) e.AdminUserIds = await PlaceAdmins(e.OrganizationId, e.AdminUserIds, ct);
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
		if (IsScopedAdmin(userData)) q = q.Where(x => x.Dorm.AdminUserIds.Contains(uid));
		else if (!IsFull(userData)) q = q.Where(x => x.Dorm.Tags.Contains(TagDorm.Active));
		if (!IsFull(userData)) p.SelectorArgs = Safe(p.SelectorArgs, IsScopedAdmin(userData));

		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title!));
		if (p.DormId.HasValue) q = q.Where(x => x.DormId == p.DormId);

		IQueryable<DormRoomResponse> projected = q.Select(Projections.DormRoomSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<DormRoomResponse?>> ReadDormRoomById(IdParams<DormRoomSelectorArgs> p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<DormRoomEntity> q = db.Set<DormRoomEntity>();
		Guid uid = UserIdOf(userData);
		if (IsScopedAdmin(userData)) q = q.Where(x => x.Dorm.AdminUserIds.Contains(uid));
		else if (!IsFull(userData)) q = q.Where(x => x.Dorm.Tags.Contains(TagDorm.Active));
		if (!IsFull(userData)) p.SelectorArgs = Safe(p.SelectorArgs, IsScopedAdmin(userData));

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
		if (IsScopedAdmin(userData)) q = q.Where(x => x.Room.Dorm.AdminUserIds.Contains(uid));
		else if (!IsFull(userData)) q = q.Where(x => x.Room.Dorm.Tags.Contains(TagDorm.Active));
		if (!IsFull(userData)) p.SelectorArgs = Safe(p.SelectorArgs, IsScopedAdmin(userData));

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
		if (IsScopedAdmin(userData)) q = q.Where(x => x.Room.Dorm.AdminUserIds.Contains(uid));
		else if (!IsFull(userData)) q = q.Where(x => x.Room.Dorm.Tags.Contains(TagDorm.Active));
		if (!IsFull(userData)) p.SelectorArgs = Safe(p.SelectorArgs, IsScopedAdmin(userData));

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
		if (!IsFull(userData)) {
			Guid uid = UserIdOf(userData);
			bool scoped = IsScopedAdmin(userData);
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
		if (!IsFull(userData)) {
			Guid uid = UserIdOf(userData);
			bool scoped = IsScopedAdmin(userData);
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
		if (!(e.Contract == null ? IsFull(userData) : await CanAct(userData, e.Contract.Bed.Room.Dorm, TagUser.PermissionManageInvoices, ct)))
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
		if (!(e.Contract == null ? IsFull(userData) : await CanAct(userData, e.Contract.Bed.Room.Dorm, TagUser.PermissionDeleteInvoices, ct)))
			return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		await SyncDormBedInvoice(e, true, ct);
		await db.SaveChangesAsync(ct);
		await db.Set<DormBedInvoiceEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}

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
				ReceiverId = MoneyAccountOf(e.Contract?.Bed.Room.Dorm.OrganizationId),
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
			await TakeCommission(e.Contract?.Bed.Room.Dorm.OrganizationId, commissionBase, ls.Get("dormInvoicePayment"), DormBedInvoiceKeyValues(e), e.Id, e.Contract?.Bed.Room.DormId, ct);
		}

		e.PaidAmount += amount;
		e.Tags = [..e.Tags.Where(x => (int)x < 200), TagDormBedInvoice.PaidOnline];
		await SyncDormBedInvoice(e, false, ct);
		await Post(e.Contract?.Bed.Room.Dorm.OrganizationId, TagVoucher.Payment, e.Id, e.Contract?.Bed.Room.DormId, e.Contract?.UserId, $"{ls.Get("dormInvoicePayment", "fa")} - {e.Contract?.Bed.Room.Dorm.Title} - {e.Contract?.Bed.Title}", DateTime.UtcNow, ct,
			new Leg(TagAccount.Wallet, amount), new Leg(TagAccount.Receivable, -amount));
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
		if (!IsFull(userData)) {
			Guid uid = userData.Id;
			bool scoped = IsScopedAdmin(userData);
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

	public async Task<UResponse<PropertyDashboardResponse?>> ReadPropertyDashboard(DashboardRangeParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<PropertyDashboardResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<PropertyDashboardResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		bool tenant = Core.App.MultiTenant && !userData.IsSystemAdmin;
		if (tenant ? !await HasOrganizationPermission(userData, null, TagUser.PermissionViewDashboard, ct) : !userData.IsSuperAdmin)
			return new UResponse<PropertyDashboardResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		Guid uid = userData.Id;
		IQueryable<UserEntity> users = tenant ? RelatedUsers(db.Set<UserEntity>(), uid) : db.Set<UserEntity>();
		IQueryable<HotelEntity> hotels = db.Set<HotelEntity>().Where(x => !tenant || x.AdminUserIds.Contains(uid));
		IQueryable<HotelRoomEntity> hotelRooms = db.Set<HotelRoomEntity>().Where(x => !tenant || x.Hotel.AdminUserIds.Contains(uid));
		IQueryable<HotelReservationEntity> reservations = db.Set<HotelReservationEntity>().Where(x => !tenant || x.Hotel.AdminUserIds.Contains(uid));
		IQueryable<DormEntity> dorms = db.Set<DormEntity>().Where(x => !tenant || x.AdminUserIds.Contains(uid));
		IQueryable<DormRoomEntity> dormRooms = db.Set<DormRoomEntity>().Where(x => !tenant || x.Dorm.AdminUserIds.Contains(uid));
		IQueryable<DormBedEntity> dormBeds = db.Set<DormBedEntity>().Where(x => !tenant || x.Room.Dorm.AdminUserIds.Contains(uid));
		IQueryable<DormBedContractEntity> contracts = db.Set<DormBedContractEntity>().Where(x => !tenant || x.Bed.Room.Dorm.AdminUserIds.Contains(uid));
		IQueryable<DormBedInvoiceEntity> invoices = db.Set<DormBedInvoiceEntity>().Where(x => !tenant || x.Contract != null && x.Contract.Bed.Room.Dorm.AdminUserIds.Contains(uid));

		DateTime now = DateTime.UtcNow;
		DateTime to = p.ToDate ?? now;
		DateTime from = p.FromDate ?? to.AddDays(-30);
		DateTime soon = now.AddDays(30);

		int usersCount = await users.CountAsync(ct);
		int newUsersCount = await users.CountAsync(x => x.CreatedAt >= from && x.CreatedAt <= to, ct);

		int hotelsCount = await hotels.CountAsync(ct);
		int hotelRoomsCount = await hotelRooms.CountAsync(ct);
		int hotelRoomsOccupiedCount = await reservations
			.Where(x => x.CheckInDate <= now && x.CheckOutDate > now)
			.Where(x => !x.Tags.Contains(TagHotelReservation.Cancelled) && !x.Tags.Contains(TagHotelReservation.NoShow) && !x.Tags.Contains(TagHotelReservation.CheckedOut))
			.Select(x => x.RoomId).Distinct().CountAsync(ct);
		int hotelRoomsAvailableCount = hotelRoomsCount - hotelRoomsOccupiedCount;

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

		List<PropertyBreakdownItem> hotelsByCity = await hotels
			.GroupBy(x => x.CityCode)
			.Select(g => new PropertyBreakdownItem { Name = g.Key, Count = g.Count() })
			.OrderByDescending(x => x.Count).Take(10).ToListAsync(ct);

		List<PropertyBreakdownItem> dormsByCity = await dorms
			.GroupBy(x => x.CityCode)
			.Select(g => new PropertyBreakdownItem { Name = g.Key, Count = g.Count() })
			.OrderByDescending(x => x.Count).Take(10).ToListAsync(ct);

		return new UResponse<PropertyDashboardResponse?>(new PropertyDashboardResponse {
			GeneratedAt = DateTime.UtcNow,
			UsersCount = usersCount,
			NewUsersCount = newUsersCount,
			HotelsCount = hotelsCount,
			HotelRoomsCount = hotelRoomsCount,
			HotelRoomsAvailableCount = hotelRoomsAvailableCount,
			HotelRoomsOccupiedCount = hotelRoomsOccupiedCount,
			HotelOccupancyRate = hotelRoomsCount == 0 ? 0 : Math.Round(hotelRoomsOccupiedCount * 100.0 / hotelRoomsCount, 1),
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
			RecentUsers = recentUsers,
			HotelsByCity = hotelsByCity,
			DormsByCity = dormsByCity
		});
	}

	private static bool CanActOnPlace(JwtClaimData u, ICollection<Guid> placeAdminUserIds, TagUser permission) => u.HasPermission(permission) && (u.IsSuperAdmin || u.IsSubAdmin && placeAdminUserIds.Contains(u.Id));

	private async Task<(ICollection<Guid> AdminUserIds, Guid? OrganizationId, TagUser Permission)?> PlaceOf(Guid? hotelId, Guid? hotelRoomId, Guid? dormId, Guid? dormRoomId, Guid? dormBedId, CancellationToken ct) {
		if (hotelId != null || hotelRoomId != null) {
			var hotel = hotelId != null
				? await db.Set<HotelEntity>().Where(x => x.Id == hotelId).Select(x => new { x.AdminUserIds, x.OrganizationId }).FirstOrDefaultAsync(ct)
				: await db.Set<HotelRoomEntity>().Where(x => x.Id == hotelRoomId).Select(x => new { x.Hotel.AdminUserIds, x.Hotel.OrganizationId }).FirstOrDefaultAsync(ct);
			return (hotel?.AdminUserIds ?? [], hotel?.OrganizationId, TagUser.PermissionManageHotels);
		}

		if (dormId == null && dormRoomId == null && dormBedId == null) return null;
		var dorm = dormId != null
			? await db.Set<DormEntity>().Where(x => x.Id == dormId).Select(x => new { x.AdminUserIds, x.OrganizationId }).FirstOrDefaultAsync(ct)
			: dormRoomId != null
				? await db.Set<DormRoomEntity>().Where(x => x.Id == dormRoomId).Select(x => new { x.Dorm.AdminUserIds, x.Dorm.OrganizationId }).FirstOrDefaultAsync(ct)
				: await db.Set<DormBedEntity>().Where(x => x.Id == dormBedId).Select(x => new { x.Room.Dorm.AdminUserIds, x.Room.Dorm.OrganizationId }).FirstOrDefaultAsync(ct);
		return (dorm?.AdminUserIds ?? [], dorm?.OrganizationId, TagUser.PermissionManageDorms);
	}

	public async Task<bool> HasOrganizationPermission(JwtClaimData u, Guid? organizationId, TagUser permission, CancellationToken ct) {
		List<OrganizationEntity> organizations = await db.Set<OrganizationEntity>()
			.Where(x => (organizationId == null || x.Id == organizationId) && !x.Tags.Contains(TagOrganization.Inactive) && (x.OwnerId == u.Id || x.AdminUserIds.Contains(u.Id)))
			.ToListAsync(ct);
		return organizations.Any(x => x.OwnerId == u.Id || x.JsonData.Members.Any(m => m.UserId == u.Id && m.Permissions.Contains(permission)));
	}

	private async Task<bool> CanActOnPlace(JwtClaimData u, Guid? organizationId, ICollection<Guid> adminUserIds, TagUser permission, CancellationToken ct) {
		if (!Core.App.MultiTenant) return CanActOnPlace(u, adminUserIds, permission);
		if (u.IsSystemAdmin) return true;
		return organizationId != null && adminUserIds.Contains(u.Id) && await HasOrganizationPermission(u, organizationId, permission, ct);
	}

	public IQueryable<UserEntity> RelatedUsers(IQueryable<UserEntity> q, Guid userId) => q.Where(x =>
		x.Id == userId ||
		db.Set<OrganizationEntity>().Any(o => (o.OwnerId == userId || o.AdminUserIds.Contains(userId)) && (o.OwnerId == x.Id || o.AdminUserIds.Contains(x.Id))) ||
		db.Set<HotelReservationEntity>().Any(r => r.UserId == x.Id && r.Hotel.AdminUserIds.Contains(userId)) ||
		db.Set<DormBedContractEntity>().Any(c => c.UserId == x.Id && c.Bed.Room.Dorm.AdminUserIds.Contains(userId)));

	public async Task<bool?> CanActOnPlaceOf(JwtClaimData u, Guid? hotelId, Guid? hotelRoomId, Guid? dormId, Guid? dormRoomId, Guid? dormBedId, CancellationToken ct) {
		(ICollection<Guid> AdminUserIds, Guid? OrganizationId, TagUser Permission)? place = await PlaceOf(hotelId, hotelRoomId, dormId, dormRoomId, dormBedId, ct);
		return place == null ? null : await CanActOnPlace(u, place.Value.OrganizationId, place.Value.AdminUserIds, place.Value.Permission, ct);
	}

	// =====================================================================================================
	// Demo data for the whole hotel & dorm system, created by ONE call: POST api/Hotel/Seed
	//   - guest / resident users            - hotels with rooms, reservations, invoices (paid, unpaid, refunded) and reviews
	//   - dorms with rooms and beds         - dorm contracts (active, expired, late) with deposit + monthly rent invoices, and reviews
	// Every text/detail field (highlights, amenities, nearby places, FAQs, policies...) is filled so all screens look complete.
	// Photos cannot be seeded (files must be uploaded); upload them from the admin panel ("Details & photos").
	// It is safe to call twice: when the demo data already exists nothing is created.
	// Requires DataSeeder/Users to have been run (it uses the system admin as creator).
	// =====================================================================================================
	public async Task<UResponse<List<KeyValue>?>> SeedHotelsAndDorms(CancellationToken ct = default) {
		Guid adminId = Core.App.Users.SystemAdmin.Id;
		DateTime now = DateTime.UtcNow;
		DateTime today = now.Date;

		if (!await db.Set<UserEntity>().AnyAsync(x => x.Id == adminId, ct)) return new UResponse<List<KeyValue>?>(null, Usc.BadRequest, "Run DataSeeder/Users first.");
		if (await db.Set<HotelEntity>().AnyAsync(x => x.Title == "هتل سنتی عباسی اصفهان", ct)) return new UResponse<List<KeyValue>?>(null, Usc.Conflict, "Hotel and dorm demo data already exists.");

		// ---------------------------------------------------------------- users (guests and dorm residents)
		string[] firstNames = ["علی", "مریم", "رضا", "زهرا", "امیر", "سارا", "حسین", "نگار", "محمد", "فاطمه", "پویا", "الهام"];
		string[] lastNames = ["احمدی", "رضایی", "کریمی", "موسوی", "حسینی", "صادقی", "نجفی", "قاسمی", "جعفری", "محمدی", "کاظمی", "یوسفی"];
		HashSet<string> takenNames = (await db.Set<UserEntity>().Select(x => x.UserName).ToListAsync(ct)).ToHashSet();
		List<UserEntity> users = [];
		for (int i = 0; i < firstNames.Length; i++) {
			string userName = $"demo{i + 1:00}";
			if (takenNames.Contains(userName)) continue;
			users.Add(new UserEntity {
				Id = Guid.CreateVersion7(),
				CreatedAt = now.AddDays(-90 + i),
				CreatorId = adminId,
				Tags = [TagUser.Unspecified, i % 2 == 0 ? TagUser.Male : TagUser.Female, TagUser.Verified],
				UserName = userName,
				Password = UPasswordHasher.Hash("Demo1234"),
				RefreshToken = "",
				PhoneNumber = $"0912000{i + 1:0000}",
				Email = $"{userName}@example.com",
				FirstName = firstNames[i],
				LastName = lastNames[i],
				JsonData = new UserJson()
			});
		}

		// Users that already exist from an earlier partial run are looked up so the demo data can still reference them.
		List<Guid> userIds = users.Select(x => x.Id).ToList();
		if (userIds.Count < firstNames.Length)
			userIds.AddRange(await db.Set<UserEntity>().Where(x => x.UserName.StartsWith("demo")).Select(x => x.Id).ToListAsync(ct));
		if (userIds.Count == 0) userIds.Add(adminId);
		Guid UserAt(int i) => userIds[i % userIds.Count];

		List<HotelEntity> hotels = [];
		List<HotelRoomEntity> rooms = [];
		List<HotelReservationEntity> reservations = [];
		List<HotelInvoiceEntity> hotelInvoices = [];
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

		// ---------------------------------------------------------------- hotels
		// Type, policies, amenities and meal plans are tags; the Json only keeps texts and numbers.
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

			// reservations: one of every status, with an invoice that matches it
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

		// ---------------------------------------------------------------- save everything in one transaction
		await db.Set<UserEntity>().AddRangeAsync(users, ct);
		await db.Set<HotelEntity>().AddRangeAsync(hotels, ct);
		await db.Set<HotelRoomEntity>().AddRangeAsync(rooms, ct);
		await db.Set<HotelReservationEntity>().AddRangeAsync(reservations, ct);
		await db.Set<HotelInvoiceEntity>().AddRangeAsync(hotelInvoices, ct);
		await db.Set<DormEntity>().AddRangeAsync(dorms, ct);
		await db.Set<DormRoomEntity>().AddRangeAsync(dormRooms, ct);
		await db.Set<DormBedEntity>().AddRangeAsync(beds, ct);
		await db.Set<DormBedContractEntity>().AddRangeAsync(contracts, ct);
		await db.Set<DormBedInvoiceEntity>().AddRangeAsync(dormInvoices, ct);
		await db.Set<CommentEntity>().AddRangeAsync(comments, ct);
		await db.SaveChangesAsync(ct);

		return new UResponse<List<KeyValue>?>([
			new KeyValue { Key = "users", Value = users.Count.ToString() },
			new KeyValue { Key = "hotels", Value = hotels.Count.ToString() },
			new KeyValue { Key = "hotelRooms", Value = rooms.Count.ToString() },
			new KeyValue { Key = "hotelReservations", Value = reservations.Count.ToString() },
			new KeyValue { Key = "hotelInvoices", Value = hotelInvoices.Count.ToString() },
			new KeyValue { Key = "dorms", Value = dorms.Count.ToString() },
			new KeyValue { Key = "dormRooms", Value = dormRooms.Count.ToString() },
			new KeyValue { Key = "dormBeds", Value = beds.Count.ToString() },
			new KeyValue { Key = "dormContracts", Value = contracts.Count.ToString() },
			new KeyValue { Key = "dormInvoices", Value = dormInvoices.Count.ToString() },
			new KeyValue { Key = "reviews", Value = comments.Count.ToString() },
			new KeyValue { Key = "demoUsersPassword", Value = "Demo1234 (usernames demo01 ... demo12)" }
		], Usc.Created);
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
				SenderId = MoneyAccountOf(e.Bed.Room.Dorm.OrganizationId),
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
		await Post(e.Bed.Room.Dorm.OrganizationId, TagVoucher.Settlement, e.Id, e.Bed.Room.DormId, e.UserId, $"{ls.Get("contractSettled", "fa")} - {e.Bed.Room.Dorm.Title} - {e.Bed.Title}", DateTime.UtcNow, ct,
			new Leg(TagAccount.DepositsHeld, depositPaid), new Leg(TagAccount.RentIncome, Math.Round(unusedCredit, 2)), new Leg(TagAccount.Receivable, -applied),
			new Leg(TagAccount.DamageIncome, -(held - applied - refund)), new Leg(TagAccount.Wallet, -refund));

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

	public async Task<UResponse> RequestOrganizationSettlement(OrganizationSettlementRequestParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!Core.App.MultiTenant) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (p.Amount <= 0) return new UResponse(Usc.BadRequest, ls.Get("amountIsNotValid"));

		OrganizationEntity? e = await db.Set<OrganizationEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.OrganizationId, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("organizationNotFound"));
		if (!userData.IsSystemAdmin && e.OwnerId != userData.Id) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		Guid id = Guid.CreateVersion7();
		UResponse<WalletTxnResponse?> transfer = await ws.Transfer(new WalletTransferParams {
			SenderId = e.Id,
			ReceiverId = Core.App.Users.SystemAdmin.Id,
			Amount = p.Amount,
			Detail1 = ls.Get("organizationSettlement"),
			KeyValues = [new KeyValue { Key = "iban", Value = p.Iban }, new KeyValue { Key = "settlementId", Value = id.ToString() }],
			TagWalletTxn = [TagWalletTxn.OrganizationSettlement]
		}, ct);
		if (transfer.Result == null) return new UResponse(transfer.Status, transfer.Message);

		e.JsonData.Settlements = [..e.JsonData.Settlements, new OrganizationSettlement { Id = id, Amount = p.Amount, Iban = p.Iban, CreatedAt = DateTime.UtcNow }];
		await Post(e.Id, TagVoucher.Payout, id, null, null, ls.Get("organizationSettlement", "fa"), DateTime.UtcNow, ct, new Leg(TagAccount.InTransit, p.Amount), new Leg(TagAccount.Wallet, -p.Amount));
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> ProcessOrganizationSettlement(OrganizationSettlementProcessParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (!Core.App.MultiTenant || !userData.IsSystemAdmin) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		OrganizationEntity? e = await db.Set<OrganizationEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.OrganizationId, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("organizationNotFound"));
		OrganizationSettlement? s = e.JsonData.Settlements.FirstOrDefault(x => x.Id == p.SettlementId && x.Approved == null);
		if (s == null) return new UResponse(Usc.NotFound, ls.Get("settlementNotFound"));

		if (!p.Approve) {
			UResponse<WalletTxnResponse?> transfer = await ws.Transfer(new WalletTransferParams {
				SenderId = Core.App.Users.SystemAdmin.Id,
				ReceiverId = e.Id,
				Amount = s.Amount,
				Detail1 = ls.Get("organizationSettlement"),
				KeyValues = [new KeyValue { Key = "settlementId", Value = s.Id.ToString() }],
				TagWalletTxn = [TagWalletTxn.OrganizationSettlementRefund],
				AllowOverdraft = true
			}, ct);
			if (transfer.Result == null) return new UResponse(transfer.Status, transfer.Message);
		}

		e.JsonData.Settlements = e.JsonData.Settlements.Select(x => x.Id != s.Id ? x : new OrganizationSettlement {
			Id = x.Id, Amount = x.Amount, Iban = x.Iban, CreatedAt = x.CreatedAt, ProcessedAt = DateTime.UtcNow, Approved = p.Approve, Note = p.Note
		}).ToList();
		await Post(e.Id, TagVoucher.Payout, s.Id, null, null, ls.Get("organizationSettlement", "fa"), DateTime.UtcNow, ct, new Leg(p.Approve ? TagAccount.Bank : TagAccount.Wallet, s.Amount), new Leg(TagAccount.InTransit, -s.Amount));
		await AddNotification(e.OwnerId, TagNotification.General, ls.Get("organizationSettlement"), p.Approve ? s.Amount.ToIntString() : p.Note ?? "", ct);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	private static readonly (string Code, string Title, TagAccount[] Tags)[] DefaultAccounts = [
		("1101", "صندوق", [TagAccount.Asset, TagAccount.Cash]),
		("1102", "بانک", [TagAccount.Asset, TagAccount.Bank]),
		("1103", "تنخواه", [TagAccount.Asset, TagAccount.PettyCash]),
		("1104", "کیف پول سامانه", [TagAccount.Asset, TagAccount.Wallet]),
		("1105", "وجوه در راه", [TagAccount.Asset, TagAccount.InTransit]),
		("1201", "بدهکاران (ساکنان و مهمانان)", [TagAccount.Asset, TagAccount.Receivable]),
		("1202", "اسناد دریافتنی", [TagAccount.Asset, TagAccount.ChecksReceivable]),
		("2101", "ودیعه‌ی ساکنان", [TagAccount.Liability, TagAccount.DepositsHeld]),
		("2102", "اسناد پرداختنی", [TagAccount.Liability, TagAccount.ChecksPayable]),
		("2103", "بستانکاران", [TagAccount.Liability, TagAccount.Payables]),
		("3101", "سرمایه", [TagAccount.Equity, TagAccount.Capital]),
		("4101", "درآمد اجاره‌ی خوابگاه", [TagAccount.Income, TagAccount.RentIncome]),
		("4102", "درآمد اقامت هتل", [TagAccount.Income, TagAccount.HotelIncome]),
		("4103", "درآمد خدمات", [TagAccount.Income, TagAccount.ServiceIncome]),
		("4104", "درآمد جریمه‌ی دیرکرد", [TagAccount.Income, TagAccount.PenaltyIncome]),
		("4105", "درآمد خسارت و کسورات", [TagAccount.Income, TagAccount.DamageIncome]),
		("5101", "کمیسیون سامانه", [TagAccount.Expense, TagAccount.CommissionExpense]),
		("5201", "حقوق و دستمزد", [TagAccount.Expense]),
		("5202", "آب، برق و گاز", [TagAccount.Expense]),
		("5203", "تعمیرات و نگهداری", [TagAccount.Expense]),
		("5204", "مواد غذایی", [TagAccount.Expense]),
		("5205", "نظافت و بهداشت", [TagAccount.Expense]),
		("5206", "اجاره‌ی ساختمان", [TagAccount.Expense]),
		("5299", "سایر هزینه‌ها", [TagAccount.Expense])
	];

	private static readonly TagAccount[] ChargeRoles = [TagAccount.DepositsHeld, TagAccount.RentIncome, TagAccount.ServiceIncome, TagAccount.PenaltyIncome, TagAccount.HotelIncome];

	private sealed record Leg(TagAccount Role, decimal Amount, Guid? AccountId = null);

	private sealed record Entry(Guid AccountId, decimal Debit, decimal Credit, Guid? PersonId, string? Description);

	private readonly Dictionary<Guid, List<AccountEntity>> _accounts = [];

	private readonly List<VoucherEntity> _pending = [];

	private static bool IsRole(TagAccount t) => (int)t >= 300;

	private static bool IsMoneyBox(AccountEntity a) => a.Tags.Contains(TagAccount.Cash) || a.Tags.Contains(TagAccount.Bank) || a.Tags.Contains(TagAccount.PettyCash);

	private async Task<(JwtClaimData? User, UResponse? Error)> BooksUser(string? token, Guid organizationId, TagUser permission, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(token);
		if (u == null) return (null, new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue")));
		if (u.IsExpired) return (null, new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired")));
		if (!Core.App.MultiTenant) return (null, new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction")));
		if (!await db.Set<OrganizationEntity>().AnyAsync(x => x.Id == organizationId, ct)) return (null, new UResponse(Usc.NotFound, ls.Get("organizationNotFound")));
		bool allowed = u.IsSystemAdmin ||
		               await HasOrganizationPermission(u, organizationId, permission, ct) ||
		               permission == TagUser.PermissionViewAccounting && await HasOrganizationPermission(u, organizationId, TagUser.PermissionManageAccounting, ct);
		return allowed ? (u, null) : (null, new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction")));
	}

	private async Task<bool> IsPlaceOf(Guid organizationId, Guid placeId, CancellationToken ct) =>
		await db.Set<HotelEntity>().AnyAsync(x => x.Id == placeId && x.OrganizationId == organizationId, ct) ||
		await db.Set<DormEntity>().AnyAsync(x => x.Id == placeId && x.OrganizationId == organizationId, ct);

	private async Task<List<AccountEntity>> AccountsOf(Guid organizationId, CancellationToken ct) {
		if (_accounts.TryGetValue(organizationId, out List<AccountEntity>? list)) return list;
		list = await db.Set<AccountEntity>().Where(x => x.OrganizationId == organizationId).ToListAsync(ct);
		if (list.Count == 0) {
			DateTime now = DateTime.UtcNow;
			list = DefaultAccounts.Select(a => new AccountEntity {
				Id = Guid.CreateVersion7(),
				CreatorId = organizationId,
				CreatedAt = now,
				Tags = a.Tags.ToList(),
				Code = a.Code,
				Title = a.Title,
				OrganizationId = organizationId,
				JsonData = new AccountJson()
			}).ToList();
			await db.Set<AccountEntity>().AddRangeAsync(list, ct);
		}

		_accounts[organizationId] = list;
		return list;
	}

	private List<VoucherEntity> PendingVouchers() => _pending.Where(x => db.Entry(x).State == EntityState.Added).ToList();

	private async Task<Guid> AddVoucher(Guid organizationId, ICollection<TagVoucher> tags, Guid? sourceId, Guid? placeId, DateTime date, string? description, Guid? registeredBy, IEnumerable<Entry> lines, CancellationToken ct) {
		Guid id = Guid.CreateVersion7();
		DateTime now = DateTime.UtcNow;
		int last = await db.Set<VoucherEntity>().Where(x => x.OrganizationId == organizationId).MaxAsync(x => (int?)x.Number, ct) ?? 0;
		VoucherEntity v = new() {
			Id = id,
			CreatorId = organizationId,
			CreatedAt = now,
			Tags = tags.Distinct().ToList(),
			Number = last + PendingVouchers().Count(x => x.OrganizationId == organizationId) + 1,
			Date = date,
			PlaceId = placeId,
			SourceId = sourceId,
			OrganizationId = organizationId,
			JsonData = new VoucherJson { Detail1 = description ?? "", RegisteredBy = registeredBy },
			Lines = lines.Select(l => new VoucherLineEntity {
				Id = Guid.CreateVersion7(),
				CreatorId = organizationId,
				CreatedAt = now,
				Tags = tags.Distinct().ToList(),
				VoucherId = id,
				AccountId = l.AccountId,
				Debit = l.Debit,
				Credit = l.Credit,
				PersonId = l.PersonId,
				Description = l.Description
			}).ToList()
		};
		await db.Set<VoucherEntity>().AddAsync(v, ct);
		_pending.Add(v);
		return id;
	}

	private async Task Post(Guid? organizationId, TagVoucher source, Guid? sourceId, Guid? placeId, Guid? personId, string description, DateTime date, CancellationToken ct, params Leg[] legs) {
		if (!Core.App.MultiTenant || organizationId == null) return;
		List<AccountEntity> accounts = await AccountsOf(organizationId.Value, ct);
		List<Entry> lines = legs
			.GroupBy(x => x.AccountId ?? accounts.First(a => a.Tags.Contains(x.Role)).Id)
			.Select(g => (AccountId: g.Key, Amount: Math.Round(g.Sum(x => x.Amount), 2)))
			.Where(x => x.Amount != 0)
			.Select(x => new Entry(x.AccountId, Math.Max(0, x.Amount), Math.Max(0, -x.Amount), personId, null))
			.ToList();
		if (lines.Count == 0) return;
		await AddVoucher(organizationId.Value, [TagVoucher.Auto, source], sourceId, placeId, date, description, null, lines, ct);
	}

	private async Task SyncCharge(Guid organizationId, Guid sourceId, Guid placeId, Guid personId, DateTime date, string description, Dictionary<TagAccount, decimal> target, CancellationToken ct) {
		List<AccountEntity> accounts = await AccountsOf(organizationId, ct);
		List<(Guid AccountId, decimal Amount)> posted = (await db.Set<VoucherLineEntity>()
				.Where(x => x.Voucher.SourceId == sourceId && x.Tags.Contains(TagVoucher.Invoice))
				.GroupBy(x => x.AccountId)
				.Select(g => new { g.Key, Amount = g.Sum(x => x.Credit - x.Debit) })
				.ToListAsync(ct))
			.Select(x => (x.Key, x.Amount))
			.Concat(PendingVouchers().Where(x => x.SourceId == sourceId && x.Tags.Contains(TagVoucher.Invoice)).SelectMany(x => x.Lines).Select(x => (x.AccountId, x.Credit - x.Debit)))
			.ToList();

		List<Leg> legs = [];
		foreach (TagAccount role in ChargeRoles) {
			Guid id = accounts.First(a => a.Tags.Contains(role)).Id;
			decimal delta = Math.Round(target.GetValueOrDefault(role) - posted.Where(x => x.AccountId == id).Sum(x => x.Amount), 2);
			if (delta == 0) continue;
			legs.Add(new Leg(role, -delta));
			legs.Add(new Leg(TagAccount.Receivable, delta));
		}

		await Post(organizationId, TagVoucher.Invoice, sourceId, placeId, personId, description, date, ct, legs.ToArray());
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
		await SyncCharge(dorm.OrganizationId.Value, e.Id, dorm.Id, e.Contract.UserId, !e.JsonData.Posted && e.DueDate < now ? e.DueDate : now, $"{ls.Get("dormBedInvoiceIssued", "fa")} - {dorm.Title} - {e.Contract.Bed.Title}", target, ct);
		e.JsonData.Posted = charged;
	}

	private async Task SyncHotelInvoice(HotelInvoiceEntity e, bool removed, CancellationToken ct) {
		HotelReservationEntity? r = e.Reservation;
		if (!Core.App.MultiTenant || r?.Hotel.OrganizationId == null) return;

		DateTime now = DateTime.UtcNow;
		bool charged = !removed && (e.DueDate <= now || !e.Tags.Contains(TagHotelInvoice.NotPaid) || e.PaidAmount > 0);
		if (!charged && !e.JsonData.Posted) return;

		decimal amount = !charged ? 0
			: r.Tags.Contains(TagHotelReservation.Cancelled) ? Math.Max(0, e.PaidAmount - e.CreditorAmount)
			: Math.Max(0, e.DebtAmount + e.PenaltyAmount - e.CreditorAmount);
		await SyncCharge(r.Hotel.OrganizationId.Value, e.Id, r.HotelId, r.UserId, !e.JsonData.Posted && e.DueDate < now ? e.DueDate : now, $"{ls.Get("hotelInvoiceIssued", "fa")} - {r.Hotel.Title}", new Dictionary<TagAccount, decimal> { [TagAccount.HotelIncome] = amount }, ct);
		e.JsonData.Posted = charged;
	}

	private async Task<UResponse?> PostReceipt(Guid? organizationId, Guid invoiceId, Guid placeId, Guid personId, Guid? contractId, decimal amount, InvoiceReceiveParams p, string description, Guid registeredBy, CancellationToken ct) {
		if (!Core.App.MultiTenant || organizationId == null) return null;

		DateTime now = DateTime.UtcNow;
		if (p.Check != null) {
			if (p.Check.Number.IsNullOrEmpty()) return new UResponse(Usc.BadRequest, ls.Get("numberRequired"));
			Guid checkId = Guid.CreateVersion7();
			await db.Set<CheckEntity>().AddAsync(new CheckEntity {
				Id = checkId,
				CreatorId = organizationId.Value,
				CreatedAt = now,
				Tags = [TagCheck.Received, TagCheck.Pending],
				Amount = amount,
				DueDate = p.Check.DueDate,
				Number = p.Check.Number,
				Bank = p.Check.Bank,
				PersonId = personId,
				ContractId = contractId,
				PlaceId = placeId,
				OrganizationId = organizationId.Value,
				JsonData = new CheckJson { SayadId = p.Check.SayadId, Drawer = p.Check.Drawer, InvoiceId = invoiceId, RegisteredBy = registeredBy }
			}, ct);
			await Post(organizationId, TagVoucher.Check, checkId, placeId, personId, description, now, ct, new Leg(TagAccount.ChecksReceivable, amount), new Leg(TagAccount.Receivable, -amount));
			return null;
		}

		AccountEntity? box = (await AccountsOf(organizationId.Value, ct)).FirstOrDefault(x => x.Id == p.AccountId && IsMoneyBox(x));
		if (box == null) return new UResponse(Usc.BadRequest, ls.Get("selectACashOrBankAccount"));
		await Post(organizationId, TagVoucher.Receipt, invoiceId, placeId, personId, description, now, ct, new Leg(TagAccount.Cash, amount, box.Id), new Leg(TagAccount.Receivable, -amount));
		return null;
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
		UResponse? error = await PostReceipt(dorm.OrganizationId, e.Id, dorm.Id, e.Contract.UserId, e.Contract.Id, amount, p, $"{ls.Get("invoiceReceipt", "fa")} - {dorm.Title} - {e.Contract.Bed.Title}", userData.Id, ct);
		if (error != null) return error;

		await AddNotification(e.Contract.UserId, TagNotification.InvoicePaid, ls.Get("invoicePaid"), ls.Get("dormInvoicePayment"), ct);
		await db.SaveChangesAsync(ct);
		return new UResponse(Usc.Success, ls.Get("paymentCompleted"));
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
		UResponse? error = await PostReceipt(r.Hotel.OrganizationId, e.Id, r.HotelId, r.UserId, null, amount, p, $"{ls.Get("invoiceReceipt", "fa")} - {r.Hotel.Title}", userData.Id, ct);
		if (error != null) return error;

		await AddNotification(r.UserId, TagNotification.InvoicePaid, ls.Get("invoicePaid"), r.Hotel.Title, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse(Usc.Success, ls.Get("paymentCompleted"));
	}

	private async Task ReopenInvoice(Guid id, decimal amount, CancellationToken ct) {
		DormBedInvoiceEntity? d = await db.Set<DormBedInvoiceEntity>().AsTracking()
			.Include(x => x.Contract).ThenInclude(x => x!.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm)
			.FirstOrDefaultAsync(x => x.Id == id, ct);
		if (d != null) {
			d.PaidAmount = Math.Max(0, d.PaidAmount - amount);
			if (!d.Tags.Contains(TagDormBedInvoice.NotPaid)) d.Tags = [..d.Tags.Where(x => (int)x < 200), TagDormBedInvoice.NotPaid];
			await SyncDormBedInvoice(d, false, ct);
			return;
		}

		HotelInvoiceEntity? h = await db.Set<HotelInvoiceEntity>().AsTracking().Include(x => x.Reservation).ThenInclude(x => x!.Hotel).FirstOrDefaultAsync(x => x.Id == id, ct);
		if (h == null) return;
		h.PaidAmount = Math.Max(0, h.PaidAmount - amount);
		if (!h.Tags.Contains(TagHotelInvoice.NotPaid)) h.Tags = [..h.Tags.Where(x => (int)x < 200), TagHotelInvoice.NotPaid];
		await SyncHotelInvoice(h, false, ct);
	}

	public async Task<UResponse<Guid?>> CreateAccount(AccountCreateParams p, CancellationToken ct) {
		(JwtClaimData? userData, UResponse? error) = await BooksUser(p.Token, p.OrganizationId, TagUser.PermissionManageAccounting, ct);
		if (error != null) return new UResponse<Guid?>(null, error.Status, error.Message);

		List<AccountEntity> accounts = await AccountsOf(p.OrganizationId, ct);
		if (accounts.Any(x => x.Code == p.Code)) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("accountCodeExists"));
		List<TagAccount> tags = p.Tags.Where(x => !IsRole(x)).Distinct().ToList();
		if (tags.Count(x => (int)x < 200) != 1) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("tagsIsRequired"));

		Guid id = Guid.CreateVersion7();
		await db.Set<AccountEntity>().AddAsync(new AccountEntity {
			Id = id,
			CreatorId = p.OrganizationId,
			CreatedAt = DateTime.UtcNow,
			Tags = tags,
			Code = p.Code,
			Title = p.Title,
			OrganizationId = p.OrganizationId,
			JsonData = new AccountJson { Detail1 = p.Detail1, Detail2 = p.Detail2, RegisteredBy = userData!.Id }
		}, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<AccountResponse>?>> ReadAccounts(AccountReadParams p, CancellationToken ct) {
		(_, UResponse? error) = await BooksUser(p.Token, p.OrganizationId, TagUser.PermissionViewAccounting, ct);
		if (error != null) return new UResponse<IEnumerable<AccountResponse>?>(null, error.Status, error.Message);

		await AccountsOf(p.OrganizationId, ct);
		await db.SaveChangesAsync(ct);

		IQueryable<VoucherLineEntity> lines = db.Set<VoucherLineEntity>().Where(l =>
			l.Voucher.OrganizationId == p.OrganizationId &&
			(p.FromDate == null || l.Voucher.Date >= p.FromDate) &&
			(p.ToDate == null || l.Voucher.Date <= p.ToDate) &&
			(p.PlaceId == null || l.Voucher.PlaceId == p.PlaceId));

		return await db.Set<AccountEntity>().Where(x => x.OrganizationId == p.OrganizationId).ApplyReadParams(p).OrderBy(x => x.Code).Select(x => new AccountResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Code = x.Code,
			Title = x.Title,
			OrganizationId = x.OrganizationId,
			Debit = lines.Where(l => l.AccountId == x.Id).Sum(l => (decimal?)l.Debit) ?? 0,
			Credit = lines.Where(l => l.AccountId == x.Id).Sum(l => (decimal?)l.Credit) ?? 0,
			Balance = lines.Where(l => l.AccountId == x.Id).Sum(l => (decimal?)(l.Debit - l.Credit)) ?? 0
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateAccount(AccountUpdateParams p, CancellationToken ct) {
		AccountEntity? e = await db.Set<AccountEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("ledgerAccountNotFound"));
		(_, UResponse? error) = await BooksUser(p.Token, e.OrganizationId, TagUser.PermissionManageAccounting, ct);
		if (error != null) return error;

		if (p.Code.IsNotNullOrEmpty() && p.Code != e.Code) {
			if (await db.Set<AccountEntity>().AnyAsync(x => x.OrganizationId == e.OrganizationId && x.Code == p.Code, ct)) return new UResponse(Usc.Conflict, ls.Get("accountCodeExists"));
			e.Code = p.Code;
		}

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		List<TagAccount> fixedTags = e.Tags.Where(x => (int)x < 200 || IsRole(x)).ToList();
		e.ApplyUpdateParam<AccountEntity, TagAccount, AccountJson>(p);
		e.Tags = fixedTags.Concat(e.Tags.Where(x => (int)x is >= 200 and < 300)).Distinct().ToList();
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteAccount(IdParams p, CancellationToken ct) {
		AccountEntity? e = await db.Set<AccountEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("ledgerAccountNotFound"));
		(_, UResponse? error) = await BooksUser(p.Token, e.OrganizationId, TagUser.PermissionManageAccounting, ct);
		if (error != null) return error;
		if (e.Tags.Any(IsRole) || await db.Set<VoucherLineEntity>().AnyAsync(x => x.AccountId == e.Id, ct)) return new UResponse(Usc.Conflict, ls.Get("accountHasEntries"));

		await db.Set<AccountEntity>().Where(x => x.Id == e.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateVoucher(VoucherCreateParams p, CancellationToken ct) {
		(JwtClaimData? userData, UResponse? error) = await BooksUser(p.Token, p.OrganizationId, TagUser.PermissionManageAccounting, ct);
		if (error != null) return new UResponse<Guid?>(null, error.Status, error.Message);

		List<AccountEntity> accounts = await AccountsOf(p.OrganizationId, ct);
		if (p.Lines.Any(x => x.Debit < 0 || x.Credit < 0 || x.Debit > 0 == x.Credit > 0 || !accounts.Any(a => a.Id == x.AccountId && !a.Tags.Contains(TagAccount.Inactive))))
			return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("amountIsNotValid"));
		if (p.Lines.Sum(x => x.Debit) != p.Lines.Sum(x => x.Credit)) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("voucherIsNotBalanced"));
		if (p.PlaceId != null && !await IsPlaceOf(p.OrganizationId, p.PlaceId.Value, ct)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		Guid id = await AddVoucher(p.OrganizationId, [TagVoucher.Manual, ..p.Tags.Where(x => x != TagVoucher.Auto)], null, p.PlaceId, p.Date ?? DateTime.UtcNow, p.Detail1, userData!.Id,
			p.Lines.Select(x => new Entry(x.AccountId, x.Debit, x.Credit, x.PersonId, x.Description)), ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<VoucherResponse>?>> ReadVouchers(VoucherReadParams p, CancellationToken ct) {
		(_, UResponse? error) = await BooksUser(p.Token, p.OrganizationId, TagUser.PermissionViewAccounting, ct);
		if (error != null) return new UResponse<IEnumerable<VoucherResponse>?>(null, error.Status, error.Message);

		IQueryable<VoucherEntity> q = db.Set<VoucherEntity>().Where(x => x.OrganizationId == p.OrganizationId).ApplyReadParams(p);
		if (p.FromDate != null) q = q.Where(x => x.Date >= p.FromDate);
		if (p.ToDate != null) q = q.Where(x => x.Date <= p.ToDate);
		if (p.PlaceId != null) q = q.Where(x => x.PlaceId == p.PlaceId);
		if (p.SourceId != null) q = q.Where(x => x.SourceId == p.SourceId);
		if (p.PersonId != null) q = q.Where(x => x.Lines.Any(l => l.PersonId == p.PersonId));
		if (p.AccountId != null) q = q.Where(x => x.Lines.Any(l => l.AccountId == p.AccountId));

		return await q.OrderByDescending(x => x.Date).ThenByDescending(x => x.Number).Select(x => new VoucherResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Number = x.Number,
			Date = x.Date,
			PlaceId = x.PlaceId,
			SourceId = x.SourceId,
			OrganizationId = x.OrganizationId,
			Total = x.Lines.Sum(l => l.Debit),
			Lines = x.Lines.OrderByDescending(l => l.Debit).Select(l => new VoucherLineResponse {
				Id = l.Id,
				AccountId = l.AccountId,
				AccountCode = l.Account.Code,
				AccountTitle = l.Account.Title,
				PersonId = l.PersonId,
				PersonName = db.Set<UserEntity>().Where(u => u.Id == l.PersonId).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault(),
				Debit = l.Debit,
				Credit = l.Credit,
				Description = l.Description
			}).ToList()
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> DeleteVoucher(IdParams p, CancellationToken ct) {
		VoucherEntity? e = await db.Set<VoucherEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("voucherNotFound"));
		(_, UResponse? error) = await BooksUser(p.Token, e.OrganizationId, TagUser.PermissionManageAccounting, ct);
		if (error != null) return error;
		if (!e.Tags.Contains(TagVoucher.Manual)) return new UResponse(Usc.Conflict, ls.Get("onlyManualVouchersCanBeDeleted"));

		await db.Set<VoucherEntity>().Where(x => x.Id == e.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<LedgerResponse?>> ReadLedger(LedgerReadParams p, CancellationToken ct) {
		(_, UResponse? error) = await BooksUser(p.Token, p.OrganizationId, TagUser.PermissionViewAccounting, ct);
		if (error != null) return new UResponse<LedgerResponse?>(null, error.Status, error.Message);

		IQueryable<VoucherLineEntity> q = db.Set<VoucherLineEntity>().Where(x => x.Voucher.OrganizationId == p.OrganizationId);
		if (p.AccountId != null) q = q.Where(x => x.AccountId == p.AccountId);
		if (p.AccountTags.IsNotNullOrEmpty()) q = q.Where(x => x.Account.Tags.Any(t => p.AccountTags!.Contains(t)));
		if (p.PersonId != null) q = q.Where(x => x.PersonId == p.PersonId);
		if (p.PlaceId != null) q = q.Where(x => x.Voucher.PlaceId == p.PlaceId);

		decimal opening = p.FromDate == null ? 0 : await q.Where(x => x.Voucher.Date < p.FromDate).SumAsync(x => (decimal?)(x.Debit - x.Credit), ct) ?? 0;
		if (p.FromDate != null) q = q.Where(x => x.Voucher.Date >= p.FromDate);
		if (p.ToDate != null) q = q.Where(x => x.Voucher.Date <= p.ToDate);

		List<LedgerLineResponse> lines = await q.OrderBy(x => x.Voucher.Date).ThenBy(x => x.Voucher.Number).Select(x => new LedgerLineResponse {
			VoucherId = x.VoucherId,
			Number = x.Voucher.Number,
			Date = x.Voucher.Date,
			Tags = x.Tags,
			Description = x.Description ?? x.Voucher.JsonData.Detail1,
			AccountId = x.AccountId,
			AccountTitle = x.Account.Title,
			PersonId = x.PersonId,
			PersonName = db.Set<UserEntity>().Where(u => u.Id == x.PersonId).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault(),
			Debit = x.Debit,
			Credit = x.Credit
		}).ToListAsync(ct);

		decimal balance = opening;
		foreach (LedgerLineResponse l in lines) {
			balance += l.Debit - l.Credit;
			l.Balance = balance;
		}

		return new UResponse<LedgerResponse?>(new LedgerResponse {
			Opening = opening,
			TotalDebit = lines.Sum(x => x.Debit),
			TotalCredit = lines.Sum(x => x.Credit),
			Closing = balance,
			Lines = lines
		});
	}

	public async Task<UResponse<LedgerReportResponse?>> ReadLedgerReport(LedgerReportParams p, CancellationToken ct) {
		(_, UResponse? error) = await BooksUser(p.Token, p.OrganizationId, TagUser.PermissionViewAccounting, ct);
		if (error != null) return new UResponse<LedgerReportResponse?>(null, error.Status, error.Message);

		List<AccountEntity> accounts = await AccountsOf(p.OrganizationId, ct);
		await db.SaveChangesAsync(ct);

		var sums = await db.Set<VoucherLineEntity>()
			.Where(x => x.Voucher.OrganizationId == p.OrganizationId && (p.FromDate == null || x.Voucher.Date >= p.FromDate) && (p.ToDate == null || x.Voucher.Date <= p.ToDate))
			.GroupBy(x => new { x.AccountId, x.Voucher.PlaceId })
			.Select(g => new { g.Key.AccountId, g.Key.PlaceId, Debit = g.Sum(x => x.Debit), Credit = g.Sum(x => x.Credit) })
			.ToListAsync(ct);
		Dictionary<Guid, decimal> opening = p.FromDate == null
			? []
			: await db.Set<VoucherLineEntity>()
				.Where(x => x.Voucher.OrganizationId == p.OrganizationId && x.Voucher.Date < p.FromDate)
				.GroupBy(x => x.AccountId)
				.Select(g => new { g.Key, Amount = g.Sum(x => x.Debit - x.Credit) })
				.ToDictionaryAsync(x => x.Key, x => x.Amount, ct);

		LedgerReportResponse r = new();
		foreach (AccountEntity a in accounts.OrderBy(x => x.Code)) {
			decimal debit = sums.Where(x => x.AccountId == a.Id).Sum(x => x.Debit);
			decimal credit = sums.Where(x => x.AccountId == a.Id).Sum(x => x.Credit);
			if (a.Tags.Contains(TagAccount.Income) && credit != debit) r.Income.Add(new LedgerReportItem { AccountId = a.Id, Code = a.Code, Title = a.Title, Amount = credit - debit });
			if (a.Tags.Contains(TagAccount.Expense) && credit != debit) r.Expense.Add(new LedgerReportItem { AccountId = a.Id, Code = a.Code, Title = a.Title, Amount = debit - credit });
			if (!IsMoneyBox(a) && !a.Tags.Contains(TagAccount.Wallet)) continue;
			decimal o = opening.GetValueOrDefault(a.Id);
			r.MoneyBoxes.Add(new LedgerMoneyBoxItem { AccountId = a.Id, Title = a.Title, Tags = a.Tags, Opening = o, In = debit, Out = credit, Closing = o + debit - credit });
		}

		r.NetProfit = r.Income.Sum(x => x.Amount) - r.Expense.Sum(x => x.Amount);

		HashSet<Guid> incomeIds = accounts.Where(x => x.Tags.Contains(TagAccount.Income)).Select(x => x.Id).ToHashSet();
		HashSet<Guid> expenseIds = accounts.Where(x => x.Tags.Contains(TagAccount.Expense)).Select(x => x.Id).ToHashSet();
		Dictionary<Guid, string> titles = (await db.Set<HotelEntity>().Where(x => x.OrganizationId == p.OrganizationId).Select(x => new { x.Id, x.Title }).ToListAsync(ct))
			.Concat(await db.Set<DormEntity>().Where(x => x.OrganizationId == p.OrganizationId).Select(x => new { x.Id, x.Title }).ToListAsync(ct))
			.ToDictionary(x => x.Id, x => x.Title);
		r.Places = sums
			.Where(x => incomeIds.Contains(x.AccountId) || expenseIds.Contains(x.AccountId))
			.GroupBy(x => x.PlaceId)
			.Select(g => new LedgerPlaceItem {
				PlaceId = g.Key,
				Title = g.Key != null ? titles.GetValueOrDefault(g.Key.Value, "") : "",
				Income = g.Where(x => incomeIds.Contains(x.AccountId)).Sum(x => x.Credit - x.Debit),
				Expense = g.Where(x => expenseIds.Contains(x.AccountId)).Sum(x => x.Debit - x.Credit)
			}).ToList();

		DateTime now = DateTime.UtcNow;
		var due = (await db.Set<DormBedInvoiceEntity>()
				.Where(x => x.Tags.Contains(TagDormBedInvoice.NotPaid) && x.DueDate <= now && x.Contract != null && x.Contract.Bed.Room.Dorm.OrganizationId == p.OrganizationId)
				.Select(x => new { PersonId = x.Contract!.UserId, x.DueDate, Amount = x.DebtAmount + x.PenaltyAmount - x.CreditorAmount - x.PaidAmount })
				.ToListAsync(ct))
			.Concat(await db.Set<HotelInvoiceEntity>()
				.Where(x => x.Tags.Contains(TagHotelInvoice.NotPaid) && x.DueDate <= now && x.Reservation != null && !x.Reservation.Tags.Contains(TagHotelReservation.Cancelled) && x.Reservation.Hotel.OrganizationId == p.OrganizationId)
				.Select(x => new { PersonId = x.Reservation!.UserId, x.DueDate, Amount = x.DebtAmount + x.PenaltyAmount - x.CreditorAmount - x.PaidAmount })
				.ToListAsync(ct))
			.Where(x => x.Amount > 0)
			.ToList();
		List<Guid> personIds = due.Select(x => x.PersonId).Distinct().ToList();
		var people = await db.Set<UserEntity>().Where(x => personIds.Contains(x.Id)).Select(x => new { x.Id, x.FirstName, x.LastName, x.PhoneNumber }).ToDictionaryAsync(x => x.Id, ct);
		r.Aging = due.GroupBy(x => x.PersonId).Select(g => {
			decimal Bucket(int min, int max) => g.Where(x => (now - x.DueDate).Days >= min && (now - x.DueDate).Days <= max).Sum(x => x.Amount);
			var person = people.GetValueOrDefault(g.Key);
			return new LedgerAgingItem {
				PersonId = g.Key,
				PersonName = person == null ? null : $"{person.FirstName} {person.LastName}".Trim(),
				PhoneNumber = person?.PhoneNumber,
				Days0 = Bucket(0, 30),
				Days30 = Bucket(31, 60),
				Days60 = Bucket(61, 90),
				Days90 = Bucket(91, int.MaxValue),
				Total = g.Sum(x => x.Amount)
			};
		}).OrderByDescending(x => x.Total).ToList();

		return new UResponse<LedgerReportResponse?>(r);
	}

	public async Task<UResponse<Guid?>> CreateCheck(CheckCreateParams p, CancellationToken ct) {
		(JwtClaimData? userData, UResponse? error) = await BooksUser(p.Token, p.OrganizationId, TagUser.PermissionManageAccounting, ct);
		if (error != null) return new UResponse<Guid?>(null, error.Status, error.Message);

		List<TagCheck> kind = p.Tags.Where(x => x is TagCheck.Received or TagCheck.Issued or TagCheck.Guarantee).Distinct().ToList();
		if (kind.Count != 1) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("tagsIsRequired"));
		if (p.Amount <= 0) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("amountIsNotValid"));
		if (p.AccountId != null && (await AccountsOf(p.OrganizationId, ct)).All(x => x.Id != p.AccountId)) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("ledgerAccountNotFound"));
		if (p.PlaceId != null && !await IsPlaceOf(p.OrganizationId, p.PlaceId.Value, ct)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		Guid id = Guid.CreateVersion7();
		DateTime now = DateTime.UtcNow;
		await db.Set<CheckEntity>().AddAsync(new CheckEntity {
			Id = id,
			CreatorId = p.OrganizationId,
			CreatedAt = now,
			Tags = [kind[0], TagCheck.Pending],
			Amount = p.Amount,
			DueDate = p.DueDate,
			Number = p.Number,
			Bank = p.Bank,
			PersonId = p.PersonId,
			ContractId = p.ContractId,
			PlaceId = p.PlaceId,
			OrganizationId = p.OrganizationId,
			JsonData = new CheckJson { Detail1 = p.Detail1, Detail2 = p.Detail2, SayadId = p.SayadId, Drawer = p.Drawer, AccountId = p.AccountId, RegisteredBy = userData!.Id }
		}, ct);

		if (kind[0] == TagCheck.Received)
			await Post(p.OrganizationId, TagVoucher.Check, id, p.PlaceId, p.PersonId, $"{ls.Get("receivedCheck", "fa")} {p.Number}", now, ct,
				new Leg(TagAccount.ChecksReceivable, p.Amount), new Leg(TagAccount.Receivable, -p.Amount, p.AccountId));
		if (kind[0] == TagCheck.Issued)
			await Post(p.OrganizationId, TagVoucher.Check, id, p.PlaceId, p.PersonId, $"{ls.Get("issuedCheck", "fa")} {p.Number}", now, ct,
				new Leg(TagAccount.Payables, p.Amount, p.AccountId), new Leg(TagAccount.ChecksPayable, -p.Amount));

		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<CheckResponse>?>> ReadChecks(CheckReadParams p, CancellationToken ct) {
		(_, UResponse? error) = await BooksUser(p.Token, p.OrganizationId, TagUser.PermissionViewAccounting, ct);
		if (error != null) return new UResponse<IEnumerable<CheckResponse>?>(null, error.Status, error.Message);

		IQueryable<CheckEntity> q = db.Set<CheckEntity>().Where(x => x.OrganizationId == p.OrganizationId).ApplyReadParams(p);
		if (p.PersonId != null) q = q.Where(x => x.PersonId == p.PersonId);
		if (p.ContractId != null) q = q.Where(x => x.ContractId == p.ContractId);
		if (p.FromDueDate != null) q = q.Where(x => x.DueDate >= p.FromDueDate);
		if (p.ToDueDate != null) q = q.Where(x => x.DueDate <= p.ToDueDate);

		return await q.OrderBy(x => x.DueDate).Select(x => new CheckResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Amount = x.Amount,
			DueDate = x.DueDate,
			Number = x.Number,
			Bank = x.Bank,
			PersonId = x.PersonId,
			PersonName = db.Set<UserEntity>().Where(u => u.Id == x.PersonId).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault(),
			ContractId = x.ContractId,
			PlaceId = x.PlaceId,
			OrganizationId = x.OrganizationId
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> SetCheckStatus(CheckStatusParams p, CancellationToken ct) {
		CheckEntity? e = await db.Set<CheckEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("checkNotFound"));
		(_, UResponse? error) = await BooksUser(p.Token, e.OrganizationId, TagUser.PermissionManageAccounting, ct);
		if (error != null) return error;
		if (!e.Tags.Contains(TagCheck.Pending) || p.Status is not (TagCheck.Cleared or TagCheck.Bounced or TagCheck.Returned))
			return new UResponse(Usc.Conflict, ls.Get("checkIsNotPending"));

		bool cleared = p.Status == TagCheck.Cleared;
		AccountEntity? box = (await AccountsOf(e.OrganizationId, ct)).FirstOrDefault(x => x.Id == p.AccountId && IsMoneyBox(x));
		if (cleared && box == null) return new UResponse(Usc.BadRequest, ls.Get("selectACashOrBankAccount"));

		DateTime date = p.Date ?? DateTime.UtcNow;
		string description = $"{ls.Get(cleared ? "checkCleared" : p.Status == TagCheck.Bounced ? "checkBounced" : "checkReturned", "fa")} {e.Number}";
		Guid? counter = e.JsonData.AccountId;
		if (e.Tags.Contains(TagCheck.Received)) {
			if (cleared) await Post(e.OrganizationId, TagVoucher.Check, e.Id, e.PlaceId, e.PersonId, description, date, ct, new Leg(TagAccount.Cash, e.Amount, box!.Id), new Leg(TagAccount.ChecksReceivable, -e.Amount));
			else {
				await Post(e.OrganizationId, TagVoucher.Check, e.Id, e.PlaceId, e.PersonId, description, date, ct, new Leg(TagAccount.Receivable, e.Amount, counter), new Leg(TagAccount.ChecksReceivable, -e.Amount));
				if (e.JsonData.InvoiceId != null) await ReopenInvoice(e.JsonData.InvoiceId.Value, e.Amount, ct);
			}
		}
		else if (e.Tags.Contains(TagCheck.Issued)) {
			if (cleared) await Post(e.OrganizationId, TagVoucher.Check, e.Id, e.PlaceId, e.PersonId, description, date, ct, new Leg(TagAccount.ChecksPayable, e.Amount), new Leg(TagAccount.Cash, -e.Amount, box!.Id));
			else await Post(e.OrganizationId, TagVoucher.Check, e.Id, e.PlaceId, e.PersonId, description, date, ct, new Leg(TagAccount.ChecksPayable, e.Amount), new Leg(TagAccount.Payables, -e.Amount, counter));
		}
		else if (cleared) await Post(e.OrganizationId, TagVoucher.Check, e.Id, e.PlaceId, e.PersonId, description, date, ct, new Leg(TagAccount.Cash, e.Amount, box!.Id), new Leg(TagAccount.Receivable, -e.Amount));

		e.Tags = [..e.Tags.Where(x => x != TagCheck.Pending), p.Status];
		await db.SaveChangesAsync(ct);
		return new UResponse();
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

			List<HotelInvoiceEntity> hotelInvoices = await db.Set<HotelInvoiceEntity>().AsTracking()
				.Include(x => x.Reservation).ThenInclude(x => x!.Hotel)
				.Where(x => !x.JsonData.Posted && x.DueDate <= now && x.Reservation != null && x.Reservation.Hotel.OrganizationId != null)
				.ToListAsync(ct);
			foreach (HotelInvoiceEntity e in hotelInvoices) await SyncHotelInvoice(e, false, ct);

			List<CheckEntity> checks = await db.Set<CheckEntity>().AsTracking().Include(x => x.Organization)
				.Where(x => x.Tags.Contains(TagCheck.Pending) && !x.JsonData.DueReminded && x.DueDate <= soon)
				.ToListAsync(ct);
			foreach (CheckEntity c in checks) {
				await AddNotification(c.Organization.OwnerId, TagNotification.General, ls.Get("checkIsDueSoon", "fa"), $"{c.Number} - {c.Amount.ToIntString()}", ct);
				c.JsonData.DueReminded = true;
			}
		}

		await db.SaveChangesAsync(ct);
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
					await scope.ServiceProvider.GetRequiredService<IHotelService>().ProcessDueInvoices(stoppingToken);
				}
				catch (OperationCanceledException) {
					throw;
				}
				catch (Exception e) {
					if (!_failureLogged) ULog.Error(e, "Dorm invoice reminders are off (are the hotel tables migrated?)");
					_failureLogged = true;
				}
			} while (await timer.WaitForNextTickAsync(stoppingToken));
		}
		catch (OperationCanceledException) {
		}
	}
}
