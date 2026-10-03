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

	public Task<UResponse<Guid?>> CreateTournament(TournamentCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<TournamentResponse>?>> ReadTournaments(TournamentReadParams p, CancellationToken ct);
	public Task<UResponse<TournamentResponse?>> ReadTournamentById(IdParams<TournamentSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> UpdateTournament(TournamentUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteTournament(IdParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<TournamentStandingResponse>?>> ReadTournamentStandings(IdParams p, CancellationToken ct);
	public Task<UResponse> GenerateTournamentMatches(IdParams p, CancellationToken ct);
	public Task<UResponse<Guid?>> CreateNextTournamentSeason(IdParams p, CancellationToken ct);

	public Task<UResponse<Guid?>> RegisterTournamentEntry(TournamentRegisterParams p, CancellationToken ct);
	public Task<UResponse> UpdateTournamentEntry(TournamentEntryUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteTournamentEntry(IdParams p, CancellationToken ct);

	public Task<UResponse> UpdateTournamentMatch(TournamentMatchUpdateParams p, CancellationToken ct);
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
		// Deleting would cascade to every player's profile and tournament; a sport that is in use is disabled instead.
		if (await db.Set<PlayerSportProfileEntity>().AnyAsync(x => x.SportId == p.Id, ct) || await db.Set<TournamentEntity>().AnyAsync(x => x.SportId == p.Id, ct))
			return new UResponse(Usc.Conflict, ls.Get("sportIsInUseDisableItInstead"));

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

		UResponse<IEnumerable<PlayerSportProfileResponse>?> result = await q.Select(Projections.PlayerSportProfileSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);

		// The change from each player's latest rated match.
		List<PlayerSportProfileResponse> items = result.Result?.ToList() ?? [];
		List<Guid> userIds = items.Select(x => x.UserId).Distinct().ToList(), sportIds = items.Select(x => x.SportId).Distinct().ToList();
		List<PlayerRatingHistoryEntity> history = await db.Set<PlayerRatingHistoryEntity>().Where(x => userIds.Contains(x.UserId) && sportIds.Contains(x.SportId)).OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
		foreach (PlayerSportProfileResponse i in items)
			if (history.FirstOrDefault(x => x.UserId == i.UserId && x.SportId == i.SportId) is { } last)
				i.LastLevelChange = last.LevelAfter - last.LevelBefore;
		return result;
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

	// ---------------- Tournament ----------------
	// Organizer = the creator or anyone in AdminUserIds; admins manage every tournament.

	private static readonly TagTournament[] AvailableParticipantTypes = [TagTournament.Singles, TagTournament.Doubles, TagTournament.Team];
	private static readonly TagTournament[] IndividualFormats = [TagTournament.Americano, TagTournament.Mexicano];

	private static bool CanManage(JwtClaimData u, TournamentEntity t) => u.IsAdmin || t.CreatorId == u.Id || t.AdminUserIds.Contains(u.Id);

	private static List<TagTournament> Group(IEnumerable<TagTournament> tags, int hundred) => tags.Where(t => (int)t / 100 == hundred / 100).Distinct().ToList();

	private static TagTournament FormatOf(TournamentEntity t) => Group(t.Tags, 100).FirstOrDefault();

	private static void SetStatus(TournamentEntity t, TagTournament status) {
		t.Tags = t.Tags.Where(x => (int)x / 100 != 3).Append(status).ToList();
	}

	private static TagSport? SportTypeOf(SportEntity sport) => sport.Tags.FirstOrDefault(x => (int)x < 200);

	/// <summary>Americano and Mexicano rotate partners, so their entries are single players.</summary>
	private static string? ValidateFormat(TagTournament format, TagTournament type) => IndividualFormats.Contains(format) && type != TagTournament.Singles ? "thisFormatIsForIndividualPlayers" : null;

	private static void ApplyOptions(TournamentJson j, int? groupCount, int? advancePerGroup, bool? thirdPlaceMatch, int? rounds, int? pointsPerMatch, int? boxSize, int? setsToWin, int? raceTo, bool? superTiebreak, bool? unrated) {
		if (groupCount.HasValue) j.GroupCount = groupCount;
		if (advancePerGroup.HasValue) j.AdvancePerGroup = Math.Max(1, advancePerGroup.Value);
		if (thirdPlaceMatch.HasValue) j.ThirdPlaceMatch = thirdPlaceMatch.Value;
		if (rounds.HasValue) j.Rounds = rounds;
		if (pointsPerMatch.HasValue) j.PointsPerMatch = pointsPerMatch.Value;
		if (boxSize.HasValue) j.BoxSize = boxSize.Value;
		if (setsToWin.HasValue) j.SetsToWin = setsToWin;
		if (raceTo.HasValue) j.RaceTo = raceTo;
		if (superTiebreak.HasValue) j.SuperTiebreak = superTiebreak.Value;
		if (unrated.HasValue) j.Unrated = unrated.Value;
	}

	public async Task<UResponse<Guid?>> CreateTournament(TournamentCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		SportEntity? sport = await db.Set<SportEntity>().FirstOrDefaultAsync(x => x.Id == p.SportId, ct);
		if (sport == null || !sport.Tags.Contains(TagSport.Active)) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("sportNotFound"));

		List<TagTournament> formats = Group(p.Tags, 100);
		if (formats.Count != 1) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("formatIsRequired"));
		List<TagTournament> types = Group(p.Tags, 200);
		if (types.Count != 1 || !AvailableParticipantTypes.Contains(types[0])) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("participantTypeIsRequired"));
		if (ValidateFormat(formats[0], types[0]) is { } formatError) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get(formatError));
		if (p.Capacity < 2) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("capacityMustBeAtLeastTwo"));
		if (p.MinLevel > p.MaxLevel) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("levelIsOutOfRange"));

		// A new tournament starts as a draft or open for registration; it moves on through the schedule and results.
		TagTournament status = p.Tags.Contains(TagTournament.Draft) ? TagTournament.Draft : TagTournament.Registration;
		TournamentJson json = new() {
			Detail1 = p.Detail1,
			Detail2 = p.Detail2,
			Description = p.Description,
			Prize = p.Prize,
			Venue = p.Venue,
			Address = p.Address,
			Latitude = p.Latitude,
			Longitude = p.Longitude,
			PointsForWin = p.PointsForWin ?? 3,
			PointsForDraw = p.PointsForDraw ?? 1,
			PointsForLoss = p.PointsForLoss ?? 0
		};
		ApplyOptions(json, p.GroupCount, p.AdvancePerGroup, p.ThirdPlaceMatch, p.Rounds, p.PointsPerMatch, p.BoxSize, p.SetsToWin, p.RaceTo, p.SuperTiebreak, p.Unrated);

		TournamentEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = [formats[0], types[0], status, ..p.Tags.Where(t => t == TagTournament.AutoApprove)],
			Title = p.Title,
			SportId = p.SportId,
			StartDate = p.StartDate,
			Capacity = p.Capacity,
			EntryFee = p.EntryFee,
			MinLevel = p.MinLevel,
			MaxLevel = p.MaxLevel,
			AdminUserIds = p.AdminUserIds ?? [],
			JsonData = json
		};

		await db.Set<TournamentEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<TournamentResponse>?>> ReadTournaments(TournamentReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<TournamentEntity> q = db.Set<TournamentEntity>().ApplyReadParams(p);
		Guid uid = userData?.Id ?? Guid.Empty;
		// Drafts are only visible to their organizers.
		if (userData is not { IsAdmin: true }) q = q.Where(x => !x.Tags.Contains(TagTournament.Draft) || x.CreatorId == uid || x.AdminUserIds.Contains(uid));
		if (userData is not { IsAdmin: true }) p.SelectorArgs.Creator = null;
		if (p.SportId.HasValue) q = q.Where(x => x.SportId == p.SportId);
		if (p.UserId.HasValue) q = q.Where(x => x.Entries.Any(e => e.Users.Any(u => u.Id == p.UserId)));
		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title!));

		return await q.Select(Projections.TournamentSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<TournamentResponse?>> ReadTournamentById(IdParams<TournamentSelectorArgs> p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData is not { IsAdmin: true }) p.SelectorArgs.Creator = null;
		TournamentResponse? e = await db.Set<TournamentEntity>().Select(Projections.TournamentSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<TournamentResponse?>(null, Usc.NotFound, ls.Get("tournamentNotFound")) : new UResponse<TournamentResponse?>(e);
	}

	public async Task<UResponse> UpdateTournament(TournamentUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		TournamentEntity? e = await db.Set<TournamentEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("tournamentNotFound"));
		if (!CanManage(userData, e)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if ((p.MinLevel ?? e.MinLevel) > (p.MaxLevel ?? e.MaxLevel)) return new UResponse(Usc.BadRequest, ls.Get("levelIsOutOfRange"));
		if (p.Capacity is < 2) return new UResponse(Usc.BadRequest, ls.Get("capacityMustBeAtLeastTwo"));

		List<TagTournament> before = Group(e.Tags, 100).Concat(Group(e.Tags, 200)).ToList();
		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		if (p.StartDate.HasValue) e.StartDate = p.StartDate.Value;
		if (p.Capacity.HasValue) e.Capacity = p.Capacity.Value;
		if (p.EntryFee.HasValue) e.EntryFee = p.EntryFee.Value;
		if (p.MinLevel.HasValue) e.MinLevel = p.MinLevel;
		if (p.MaxLevel.HasValue) e.MaxLevel = p.MaxLevel;
		if (p.Description.IsNotNull()) e.JsonData.Description = p.Description;
		if (p.Prize.IsNotNull()) e.JsonData.Prize = p.Prize;
		if (p.Venue.IsNotNull()) e.JsonData.Venue = p.Venue;
		if (p.Address.IsNotNull()) e.JsonData.Address = p.Address;
		if (p.Latitude.HasValue) e.JsonData.Latitude = p.Latitude;
		if (p.Longitude.HasValue) e.JsonData.Longitude = p.Longitude;
		if (p.PointsForWin.HasValue) e.JsonData.PointsForWin = p.PointsForWin.Value;
		if (p.PointsForDraw.HasValue) e.JsonData.PointsForDraw = p.PointsForDraw.Value;
		if (p.PointsForLoss.HasValue) e.JsonData.PointsForLoss = p.PointsForLoss.Value;
		ApplyOptions(e.JsonData, p.GroupCount, p.AdvancePerGroup, p.ThirdPlaceMatch, p.Rounds, p.PointsPerMatch, p.BoxSize, p.SetsToWin, p.RaceTo, p.SuperTiebreak, p.Unrated);
		e.ApplyUpdateParam<TournamentEntity, TagTournament, TournamentJson>(p);

		// The format and participant type can't change once matches exist.
		List<TagTournament> formats = Group(e.Tags, 100), types = Group(e.Tags, 200);
		if (formats.Count != 1 || types.Count != 1 || Group(e.Tags, 300).Count != 1) return new UResponse(Usc.BadRequest, ls.Get("participantTypeIsRequired"));
		if (ValidateFormat(formats[0], types[0]) is { } formatError) return new UResponse(Usc.BadRequest, ls.Get(formatError));
		if (!before.SequenceEqual(formats.Concat(types)) && await db.Set<TournamentMatchEntity>().AnyAsync(x => x.TournamentId == e.Id, ct))
			return new UResponse(Usc.Conflict, ls.Get("tournamentHasStarted"));

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteTournament(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		TournamentEntity? e = await db.Set<TournamentEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("tournamentNotFound"));
		if (!CanManage(userData, e)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		// A tournament with results stays as history; only admins remove it.
		if (!userData.IsAdmin && (e.Tags.Contains(TagTournament.InProgress) || e.Tags.Contains(TagTournament.Finished))) return new UResponse(Usc.Conflict, ls.Get("tournamentHasStarted"));

		await db.Set<TournamentMatchEntity>().Where(x => x.TournamentId == p.Id).ExecuteDeleteAsync(ct);
		await db.Set<TournamentEntity>().Where(x => x.Id == p.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	private Task<List<TournamentStandingResponse>> StandingRows(Guid tournamentId, CancellationToken ct) => db.Set<TournamentEntryEntity>()
		.Where(x => x.TournamentId == tournamentId && x.Tags.Contains(TagTournamentEntry.Approved))
		.OrderBy(x => x.Seed ?? int.MaxValue).ThenBy(x => x.CreatedAt)
		.Select(x => new TournamentStandingResponse { EntryId = x.Id, Title = x.Title, GroupNumber = x.GroupNumber, Users = x.Users.AsQueryable().Select(Projections.PublicUserSelector()).ToList() })
		.ToListAsync(ct);

	/// <summary>The standings of every format: league tables (per group / box), individual points, or knockout placings.</summary>
	private static List<TournamentStandingResponse> ComputeStandings(TournamentEntity t, List<TournamentStandingResponse> rows, List<TournamentMatchEntity> matches) {
		List<TournamentStandingResponse> PerGroup(Func<List<TournamentStandingResponse>, List<TournamentMatchEntity>, List<TournamentStandingResponse>> table) => rows
			.GroupBy(x => x.GroupNumber ?? 0)
			.OrderBy(g => g.Key)
			.SelectMany(g => table(g.ToList(), matches.Where(m => (m.GroupNumber ?? 0) == g.Key && !TournamentEngine.IsKnockout(m)).ToList()))
			.ToList();

		switch (FormatOf(t)) {
			case TagTournament.SingleElimination:
			case TagTournament.DoubleElimination:
				return TournamentEngine.Placements(rows, matches);
			case TagTournament.Americano:
			case TagTournament.Mexicano:
				return TournamentEngine.Individual(rows, matches);
			case TagTournament.Swiss:
				return TournamentEngine.Table(rows, matches, t.JsonData, true);
			case TagTournament.GroupsKnockout:
				return PerGroup((r, m) => TournamentEngine.Table(r, m, t.JsonData));
			case TagTournament.Ladder:
				int lastBox = rows.Select(x => x.GroupNumber ?? 1).DefaultIfEmpty(1).Max();
				return PerGroup((r, m) => {
					List<TournamentStandingResponse> table = TournamentEngine.Table(r, m, t.JsonData);
					int box = table.FirstOrDefault()?.GroupNumber ?? 1;
					if (table.Count > 1 && box > 1) table[0].Promotion = 1;
					if (table.Count > 1 && box < lastBox) table[^1].Promotion = -1;
					return table;
				});
			default:
				return TournamentEngine.Table(rows, matches, t.JsonData);
		}
	}

	public async Task<UResponse<IEnumerable<TournamentStandingResponse>?>> ReadTournamentStandings(IdParams p, CancellationToken ct) {
		TournamentEntity? t = await db.Set<TournamentEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (t == null) return new UResponse<IEnumerable<TournamentStandingResponse>?>(null, Usc.NotFound, ls.Get("tournamentNotFound"));

		List<TournamentStandingResponse> rows = await StandingRows(t.Id, ct);
		List<TournamentMatchEntity> matches = await db.Set<TournamentMatchEntity>().Where(x => x.TournamentId == p.Id).ToListAsync(ct);
		return new UResponse<IEnumerable<TournamentStandingResponse>?>(ComputeStandings(t, rows, matches));
	}

	private static TournamentMatchEntity ToEntity(MatchPlan plan, Guid tournamentId, Guid creatorId) {
		TournamentMatchEntity m = new() {
			Id = plan.Id,
			CreatorId = creatorId,
			CreatedAt = DateTime.UtcNow,
			Tags = [TagTournamentMatch.Scheduled, plan.Bracket],
			TournamentId = tournamentId,
			Round = plan.Round,
			Order = plan.Order,
			EntryAId = plan.A,
			EntryBId = plan.B,
			PartnerAId = plan.A2,
			PartnerBId = plan.B2,
			GroupNumber = plan.Group,
			NextMatchId = plan.Next,
			NextMatchSlot = plan.NextSlot,
			LoserNextMatchId = plan.LoserNext,
			LoserNextMatchSlot = plan.LoserNextSlot,
			JsonData = new TournamentMatchJson { Court = plan.Court }
		};
		// A Swiss bye: one player, no opponent, counts as a win.
		if (plan.Bracket == TagTournamentMatch.Group && plan.A != null && plan.B == null) {
			m.WinnerEntryId = plan.A;
			TournamentEngine.SetMatchStatus(m, TagTournamentMatch.Bye);
		}

		return m;
	}

	public async Task<UResponse> GenerateTournamentMatches(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		TournamentEntity? t = await db.Set<TournamentEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (t == null) return new UResponse(Usc.NotFound, ls.Get("tournamentNotFound"));
		if (!CanManage(userData, t)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (!t.Tags.Contains(TagTournament.Registration) && !t.Tags.Contains(TagTournament.Draft)) return new UResponse(Usc.Conflict, ls.Get("tournamentHasStarted"));

		List<TournamentEntryEntity> entries = await db.Set<TournamentEntryEntity>().AsTracking()
			.Where(x => x.TournamentId == t.Id && x.Tags.Contains(TagTournamentEntry.Approved))
			.OrderBy(x => x.Seed ?? int.MaxValue).ThenBy(x => x.CreatedAt)
			.ToListAsync(ct);
		List<Guid> ids = entries.Select(x => x.Id).ToList();
		int n = ids.Count;
		TagTournament format = FormatOf(t);
		if (n < (format switch { TagTournament.DoubleElimination or TagTournament.Swiss or TagTournament.Ladder => 3, TagTournament.GroupsKnockout or TagTournament.Americano or TagTournament.Mexicano => 4, _ => 2 }))
			return new UResponse(Usc.BadRequest, ls.Get("notEnoughEntriesForThisFormat"));
		if (IndividualFormats.Contains(format) && n % 4 != 0) return new UResponse(Usc.BadRequest, ls.Get("playersMustBeAMultipleOfFour"));

		List<MatchPlan> plans;
		switch (format) {
			case TagTournament.RoundRobin:
				plans = TournamentEngine.RoundRobin(ids);
				break;
			case TagTournament.SingleElimination:
				plans = TournamentEngine.SingleElimination(ids, t.JsonData.ThirdPlaceMatch);
				break;
			case TagTournament.DoubleElimination:
				plans = TournamentEngine.DoubleElimination(ids);
				break;
			case TagTournament.Americano:
				plans = TournamentEngine.Americano(ids);
				break;
			case TagTournament.Mexicano:
				plans = TournamentEngine.MexicanoRound(ids, 1, 1);
				break;
			case TagTournament.Swiss:
				plans = TournamentEngine.SwissRound(ids, [], [], 1, 1);
				break;
			case TagTournament.GroupsKnockout:
			case TagTournament.Ladder: {
				Dictionary<Guid, int> groups;
				if (format == TagTournament.GroupsKnockout) {
					int groupCount = t.JsonData.GroupCount ?? Math.Max(2, (int)Math.Round(n / 4.0));
					if (groupCount * Math.Max(2, t.JsonData.AdvancePerGroup) > n) return new UResponse(Usc.BadRequest, ls.Get("notEnoughEntriesForThisFormat"));
					groups = TournamentEngine.SnakeGroups(ids, groupCount);
				}
				// A box league that continues an earlier period keeps the boxes it was given.
				else groups = entries.All(x => x.GroupNumber != null) ? entries.ToDictionary(x => x.Id, x => x.GroupNumber!.Value) : TournamentEngine.Boxes(ids, t.JsonData.BoxSize);

				foreach (TournamentEntryEntity entry in entries) entry.GroupNumber = groups[entry.Id];
				plans = [];
				foreach (IGrouping<int, Guid> g in ids.GroupBy(x => groups[x]).OrderBy(g => g.Key))
					plans.AddRange(TournamentEngine.RoundRobin(g.ToList(), g.Key, plans.Count + 1));
				break;
			}
			default:
				return new UResponse(Usc.BadRequest, ls.Get("thisFormatIsNotAvailableYet"));
		}

		List<TournamentMatchEntity> matches = plans.Select(x => ToEntity(x, t.Id, userData.Id)).ToList();
		TournamentEngine.ResolveByes(matches);
		await db.Set<TournamentMatchEntity>().Where(x => x.TournamentId == t.Id).ExecuteDeleteAsync(ct);
		await db.Set<TournamentMatchEntity>().AddRangeAsync(matches, ct);
		SetStatus(t, TagTournament.InProgress);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> RegisterTournamentEntry(TournamentRegisterParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		TournamentEntity? t = await db.Set<TournamentEntity>().FirstOrDefaultAsync(x => x.Id == p.TournamentId, ct);
		if (t == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("tournamentNotFound"));
		if (!t.Tags.Contains(TagTournament.Registration)) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("registrationIsClosed"));
		if (await db.Set<TournamentEntryEntity>().CountAsync(x => x.TournamentId == t.Id && !x.Tags.Contains(TagTournamentEntry.Rejected), ct) >= t.Capacity)
			return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("tournamentIsFull"));
		if (t.Tags.Contains(TagTournament.Team) && p.Title.IsNullOrEmpty()) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("teamNameIsRequired"));

		List<Guid> playerIds = [userData.Id];
		if (t.Tags.Contains(TagTournament.Doubles)) {
			if (p.PartnerEmail.IsNullOrEmpty()) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("partnerEmailIsRequired"));
			string email = p.PartnerEmail.Trim().ToLowerInvariant();
			Guid? partnerId = await db.Set<UserEntity>().Where(x => x.Email == email || x.Email == p.PartnerEmail.Trim()).Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);
			if (partnerId == null || partnerId == userData.Id) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("partnerNotFound"));
			playerIds.Add(partnerId.Value);
		}

		if (await db.Set<TournamentEntryEntity>().AnyAsync(x => x.TournamentId == t.Id && !x.Tags.Contains(TagTournamentEntry.Rejected) && x.Users.Any(u => playerIds.Contains(u.Id)), ct))
			return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("aPlayerIsAlreadyRegistered"));

		// Every player needs this sport in their profile, inside the tournament's level range (a team: its captain).
		List<PlayerSportProfileEntity> profiles = await db.Set<PlayerSportProfileEntity>().Where(x => x.SportId == t.SportId && playerIds.Contains(x.UserId)).ToListAsync(ct);
		if (profiles.Count != playerIds.Count) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("addThisSportToYourProfileFirst"));
		if (profiles.Any(x => x.Level < (t.MinLevel ?? decimal.MinValue) || x.Level > (t.MaxLevel ?? decimal.MaxValue)))
			return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("levelIsNotInTheTournamentRange"));

		List<UserEntity> users = await db.Set<UserEntity>().AsTracking().Where(x => playerIds.Contains(x.Id)).ToListAsync(ct);
		TournamentEntryEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatorId = userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = [t.Tags.Contains(TagTournament.AutoApprove) ? TagTournamentEntry.Approved : TagTournamentEntry.Pending],
			Title = p.Title.IsNotNullOrEmpty() ? p.Title : null,
			TournamentId = t.Id,
			Users = users,
			JsonData = new TournamentEntryJson()
		};

		await db.Set<TournamentEntryEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse> UpdateTournamentEntry(TournamentEntryUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		TournamentEntryEntity? e = await db.Set<TournamentEntryEntity>().AsTracking().Include(x => x.Tournament).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("entryNotFound"));
		if (!CanManage(userData, e.Tournament)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		if (p.Title.IsNotNull()) e.Title = p.Title.IsNotNullOrEmpty() ? p.Title : null;
		if (p.Seed.HasValue) e.Seed = p.Seed;
		if (p.GroupNumber.HasValue) e.GroupNumber = p.GroupNumber;
		e.ApplyUpdateParam<TournamentEntryEntity, TagTournamentEntry, TournamentEntryJson>(p);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteTournamentEntry(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		TournamentEntryEntity? e = await db.Set<TournamentEntryEntity>().AsTracking().Include(x => x.Tournament).Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("entryNotFound"));
		bool isMember = e.Users.Any(x => x.Id == userData.Id);
		if (!isMember && !CanManage(userData, e.Tournament)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		// Once the schedule exists the entry is part of the matches.
		if (await db.Set<TournamentMatchEntity>().AnyAsync(x => x.EntryAId == e.Id || x.EntryBId == e.Id || x.PartnerAId == e.Id || x.PartnerBId == e.Id, ct))
			return new UResponse(Usc.Conflict, ls.Get("tournamentHasStarted"));

		db.Set<TournamentEntryEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> UpdateTournamentMatch(TournamentMatchUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		TournamentMatchEntity? found = await db.Set<TournamentMatchEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (found == null) return new UResponse(Usc.NotFound, ls.Get("matchNotFound"));
		TournamentEntity t = await db.Set<TournamentEntity>().AsTracking().Include(x => x.Sport).FirstAsync(x => x.Id == found.TournamentId, ct);
		if (!CanManage(userData, t)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (!t.Tags.Contains(TagTournament.InProgress) && !t.Tags.Contains(TagTournament.Finished)) return new UResponse(Usc.Conflict, ls.Get("tournamentHasNotStarted"));

		List<TournamentMatchEntity> all = await db.Set<TournamentMatchEntity>().AsTracking().Where(x => x.TournamentId == t.Id).ToListAsync(ct);
		Dictionary<Guid, TournamentMatchEntity> byId = all.ToDictionary(x => x.Id);
		TournamentMatchEntity m = byId[p.Id];
		TagTournament format = FormatOf(t);

		if (p.ScheduledAt.HasValue) m.ScheduledAt = p.ScheduledAt;
		if (p.Court.IsNotNull()) m.JsonData.Court = p.Court;
		if (p.Tags?.Contains(TagTournamentMatch.Live) == true && !TournamentEngine.IsDone(m)) TournamentEngine.SetMatchStatus(m, TagTournamentMatch.Live);

		if (p.Sets != null) {
			if (m.Tags.Contains(TagTournamentMatch.Bye) || p.Sets.Count > 0 && (m.EntryAId == null || m.EntryBId == null)) return new UResponse(Usc.BadRequest, ls.Get("matchIsNotReady"));
			string? error = TournamentEngine.ValidateScore(SportTypeOf(t.Sport), TournamentEngine.IsKnockout(m), IndividualFormats.Contains(format), p.Sets, t.JsonData);
			if (error != null) return new UResponse(Usc.BadRequest, ls.Get(error));

			// The group stage is fixed once the knockout has been drawn from it.
			if (format == TagTournament.GroupsKnockout && !TournamentEngine.IsKnockout(m) && all.Any(TournamentEngine.IsKnockout)) return new UResponse(Usc.Conflict, ls.Get("aLaterMatchIsAlreadyPlayed"));

			Guid? winner = p.Sets.Count == 0 ? null : TournamentEngine.Winner(p.Sets, m.EntryAId, m.EntryBId);
			if (TournamentEngine.IsKnockout(m) && TournamentEngine.IsDone(m) && (winner != m.WinnerEntryId || p.Sets.Count == 0) && !TournamentEngine.Unpropagate(m, byId))
				return new UResponse(Usc.Conflict, ls.Get("aLaterMatchIsAlreadyPlayed"));

			m.JsonData.Sets = p.Sets;
			m.JsonData.History.Add(new MatchScoreChange { UserId = userData.Id, At = DateTime.UtcNow, Sets = p.Sets });
			m.WinnerEntryId = winner;
			TournamentEngine.SetMatchStatus(m, p.Sets.Count == 0 ? TagTournamentMatch.Scheduled : TagTournamentMatch.Finished);
			if (TournamentEngine.IsKnockout(m) && p.Sets.Count > 0) {
				TournamentEngine.Propagate(m, byId);
				TournamentEngine.ResolveByes(all);
			}

			await RevertRating(m.Id, ct);
			if (p.Sets.Count > 0 && !t.JsonData.Unrated) await ApplyRating(t, m, ct);
		}

		List<TournamentMatchEntity> created = await NextStage(t, all, userData.Id, ct);
		if (created.Count > 0) await db.Set<TournamentMatchEntity>().AddRangeAsync(created, ct);

		// Finished once every match is done and nothing more will be drawn; a cleared result reopens it.
		bool allDone = created.Count == 0 && all.All(TournamentEngine.IsDone);
		if (allDone && t.Tags.Contains(TagTournament.InProgress)) SetStatus(t, TagTournament.Finished);
		if (!allDone && t.Tags.Contains(TagTournament.Finished)) SetStatus(t, TagTournament.InProgress);

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	/// <summary>Formats that grow while playing: the knockout after the groups, the next Mexicano / Swiss round.</summary>
	private async Task<List<TournamentMatchEntity>> NextStage(TournamentEntity t, List<TournamentMatchEntity> all, Guid userId, CancellationToken ct) {
		TagTournament format = FormatOf(t);
		if (format is not (TagTournament.GroupsKnockout or TagTournament.Mexicano or TagTournament.Swiss)) return [];
		if (all.Count == 0 || !all.All(TournamentEngine.IsDone)) return [];

		List<TournamentStandingResponse> rows = await StandingRows(t.Id, ct);
		int lastRound = all.Max(x => x.Round), nextOrder = all.Max(x => x.Order) + 1;
		List<MatchPlan> plans = [];
		switch (format) {
			case TagTournament.GroupsKnockout when !all.Any(TournamentEngine.IsKnockout): {
				// Group winners first, then runners-up...: the seeding keeps players from one group apart.
				List<TournamentStandingResponse> tables = ComputeStandings(t, rows, all);
				List<Guid> seeded = [];
				for (int place = 1; place <= Math.Max(1, t.JsonData.AdvancePerGroup); place++)
					seeded.AddRange(tables.Where(x => x.Rank == place).OrderBy(x => x.GroupNumber).Select(x => x.EntryId));
				plans = TournamentEngine.SingleElimination(seeded, t.JsonData.ThirdPlaceMatch);
				break;
			}
			case TagTournament.Mexicano when lastRound < (t.JsonData.Rounds ?? 5):
				plans = TournamentEngine.MexicanoRound(ComputeStandings(t, rows, all).Select(x => x.EntryId).ToList(), lastRound + 1, nextOrder);
				break;
			case TagTournament.Swiss when lastRound < (t.JsonData.Rounds ?? Math.Max(3, (int)Math.Ceiling(Math.Log2(rows.Count)))): {
				HashSet<(Guid, Guid)> played = all.Where(x => x.EntryAId != null && x.EntryBId != null).Select(x => (x.EntryAId!.Value, x.EntryBId!.Value)).ToHashSet();
				HashSet<Guid> hadBye = all.Where(x => x.Tags.Contains(TagTournamentMatch.Bye) && x.EntryAId != null).Select(x => x.EntryAId!.Value).ToHashSet();
				plans = TournamentEngine.SwissRound(ComputeStandings(t, rows, all).Select(x => x.EntryId).ToList(), played, hadBye, lastRound + 1, nextOrder);
				break;
			}
		}

		// Knockout plans start their own numbering after the group rounds.
		List<TournamentMatchEntity> created = plans.Select(x => ToEntity(x, t.Id, userId)).ToList();
		if (format == TagTournament.GroupsKnockout)
			foreach (TournamentMatchEntity m in created) {
				m.Round += lastRound;
				m.Order += nextOrder;
			}

		TournamentEngine.ResolveByes(created);
		return created;
	}

	public async Task<UResponse<Guid?>> CreateNextTournamentSeason(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		TournamentEntity? t = await db.Set<TournamentEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (t == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("tournamentNotFound"));
		if (!CanManage(userData, t)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (FormatOf(t) != TagTournament.Ladder || !t.Tags.Contains(TagTournament.Finished)) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("onlyAFinishedBoxLeagueCanContinue"));

		List<TournamentStandingResponse> table = ComputeStandings(t, await StandingRows(t.Id, ct), await db.Set<TournamentMatchEntity>().Where(x => x.TournamentId == t.Id).ToListAsync(ct));
		List<TournamentEntryEntity> oldEntries = await db.Set<TournamentEntryEntity>().Include(x => x.Users).Where(x => x.TournamentId == t.Id && x.Tags.Contains(TagTournamentEntry.Approved)).ToListAsync(ct);
		List<Guid> userIds = oldEntries.SelectMany(x => x.Users.Select(u => u.Id)).Distinct().ToList();
		Dictionary<Guid, UserEntity> users = await db.Set<UserEntity>().AsTracking().Where(x => userIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);

		DateTime now = DateTime.UtcNow;
		TournamentEntity next = new() {
			Id = Guid.CreateVersion7(),
			CreatorId = t.CreatorId,
			CreatedAt = now,
			Tags = t.Tags.Where(x => (int)x / 100 != 3).Append(TagTournament.Registration).ToList(),
			Title = t.Title,
			SportId = t.SportId,
			StartDate = t.StartDate.AddMonths(1),
			Capacity = t.Capacity,
			EntryFee = t.EntryFee,
			MinLevel = t.MinLevel,
			MaxLevel = t.MaxLevel,
			AdminUserIds = t.AdminUserIds,
			JsonData = new TournamentJson {
				Description = t.JsonData.Description, Prize = t.JsonData.Prize, Venue = t.JsonData.Venue, Address = t.JsonData.Address,
				Latitude = t.JsonData.Latitude, Longitude = t.JsonData.Longitude, PointsForWin = t.JsonData.PointsForWin, PointsForDraw = t.JsonData.PointsForDraw,
				PointsForLoss = t.JsonData.PointsForLoss, BoxSize = t.JsonData.BoxSize, SetsToWin = t.JsonData.SetsToWin, RaceTo = t.JsonData.RaceTo,
				SuperTiebreak = t.JsonData.SuperTiebreak, Unrated = t.JsonData.Unrated, PreviousSeasonId = t.Id
			},
			// Everyone stays registered; box winners move up a box, the last of each box moves down.
			Entries = table.Select(r => {
				TournamentEntryEntity old = oldEntries.First(x => x.Id == r.EntryId);
				return new TournamentEntryEntity {
					Id = Guid.CreateVersion7(),
					CreatorId = old.CreatorId,
					CreatedAt = now,
					Tags = [TagTournamentEntry.Approved],
					Title = old.Title,
					Seed = old.Seed,
					GroupNumber = (r.GroupNumber ?? 1) - r.Promotion,
					TournamentId = Guid.Empty,
					Users = old.Users.Select(u => users[u.Id]).ToList(),
					JsonData = new TournamentEntryJson()
				};
			}).ToList()
		};
		foreach (TournamentEntryEntity entry in next.Entries) entry.TournamentId = next.Id;

		await db.Set<TournamentEntity>().AddAsync(next, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(next.Id, Usc.Created);
	}

	// ---------------- Rating ----------------

	/// <summary>Undoes the level changes a match made (only for players whose latest rated match it is).</summary>
	private async Task RevertRating(Guid matchId, CancellationToken ct) {
		List<PlayerRatingHistoryEntity> history = await db.Set<PlayerRatingHistoryEntity>().AsTracking().Where(x => x.MatchId == matchId).ToListAsync(ct);
		foreach (PlayerRatingHistoryEntity h in history) {
			Guid latest = await db.Set<PlayerRatingHistoryEntity>().Where(x => x.UserId == h.UserId && x.SportId == h.SportId).OrderByDescending(x => x.CreatedAt).Select(x => x.Id).FirstAsync(ct);
			PlayerSportProfileEntity? profile = await db.Set<PlayerSportProfileEntity>().AsTracking().FirstOrDefaultAsync(x => x.UserId == h.UserId && x.SportId == h.SportId, ct);
			if (latest == h.Id && profile != null) {
				profile.Level = h.LevelBefore;
				profile.JsonData.Rating = h.JsonData.RatingBefore;
				profile.JsonData.Deviation = h.JsonData.DeviationBefore;
				profile.JsonData.Volatility = h.JsonData.VolatilityBefore;
				profile.JsonData.MatchesPlayed = h.JsonData.MatchesPlayedBefore;
			}

			db.Set<PlayerRatingHistoryEntity>().Remove(h);
		}
	}

	/// <summary>Glicko-2 update for every player in a finished match; a side's strength is its players' average.</summary>
	private async Task ApplyRating(TournamentEntity t, TournamentMatchEntity m, CancellationToken ct) {
		List<Guid> sideA = new[] { m.EntryAId, m.PartnerAId }.OfType<Guid>().ToList();
		List<Guid> sideB = new[] { m.EntryBId, m.PartnerBId }.OfType<Guid>().ToList();
		var entryUsers = await db.Set<TournamentEntryEntity>().Where(x => sideA.Contains(x.Id) || sideB.Contains(x.Id)).Select(x => new { x.Id, Users = x.Users.Select(u => u.Id).ToList() }).ToListAsync(ct);
		List<Guid> usersA = entryUsers.Where(x => sideA.Contains(x.Id)).SelectMany(x => x.Users).ToList();
		List<Guid> usersB = entryUsers.Where(x => sideB.Contains(x.Id)).SelectMany(x => x.Users).ToList();
		List<PlayerSportProfileEntity> profiles = await db.Set<PlayerSportProfileEntity>().AsTracking().Where(x => x.SportId == t.SportId && (usersA.Contains(x.UserId) || usersB.Contains(x.UserId))).ToListAsync(ct);
		if (profiles.Count == 0) return;

		decimal min = t.Sport.MinLevel, max = t.Sport.MaxLevel;
		(double r, double d, double v) State(PlayerSportProfileEntity x) => (
			x.JsonData.Rating ?? TournamentEngine.LevelToRating(x.Level, min, max),
			x.JsonData.Deviation ?? TournamentEngine.InitialDeviation,
			x.JsonData.Volatility ?? TournamentEngine.InitialVolatility);
		(double r, double d) Side(List<Guid> userIds) {
			List<(double r, double d, double v)> s = profiles.Where(x => userIds.Contains(x.UserId)).Select(State).ToList();
			return s.Count == 0 ? (1500, TournamentEngine.InitialDeviation) : (s.Average(x => x.r), Math.Sqrt(s.Average(x => x.d * x.d)));
		}

		(double r, double d) strengthA = Side(usersA), strengthB = Side(usersB);
		double scoreA = m.WinnerEntryId == null ? 0.5 : m.WinnerEntryId == m.EntryAId ? 1 : 0;
		DateTime now = DateTime.UtcNow;
		foreach (PlayerSportProfileEntity profile in profiles) {
			bool onA = usersA.Contains(profile.UserId);
			(double r, double d, double v) before = State(profile);
			(double rating, double deviation, double volatility) after = TournamentEngine.Glicko2(before.r, before.d, before.v, onA ? strengthB.r : strengthA.r, onA ? strengthB.d : strengthA.d, onA ? scoreA : 1 - scoreA);
			decimal newLevel = TournamentEngine.RatingToLevel(after.rating, min, max);

			await db.Set<PlayerRatingHistoryEntity>().AddAsync(new PlayerRatingHistoryEntity {
				Id = Guid.CreateVersion7(),
				CreatorId = profile.UserId,
				CreatedAt = now,
				Tags = [TagPlayerRatingHistory.Match],
				UserId = profile.UserId,
				SportId = t.SportId,
				MatchId = m.Id,
				LevelBefore = profile.Level,
				LevelAfter = newLevel,
				JsonData = new PlayerRatingHistoryJson {
					RatingBefore = profile.JsonData.Rating,
					DeviationBefore = profile.JsonData.Deviation,
					VolatilityBefore = profile.JsonData.Volatility,
					MatchesPlayedBefore = profile.JsonData.MatchesPlayed
				}
			}, ct);

			profile.Level = newLevel;
			profile.JsonData.Rating = after.rating;
			profile.JsonData.Deviation = after.deviation;
			profile.JsonData.Volatility = after.volatility;
			profile.JsonData.MatchesPlayed++;
		}
	}
}

/// <summary>A match to create; the engine gives every planned match its id so brackets can link to each other.</summary>
public sealed class MatchPlan {
	public Guid Id { get; init; } = Guid.CreateVersion7();
	public required int Round { get; init; }
	public required int Order { get; init; }
	public Guid? A { get; set; }
	public Guid? B { get; set; }
	public Guid? A2 { get; init; }
	public Guid? B2 { get; init; }
	public int? Group { get; init; }
	public TagTournamentMatch Bracket { get; init; } = TagTournamentMatch.Group;
	public Guid? Next { get; set; }
	public int? NextSlot { get; set; }
	public Guid? LoserNext { get; set; }
	public int? LoserNextSlot { get; set; }
	public string? Court { get; init; }
}

/// <summary>Pure tournament logic (no database): schedules, brackets, results, standings and ratings.</summary>
public static class TournamentEngine {
	public static readonly TagTournamentMatch[] KnockoutBrackets = [TagTournamentMatch.Winners, TagTournamentMatch.Losers, TagTournamentMatch.GrandFinal, TagTournamentMatch.ThirdPlace];

	public static bool IsDone(TournamentMatchEntity m) => m.Tags.Contains(TagTournamentMatch.Finished) || m.Tags.Contains(TagTournamentMatch.Bye);

	public static bool IsKnockout(TournamentMatchEntity m) => m.Tags.Any(t => KnockoutBrackets.Contains(t));

	public static void SetMatchStatus(TournamentMatchEntity m, TagTournamentMatch status) => m.Tags = m.Tags.Where(t => (int)t / 100 != 1).Append(status).ToList();

	// ---------------- Schedules ----------------

	/// <summary>Everyone plays everyone once (circle method); an odd count gives one entry a bye each round.</summary>
	public static List<MatchPlan> RoundRobin(List<Guid> entries, int? group = null, int firstOrder = 1) {
		List<Guid?> ring = entries.Select(x => (Guid?)x).ToList();
		if (ring.Count % 2 == 1) ring.Add(null);
		int n = ring.Count;
		List<MatchPlan> result = [];
		int order = firstOrder;
		for (int round = 1; round < n; round++) {
			for (int i = 0; i < n / 2; i++) {
				Guid? a = ring[i];
				Guid? b = ring[n - 1 - i];
				if (a != null && b != null) result.Add(new MatchPlan { Round = round, Order = order++, A = a, B = b, Group = group });
			}

			// Keep the first fixed, rotate the rest by one.
			Guid? last = ring[n - 1];
			ring.RemoveAt(n - 1);
			ring.Insert(1, last);
		}

		return result;
	}

	/// <summary>Bracket order of seeds so the top seeds meet as late as possible: size 8 → 1,8,4,5,2,7,3,6.</summary>
	public static List<int> SeedPositions(int size) {
		List<int> positions = [1];
		while (positions.Count < size) {
			int total = positions.Count * 2 + 1;
			positions = positions.SelectMany(p => new[] { p, total - p }).ToList();
		}

		return positions;
	}

	private static int BracketSize(int count) {
		int size = 2;
		while (size < count) size *= 2;
		return size;
	}

	/// <summary>Knockout from seeded entries; missing seeds are byes. Optionally a match for third place between the semi-final losers.</summary>
	public static List<MatchPlan> SingleElimination(List<Guid> seeded, bool thirdPlace) {
		int size = BracketSize(seeded.Count);
		List<int> positions = SeedPositions(size);
		List<MatchPlan> result = [];
		List<MatchPlan> previous = [];
		int order = 1;
		for (int i = 0; i < size / 2; i++) {
			int seedA = positions[2 * i], seedB = positions[2 * i + 1];
			previous.Add(new MatchPlan {
				Round = 1, Order = order++, Bracket = TagTournamentMatch.Winners,
				A = seedA <= seeded.Count ? seeded[seedA - 1] : null,
				B = seedB <= seeded.Count ? seeded[seedB - 1] : null
			});
		}

		result.AddRange(previous);
		int round = 2;
		while (previous.Count > 1) {
			List<MatchPlan> current = [];
			for (int i = 0; i < previous.Count / 2; i++) {
				MatchPlan m = new() { Round = round, Order = order++, Bracket = TagTournamentMatch.Winners };
				Link(previous[2 * i], m, 1);
				Link(previous[2 * i + 1], m, 2);
				current.Add(m);
			}

			result.AddRange(current);
			previous = current;
			round++;
		}

		if (thirdPlace && size >= 4) {
			List<MatchPlan> semis = result.Where(x => x.Round == round - 2).ToList();
			MatchPlan third = new() { Round = round - 1, Order = order, Bracket = TagTournamentMatch.ThirdPlace };
			semis[0].LoserNext = third.Id;
			semis[0].LoserNextSlot = 1;
			semis[1].LoserNext = third.Id;
			semis[1].LoserNextSlot = 2;
			result.Add(third);
		}

		return result;
	}

	/// <summary>Winners bracket, losers bracket (two losses and you're out) and a grand final.</summary>
	public static List<MatchPlan> DoubleElimination(List<Guid> seeded) {
		List<MatchPlan> winners = SingleElimination(seeded, false);
		int rounds = winners.Max(x => x.Round);
		Dictionary<int, List<MatchPlan>> wb = winners.GroupBy(x => x.Round).ToDictionary(g => g.Key, g => g.OrderBy(x => x.Order).ToList());
		int order = winners.Max(x => x.Order) + 1;
		List<MatchPlan> losers = [];

		// Losers round 1: the losers of winners round 1, in pairs.
		List<MatchPlan> previous = [];
		for (int i = 0; i < wb[1].Count / 2; i++) {
			MatchPlan m = new() { Round = 1, Order = order++, Bracket = TagTournamentMatch.Losers };
			LinkLoser(wb[1][2 * i], m, 1);
			LinkLoser(wb[1][2 * i + 1], m, 2);
			previous.Add(m);
		}

		losers.AddRange(previous);
		int lbRound = 2;
		for (int w = 2; w <= rounds; w++) {
			// Even round: survivors meet the players dropping down from winners round w (reversed to avoid quick rematches).
			List<MatchPlan> drop = [];
			List<MatchPlan> wbRound = wb[w];
			for (int i = 0; i < previous.Count; i++) {
				MatchPlan m = new() { Round = lbRound, Order = order++, Bracket = TagTournamentMatch.Losers };
				Link(previous[i], m, 1);
				LinkLoser(wbRound[wbRound.Count - 1 - i], m, 2);
				drop.Add(m);
			}

			losers.AddRange(drop);
			previous = drop;
			lbRound++;
			if (previous.Count == 1) break;

			// Odd round: the survivors play each other.
			List<MatchPlan> halve = [];
			for (int i = 0; i < previous.Count / 2; i++) {
				MatchPlan m = new() { Round = lbRound, Order = order++, Bracket = TagTournamentMatch.Losers };
				Link(previous[2 * i], m, 1);
				Link(previous[2 * i + 1], m, 2);
				halve.Add(m);
			}

			losers.AddRange(halve);
			previous = halve;
			lbRound++;
		}

		MatchPlan grandFinal = new() { Round = rounds + 1, Order = order, Bracket = TagTournamentMatch.GrandFinal };
		Link(wb[rounds][0], grandFinal, 1);
		Link(previous[0], grandFinal, 2);
		return [..winners, ..losers, grandFinal];
	}

	private static void Link(MatchPlan from, MatchPlan to, int slot) {
		from.Next = to.Id;
		from.NextSlot = slot;
	}

	private static void LinkLoser(MatchPlan from, MatchPlan to, int slot) {
		from.LoserNext = to.Id;
		from.LoserNextSlot = slot;
	}

	/// <summary>Snake draw: seeds 1..n go 1,2,3,3,2,1,... over [groupCount] groups.</summary>
	public static Dictionary<Guid, int> SnakeGroups(List<Guid> seeded, int groupCount) {
		Dictionary<Guid, int> result = [];
		for (int i = 0; i < seeded.Count; i++) {
			int lap = i / groupCount, pos = i % groupCount;
			result[seeded[i]] = lap % 2 == 0 ? pos + 1 : groupCount - pos;
		}

		return result;
	}

	/// <summary>Consecutive boxes of [boxSize] from the strongest down.</summary>
	public static Dictionary<Guid, int> Boxes(List<Guid> seeded, int boxSize) {
		boxSize = Math.Max(3, boxSize);
		int boxes = Math.Max(1, (int)Math.Round(seeded.Count / (double)boxSize));
		Dictionary<Guid, int> result = [];
		for (int i = 0; i < seeded.Count; i++) result[seeded[i]] = Math.Min(boxes, i / boxSize + 1);
		return result;
	}

	/// <summary>Americano for 4k players: over n-1 rounds everyone partners everyone once; n/4 courts each round.</summary>
	public static List<MatchPlan> Americano(List<Guid> players) {
		List<MatchPlan> pairs = RoundRobin(players); // each round of the circle method is a perfect matching = this round's partnerships
		List<MatchPlan> result = [];
		int order = 1;
		foreach (IGrouping<int, MatchPlan> round in pairs.GroupBy(x => x.Round)) {
			List<MatchPlan> teams = round.OrderBy(x => x.Order).ToList();
			for (int i = 0; i + 1 < teams.Count; i += 2)
				result.Add(new MatchPlan {
					Round = round.Key, Order = order++, Court = (i / 2 + 1).ToString(),
					A = teams[i].A, A2 = teams[i].B, B = teams[i + 1].A, B2 = teams[i + 1].B
				});
		}

		return result;
	}

	/// <summary>Mexicano round: players by current ranking in fours; 1st+3rd play 2nd+4th.</summary>
	public static List<MatchPlan> MexicanoRound(List<Guid> ranked, int round, int firstOrder) {
		List<MatchPlan> result = [];
		int order = firstOrder;
		for (int i = 0; i + 3 < ranked.Count; i += 4)
			result.Add(new MatchPlan {
				Round = round, Order = order++, Court = (i / 4 + 1).ToString(),
				A = ranked[i], A2 = ranked[i + 2], B = ranked[i + 1], B2 = ranked[i + 3]
			});
		return result;
	}

	/// <summary>Swiss round: neighbours by score who haven't met; with an odd count the lowest without a bye gets one.</summary>
	public static List<MatchPlan> SwissRound(List<Guid> ranked, HashSet<(Guid, Guid)> played, HashSet<Guid> hadBye, int round, int firstOrder) {
		List<Guid> pool = [..ranked];
		List<MatchPlan> result = [];
		int order = firstOrder;
		if (pool.Count % 2 == 1) {
			Guid bye = pool.LastOrDefault(x => !hadBye.Contains(x));
			if (bye == Guid.Empty) bye = pool[^1];
			pool.Remove(bye);
			result.Add(new MatchPlan { Round = round, Order = order++, A = bye });
		}

		while (pool.Count > 0) {
			Guid a = pool[0];
			pool.RemoveAt(0);
			int j = pool.FindIndex(b => !played.Contains((a, b)) && !played.Contains((b, a)));
			if (j < 0) j = 0; // everyone left already met: allow a rematch
			result.Add(new MatchPlan { Round = round, Order = order++, A = a, B = pool[j] });
			pool.RemoveAt(j);
		}

		return result;
	}

	// ---------------- Results ----------------

	/// <summary>The side that won more sets; null on a draw.</summary>
	public static Guid? Winner(List<MatchSetScore> sets, Guid? a, Guid? b) {
		int setsA = sets.Count(x => x.A > x.B);
		int setsB = sets.Count(x => x.B > x.A);
		return setsA > setsB ? a : setsB > setsA ? b : null;
	}

	/// <summary>Checks a result against the sport's rules; returns the error key or null.</summary>
	public static string? ValidateScore(TagSport? sport, bool knockout, bool pointsFormat, List<MatchSetScore> sets, TournamentJson rules) {
		if (sets.Count == 0) return null;
		if (sets.Any(x => x.A < 0 || x.B < 0)) return "scoresAreInvalid";

		if (pointsFormat) return sets.Count == 1 && sets[0].A + sets[0].B == rules.PointsPerMatch ? null : "scoresMustAddUpToPointsPerMatch";

		string? error = sport switch {
			TagSport.Padel or TagSport.Tennis => ValidateSets(sets, rules.SetsToWin ?? 2, (hi, lo, decider) =>
				hi == 6 && lo <= 4 || hi == 7 && lo is 5 or 6 || rules.SuperTiebreak && decider && hi >= 10 && hi - lo >= 2 && (hi == 10 || hi - lo == 2)),
			TagSport.Squash => ValidateSets(sets, rules.SetsToWin ?? 3, (hi, lo, _) => hi == 11 && lo <= 9 || hi > 11 && hi - lo == 2),
			TagSport.Billiards or TagSport.Snooker => sets.Count == 1 && Math.Max(sets[0].A, sets[0].B) == (rules.RaceTo ?? (sport == TagSport.Billiards ? 5 : 3)) && sets[0].A != sets[0].B
				? null
				: "matchIsNotComplete",
			// Football, karate and the rest: one score; a knockout draw is settled by a second "set" (penalties / decision).
			_ => sets.Count == 1 || knockout && sets.Count == 2 && sets[0].A == sets[0].B ? null : "scoresAreInvalid"
		};
		if (error != null) return error;
		return knockout && Winner(sets, Guid.Empty, Guid.NewGuid()) == null ? "drawsAreNotAllowedInKnockout" : null;
	}

	/// <summary>Best-of sets: every set valid, nothing played after the match was decided, and someone reached [setsToWin].</summary>
	private static string? ValidateSets(List<MatchSetScore> sets, int setsToWin, Func<int, int, bool, bool> validSet) {
		int wonA = 0, wonB = 0;
		foreach (MatchSetScore set in sets) {
			if (wonA == setsToWin || wonB == setsToWin) return "matchIsNotComplete";
			bool decider = wonA == setsToWin - 1 && wonB == setsToWin - 1;
			if (!validSet(Math.Max(set.A, set.B), Math.Min(set.A, set.B), decider)) return "invalidSetScore";
			if (set.A > set.B) wonA++;
			else wonB++;
		}

		return wonA == setsToWin || wonB == setsToWin ? null : "matchIsNotComplete";
	}

	/// <summary>Puts the winner / loser of [m] into the matches it feeds.</summary>
	public static void Propagate(TournamentMatchEntity m, Dictionary<Guid, TournamentMatchEntity> byId) {
		Guid? loser = m.WinnerEntryId == null ? null : m.WinnerEntryId == m.EntryAId ? m.EntryBId : m.EntryAId;
		Place(m.NextMatchId, m.NextMatchSlot, m.WinnerEntryId, byId);
		Place(m.LoserNextMatchId, m.LoserNextMatchSlot, loser, byId);
	}

	private static void Place(Guid? matchId, int? slot, Guid? entry, Dictionary<Guid, TournamentMatchEntity> byId) {
		if (matchId == null || !byId.TryGetValue(matchId.Value, out TournamentMatchEntity? target)) return;
		if (slot == 1) target.EntryAId = entry;
		else target.EntryBId = entry;
	}

	/// <summary>Takes [m]'s result back out of the bracket. False when a later match already has a real result.</summary>
	public static bool Unpropagate(TournamentMatchEntity m, Dictionary<Guid, TournamentMatchEntity> byId) {
		foreach ((Guid? id, int? slot) in new[] { (m.NextMatchId, m.NextMatchSlot), (m.LoserNextMatchId, m.LoserNextMatchSlot) }) {
			if (id == null || !byId.TryGetValue(id.Value, out TournamentMatchEntity? target)) continue;
			if (target.Tags.Contains(TagTournamentMatch.Finished)) return false;
			if (target.Tags.Contains(TagTournamentMatch.Bye)) {
				if (!Unpropagate(target, byId)) return false;
				target.WinnerEntryId = null;
				SetMatchStatus(target, TagTournamentMatch.Scheduled);
			}

			Place(id, slot, null, byId);
		}

		return true;
	}

	/// <summary>Finishes bracket matches that can only have one (or no) player: once every feeding match is done, the one present advances.</summary>
	public static void ResolveByes(List<TournamentMatchEntity> matches) {
		Dictionary<Guid, TournamentMatchEntity> byId = matches.ToDictionary(x => x.Id);
		Dictionary<Guid, List<TournamentMatchEntity>> feeders = matches
			.SelectMany(m => new[] { (m.NextMatchId, m), (m.LoserNextMatchId, m) })
			.Where(x => x.Item1 != null)
			.GroupBy(x => x.Item1!.Value)
			.ToDictionary(g => g.Key, g => g.Select(x => x.m).ToList());

		bool changed = true;
		while (changed) {
			changed = false;
			foreach (TournamentMatchEntity m in matches.Where(x => IsKnockout(x) && !IsDone(x))) {
				if (feeders.TryGetValue(m.Id, out List<TournamentMatchEntity>? from) && from.Any(f => !IsDone(f))) continue;
				if (m.EntryAId != null && m.EntryBId != null) continue;
				m.WinnerEntryId = m.EntryAId ?? m.EntryBId;
				SetMatchStatus(m, TagTournamentMatch.Bye);
				Propagate(m, byId);
				changed = true;
			}
		}
	}

	// ---------------- Standings ----------------

	/// <summary>League table: points, then set difference, then score difference, then score for (Swiss: then Buchholz).</summary>
	public static List<TournamentStandingResponse> Table(List<TournamentStandingResponse> rows, List<TournamentMatchEntity> matches, TournamentJson rules, bool buchholz = false) {
		Dictionary<Guid, TournamentStandingResponse> byId = rows.ToDictionary(x => x.EntryId);
		foreach (TournamentMatchEntity m in matches.Where(IsDone)) {
			// A Swiss bye counts as a win.
			if (m.Tags.Contains(TagTournamentMatch.Bye)) {
				if (m.EntryAId != null && m.EntryBId == null && byId.TryGetValue(m.EntryAId.Value, out TournamentStandingResponse? alone)) {
					alone.Won++;
					alone.Points += rules.PointsForWin;
				}

				continue;
			}

			if (m.EntryAId == null || m.EntryBId == null || !byId.TryGetValue(m.EntryAId.Value, out TournamentStandingResponse? a) || !byId.TryGetValue(m.EntryBId.Value, out TournamentStandingResponse? b)) continue;
			int setsA = m.JsonData.Sets.Count(x => x.A > x.B), setsB = m.JsonData.Sets.Count(x => x.B > x.A);
			int scoreA = m.JsonData.Sets.Sum(x => x.A), scoreB = m.JsonData.Sets.Sum(x => x.B);
			Add(a, setsA, setsB, scoreA, scoreB, m.WinnerEntryId, rules);
			Add(b, setsB, setsA, scoreB, scoreA, m.WinnerEntryId, rules);
		}

		Dictionary<Guid, int> buchholzScores = [];
		if (buchholz)
			foreach (TournamentStandingResponse r in rows)
				buchholzScores[r.EntryId] = matches
					.Where(m => m.Tags.Contains(TagTournamentMatch.Finished) && (m.EntryAId == r.EntryId || m.EntryBId == r.EntryId))
					.Select(m => m.EntryAId == r.EntryId ? m.EntryBId : m.EntryAId)
					.Sum(o => o != null && byId.TryGetValue(o.Value, out TournamentStandingResponse? opp) ? opp.Points : 0);

		List<TournamentStandingResponse> ranked = rows
			.OrderByDescending(x => x.Points)
			.ThenByDescending(x => buchholzScores.GetValueOrDefault(x.EntryId))
			.ThenByDescending(x => x.SetsFor - x.SetsAgainst)
			.ThenByDescending(x => x.ScoreFor - x.ScoreAgainst)
			.ThenByDescending(x => x.ScoreFor)
			.ToList();
		for (int i = 0; i < ranked.Count; i++) ranked[i].Rank = i + 1;
		return ranked;
	}

	private static void Add(TournamentStandingResponse r, int setsFor, int setsAgainst, int scoreFor, int scoreAgainst, Guid? winner, TournamentJson rules) {
		r.Played++;
		r.SetsFor += setsFor;
		r.SetsAgainst += setsAgainst;
		r.ScoreFor += scoreFor;
		r.ScoreAgainst += scoreAgainst;
		if (winner == null) {
			r.Drawn++;
			r.Points += rules.PointsForDraw;
		}
		else if (winner == r.EntryId) {
			r.Won++;
			r.Points += rules.PointsForWin;
		}
		else {
			r.Lost++;
			r.Points += rules.PointsForLoss;
		}
	}

	/// <summary>Americano / Mexicano: every player collects the points their side scored.</summary>
	public static List<TournamentStandingResponse> Individual(List<TournamentStandingResponse> rows, List<TournamentMatchEntity> matches) {
		Dictionary<Guid, TournamentStandingResponse> byId = rows.ToDictionary(x => x.EntryId);
		foreach (TournamentMatchEntity m in matches.Where(x => x.Tags.Contains(TagTournamentMatch.Finished) && x.JsonData.Sets.Count > 0)) {
			int scoreA = m.JsonData.Sets.Sum(x => x.A), scoreB = m.JsonData.Sets.Sum(x => x.B);
			foreach ((Guid? id, int scored, int conceded) in new[] { (m.EntryAId, scoreA, scoreB), (m.PartnerAId, scoreA, scoreB), (m.EntryBId, scoreB, scoreA), (m.PartnerBId, scoreB, scoreA) }) {
				if (id == null || !byId.TryGetValue(id.Value, out TournamentStandingResponse? r)) continue;
				r.Played++;
				r.ScoreFor += scored;
				r.ScoreAgainst += conceded;
				r.Points += scored;
				if (scored > conceded) r.Won++;
				else if (scored < conceded) r.Lost++;
				else r.Drawn++;
			}
		}

		List<TournamentStandingResponse> ranked = rows.OrderByDescending(x => x.Points).ThenByDescending(x => x.Won).ThenByDescending(x => x.ScoreFor - x.ScoreAgainst).ToList();
		for (int i = 0; i < ranked.Count; i++) ranked[i].Rank = i + 1;
		return ranked;
	}

	/// <summary>Knockout placings: the champion, the finalist, then by how far each entry got (third place match decides 3rd/4th).</summary>
	public static List<TournamentStandingResponse> Placements(List<TournamentStandingResponse> rows, List<TournamentMatchEntity> matches) {
		Dictionary<Guid, TournamentStandingResponse> byId = rows.ToDictionary(x => x.EntryId);
		Dictionary<Guid, double> progress = rows.ToDictionary(x => x.EntryId, _ => 0d);
		int maxWinnersRound = matches.Where(x => x.Tags.Contains(TagTournamentMatch.Winners)).Select(x => x.Round).DefaultIfEmpty(0).Max();
		foreach (TournamentMatchEntity m in matches.Where(x => x.Tags.Contains(TagTournamentMatch.Finished))) {
			foreach (Guid? id in new[] { m.EntryAId, m.EntryBId }) {
				if (id == null || !byId.TryGetValue(id.Value, out TournamentStandingResponse? r)) continue;
				r.Played++;
				if (m.WinnerEntryId == id) r.Won++;
				else r.Lost++;
			}

			// How far a match is, on one scale: winners rounds, losers rounds (between), third place, grand final.
			double stage = m.Tags.Contains(TagTournamentMatch.GrandFinal) ? 1000
				: m.Tags.Contains(TagTournamentMatch.ThirdPlace) ? maxWinnersRound - 0.5
				: m.Tags.Contains(TagTournamentMatch.Losers) ? m.Round / 2.0 + 0.25
				: m.Round;
			foreach (Guid? id in new[] { m.EntryAId, m.EntryBId }) {
				if (id == null || !progress.ContainsKey(id.Value)) continue;
				double reached = m.WinnerEntryId == id ? stage + 0.1 : stage;
				if (m.Tags.Contains(TagTournamentMatch.ThirdPlace)) reached = m.WinnerEntryId == id ? maxWinnersRound - 0.4 : maxWinnersRound - 0.6;
				progress[id.Value] = Math.Max(progress[id.Value], reached);
			}
		}

		List<TournamentStandingResponse> ranked = rows.OrderByDescending(x => progress[x.EntryId]).ToList();
		for (int i = 0; i < ranked.Count; i++)
			ranked[i].Rank = i > 0 && Math.Abs(progress[ranked[i].EntryId] - progress[ranked[i - 1].EntryId]) < 0.001 ? ranked[i - 1].Rank : i + 1;
		return ranked;
	}

	// ---------------- Rating (Glicko-2) ----------------

	private const double GlickoScale = 173.7178;
	private const double Tau = 0.5;
	public const double InitialDeviation = 150; // a self-assessed level is a fair first guess
	public const double InitialVolatility = 0.06;

	/// <summary>Level (sport's own range) ↔ Glicko rating: one level is 300 rating points, the middle of the range is 1500.</summary>
	private const double PointsPerLevel = 300;

	public static double LevelToRating(decimal level, decimal min, decimal max) => 1500 + (double)(level - (min + max) / 2) * PointsPerLevel;

	public static decimal RatingToLevel(double rating, decimal min, decimal max) =>
		Math.Clamp(Math.Round((min + max) / 2 + (decimal)((rating - 1500) / PointsPerLevel), 2), min, max);

	/// <summary>One Glicko-2 rating period with a single game against an opponent (a team is its average). [score]: 1 win, 0.5 draw, 0 loss.</summary>
	public static (double rating, double deviation, double volatility) Glicko2(double rating, double deviation, double volatility, double opponentRating, double opponentDeviation, double score) {
		double mu = (rating - 1500) / GlickoScale, phi = deviation / GlickoScale;
		double muJ = (opponentRating - 1500) / GlickoScale, phiJ = opponentDeviation / GlickoScale;
		double g = 1 / Math.Sqrt(1 + 3 * phiJ * phiJ / (Math.PI * Math.PI));
		double e = 1 / (1 + Math.Exp(-g * (mu - muJ)));
		double v = 1 / (g * g * e * (1 - e));
		double delta = v * g * (score - e);

		double a = Math.Log(volatility * volatility);
		double F(double x) => Math.Exp(x) * (delta * delta - phi * phi - v - Math.Exp(x)) / (2 * Math.Pow(phi * phi + v + Math.Exp(x), 2)) - (x - a) / (Tau * Tau);
		double lower = a, upper;
		if (delta * delta > phi * phi + v) upper = Math.Log(delta * delta - phi * phi - v);
		else {
			int k = 1;
			while (F(a - k * Tau) < 0 && k < 100) k++;
			upper = a - k * Tau;
		}

		double fLower = F(lower), fUpper = F(upper);
		for (int i = 0; i < 100 && Math.Abs(upper - lower) > 1e-6; i++) {
			double c = lower + (lower - upper) * fLower / (fUpper - fLower);
			double fC = F(c);
			if (fC * fUpper <= 0) {
				lower = upper;
				fLower = fUpper;
			}
			else fLower /= 2;

			upper = c;
			fUpper = fC;
		}

		double newVolatility = Math.Exp(lower / 2);
		double phiStar = Math.Sqrt(phi * phi + newVolatility * newVolatility);
		double newPhi = 1 / Math.Sqrt(1 / (phiStar * phiStar) + 1 / v);
		double newMu = mu + newPhi * newPhi * g * (score - e);
		return (newMu * GlickoScale + 1500, Math.Clamp(newPhi * GlickoScale, 30, 350), newVolatility);
	}
}
