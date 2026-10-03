namespace SinaMN75U.Services;

public interface ISportService {
	public Task<UResponse<Guid?>> CreateSport(SportCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<SportResponse>?>> ReadSports(SportReadParams p, CancellationToken ct);
	public Task<UResponse<SportResponse?>> ReadSportById(IdParams<SportSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> UpdateSport(SportUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteSport(IdParams p, CancellationToken ct);

	public Task<UResponse<Guid?>> CreatePlayerSportProfile(PlayerSportProfileCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<PlayerSportProfileResponse>?>> ReadPlayerSportProfiles(PlayerSportProfileReadParams p, CancellationToken ct);
	public Task<UResponse> UpdatePlayerSportProfile(PlayerSportProfileUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeletePlayerSportProfile(IdParams p, CancellationToken ct);
}

public class SportService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : ISportService {
	// ---- Who sees and changes what ----
	// Admins: manage the sports catalog and every profile.
	// Everybody: reads active and coming-soon sports and all profiles; creates/changes/deletes only their own profiles.

	// Sport types are the 1xx tags; every sport has exactly one and no two sports share it.
	private static List<TagSport> Types(IEnumerable<TagSport> tags) => tags.Where(t => (int)t < 200).Distinct().ToList();

	public async Task<UResponse<Guid?>> CreateSport(SportCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (p.MinLevel >= p.MaxLevel) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("levelIsOutOfRange"));
		List<TagSport> types = Types(p.Tags);
		if (types.Count != 1) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("sportTypeIsRequired"));
		TagSport type = types[0];
		if (await db.Set<SportEntity>().AnyAsync(x => x.Tags.Contains(type), ct)) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("thisSportAlreadyExists"));

		SportEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = p.Tags,
			Title = p.Title,
			Order = p.Order,
			MinLevel = p.MinLevel,
			MaxLevel = p.MaxLevel,
			JsonData = new SportJson {
				Detail1 = p.Detail1,
				Detail2 = p.Detail2,
				Icon = p.Icon
			}
		};

		await db.Set<SportEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<SportResponse>?>> ReadSports(SportReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<SportEntity> q = db.Set<SportEntity>().ApplyReadParams(p);
		if (userData is not { IsAdmin: true }) q = q.Where(x => !x.Tags.Contains(TagSport.Disabled));

		return await q.Select(Projections.SportSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<SportResponse?>> ReadSportById(IdParams<SportSelectorArgs> p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<SportEntity> q = db.Set<SportEntity>();
		if (userData is not { IsAdmin: true }) q = q.Where(x => !x.Tags.Contains(TagSport.Disabled));

		SportResponse? e = await q.Select(Projections.SportSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<SportResponse?>(null, Usc.NotFound, ls.Get("sportNotFound")) : new UResponse<SportResponse?>(e);
	}

	public async Task<UResponse> UpdateSport(SportUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (!userData.IsAdmin) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		SportEntity? e = await db.Set<SportEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("sportNotFound"));
		if ((p.MinLevel ?? e.MinLevel) >= (p.MaxLevel ?? e.MaxLevel)) return new UResponse(Usc.BadRequest, ls.Get("levelIsOutOfRange"));

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		if (p.Order.HasValue) e.Order = p.Order.Value;
		if (p.MinLevel.HasValue) e.MinLevel = p.MinLevel.Value;
		if (p.MaxLevel.HasValue) e.MaxLevel = p.MaxLevel.Value;
		if (p.Icon.IsNotNull()) e.JsonData.Icon = p.Icon;
		e.ApplyUpdateParam<SportEntity, TagSport, SportJson>(p);

		List<TagSport> types = Types(e.Tags);
		if (types.Count != 1) return new UResponse(Usc.BadRequest, ls.Get("sportTypeIsRequired"));
		TagSport type = types[0];
		if (await db.Set<SportEntity>().AnyAsync(x => x.Id != e.Id && x.Tags.Contains(type), ct)) return new UResponse(Usc.Conflict, ls.Get("thisSportAlreadyExists"));
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> DeleteSport(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (!userData.IsAdmin) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		if (!await db.Set<SportEntity>().AnyAsync(x => x.Id == p.Id, ct)) return new UResponse(Usc.NotFound, ls.Get("sportNotFound"));
		// Deleting would cascade to every player's profile; a sport that is in use is disabled instead.
		if (await db.Set<PlayerSportProfileEntity>().AnyAsync(x => x.SportId == p.Id, ct)) return new UResponse(Usc.Conflict, ls.Get("sportIsInUseDisableItInstead"));

		await db.Set<SportEntity>().Where(x => x.Id == p.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreatePlayerSportProfile(PlayerSportProfileCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		Guid userId = userData.IsAdmin ? p.UserId ?? userData.Id : userData.Id;
		SportEntity? sport = await db.Set<SportEntity>().FirstOrDefaultAsync(x => x.Id == p.SportId, ct);
		if (sport == null || !sport.Tags.Contains(TagSport.Active)) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("sportNotFound"));
		if (p.Level < sport.MinLevel || p.Level > sport.MaxLevel) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("levelIsOutOfRange"));
		if (await db.Set<PlayerSportProfileEntity>().AnyAsync(x => x.UserId == userId && x.SportId == p.SportId, ct))
			return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("thisSportIsAlreadyInYourProfile"));

		PlayerSportProfileEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = p.Tags,
			Level = p.Level,
			UserId = userId,
			SportId = p.SportId,
			JsonData = new PlayerSportProfileJson { Detail1 = p.Detail1, Detail2 = p.Detail2 }
		};

		await db.Set<PlayerSportProfileEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<PlayerSportProfileResponse>?>> ReadPlayerSportProfiles(PlayerSportProfileReadParams p, CancellationToken ct) {
		IQueryable<PlayerSportProfileEntity> q = db.Set<PlayerSportProfileEntity>().ApplyReadParams(p);
		if (p.UserId.HasValue) q = q.Where(x => x.UserId == p.UserId);
		if (p.SportId.HasValue) q = q.Where(x => x.SportId == p.SportId);

		return await q.Select(Projections.PlayerSportProfileSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdatePlayerSportProfile(PlayerSportProfileUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		PlayerSportProfileEntity? e = await db.Set<PlayerSportProfileEntity>().AsTracking().Include(x => x.Sport).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("playerSportProfileNotFound"));
		if (!userData.IsAdmin && e.UserId != userData.Id) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		if (p.Level.HasValue) {
			if (p.Level < e.Sport.MinLevel || p.Level > e.Sport.MaxLevel) return new UResponse(Usc.BadRequest, ls.Get("levelIsOutOfRange"));
			e.Level = p.Level.Value;
		}

		e.ApplyUpdateParam<PlayerSportProfileEntity, TagPlayerSportProfile, PlayerSportProfileJson>(p);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeletePlayerSportProfile(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		int count = await db.Set<PlayerSportProfileEntity>().Where(x => x.Id == p.Id && (userData.IsAdmin || x.UserId == userData.Id)).ExecuteDeleteAsync(ct);
		return count == 0 ? new UResponse(Usc.NotFound, ls.Get("playerSportProfileNotFound")) : new UResponse();
	}
}
