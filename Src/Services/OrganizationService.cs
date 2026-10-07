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
}

public class OrganizationService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IOrganizationService, IUserScope {
	private static readonly MethodInfo ReplaceInMethod = typeof(OrganizationService).GetMethod(nameof(ReplaceIn), BindingFlags.NonPublic | BindingFlags.Instance)!;

	private static readonly MethodInfo AnyInMethod = typeof(OrganizationService).GetMethod(nameof(AnyIn), BindingFlags.NonPublic | BindingFlags.Instance)!;

	private static readonly MethodInfo TitlesInMethod = typeof(OrganizationService).GetMethod(nameof(TitlesIn), BindingFlags.NonPublic | BindingFlags.Instance)!;

	public static bool IsFull(JwtClaimData? u) => u is { IsSuperAdmin: true };

	public static bool IsScopedAdmin(JwtClaimData? u) => !IsFull(u) && (u is { IsSubAdmin: true } || Core.App.MultiTenant && u?.Tags.Contains(TagUser.SuperAdmin) == true);

	public static Guid WalletOf(Guid? organizationId) => Core.App.MultiTenant && organizationId != null ? organizationId.Value : Core.App.Users.SystemAdmin.Id;

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
}
