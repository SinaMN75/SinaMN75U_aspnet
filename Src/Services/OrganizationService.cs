namespace SinaMN75U.Services;

public interface IOrganizationService {
	public Task<bool> CanCreatePlace(JwtClaimData u, Guid? organizationId, TagUser permission, CancellationToken ct);
	public Task<bool> IsOwner(JwtClaimData u, Guid? organizationId, CancellationToken ct);
	public Task<List<Guid>> PlaceAdmins(Guid? organizationId, ICollection<Guid> ids, CancellationToken ct);
	public Task<UResponse<Guid?>> CreateOrganization(OrganizationCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<OrganizationResponse>?>> ReadOrganizations(OrganizationReadParams p, CancellationToken ct);
	public Task<UResponse> UpdateOrganization(OrganizationUpdateParams p, CancellationToken ct);
	public Task<UResponse> SetOrganizationMember(OrganizationMemberParams p, CancellationToken ct);
	public Task<UResponse> RemoveOrganizationMember(OrganizationMemberParams p, CancellationToken ct);
	public Task<bool> IsPlaceOf(Guid organizationId, Guid placeId, CancellationToken ct);
	public Task<Dictionary<Guid, string>> PlaceTitles(Guid organizationId, CancellationToken ct);
	public Task<bool> HasOrganizationPermission(JwtClaimData u, Guid? organizationId, TagUser permission, CancellationToken ct);
	public Task<bool> CanActOnPlace(JwtClaimData u, Guid? organizationId, ICollection<Guid> adminUserIds, TagUser permission, CancellationToken ct);
	public IQueryable<Guid> RelatedUserIds(Guid userId);
	public Task<bool> CanManage(JwtClaimData u, Guid organizationId, TagUser permission, CancellationToken ct);
	public Task<string?> PlanError(JwtClaimData u, Guid? organizationId, Func<OrganizationPlan, int?> limit, Func<Task<int>> count, CancellationToken ct);
	public Task<int> PlaceCount(Guid organizationId, CancellationToken ct);
	public Task<Guid?> OrganizationOfPlace(Guid placeId, CancellationToken ct);
	public Task<bool> IsBlacklisted(Guid? organizationId, Guid userId, CancellationToken ct);
	public Task<OrganizationEntity?> ReadOrganization(Guid? organizationId, CancellationToken ct);
	public Task<UResponse<Guid?>> CreateShift(StaffShiftCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<StaffShiftResponse>?>> ReadShifts(StaffShiftReadParams p, CancellationToken ct);
	public Task<UResponse> UpdateShift(StaffShiftUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteShift(IdParams p, CancellationToken ct);
	public Task<UResponse> ClockShift(IdParams p, CancellationToken ct);
	public Task<UResponse<Guid?>> CreateTask(StaffTaskCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<StaffTaskResponse>?>> ReadTasks(StaffTaskReadParams p, CancellationToken ct);
	public Task<UResponse> UpdateTask(StaffTaskUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteTask(IdParams p, CancellationToken ct);
	public Task<UResponse> SetCustomer(OrganizationCustomerSetParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<OrganizationCustomerResponse>?>> ReadCustomers(OrganizationCustomerReadParams p, CancellationToken ct);
	public Task<UResponse> DeleteCustomer(IdParams p, CancellationToken ct);
	public Task LogActivity(JwtClaimData u, string path, BaseParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<ActivityLogResponse>?>> ReadActivityLogs(ActivityLogReadParams p, CancellationToken ct);
}

public interface IPlaceResidency {
	Task<bool> IsResidentOf(Guid userId, Guid placeId, CancellationToken ct);
}

public class OrganizationService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IServiceProvider sp
) : IOrganizationService, IUserScope {
	private static readonly MethodInfo ReplaceInMethod = typeof(OrganizationService).GetMethod(nameof(ReplaceIn), BindingFlags.NonPublic | BindingFlags.Instance)!;

	private static readonly MethodInfo AnyInMethod = typeof(OrganizationService).GetMethod(nameof(AnyIn), BindingFlags.NonPublic | BindingFlags.Instance)!;

	private static readonly MethodInfo TitlesInMethod = typeof(OrganizationService).GetMethod(nameof(TitlesIn), BindingFlags.NonPublic | BindingFlags.Instance)!;

	private static readonly MethodInfo CountInMethod = typeof(OrganizationService).GetMethod(nameof(CountIn), BindingFlags.NonPublic | BindingFlags.Instance)!;

	private static readonly MethodInfo OrganizationInMethod = typeof(OrganizationService).GetMethod(nameof(OrganizationIn), BindingFlags.NonPublic | BindingFlags.Instance)!;

	public static bool IsFull(JwtClaimData? u) => u is { IsSuperAdmin: true };

	public static bool IsScopedAdmin(JwtClaimData? u) => !IsFull(u) && (u is { IsSubAdmin: true } || Core.App.MultiTenant && u?.Tags.Contains(TagUser.SuperAdmin) == true);

	public static Guid WalletOf(Guid? organizationId) => organizationId != null ? organizationId.Value : Core.App.Users.SystemAdmin.Id;

	public static bool TouchesAdminUserIds<T>(BaseUpdateParams<T> p) => p.AdminUserIds.IsNotNullOrEmpty() || p.AddAdminUserIds.IsNotNullOrEmpty() || p.RemoveAdminUserIds.IsNotNullOrEmpty();

	public async Task<bool> CanCreatePlace(JwtClaimData u, Guid? organizationId, TagUser permission, CancellationToken ct) =>
		Core.App.MultiTenant ? u.IsSystemAdmin || organizationId != null && await HasOrganizationPermission(u, organizationId, permission, ct) : u.HasPermission(permission);

	public async Task<bool> IsOwner(JwtClaimData u, Guid? organizationId, CancellationToken ct) =>
		organizationId != null && await db.Set<OrganizationEntity>().AnyAsync(x => x.Id == organizationId && x.OwnerId == u.Id, ct);

	public async Task<List<Guid>> PlaceAdmins(Guid? organizationId, ICollection<Guid> ids, CancellationToken ct) {
		if (!Core.App.MultiTenant || organizationId == null) return ids.ToList();
		OrganizationEntity? o = await db.Set<OrganizationEntity>().FirstOrDefaultAsync(x => x.Id == organizationId, ct);
		return o == null ? [] : ids.Where(o.AdminUserIds.Contains).Append(o.OwnerId).Distinct().ToList();
	}

	public async Task<UResponse<Guid?>> CreateOrganization(OrganizationCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsSystemAdmin) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

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
			JsonData = new OrganizationJson {
				Detail1 = p.Detail1,
				Detail2 = p.Detail2,
				CommissionPercent = p.CommissionPercent,
				Plan = p.Plan,
				LogoUrl = p.LogoUrl,
				Address = p.Address,
				PhoneNumber = p.PhoneNumber,
				NationalId = p.NationalId,
				EconomicCode = p.EconomicCode,
				VatPercent = Math.Clamp(p.VatPercent ?? 0, 0, 100),
				TaxServiceId = p.TaxServiceId
			}
		}, ct);
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
		if (TouchesAdminUserIds(p)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		OrganizationEntity? e = await db.Set<OrganizationEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("organizationNotFound"));

		bool platformChange = p.OwnerId.HasValue && p.OwnerId != e.OwnerId || p.CommissionPercent.HasValue || p.Plan != null || p.Tags != null || p.AddTags != null || p.RemoveTags != null;
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
		if (p.Plan != null) e.JsonData.Plan = p.Plan;
		if (p.LogoUrl != null) e.JsonData.LogoUrl = p.LogoUrl.NullIfEmpty();
		if (p.Address != null) e.JsonData.Address = p.Address.NullIfEmpty();
		if (p.PhoneNumber != null) e.JsonData.PhoneNumber = p.PhoneNumber.NullIfEmpty();
		if (p.NationalId != null) e.JsonData.NationalId = p.NationalId.NullIfEmpty();
		if (p.EconomicCode != null) e.JsonData.EconomicCode = p.EconomicCode.NullIfEmpty();
		if (p.VatPercent != null) e.JsonData.VatPercent = Math.Clamp(p.VatPercent.Value, 0, 100);
		if (p.TaxServiceId != null) e.JsonData.TaxServiceId = p.TaxServiceId.NullIfEmpty();
		e.ApplyUpdateParam<OrganizationEntity, TagOrganization, OrganizationJson>(p);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> SetOrganizationMember(OrganizationMemberParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

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

	private IEnumerable<Type> ScopedTypes() => db.Model.GetEntityTypes().Select(x => x.ClrType).Where(x => typeof(IOrganizationScoped).IsAssignableFrom(x)).Distinct();

	private async Task ReplacePlaceAdmin(Guid organizationId, Guid oldId, Guid? newId, CancellationToken ct) {
		foreach (Type t in ScopedTypes()) await (Task)ReplaceInMethod.MakeGenericMethod(t).Invoke(this, [organizationId, oldId, newId, ct])!;
	}

	private async Task ReplaceIn<T>(Guid organizationId, Guid oldId, Guid? newId, CancellationToken ct) where T : class, IOrganizationScoped {
		foreach (T x in await db.Set<T>().AsTracking().Where(x => x.OrganizationId == organizationId).ToListAsync(ct)) x.AdminUserIds = SwapId(x.AdminUserIds, oldId, newId);
	}

	public async Task<bool> IsPlaceOf(Guid organizationId, Guid placeId, CancellationToken ct) {
		foreach (Type t in ScopedTypes())
			if (await (Task<bool>)AnyInMethod.MakeGenericMethod(t).Invoke(this, [organizationId, placeId, ct])!)
				return true;
		return false;
	}

	private Task<bool> AnyIn<T>(Guid organizationId, Guid placeId, CancellationToken ct) where T : class, IOrganizationScoped =>
		db.Set<T>().AnyAsync(x => x.Id == placeId && x.OrganizationId == organizationId, ct);

	public async Task<Dictionary<Guid, string>> PlaceTitles(Guid organizationId, CancellationToken ct) {
		Dictionary<Guid, string> titles = [];
		foreach (Type t in ScopedTypes())
			foreach (KeyValuePair<Guid, string> x in await (Task<Dictionary<Guid, string>>)TitlesInMethod.MakeGenericMethod(t).Invoke(this, [organizationId, ct])!)
				titles[x.Key] = x.Value;
		return titles;
	}

	private Task<Dictionary<Guid, string>> TitlesIn<T>(Guid organizationId, CancellationToken ct) where T : class, IOrganizationScoped =>
		db.Set<T>().Where(x => x.OrganizationId == organizationId).ToDictionaryAsync(x => x.Id, x => x.Title, ct);

	private async Task SyncMemberTags(UserEntity user, OrganizationEntity changed, CancellationToken ct) {
		List<OrganizationEntity> organizations = await db.Set<OrganizationEntity>().Where(x => x.Id != changed.Id && x.AdminUserIds.Contains(user.Id)).ToListAsync(ct);
		if (changed.AdminUserIds.Contains(user.Id)) organizations.Add(changed);
		List<TagUser> tags = user.Tags.Where(x => x != TagUser.SubAdmin && (int)x is < 600 or >= 700).ToList();
		if (organizations.Count > 0) tags = [..tags, TagUser.SubAdmin, ..organizations.SelectMany(x => x.JsonData.Members.Where(m => m.UserId == user.Id).SelectMany(m => m.Permissions)).Distinct()];
		user.Tags = tags;
	}

	public async Task<bool> HasOrganizationPermission(JwtClaimData u, Guid? organizationId, TagUser permission, CancellationToken ct) {
		List<OrganizationEntity> organizations = await db.Set<OrganizationEntity>()
			.Where(x => (organizationId == null || x.Id == organizationId) && !x.Tags.Contains(TagOrganization.Inactive) && (x.OwnerId == u.Id || x.AdminUserIds.Contains(u.Id)))
			.ToListAsync(ct);
		return organizations.Any(x => x.OwnerId == u.Id || x.JsonData.Members.Any(m => m.UserId == u.Id && m.Permissions.Contains(permission)));
	}

	private static bool CanActOnPlace(JwtClaimData u, ICollection<Guid> placeAdminUserIds, TagUser permission) => u.HasPermission(permission) && (u.IsSuperAdmin || u.IsSubAdmin && placeAdminUserIds.Contains(u.Id));

	public async Task<bool> CanActOnPlace(JwtClaimData u, Guid? organizationId, ICollection<Guid> adminUserIds, TagUser permission, CancellationToken ct) {
		if (!Core.App.MultiTenant) return CanActOnPlace(u, adminUserIds, permission);
		if (u.IsSystemAdmin) return true;
		return organizationId != null && adminUserIds.Contains(u.Id) && await HasOrganizationPermission(u, organizationId, permission, ct);
	}

	public IQueryable<Guid> RelatedUserIds(Guid userId) {
		IQueryable<OrganizationEntity> mine = db.Set<OrganizationEntity>().Where(o => o.OwnerId == userId || o.AdminUserIds.Contains(userId));
		return mine.Select(o => o.OwnerId).Union(mine.SelectMany(o => o.AdminUserIds));
	}

	public async Task<bool> CanManage(JwtClaimData u, Guid organizationId, TagUser permission, CancellationToken ct) =>
		IsFull(u) || !Core.App.MultiTenant && u.HasPermission(permission) || await HasOrganizationPermission(u, organizationId, permission, ct);

	public Task<OrganizationEntity?> ReadOrganization(Guid? organizationId, CancellationToken ct) =>
		organizationId == null ? Task.FromResult<OrganizationEntity?>(null) : db.Set<OrganizationEntity>().FirstOrDefaultAsync(x => x.Id == organizationId, ct);

	public async Task<string?> PlanError(JwtClaimData u, Guid? organizationId, Func<OrganizationPlan, int?> limit, Func<Task<int>> count, CancellationToken ct) {
		if (organizationId == null || u.IsSystemAdmin) return null;
		OrganizationPlan? plan = (await ReadOrganization(organizationId, ct))?.JsonData.Plan;
		if (plan == null) return null;
		if (plan.ExpiresAt != null && plan.ExpiresAt < DateTime.UtcNow) return ls.Get("organizationPlanExpired");
		int? max = limit(plan);
		return max != null && await count() >= max ? ls.Get("organizationPlanLimitReached") : null;
	}

	public async Task<int> PlaceCount(Guid organizationId, CancellationToken ct) {
		int count = 0;
		foreach (Type t in ScopedTypes()) count += await (Task<int>)CountInMethod.MakeGenericMethod(t).Invoke(this, [organizationId, ct])!;
		return count;
	}

	private Task<int> CountIn<T>(Guid organizationId, CancellationToken ct) where T : class, IOrganizationScoped =>
		db.Set<T>().CountAsync(x => x.OrganizationId == organizationId, ct);

	public async Task<Guid?> OrganizationOfPlace(Guid placeId, CancellationToken ct) {
		foreach (Type t in ScopedTypes()) {
			Guid? id = await (Task<Guid?>)OrganizationInMethod.MakeGenericMethod(t).Invoke(this, [placeId, ct])!;
			if (id != null) return id;
		}

		return null;
	}

	private Task<Guid?> OrganizationIn<T>(Guid placeId, CancellationToken ct) where T : class, IOrganizationScoped =>
		db.Set<T>().Where(x => x.Id == placeId).Select(x => x.OrganizationId).FirstOrDefaultAsync(ct);

	public async Task<bool> IsBlacklisted(Guid? organizationId, Guid userId, CancellationToken ct) =>
		organizationId != null && await db.Set<OrganizationCustomerEntity>().AnyAsync(x => x.OrganizationId == organizationId && x.UserId == userId && x.Tags.Contains(TagOrganizationCustomer.Blacklisted), ct);

	private async Task AddNotification(Guid userId, string title, string body, CancellationToken ct) =>
		await db.Set<NotificationEntity>().AddAsync(new NotificationEntity {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = userId,
			UserId = userId,
			Tags = [TagNotification.General, TagNotification.Unread],
			JsonData = new NotificationJson { Detail1 = title, Detail2 = body }
		}, ct);

	private async Task<(JwtClaimData? User, UResponse? Error)> Staff(string? token, Guid? organizationId, TagUser permission, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(token);
		if (u == null) return (null, new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue")));
		if (u.IsExpired) return (null, new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired")));
		if (organizationId == null || !await db.Set<OrganizationEntity>().AnyAsync(x => x.Id == organizationId, ct)) return (null, new UResponse(Usc.NotFound, ls.Get("organizationNotFound")));
		return await CanManage(u, organizationId.Value, permission, ct) ? (u, null) : (null, new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction")));
	}

	private IQueryable<string?> NameOf(Guid? userId) => db.Set<UserEntity>().Where(x => x.Id == userId).Select(x => x.FirstName + " " + x.LastName);

	public async Task<UResponse<Guid?>> CreateShift(StaffShiftCreateParams p, CancellationToken ct) {
		(JwtClaimData? u, UResponse? error) = await Staff(p.Token, p.OrganizationId, TagUser.PermissionManageStaff, ct);
		if (error != null) return new UResponse<Guid?>(null, error.Status, error.Message);
		if (p.EndAt <= p.StartAt) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("endDateMustBeAfterStartDate"));
		if (!await db.Set<UserEntity>().AnyAsync(x => x.Id == p.UserId, ct)) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("accountNotFound"));

		Guid id = Guid.CreateVersion7();
		await db.Set<StaffShiftEntity>().AddAsync(new StaffShiftEntity {
			Id = id,
			CreatorId = p.OrganizationId,
			CreatedAt = DateTime.UtcNow,
			Tags = [..p.Tags.Where(x => (int)x < 200).Distinct(), TagStaffShift.Planned],
			StartAt = p.StartAt,
			EndAt = p.EndAt,
			PlaceId = p.PlaceId,
			UserId = p.UserId,
			OrganizationId = p.OrganizationId,
			JsonData = new StaffShiftJson { Detail1 = p.Detail1, Detail2 = p.Detail2, RegisteredBy = u!.Id }
		}, ct);
		await AddNotification(p.UserId, ls.Get("newShiftAssigned", "fa"), p.StartAt.ToPersian().ToString(), ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<StaffShiftResponse>?>> ReadShifts(StaffShiftReadParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u == null) return new UResponse<IEnumerable<StaffShiftResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (u.IsExpired) return new UResponse<IEnumerable<StaffShiftResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		IQueryable<StaffShiftEntity> q = db.Set<StaffShiftEntity>().ApplyReadParams(p);
		if (p.OrganizationId != null && await CanManage(u, p.OrganizationId.Value, TagUser.PermissionManageStaff, ct)) q = q.Where(x => x.OrganizationId == p.OrganizationId);
		else {
			Guid me = u.Id;
			q = q.Where(x => x.UserId == me);
			if (p.OrganizationId != null) q = q.Where(x => x.OrganizationId == p.OrganizationId);
		}

		if (p.UserId != null) q = q.Where(x => x.UserId == p.UserId);
		if (p.PlaceId != null) q = q.Where(x => x.PlaceId == p.PlaceId);
		if (p.FromDate != null) q = q.Where(x => x.EndAt >= p.FromDate);
		if (p.ToDate != null) q = q.Where(x => x.StartAt <= p.ToDate);

		return await q.Select(x => new StaffShiftResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Tags = x.Tags,
			JsonData = x.JsonData,
			StartAt = x.StartAt,
			EndAt = x.EndAt,
			PlaceId = x.PlaceId,
			UserId = x.UserId,
			UserName = x.User.FirstName + " " + x.User.LastName,
			OrganizationId = x.OrganizationId
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateShift(StaffShiftUpdateParams p, CancellationToken ct) {
		StaffShiftEntity? e = await db.Set<StaffShiftEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		(_, UResponse? error) = await Staff(p.Token, e.OrganizationId, TagUser.PermissionManageStaff, ct);
		if (error != null) return error;

		if (p.UserId != null) e.UserId = p.UserId.Value;
		if (p.StartAt != null) e.StartAt = p.StartAt.Value;
		if (p.EndAt != null) e.EndAt = p.EndAt.Value;
		if (p.PlaceId != null) e.PlaceId = p.PlaceId;
		e.ApplyUpdateParam<StaffShiftEntity, TagStaffShift, StaffShiftJson>(p);
		if (e.EndAt <= e.StartAt) return new UResponse(Usc.BadRequest, ls.Get("endDateMustBeAfterStartDate"));
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteShift(IdParams p, CancellationToken ct) {
		StaffShiftEntity? e = await db.Set<StaffShiftEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		(_, UResponse? error) = await Staff(p.Token, e.OrganizationId, TagUser.PermissionManageStaff, ct);
		if (error != null) return error;
		db.Set<StaffShiftEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> ClockShift(IdParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		StaffShiftEntity? e = await db.Set<StaffShiftEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		if (e.UserId != u.Id && !await CanManage(u, e.OrganizationId, TagUser.PermissionManageStaff, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (e.JsonData.CheckedOutAt != null) return new UResponse(Usc.Conflict, ls.Get("shiftIsAlreadyClosed"));

		if (e.JsonData.CheckedInAt == null) {
			e.JsonData.CheckedInAt = DateTime.UtcNow;
			e.Tags = [..e.Tags.Where(x => (int)x < 200), TagStaffShift.Present];
		}
		else e.JsonData.CheckedOutAt = DateTime.UtcNow;

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	private static List<TagStaffTask> TaskTags(IEnumerable<TagStaffTask> tags, TagStaffTask? kind = null) {
		List<TagStaffTask> list = tags.Distinct().ToList();
		TagStaffTask k = kind ?? list.FirstOrDefault(x => (int)x < 200, TagStaffTask.Task);
		TagStaffTask status = list.FirstOrDefault(x => (int)x is >= 200 and < 300, TagStaffTask.Open);
		TagStaffTask priority = list.FirstOrDefault(x => (int)x >= 300, TagStaffTask.Normal);
		return [k, status, priority];
	}

	public async Task<UResponse<Guid?>> CreateTask(StaffTaskCreateParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (u.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		Guid? organizationId = p.OrganizationId ?? (p.PlaceId == null ? null : await OrganizationOfPlace(p.PlaceId.Value, ct));
		if (organizationId == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("organizationNotFound"));
		if (p.PlaceId != null && !await IsPlaceOf(organizationId.Value, p.PlaceId.Value, ct)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		bool staff = await CanManage(u, organizationId.Value, TagUser.PermissionManageStaff, ct);
		if (!staff) {
			bool resident = false;
			if (p.PlaceId != null)
				foreach (IPlaceResidency r in sp.GetServices<IPlaceResidency>())
					resident = resident || await r.IsResidentOf(u.Id, p.PlaceId.Value, ct);
			if (!resident) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		}

		OrganizationEntity? o = await ReadOrganization(organizationId, ct);
		Guid id = Guid.CreateVersion7();
		await db.Set<StaffTaskEntity>().AddAsync(new StaffTaskEntity {
			Id = id,
			CreatorId = organizationId.Value,
			CreatedAt = DateTime.UtcNow,
			Tags = staff ? TaskTags(p.Tags) : TaskTags(p.Tags.Where(x => (int)x >= 300), TagStaffTask.Maintenance),
			Title = p.Title,
			PlaceId = p.PlaceId,
			AssigneeId = staff ? p.AssigneeId : null,
			DueDate = staff ? p.DueDate : null,
			OrganizationId = organizationId.Value,
			JsonData = new StaffTaskJson {
				Detail1 = p.Detail1,
				Detail2 = p.Detail2,
				Description = p.Description,
				Location = p.Location,
				RoomId = p.RoomId,
				BedId = p.BedId,
				RequesterId = u.Id
			}
		}, ct);
		if (staff && p.AssigneeId != null) await AddNotification(p.AssigneeId.Value, ls.Get("newTaskAssigned", "fa"), p.Title, ct);
		if (!staff && o != null) await AddNotification(o.OwnerId, ls.Get("newMaintenanceRequest", "fa"), p.Title, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<StaffTaskResponse>?>> ReadTasks(StaffTaskReadParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u == null) return new UResponse<IEnumerable<StaffTaskResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (u.IsExpired) return new UResponse<IEnumerable<StaffTaskResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		Guid me = u.Id;
		IQueryable<StaffTaskEntity> q = db.Set<StaffTaskEntity>().ApplyReadParams(p);
		if (!p.Mine && p.OrganizationId != null && await CanManage(u, p.OrganizationId.Value, TagUser.PermissionManageStaff, ct)) q = q.Where(x => x.OrganizationId == p.OrganizationId);
		else {
			q = q.Where(x => x.AssigneeId == me || x.JsonData.RequesterId == me);
			if (p.OrganizationId != null) q = q.Where(x => x.OrganizationId == p.OrganizationId);
		}

		if (p.PlaceId != null) q = q.Where(x => x.PlaceId == p.PlaceId);
		if (p.AssigneeId != null) q = q.Where(x => x.AssigneeId == p.AssigneeId);

		return await q.Select(x => new StaffTaskResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Title = x.Title,
			PlaceId = x.PlaceId,
			AssigneeId = x.AssigneeId,
			AssigneeName = db.Set<UserEntity>().Where(y => y.Id == x.AssigneeId).Select(y => y.FirstName + " " + y.LastName).FirstOrDefault(),
			RequesterName = db.Set<UserEntity>().Where(y => y.Id == x.JsonData.RequesterId).Select(y => y.FirstName + " " + y.LastName).FirstOrDefault(),
			DueDate = x.DueDate,
			OrganizationId = x.OrganizationId
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateTask(StaffTaskUpdateParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		StaffTaskEntity? e = await db.Set<StaffTaskEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		bool staff = await CanManage(u, e.OrganizationId, TagUser.PermissionManageStaff, ct);
		if (!staff && e.AssigneeId != u.Id) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		bool wasDone = e.Tags.Contains(TagStaffTask.Done);
		Guid? oldAssignee = e.AssigneeId;
		if (staff) {
			if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title!;
			if (p.PlaceId != null) e.PlaceId = p.PlaceId;
			if (p.AssigneeId != null) e.AssigneeId = p.AssigneeId;
			if (p.DueDate != null) e.DueDate = p.DueDate;
			if (p.Description != null) e.JsonData.Description = p.Description;
			if (p.Location != null) e.JsonData.Location = p.Location;
		}

		if (p.DoneNote != null) e.JsonData.DoneNote = p.DoneNote;
		if (p.Cost != null) e.JsonData.Cost = p.Cost;
		if (p.Detail1.IsNotNullOrEmpty()) e.JsonData.Detail1 = p.Detail1!;
		if (p.Detail2.IsNotNullOrEmpty()) e.JsonData.Detail2 = p.Detail2!;
		List<TagStaffTask> incoming = [..p.Tags ?? p.AddTags ?? []];
		if (incoming.Count != 0) e.Tags = TaskTags([..incoming, ..e.Tags.Where(x => incoming.All(t => (int)t / 100 != (int)x / 100))]);

		bool done = e.Tags.Contains(TagStaffTask.Done);
		if (done && !wasDone) {
			e.JsonData.DoneAt = DateTime.UtcNow;
			if (e.JsonData.RequesterId != null && e.JsonData.RequesterId != u.Id) await AddNotification(e.JsonData.RequesterId.Value, ls.Get("yourRequestIsDone", "fa"), e.Title, ct);
		}

		if (e.AssigneeId != null && e.AssigneeId != oldAssignee) await AddNotification(e.AssigneeId.Value, ls.Get("newTaskAssigned", "fa"), e.Title, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteTask(IdParams p, CancellationToken ct) {
		StaffTaskEntity? e = await db.Set<StaffTaskEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		(_, UResponse? error) = await Staff(p.Token, e.OrganizationId, TagUser.PermissionManageStaff, ct);
		if (error != null) return error;
		db.Set<StaffTaskEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> SetCustomer(OrganizationCustomerSetParams p, CancellationToken ct) {
		(JwtClaimData? u, UResponse? error) = await Staff(p.Token, p.OrganizationId, TagUser.PermissionManageUsers, ct);
		if (error != null) return error;
		if (!await db.Set<UserEntity>().AnyAsync(x => x.Id == p.UserId, ct)) return new UResponse(Usc.NotFound, ls.Get("accountNotFound"));

		OrganizationCustomerEntity? e = await db.Set<OrganizationCustomerEntity>().AsTracking().FirstOrDefaultAsync(x => x.OrganizationId == p.OrganizationId && x.UserId == p.UserId, ct);
		List<TagOrganizationCustomer> tags = p.Tags.Count == 0 ? [TagOrganizationCustomer.Regular] : p.Tags.Distinct().ToList();
		if (e == null)
			await db.Set<OrganizationCustomerEntity>().AddAsync(new OrganizationCustomerEntity {
				Id = Guid.CreateVersion7(),
				CreatorId = p.OrganizationId,
				CreatedAt = DateTime.UtcNow,
				Tags = tags,
				UserId = p.UserId,
				OrganizationId = p.OrganizationId,
				JsonData = new OrganizationCustomerJson { Note = p.Note, RegisteredBy = u!.Id }
			}, ct);
		else {
			e.Tags = tags;
			e.JsonData.Note = p.Note;
			e.JsonData.RegisteredBy = u!.Id;
		}

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<IEnumerable<OrganizationCustomerResponse>?>> ReadCustomers(OrganizationCustomerReadParams p, CancellationToken ct) {
		(_, UResponse? error) = await Staff(p.Token, p.OrganizationId, TagUser.PermissionManageUsers, ct);
		if (error != null) return new UResponse<IEnumerable<OrganizationCustomerResponse>?>(null, error.Status, error.Message);

		IQueryable<OrganizationCustomerEntity> q = db.Set<OrganizationCustomerEntity>().Where(x => x.OrganizationId == p.OrganizationId).ApplyReadParams(p);
		if (p.UserId != null) q = q.Where(x => x.UserId == p.UserId);
		return await q.Select(x => new OrganizationCustomerResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Tags = x.Tags,
			JsonData = x.JsonData,
			UserId = x.UserId,
			UserName = x.User.FirstName + " " + x.User.LastName,
			PhoneNumber = x.User.PhoneNumber,
			NationalCode = x.User.NationalCode,
			OrganizationId = x.OrganizationId
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> DeleteCustomer(IdParams p, CancellationToken ct) {
		OrganizationCustomerEntity? e = await db.Set<OrganizationCustomerEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		(_, UResponse? error) = await Staff(p.Token, e.OrganizationId, TagUser.PermissionManageUsers, ct);
		if (error != null) return error;
		db.Set<OrganizationCustomerEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	private static readonly string[] HiddenFields = ["token", "apiKey", "password", "ownerPassword", "newPassword"];

	public async Task LogActivity(JwtClaimData u, string path, BaseParams p, CancellationToken ct) {
		Guid? organizationId = p.GetType().GetProperty("OrganizationId")?.GetValue(p) as Guid?;
		if (organizationId == null) {
			List<Guid> mine = await db.Set<OrganizationEntity>().Where(x => x.OwnerId == u.Id || x.AdminUserIds.Contains(u.Id)).Select(x => x.Id).Take(2).ToListAsync(ct);
			if (mine.Count == 1) organizationId = mine[0];
		}

		if (organizationId == null) return;

		JsonObject? body = JsonSerializer.SerializeToNode(p, p.GetType(), Core.Default) as JsonObject;
		if (body != null)
			foreach (string key in body.Select(x => x.Key).Where(k => HiddenFields.Any(h => h.Equals(k, StringComparison.OrdinalIgnoreCase))).ToList())
				body.Remove(key);
		string text = body?.ToJsonString() ?? "";
		string name = path.Split('/').LastOrDefault() ?? "";
		await db.Set<ActivityLogEntity>().AddAsync(new ActivityLogEntity {
			Id = Guid.CreateVersion7(),
			CreatorId = u.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = [name.StartsWith("Create") ? TagActivityLog.Create : name.StartsWith("Update") ? TagActivityLog.Update : name.StartsWith("Delete") || name.StartsWith("Remove") ? TagActivityLog.Delete : TagActivityLog.Action],
			Path = path.Length > 200 ? path[..200] : path,
			OrganizationId = organizationId,
			EntityId = p.GetType().GetProperty("Id")?.GetValue(p) as Guid?,
			JsonData = new ActivityLogJson { UserName = u.FullName.NullIfEmpty() ?? $"{u.FirstName} {u.LastName}".NullIfEmpty() ?? u.UserName, Body = text.Length > 4000 ? text[..4000] : text }
		}, ct);
		await db.SaveChangesAsync(ct);
	}

	public async Task<UResponse<IEnumerable<ActivityLogResponse>?>> ReadActivityLogs(ActivityLogReadParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u == null) return new UResponse<IEnumerable<ActivityLogResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (u.IsExpired) return new UResponse<IEnumerable<ActivityLogResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!u.IsSystemAdmin && (p.OrganizationId == null || !await CanManage(u, p.OrganizationId.Value, TagUser.PermissionManageStaff, ct)))
			return new UResponse<IEnumerable<ActivityLogResponse>?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		IQueryable<ActivityLogEntity> q = db.Set<ActivityLogEntity>().ApplyReadParams(p);
		if (p.OrganizationId != null) q = q.Where(x => x.OrganizationId == p.OrganizationId);
		if (p.UserId != null) q = q.Where(x => x.CreatorId == p.UserId);
		if (p.Path.IsNotNullOrEmpty()) q = q.Where(x => x.Path.Contains(p.Path!));
		return await q.Select(x => new ActivityLogResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Path = x.Path,
			OrganizationId = x.OrganizationId,
			EntityId = x.EntityId
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}
}
