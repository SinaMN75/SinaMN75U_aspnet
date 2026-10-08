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
	public Task<bool> HasOrganizationPermission(JwtClaimData u, Guid? organizationId, TagUser permission, CancellationToken ct, TagModule? module = null);
	public Task<bool> CanActOnPlace(JwtClaimData u, Guid? organizationId, ICollection<Guid> adminUserIds, TagUser permission, CancellationToken ct);
	public IQueryable<Guid> RelatedUserIds(Guid userId);
	public Task<bool> CanManage(JwtClaimData u, Guid organizationId, TagUser permission, CancellationToken ct, TagModule? module = null);
	public Task<bool> HasModule(JwtClaimData? u, Guid? organizationId, TagModule module, CancellationToken ct);
	public Task<string?> PlanError(JwtClaimData u, Guid? organizationId, TagPlanLimit limit, Func<Task<int>> count, CancellationToken ct);
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
	public Task<UResponse<Guid?>> CreatePlan(SubscriptionPlanCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<SubscriptionPlanResponse>?>> ReadPlans(SubscriptionPlanReadParams p, CancellationToken ct);
	public Task<UResponse> UpdatePlan(SubscriptionPlanUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeletePlan(IdParams p, CancellationToken ct);
	public Task<UResponse<SubscriptionQuoteResponse?>> QuoteSubscription(SubscriptionQuoteParams p, CancellationToken ct);
	public Task<UResponse<SubscriptionBuyResponse?>> BuySubscription(SubscriptionBuyParams p, CancellationToken ct);
	public Task<UResponse> PaySubscription(IdParams p, CancellationToken ct);
	public Task<UResponse> PaySubscriptionInternal(Guid organizationId, Guid userId, CancellationToken ct);
	public Task<UResponse> GrantSubscription(SubscriptionGrantParams p, CancellationToken ct);
	public Task<UResponse> CancelSubscription(SubscriptionCancelParams p, CancellationToken ct);
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
		Core.App.MultiTenant
			? u.IsSystemAdmin || organizationId != null && await HasOrganizationPermission(u, organizationId, permission, ct)
			: u.HasPermission(permission) && await HasModuleFor(u, organizationId, permission, null, ct);

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

		OrganizationEntity e = await AddOrganization(p.Id ?? Guid.CreateVersion7(), p.Title, owner, userData.Id, p.Tags, new OrganizationJson {
			Detail1 = p.Detail1,
			Detail2 = p.Detail2,
			CommissionPercent = p.CommissionPercent,
			LogoUrl = p.LogoUrl,
			Address = p.Address,
			PhoneNumber = p.PhoneNumber,
			NationalId = p.NationalId,
			EconomicCode = p.EconomicCode,
			VatPercent = Math.Clamp(p.VatPercent ?? 0, 0, 100),
			TaxServiceId = p.TaxServiceId
		}, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	private async Task<OrganizationEntity> AddOrganization(Guid id, string title, UserEntity owner, Guid creatorId, ICollection<TagOrganization> tags, OrganizationJson json, CancellationToken ct) {
		if (!owner.Tags.Contains(TagUser.SuperAdmin)) owner.Tags = [..owner.Tags, TagUser.SuperAdmin];
		DateTime now = DateTime.UtcNow;
		await db.Set<UserEntity>().AddAsync(new UserEntity {
			Id = id,
			CreatorId = creatorId,
			CreatedAt = now,
			UserName = "organization_" + id.ToString("N"),
			Password = UPasswordHasher.Hash(Guid.NewGuid().ToString()),
			RefreshToken = "",
			FirstName = title,
			JsonData = new UserJson(),
			Tags = [TagUser.Organization],
			Wallets = [new WalletEntity { Id = id, CreatorId = id, CreatedAt = now, JsonData = new WalletJson(), Tags = [TagWallet.Primary], Balance = 0 }]
		}, ct);
		OrganizationEntity e = new() {
			Id = id,
			CreatorId = creatorId,
			CreatedAt = now,
			Title = title,
			OwnerId = owner.Id,
			Tags = tags,
			JsonData = json
		};
		await db.Set<OrganizationEntity>().AddAsync(e, ct);
		return e;
	}

	public async Task<UResponse<IEnumerable<OrganizationResponse>?>> ReadOrganizations(OrganizationReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<OrganizationResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<OrganizationResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		Guid uid = userData.Id;
		IQueryable<OrganizationEntity> q = db.Set<OrganizationEntity>().ApplyReadParams(p);
		if (!IsFull(userData)) q = q.Where(x => x.OwnerId == uid || x.AdminUserIds.Contains(uid));
		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title!));

		UResponse<IEnumerable<OrganizationResponse>?> r = await q.Select(x => new OrganizationResponse {
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
		DateTime now = DateTime.UtcNow;
		foreach (OrganizationResponse o in r.Result ?? []) {
			o.Modules = ModulesOf(o.JsonData, now);
			o.SubscriptionEndsAt = o.JsonData.Subscriptions.Where(x => IsLive(x, now)).Max(x => x.ExpiresAt);
		}

		return r;
	}

	public async Task<UResponse> UpdateOrganization(OrganizationUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (TouchesAdminUserIds(p)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

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

		if (e.JsonData.Members.All(x => x.UserId != user.Id)) {
			string? planError = await PlanError(userData, e.Id, TagPlanLimit.Members, () => Task.FromResult(e.JsonData.Members.Count), ct);
			if (planError != null) return new UResponse(Usc.Forbidden, planError);
		}

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

	public async Task<UResponse<Guid?>> CreatePlan(SubscriptionPlanCreateParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u is not { IsSystemAdmin: true }) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		string? error = PlanDataError(p.Modules, p.Prices);
		if (error != null) return new UResponse<Guid?>(null, Usc.BadRequest, error);

		Guid id = p.Id ?? Guid.CreateVersion7();
		await db.Set<SubscriptionPlanEntity>().AddAsync(new SubscriptionPlanEntity {
			Id = id,
			CreatorId = u.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = p.Tags.Count == 0 ? [TagSubscriptionPlan.Active] : p.Tags,
			Title = p.Title,
			Order = p.Order,
			JsonData = new SubscriptionPlanJson {
				Detail1 = p.Detail1,
				Detail2 = p.Detail2,
				Modules = p.Modules.Distinct().ToList(),
				Prices = p.Prices.OrderBy(x => x.Months).ToList(),
				Limits = p.Limits.Where(x => x.Value > 0).ToList(),
				Features = p.Features.Where(x => x.IsNotNullOrEmpty()).ToList(),
				TrialDays = Math.Max(0, p.TrialDays)
			}
		}, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<SubscriptionPlanResponse>?>> ReadPlans(SubscriptionPlanReadParams p, CancellationToken ct) {
		bool all = ts.ExtractClaims(p.Token) is { IsSystemAdmin: true };
		IQueryable<SubscriptionPlanEntity> q = db.Set<SubscriptionPlanEntity>().ApplyReadParams(p);
		if (!all) q = q.Where(x => x.Tags.Contains(TagSubscriptionPlan.Active));
		List<SubscriptionPlanResponse> list = await q.OrderBy(x => x.Order).ThenBy(x => x.CreatedAt).Select(x => new SubscriptionPlanResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Title = x.Title,
			Order = x.Order
		}).ToListAsync(ct);
		if (p.Module != null) list = list.Where(x => x.JsonData.Modules.Contains(p.Module.Value)).ToList();
		return new UResponse<IEnumerable<SubscriptionPlanResponse>?>(list);
	}

	public async Task<UResponse> UpdatePlan(SubscriptionPlanUpdateParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u is not { IsSystemAdmin: true }) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		SubscriptionPlanEntity? e = await db.Set<SubscriptionPlanEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("planNotFound"));
		string? error = PlanDataError(p.Modules ?? e.JsonData.Modules, p.Prices ?? e.JsonData.Prices);
		if (error != null) return new UResponse(Usc.BadRequest, error);

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		if (p.Order != null) e.Order = p.Order.Value;
		if (p.Modules != null) e.JsonData.Modules = p.Modules.Distinct().ToList();
		if (p.Prices != null) e.JsonData.Prices = p.Prices.OrderBy(x => x.Months).ToList();
		if (p.Limits != null) e.JsonData.Limits = p.Limits.Where(x => x.Value > 0).ToList();
		if (p.Features != null) e.JsonData.Features = p.Features.Where(x => x.IsNotNullOrEmpty()).ToList();
		if (p.TrialDays != null) e.JsonData.TrialDays = Math.Max(0, p.TrialDays.Value);
		e.ApplyUpdateParam<SubscriptionPlanEntity, TagSubscriptionPlan, SubscriptionPlanJson>(p);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeletePlan(IdParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u is not { IsSystemAdmin: true }) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		await db.Set<SubscriptionPlanEntity>().Where(x => x.Id == p.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	private string? PlanDataError(ICollection<TagModule> modules, ICollection<PlanPrice> prices) =>
		modules.Count == 0 ? ls.Get("planModulesRequired")
		: prices.Count == 0 || prices.Any(x => x.Months <= 0 || x.Price < 0) || prices.Select(x => x.Months).Distinct().Count() != prices.Count ? ls.Get("planPricesAreNotValid")
		: null;

	private static decimal RemainingValue(OrganizationSubscription s, DateTime now) {
		if (s.StartsAt == null || s.ExpiresAt == null || s.ExpiresAt <= s.StartsAt) return 0;
		double total = (s.ExpiresAt.Value - s.StartsAt.Value).TotalDays;
		double left = (s.ExpiresAt.Value - (now > s.StartsAt.Value ? now : s.StartsAt.Value)).TotalDays;
		return Math.Round(s.Price * (decimal)Math.Clamp(left / total, 0, 1));
	}

	private (SubscriptionQuoteResponse? Quote, List<Guid> Replaces, string? Error) Quote(OrganizationEntity? org, SubscriptionPlanEntity plan, int months, bool trial, DateTime now) {
		if (!plan.Tags.Contains(TagSubscriptionPlan.Active)) return (null, [], ls.Get("planNotFound"));
		List<OrganizationSubscription> current = org?.JsonData.Subscriptions.Where(x => x.Status == TagSubscription.Active && x.ExpiresAt > now).ToList() ?? [];
		decimal price;
		DateTime start = now;
		DateTime end;
		if (trial) {
			if (plan.JsonData.TrialDays <= 0 || org != null && org.JsonData.Subscriptions.Any(x => x.Trial && x.Status != TagSubscription.Pending)) return (null, [], ls.Get("trialNotAvailable"));
			months = 0;
			price = 0;
			end = now.AddDays(plan.JsonData.TrialDays);
		}
		else {
			PlanPrice? planPrice = plan.JsonData.Prices.FirstOrDefault(x => x.Months == months);
			if (planPrice == null) return (null, [], ls.Get("planPricesAreNotValid"));
			price = planPrice.Price;
			List<OrganizationSubscription> same = current.Where(x => x.PlanId == plan.Id).ToList();
			if (same.Count != 0) start = same.Max(x => x.ExpiresAt!.Value);
			end = start.AddMonths(months);
		}

		bool renewal = start > now;
		List<OrganizationSubscription> replaced = renewal || trial ? [] : current.Where(x => x.PlanId != plan.Id && x.Modules.All(plan.JsonData.Modules.Contains)).ToList();
		decimal credit = Math.Min(price, replaced.Sum(x => RemainingValue(x, now)));
		return (new SubscriptionQuoteResponse {
			PlanId = plan.Id,
			Title = plan.Title,
			Months = months,
			Trial = trial,
			Renewal = renewal,
			Price = price,
			Credit = credit,
			Payable = price - credit,
			StartsAt = start,
			ExpiresAt = end,
			Replaces = replaced.Select(x => x.Title).ToList()
		}, replaced.Select(x => x.Id).ToList(), null);
	}

	private static void Activate(OrganizationJson j, OrganizationSubscription s, DateTime now) {
		List<OrganizationSubscription> current = j.Subscriptions.Where(x => x.Id != s.Id && x.Status == TagSubscription.Active && x.ExpiresAt > now).ToList();
		DateTime start = s.Trial || s.PlanId == null ? now : current.Where(x => x.PlanId == s.PlanId).Select(x => x.ExpiresAt!.Value).DefaultIfEmpty(now).Max();
		if (start < now) start = now;
		s.StartsAt = start;
		s.ExpiresAt = start.AddMonths(s.Months).AddDays(s.Days);
		s.Status = TagSubscription.Active;
		foreach (OrganizationSubscription x in current.Where(x => s.Replaces.Contains(x.Id))) x.Status = TagSubscription.Replaced;
	}

	private static OrganizationSubscription NewSubscription(Guid? planId, string title, IEnumerable<TagModule> modules, IEnumerable<PlanLimit> limits, Guid registeredBy, DateTime now) => new() {
		Id = Guid.CreateVersion7(),
		PlanId = planId,
		Title = title,
		Modules = modules.Distinct().ToList(),
		Limits = limits.Select(x => new PlanLimit { Kind = x.Kind, Value = x.Value }).ToList(),
		Status = TagSubscription.Pending,
		CreatedAt = now,
		RegisteredBy = registeredBy
	};

	private async Task<(JwtClaimData? User, SubscriptionPlanEntity? Plan, OrganizationEntity? Organization, UResponse? Error)> SubscriptionContext(string? token, Guid planId, Guid? organizationId, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(token);
		if (u == null) return (null, null, null, new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue")));
		if (u.IsExpired) return (null, null, null, new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired")));
		SubscriptionPlanEntity? plan = await db.Set<SubscriptionPlanEntity>().FirstOrDefaultAsync(x => x.Id == planId, ct);
		if (plan == null) return (null, null, null, new UResponse(Usc.NotFound, ls.Get("planNotFound")));
		if (organizationId == null) return (u, plan, null, null);
		OrganizationEntity? org = await db.Set<OrganizationEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == organizationId, ct);
		if (org == null) return (null, null, null, new UResponse(Usc.NotFound, ls.Get("organizationNotFound")));
		if (!u.IsSystemAdmin && org.OwnerId != u.Id) return (null, null, null, new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction")));
		return (u, plan, org, null);
	}

	public async Task<UResponse<SubscriptionQuoteResponse?>> QuoteSubscription(SubscriptionQuoteParams p, CancellationToken ct) {
		(_, SubscriptionPlanEntity? plan, OrganizationEntity? org, UResponse? error) = await SubscriptionContext(p.Token, p.PlanId, p.OrganizationId, ct);
		if (error != null) return new UResponse<SubscriptionQuoteResponse?>(null, error.Status, error.Message);
		(SubscriptionQuoteResponse? quote, _, string? quoteError) = Quote(org, plan!, p.Months, p.Trial, DateTime.UtcNow);
		return quote == null ? new UResponse<SubscriptionQuoteResponse?>(null, Usc.BadRequest, quoteError!) : new UResponse<SubscriptionQuoteResponse?>(quote);
	}

	public async Task<UResponse<SubscriptionBuyResponse?>> BuySubscription(SubscriptionBuyParams p, CancellationToken ct) {
		(JwtClaimData? u, SubscriptionPlanEntity? plan, OrganizationEntity? org, UResponse? error) = await SubscriptionContext(p.Token, p.PlanId, p.OrganizationId, ct);
		if (error != null) return new UResponse<SubscriptionBuyResponse?>(null, error.Status, error.Message);
		DateTime now = DateTime.UtcNow;
		(SubscriptionQuoteResponse? quote, List<Guid> replaces, string? quoteError) = Quote(org, plan!, p.Months, p.Trial, now);
		if (quote == null) return new UResponse<SubscriptionBuyResponse?>(null, Usc.BadRequest, quoteError!);

		if (org == null) {
			if (p.Title.IsNullOrEmpty() || p.Title!.Trim().Length < 2) return new UResponse<SubscriptionBuyResponse?>(null, Usc.BadRequest, ls.Get("titleIsRequired"));
			UserEntity? owner = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == u!.Id, ct);
			if (owner == null) return new UResponse<SubscriptionBuyResponse?>(null, Usc.NotFound, ls.Get("accountNotFound"));
			if (!SetFirstAdminPassword(owner, p.Password)) return new UResponse<SubscriptionBuyResponse?>(null, Usc.BadRequest, ls.Get("pleaseEnterAPassword"));
			org = await AddOrganization(Guid.CreateVersion7(), p.Title.Trim(), owner, owner.Id, [TagOrganization.Active], new OrganizationJson(), ct);
		}

		OrganizationSubscription s = NewSubscription(plan!.Id, plan.Title, plan.JsonData.Modules, plan.JsonData.Limits, u!.Id, now);
		s.Months = quote.Months;
		s.Days = quote.Trial ? plan.JsonData.TrialDays : 0;
		s.Trial = quote.Trial;
		s.Price = quote.Price;
		s.Credit = quote.Credit;
		s.Replaces = replaces;
		org.JsonData.Subscriptions = [..org.JsonData.Subscriptions.Where(x => x.Status != TagSubscription.Pending), s];
		if (quote.Payable <= 0) Activate(org.JsonData, s, now);
		_json.Remove(org.Id);
		await db.SaveChangesAsync(ct);

		bool active = quote.Payable <= 0;
		if (!active && p.FromWallet) active = (await PaySubscriptionInternal(org.Id, u.Id, ct)).Status == Usc.Success;
		return new UResponse<SubscriptionBuyResponse?>(new SubscriptionBuyResponse { OrganizationId = org.Id, SubscriptionId = s.Id, Payable = quote.Payable, Active = active });
	}

	public async Task<UResponse> PaySubscription(IdParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (u.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!u.IsSystemAdmin && !await IsOwner(u, p.Id, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		return await PaySubscriptionInternal(p.Id, u.Id, ct);
	}

	public async Task<UResponse> PaySubscriptionInternal(Guid organizationId, Guid userId, CancellationToken ct) {
		OrganizationEntity? org = await db.Set<OrganizationEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == organizationId, ct);
		OrganizationSubscription? s = org?.JsonData.Subscriptions.FirstOrDefault(x => x.Status == TagSubscription.Pending);
		if (org == null || s == null) return new UResponse(Usc.NotFound, ls.Get("subscriptionNotFound"));

		decimal amount = s.Price - s.Credit - s.Paid;
		if (amount > 0) {
			UResponse<WalletTxnResponse?> transfer = await sp.GetRequiredService<IWalletService>().Transfer(new WalletTransferParams {
				SenderId = userId,
				ReceiverId = Core.App.Users.SystemAdmin.Id,
				Amount = amount,
				Detail1 = $"{ls.Get("subscription")} {s.Title} - {org.Title}",
				KeyValues = [new KeyValue { Key = ls.Get("organization"), Value = org.Title }],
				TagWalletTxn = [TagWalletTxn.Subscription]
			}, ct);
			if (transfer.Result == null) return new UResponse(transfer.Status, transfer.Message);
			s.Paid += amount;
		}

		Activate(org.JsonData, s, DateTime.UtcNow);
		org.JsonData.Subscriptions = org.JsonData.Subscriptions.ToList();
		_json.Remove(org.Id);
		await db.SaveChangesAsync(ct);
		return new UResponse(Usc.Success, ls.Get("paymentCompleted"));
	}

	public async Task<UResponse> GrantSubscription(SubscriptionGrantParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u is not { IsSystemAdmin: true }) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		OrganizationEntity? org = await db.Set<OrganizationEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.OrganizationId, ct);
		if (org == null) return new UResponse(Usc.NotFound, ls.Get("organizationNotFound"));
		SubscriptionPlanEntity? plan = p.PlanId == null ? null : await db.Set<SubscriptionPlanEntity>().FirstOrDefaultAsync(x => x.Id == p.PlanId, ct);
		List<TagModule> modules = p.Modules ?? plan?.JsonData.Modules ?? [];
		if (modules.Count == 0) return new UResponse(Usc.BadRequest, ls.Get("planModulesRequired"));
		if (p.Months <= 0 && p.Days <= 0) return new UResponse(Usc.BadRequest, ls.Get("planPricesAreNotValid"));

		DateTime now = DateTime.UtcNow;
		OrganizationSubscription s = NewSubscription(plan?.Id, p.Title.NullIfEmpty() ?? plan?.Title ?? ls.Get("subscription"), modules, p.Limits ?? plan?.JsonData.Limits ?? [], u.Id, now);
		s.Months = Math.Max(0, p.Months);
		s.Days = Math.Max(0, p.Days);
		org.JsonData.Subscriptions = [..org.JsonData.Subscriptions, s];
		Activate(org.JsonData, s, now);
		_json.Remove(org.Id);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> CancelSubscription(SubscriptionCancelParams p, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(p.Token);
		if (u is not { IsSystemAdmin: true }) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		OrganizationEntity? org = await db.Set<OrganizationEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.OrganizationId, ct);
		OrganizationSubscription? s = org?.JsonData.Subscriptions.FirstOrDefault(x => x.Id == p.SubscriptionId);
		if (org == null || s == null) return new UResponse(Usc.NotFound, ls.Get("subscriptionNotFound"));
		s.Status = TagSubscription.Cancelled;
		org.JsonData.Subscriptions = org.JsonData.Subscriptions.ToList();
		_json.Remove(org.Id);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<bool> HasOrganizationPermission(JwtClaimData u, Guid? organizationId, TagUser permission, CancellationToken ct, TagModule? module = null) {
		List<OrganizationEntity> organizations = await db.Set<OrganizationEntity>()
			.Where(x => (organizationId == null || x.Id == organizationId) && !x.Tags.Contains(TagOrganization.Inactive) && (x.OwnerId == u.Id || x.AdminUserIds.Contains(u.Id)))
			.ToListAsync(ct);
		DateTime now = DateTime.UtcNow;
		TagModule? m = module ?? ModuleOf(permission);
		return organizations.Any(x => Allows(x.JsonData, m, now) && (x.OwnerId == u.Id || x.JsonData.Members.Any(y => y.UserId == u.Id && y.Permissions.Contains(permission))));
	}

	private static readonly Dictionary<TagUser, TagModule> PermissionModules = new() {
		[TagUser.PermissionManageHotels] = TagModule.Hotel,
		[TagUser.PermissionDeleteHotels] = TagModule.Hotel,
		[TagUser.PermissionManageReservations] = TagModule.Hotel,
		[TagUser.PermissionDeleteReservations] = TagModule.Hotel,
		[TagUser.PermissionManageDorms] = TagModule.Dorm,
		[TagUser.PermissionDeleteDorms] = TagModule.Dorm,
		[TagUser.PermissionManageContracts] = TagModule.Dorm,
		[TagUser.PermissionDeleteContracts] = TagModule.Dorm,
		[TagUser.PermissionManageAccounting] = TagModule.Accounting,
		[TagUser.PermissionViewAccounting] = TagModule.Accounting,
		[TagUser.PermissionManageInventory] = TagModule.Inventory,
		[TagUser.PermissionManageStaff] = TagModule.Staff
	};

	private static TagModule? ModuleOf(TagUser permission) => PermissionModules.TryGetValue(permission, out TagModule m) ? m : null;

	public static bool IsLive(OrganizationSubscription s, DateTime now) => s.Status == TagSubscription.Active && s.StartsAt <= now && s.ExpiresAt > now;

	public static List<TagModule>? ModulesOf(OrganizationJson j, DateTime now) =>
		j.Subscriptions.Count == 0 ? null : j.Subscriptions.Where(x => IsLive(x, now)).SelectMany(x => x.Modules).Distinct().Order().ToList();

	public static bool Allows(OrganizationJson j, TagModule? module, DateTime now) => module == null || ModulesOf(j, now) is not { } list || list.Contains(module.Value);

	private readonly Dictionary<Guid, OrganizationJson?> _json = [];

	private async Task<OrganizationJson?> JsonOf(Guid organizationId, CancellationToken ct) {
		if (_json.TryGetValue(organizationId, out OrganizationJson? j)) return j;
		j = await db.Set<OrganizationEntity>().Where(x => x.Id == organizationId).Select(x => x.JsonData).FirstOrDefaultAsync(ct);
		_json[organizationId] = j;
		return j;
	}

	public async Task<bool> HasModule(JwtClaimData? u, Guid? organizationId, TagModule module, CancellationToken ct) {
		if (organizationId == null || u?.IsSystemAdmin == true) return true;
		OrganizationJson? j = await JsonOf(organizationId.Value, ct);
		return j == null || Allows(j, module, DateTime.UtcNow);
	}

	private async Task<bool> HasModuleFor(JwtClaimData u, Guid? organizationId, TagUser permission, TagModule? module, CancellationToken ct) =>
		(module ?? ModuleOf(permission)) is not { } m || await HasModule(u, organizationId, m, ct);

	private static bool CanActOnPlace(JwtClaimData u, ICollection<Guid> placeAdminUserIds, TagUser permission) => u.HasPermission(permission) && (u.IsSuperAdmin || u.IsSubAdmin && placeAdminUserIds.Contains(u.Id));

	public async Task<bool> CanActOnPlace(JwtClaimData u, Guid? organizationId, ICollection<Guid> adminUserIds, TagUser permission, CancellationToken ct) {
		if (!Core.App.MultiTenant) return CanActOnPlace(u, adminUserIds, permission) && await HasModuleFor(u, organizationId, permission, null, ct);
		if (u.IsSystemAdmin) return true;
		return organizationId != null && adminUserIds.Contains(u.Id) && await HasOrganizationPermission(u, organizationId, permission, ct);
	}

	public IQueryable<Guid> RelatedUserIds(Guid userId) {
		IQueryable<OrganizationEntity> mine = db.Set<OrganizationEntity>().Where(o => o.OwnerId == userId || o.AdminUserIds.Contains(userId));
		return mine.Select(o => o.OwnerId).Union(mine.SelectMany(o => o.AdminUserIds));
	}

	public async Task<bool> CanManage(JwtClaimData u, Guid organizationId, TagUser permission, CancellationToken ct, TagModule? module = null) =>
		(IsFull(u) || !Core.App.MultiTenant && u.HasPermission(permission)) && await HasModuleFor(u, organizationId, permission, module, ct) ||
		await HasOrganizationPermission(u, organizationId, permission, ct, module);

	public Task<OrganizationEntity?> ReadOrganization(Guid? organizationId, CancellationToken ct) =>
		organizationId == null ? Task.FromResult<OrganizationEntity?>(null) : db.Set<OrganizationEntity>().FirstOrDefaultAsync(x => x.Id == organizationId, ct);

	public static int? LimitOf(OrganizationJson j, TagPlanLimit kind, DateTime now) {
		List<int> values = j.Subscriptions.Where(x => IsLive(x, now)).SelectMany(x => x.Limits).Where(x => x.Kind == kind).Select(x => x.Value).ToList();
		return values.Count == 0 ? null : values.Max();
	}

	public async Task<string?> PlanError(JwtClaimData u, Guid? organizationId, TagPlanLimit limit, Func<Task<int>> count, CancellationToken ct) {
		if (organizationId == null || u.IsSystemAdmin) return null;
		OrganizationJson? j = await JsonOf(organizationId.Value, ct);
		if (j == null || j.Subscriptions.Count == 0) return null;
		DateTime now = DateTime.UtcNow;
		if (!j.Subscriptions.Any(x => IsLive(x, now))) return ls.Get("organizationPlanExpired");
		int? max = LimitOf(j, limit, now);
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

		if (!await HasModule(u, organizationId, TagModule.Staff, ct)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("subscriptionInactive"));
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
