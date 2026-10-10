namespace SinaMN75U.Services;

public interface ISportService {
	Task<UResponse<SportSeedResponse?>> SeedSportopia(SportSeedParams p, CancellationToken ct);
	Task NotifyUpcomingGames(CancellationToken ct);
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
	public Task<UResponse<IEnumerable<TournamentMatchResponse>?>> ReadTournamentMatches(TournamentMatchReadParams p, CancellationToken ct);

	public Task<UResponse<IEnumerable<PlayerRatingHistoryResponse>?>> ReadPlayerRatingHistory(PlayerRatingHistoryReadParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<PlayerAchievementResponse>?>> ReadPlayerAchievements(PlayerAchievementReadParams p, CancellationToken ct);
	public Task<UResponse> UpdatePlayerAchievement(PlayerAchievementUpdateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<LeaderboardRowResponse>?>> ReadLeaderboard(LeaderboardParams p, CancellationToken ct);
	public Task<UResponse<PlayerStatsResponse?>> ReadPlayerStats(PlayerStatsParams p, CancellationToken ct);

	public Task<UResponse<Guid?>> CreateOpenMatch(OpenMatchCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<OpenMatchResponse>?>> ReadOpenMatches(OpenMatchReadParams p, CancellationToken ct);
	public Task<UResponse<OpenMatchResponse?>> ReadOpenMatchById(IdParams<OpenMatchSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> UpdateOpenMatch(OpenMatchUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteOpenMatch(IdParams p, CancellationToken ct);
	public Task<UResponse> JoinOpenMatch(IdParams p, CancellationToken ct);
	public Task<UResponse> LeaveOpenMatch(IdParams p, CancellationToken ct);
	public Task<UResponse> SetOpenMatchResult(OpenMatchResultParams p, CancellationToken ct);
}

public class SportService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IWalletService ws,
	IRealtimeService rt,
	IVenueService venues,
	IWebHostEnvironment env
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
		await AwardBadges([userData.Id], ct);
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
		bool wasCancelled = e.Tags.Contains(TagTournament.Cancelled);
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
		if (!wasCancelled && e.Tags.Contains(TagTournament.Cancelled)) {
			List<TournamentEntryEntity> entries = await db.Set<TournamentEntryEntity>().AsTracking().Include(x => x.Users).Where(x => x.TournamentId == e.Id).ToListAsync(ct);
			foreach (TournamentEntryEntity entry in entries) await RefundEntry(entry, e.Title, ct);
			await db.AddNotifications(entries.SelectMany(x => x.Users.Select(u => u.Id)), userData.Id, "notifTournamentCancelled", e.Title, "tournament", e.Id, ct);
			await db.SaveChangesAsync(ct);
		}

		await rt.ToGroup(RealtimeGroups.Tournament(e.Id), "tournament", e.Id);
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

		foreach (TournamentEntryEntity entry in await db.Set<TournamentEntryEntity>().AsTracking().Where(x => x.TournamentId == e.Id).ToListAsync(ct)) await RefundEntry(entry, e.Title, ct);
		await db.SaveChangesAsync(ct);
		await db.Set<PlayerAchievementEntity>().Where(x => x.TournamentId == p.Id).ExecuteDeleteAsync(ct);

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

		// Entries still waiting are out (and refunded); the fees of the playing entries go to the organizer.
		List<TournamentEntryEntity> pending = await db.Set<TournamentEntryEntity>().AsTracking().Where(x => x.TournamentId == t.Id && x.Tags.Contains(TagTournamentEntry.Pending)).ToListAsync(ct);
		foreach (TournamentEntryEntity entry in pending) {
			entry.Tags = [TagTournamentEntry.Rejected];
			await RefundEntry(entry, t.Title, ct);
		}

		decimal fees = entries.Where(x => x.JsonData is { PaidAmount: > 0, Refunded: false, Settled: false }).Sum(x => x.JsonData.PaidAmount);
		if (fees > 0) {
			UResponse<WalletTxnResponse?> paid = await ws.Transfer(new WalletTransferParams {
				SenderId = Core.App.Users.SystemAdmin.Id,
				ReceiverId = t.CreatorId,
				Amount = fees,
				Detail1 = ls.Get("tournamentEntryFees"),
				KeyValues = [new KeyValue { Key = ULocalizedConstants.Tournament, Value = t.Title }],
				TagWalletTxn = [TagWalletTxn.TournamentEntrySettlement]
			}, ct);
			if (paid.Result != null)
				foreach (TournamentEntryEntity entry in entries.Where(x => x.JsonData is { PaidAmount: > 0, Refunded: false })) entry.JsonData.Settled = true;
		}

		List<Guid> players = await db.Set<TournamentEntryEntity>().Where(x => x.TournamentId == t.Id && x.Tags.Contains(TagTournamentEntry.Approved)).SelectMany(x => x.Users.Select(u => u.Id)).ToListAsync(ct);
		await db.AddNotifications(players, userData.Id, "notifTournamentStarted", t.Title, "tournament", t.Id, ct);
		await db.SaveChangesAsync(ct);
		await rt.ToUsers(players, "notification");
		await rt.ToGroup(RealtimeGroups.Tournament(t.Id), "tournament", t.Id);
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

		decimal paidAmount = 0;
		if (t.EntryFee > 0 && p.PayFromWallet) {
			UResponse<WalletTxnResponse?> paid = await ws.Transfer(new WalletTransferParams {
				SenderId = userData.Id,
				ReceiverId = Core.App.Users.SystemAdmin.Id,
				Amount = t.EntryFee,
				Detail1 = ls.Get("tournamentEntryFee"),
				KeyValues = [new KeyValue { Key = ULocalizedConstants.Tournament, Value = t.Title }],
				TagWalletTxn = [TagWalletTxn.TournamentEntryFee]
			}, ct);
			if (paid.Result == null) return new UResponse<Guid?>(null, paid.Status, paid.Message);
			paidAmount = t.EntryFee;
		}

		List<UserEntity> users = await db.Set<UserEntity>().AsTracking().Where(x => playerIds.Contains(x.Id)).ToListAsync(ct);
		TournamentEntryEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatorId = userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = [t.Tags.Contains(TagTournament.AutoApprove) ? TagTournamentEntry.Approved : TagTournamentEntry.Pending],
			Title = p.Title.IsNotNullOrEmpty() ? p.Title : null,
			TournamentId = t.Id,
			Users = users,
			JsonData = new TournamentEntryJson { PaidAmount = paidAmount }
		};

		await db.Set<TournamentEntryEntity>().AddAsync(e, ct);
		await db.AddNotifications(playerIds.Where(x => x != userData.Id).Append(t.CreatorId), userData.Id, "notifNewEntry", t.Title, "tournament", t.Id, ct);
		await db.SaveChangesAsync(ct);
		await rt.ToUsers([t.CreatorId, ..playerIds], "notification");
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse> UpdateTournamentEntry(TournamentEntryUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		TournamentEntryEntity? e = await db.Set<TournamentEntryEntity>().AsTracking().Include(x => x.Tournament).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("entryNotFound"));
		if (!CanManage(userData, e.Tournament)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		bool wasApproved = e.Tags.Contains(TagTournamentEntry.Approved), wasRejected = e.Tags.Contains(TagTournamentEntry.Rejected);
		if (p.Title.IsNotNull()) e.Title = p.Title.IsNotNullOrEmpty() ? p.Title : null;
		if (p.Seed.HasValue) e.Seed = p.Seed;
		if (p.GroupNumber.HasValue) e.GroupNumber = p.GroupNumber;
		e.ApplyUpdateParam<TournamentEntryEntity, TagTournamentEntry, TournamentEntryJson>(p);

		List<Guid> players = await db.Set<TournamentEntryEntity>().Where(x => x.Id == e.Id).SelectMany(x => x.Users.Select(u => u.Id)).ToListAsync(ct);
		if (!wasRejected && e.Tags.Contains(TagTournamentEntry.Rejected)) {
			await RefundEntry(e, e.Tournament.Title, ct);
			await db.AddNotifications(players, userData.Id, "notifEntryRejected", e.Tournament.Title, "tournament", e.TournamentId, ct);
		}
		else if (!wasApproved && e.Tags.Contains(TagTournamentEntry.Approved))
			await db.AddNotifications(players, userData.Id, "notifEntryApproved", e.Tournament.Title, "tournament", e.TournamentId, ct);

		await db.SaveChangesAsync(ct);
		await rt.ToUsers(players, "notification");
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

		await RefundEntry(e, e.Tournament.Title, ct);
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
		bool finishing = allDone && t.Tags.Contains(TagTournament.InProgress), reopening = !allDone && t.Tags.Contains(TagTournament.Finished);
		if (finishing) SetStatus(t, TagTournament.Finished);
		if (reopening) SetStatus(t, TagTournament.InProgress);

		List<Guid> sides = new[] { m.EntryAId, m.EntryBId, m.PartnerAId, m.PartnerBId }.OfType<Guid>().ToList();
		List<Guid> players = await db.Set<TournamentEntryEntity>().Where(x => sides.Contains(x.Id)).SelectMany(x => x.Users.Select(u => u.Id)).ToListAsync(ct);
		if (p.Sets is { Count: > 0 }) await db.AddNotifications(players, userData.Id, "notifMatchResult", t.Title, "tournament", t.Id, ct);
		await db.SaveChangesAsync(ct);

		if (finishing) await AwardPlacements(t, all, userData.Id, ct);
		if (reopening) await db.Set<PlayerAchievementEntity>().Where(x => x.TournamentId == t.Id).ExecuteDeleteAsync(ct);
		if (p.Sets != null) await AwardBadges(players, ct);

		await rt.ToGroup(RealtimeGroups.Tournament(t.Id), "tournament", t.Id);
		await rt.ToUsers(players, "notification");
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

	/// <summary>Glicko-2 update for every player in a finished tournament match; a side's strength is its players' average.</summary>
	private async Task ApplyRating(TournamentEntity t, TournamentMatchEntity m, CancellationToken ct) {
		List<Guid> sideA = new[] { m.EntryAId, m.PartnerAId }.OfType<Guid>().ToList();
		List<Guid> sideB = new[] { m.EntryBId, m.PartnerBId }.OfType<Guid>().ToList();
		var entryUsers = await db.Set<TournamentEntryEntity>().Where(x => sideA.Contains(x.Id) || sideB.Contains(x.Id)).Select(x => new { x.Id, Users = x.Users.Select(u => u.Id).ToList() }).ToListAsync(ct);
		List<Guid> usersA = entryUsers.Where(x => sideA.Contains(x.Id)).SelectMany(x => x.Users).ToList();
		List<Guid> usersB = entryUsers.Where(x => sideB.Contains(x.Id)).SelectMany(x => x.Users).ToList();
		double scoreA = m.WinnerEntryId == null ? 0.5 : m.WinnerEntryId == m.EntryAId ? 1 : 0;
		await ApplyRating(t.Sport, m.Id, usersA, usersB, scoreA, ct);
	}

	/// <summary>Glicko-2 update for both sides of a match (tournament or open game); scoreA is 1 / 0.5 / 0.</summary>
	private async Task ApplyRating(SportEntity sport, Guid matchId, List<Guid> usersA, List<Guid> usersB, double scoreA, CancellationToken ct) {
		List<PlayerSportProfileEntity> profiles = await db.Set<PlayerSportProfileEntity>().AsTracking().Where(x => x.SportId == sport.Id && (usersA.Contains(x.UserId) || usersB.Contains(x.UserId))).ToListAsync(ct);
		if (profiles.Count == 0) return;

		decimal min = sport.MinLevel, max = sport.MaxLevel;
		(double r, double d, double v) State(PlayerSportProfileEntity x) => (
			x.JsonData.Rating ?? TournamentEngine.LevelToRating(x.Level, min, max),
			x.JsonData.Deviation ?? TournamentEngine.InitialDeviation,
			x.JsonData.Volatility ?? TournamentEngine.InitialVolatility);
		(double r, double d) Side(List<Guid> userIds) {
			List<(double r, double d, double v)> s = profiles.Where(x => userIds.Contains(x.UserId)).Select(State).ToList();
			return s.Count == 0 ? (1500, TournamentEngine.InitialDeviation) : (s.Average(x => x.r), Math.Sqrt(s.Average(x => x.d * x.d)));
		}

		(double r, double d) strengthA = Side(usersA), strengthB = Side(usersB);
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
				SportId = sport.Id,
				MatchId = matchId,
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

	// ---------------- Entry fees ----------------

	/// <summary>Gives a paid entry fee back (once), unless it was already paid out to the organizer.</summary>
	private async Task RefundEntry(TournamentEntryEntity e, string title, CancellationToken ct) {
		if (e.JsonData is not { PaidAmount: > 0, Refunded: false, Settled: false }) return;
		UResponse<WalletTxnResponse?> refund = await ws.Transfer(new WalletTransferParams {
			SenderId = Core.App.Users.SystemAdmin.Id,
			ReceiverId = e.CreatorId,
			Amount = e.JsonData.PaidAmount,
			Detail1 = ls.Get("tournamentEntryRefund"),
			KeyValues = [new KeyValue { Key = ULocalizedConstants.Tournament, Value = title }],
			TagWalletTxn = [TagWalletTxn.TournamentEntryRefund]
		}, ct);
		if (refund.Result != null) e.JsonData.Refunded = true;
	}

	// ---------------- Match history ----------------

	public async Task<UResponse<IEnumerable<TournamentMatchResponse>?>> ReadTournamentMatches(TournamentMatchReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<TournamentMatchEntity> q = db.Set<TournamentMatchEntity>().ApplyReadParams(p).Where(x => !x.Tags.Contains(TagTournamentMatch.Bye));
		if (userData is not { IsAdmin: true }) {
			p.SelectorArgs.Creator = null;
			if (p.SelectorArgs.Tournament != null) p.SelectorArgs.Tournament.Creator = null;
			q = q.Where(x => !x.Tournament.Tags.Contains(TagTournament.Draft));
		}

		if (p.TournamentId.HasValue) q = q.Where(x => x.TournamentId == p.TournamentId);
		if (p.SportId.HasValue) q = q.Where(x => x.Tournament.SportId == p.SportId);
		if (p.UserId.HasValue) {
			Guid uid = p.UserId.Value;
			q = q.Where(x => x.EntryA!.Users.Any(u => u.Id == uid) || x.EntryB!.Users.Any(u => u.Id == uid) || x.PartnerA!.Users.Any(u => u.Id == uid) || x.PartnerB!.Users.Any(u => u.Id == uid));
		}

		q = p.Upcoming switch {
			true => q.Where(x => !x.Tags.Contains(TagTournamentMatch.Finished) && x.EntryAId != null && x.EntryBId != null).OrderBy(x => x.ScheduledAt ?? x.Tournament.StartDate).ThenBy(x => x.Round),
			false => q.Where(x => x.Tags.Contains(TagTournamentMatch.Finished)).OrderByDescending(x => x.ScheduledAt ?? x.Tournament.StartDate).ThenByDescending(x => x.Round),
			_ => q
		};

		return await q.Select(Projections.TournamentMatchSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<IEnumerable<PlayerRatingHistoryResponse>?>> ReadPlayerRatingHistory(PlayerRatingHistoryReadParams p, CancellationToken ct) {
		IQueryable<PlayerRatingHistoryEntity> q = db.Set<PlayerRatingHistoryEntity>().ApplyReadParams(p);
		if (p.UserId.HasValue) q = q.Where(x => x.UserId == p.UserId);
		if (p.SportId.HasValue) q = q.Where(x => x.SportId == p.SportId);
		return await q.OrderBy(x => x.CreatedAt).Select(Projections.PlayerRatingHistorySelector()).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	// ---------------- Achievements, leaderboard, stats ----------------

	public async Task<UResponse<IEnumerable<PlayerAchievementResponse>?>> ReadPlayerAchievements(PlayerAchievementReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<PlayerAchievementEntity> q = db.Set<PlayerAchievementEntity>().ApplyReadParams(p);
		if (p.UserId.HasValue) q = q.Where(x => x.UserId == p.UserId);
		if (p.SportId.HasValue) q = q.Where(x => x.SportId == p.SportId);
		if (p.TournamentId.HasValue) q = q.Where(x => x.TournamentId == p.TournamentId);
		// Hidden trophies are only for their owner.
		Guid uid = userData?.Id ?? Guid.Empty;
		if (userData is not { IsAdmin: true }) q = q.Where(x => !x.Tags.Contains(TagPlayerAchievement.Hidden) || x.UserId == uid);
		p.SelectorArgs.Creator = null;

		return await q.Select(Projections.PlayerAchievementSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdatePlayerAchievement(PlayerAchievementUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		PlayerAchievementEntity? e = await db.Set<PlayerAchievementEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("achievementNotFound"));
		if (!userData.IsAdmin && e.UserId != userData.Id) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		// Only showing / hiding is up to the player.
		bool hide = (p.Tags ?? e.Tags).Contains(TagPlayerAchievement.Hidden);
		if (p.AddTags?.Contains(TagPlayerAchievement.Hidden) == true) hide = true;
		if (p.RemoveTags?.Contains(TagPlayerAchievement.Hidden) == true) hide = false;
		e.Tags = e.Tags.Where(x => x != TagPlayerAchievement.Hidden).Concat(hide ? [TagPlayerAchievement.Hidden] : []).ToList();
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<IEnumerable<LeaderboardRowResponse>?>> ReadLeaderboard(LeaderboardParams p, CancellationToken ct) {
		int pageSize = Math.Clamp(p.PageSize, 1, 200), pageNumber = Math.Max(1, p.PageNumber);
		string? country = p.Country.IsNotNullOrEmpty() ? p.Country!.ToUpperInvariant() : null;
		string? city = p.City.IsNotNullOrEmpty() ? p.City : null;

		List<(Guid UserId, int Points)> ranked;
		int total;
		if (p.ByPoints) {
			IQueryable<PlayerAchievementEntity> q = db.Set<PlayerAchievementEntity>().Where(x => x.SportId == p.SportId && x.Points > 0);
			if (p.Year.HasValue) q = q.Where(x => x.CreatedAt.Year == p.Year);
			if (country != null) q = q.Where(x => x.User.JsonData.Country == country);
			if (city != null) q = q.Where(x => x.User.JsonData.City == city);
			var grouped = q.GroupBy(x => x.UserId).Select(g => new { UserId = g.Key, Points = g.Sum(x => x.Points) });
			total = await grouped.CountAsync(ct);
			ranked = (await grouped.OrderByDescending(x => x.Points).ThenBy(x => x.UserId).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct)).Select(x => (x.UserId, x.Points)).ToList();
		}
		else {
			IQueryable<PlayerSportProfileEntity> q = db.Set<PlayerSportProfileEntity>().Where(x => x.SportId == p.SportId);
			if (country != null) q = q.Where(x => x.User.JsonData.Country == country);
			if (city != null) q = q.Where(x => x.User.JsonData.City == city);
			total = await q.CountAsync(ct);
			ranked = (await q.OrderByDescending(x => x.Level).ThenBy(x => x.UserId).Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(x => x.UserId).ToListAsync(ct)).Select(x => (x, 0)).ToList();
		}

		List<Guid> ids = ranked.Select(x => x.UserId).ToList();
		Dictionary<Guid, UserResponse> users = await db.Set<UserEntity>().Where(x => ids.Contains(x.Id)).Select(Projections.PublicUserSelector()).ToDictionaryAsync(x => x.Id, ct);
		Dictionary<Guid, PlayerSportProfileEntity> profiles = await db.Set<PlayerSportProfileEntity>().Where(x => x.SportId == p.SportId && ids.Contains(x.UserId)).ToDictionaryAsync(x => x.UserId, ct);
		Dictionary<Guid, int> points = p.ByPoints
			? ranked.ToDictionary(x => x.UserId, x => x.Points)
			: await db.Set<PlayerAchievementEntity>().Where(x => x.SportId == p.SportId && ids.Contains(x.UserId) && (p.Year == null || x.CreatedAt.Year == p.Year)).GroupBy(x => x.UserId).Select(g => new { g.Key, Points = g.Sum(x => x.Points) }).ToDictionaryAsync(x => x.Key, x => x.Points, ct);

		List<LeaderboardRowResponse> rows = ranked.Where(x => users.ContainsKey(x.UserId)).Select((x, i) => new LeaderboardRowResponse {
			Rank = (pageNumber - 1) * pageSize + i + 1,
			User = users[x.UserId],
			Level = profiles.GetValueOrDefault(x.UserId)?.Level ?? 0,
			Points = points.GetValueOrDefault(x.UserId),
			MatchesPlayed = profiles.GetValueOrDefault(x.UserId)?.JsonData.MatchesPlayed ?? 0
		}).ToList();

		return new UResponse<IEnumerable<LeaderboardRowResponse>?>(rows) { TotalCount = total, PageSize = pageSize, PageCount = (int)Math.Ceiling(total / (decimal)pageSize) };
	}

	/// <summary>One finished match of a player: 1 won, 0 drawn, -1 lost.</summary>
	private sealed record PlayedMatch(Guid SportId, DateTime At, int Result);

	/// <summary>Every finished tournament match and open game of a player, oldest first.</summary>
	private async Task<List<PlayedMatch>> PlayedMatches(Guid userId, Guid? sportId, CancellationToken ct) {
		List<Guid> entryIds = await db.Set<TournamentEntryEntity>()
			.Where(x => x.Users.Any(u => u.Id == userId) && (sportId == null || x.Tournament.SportId == sportId))
			.Select(x => x.Id).ToListAsync(ct);
		var tournamentMatches = await db.Set<TournamentMatchEntity>()
			.Where(x => x.Tags.Contains(TagTournamentMatch.Finished) && x.EntryBId != null &&
			            (entryIds.Contains(x.EntryAId ?? Guid.Empty) || entryIds.Contains(x.EntryBId ?? Guid.Empty) || entryIds.Contains(x.PartnerAId ?? Guid.Empty) || entryIds.Contains(x.PartnerBId ?? Guid.Empty)))
			.Select(x => new { x.Tournament.SportId, At = x.ScheduledAt ?? x.Tournament.StartDate, x.EntryAId, x.PartnerAId, x.WinnerEntryId, x.JsonData })
			.ToListAsync(ct);
		List<PlayedMatch> result = tournamentMatches.Select(x => {
			bool onA = entryIds.Contains(x.EntryAId ?? Guid.Empty) || entryIds.Contains(x.PartnerAId ?? Guid.Empty);
			int scoreA = x.JsonData.Sets.Sum(s => s.A), scoreB = x.JsonData.Sets.Sum(s => s.B);
			// Americano / Mexicano have no winner entry: the side that scored more won.
			int resultA = x.WinnerEntryId != null ? (x.WinnerEntryId == x.EntryAId ? 1 : -1) : x.PartnerAId != null ? Math.Sign(scoreA - scoreB) : 0;
			return new PlayedMatch(x.SportId, x.At, onA ? resultA : -resultA);
		}).ToList();

		var openMatches = await db.Set<OpenMatchEntity>()
			.Where(x => x.Tags.Contains(TagOpenMatch.Finished) && x.Users.Any(u => u.Id == userId) && (sportId == null || x.SportId == sportId))
			.Select(x => new { x.SportId, x.StartAt, x.JsonData })
			.ToListAsync(ct);
		foreach (var m in openMatches) {
			bool onA = m.JsonData.TeamA.Contains(userId);
			if (!onA && !m.JsonData.TeamB.Contains(userId)) continue;
			int resultA = Math.Sign(m.JsonData.Sets.Count(s => s.A > s.B) - m.JsonData.Sets.Count(s => s.B > s.A));
			result.Add(new PlayedMatch(m.SportId, m.StartAt, onA ? resultA : -resultA));
		}

		return result.OrderBy(x => x.At).ToList();
	}

	private static int WinStreak(IEnumerable<PlayedMatch> matches, bool best) {
		int current = 0, max = 0;
		foreach (PlayedMatch m in matches) {
			current = m.Result == 1 ? current + 1 : 0;
			max = Math.Max(max, current);
		}

		return best ? max : current;
	}

	/// <summary>Weeks in a row (counting back from this week, or last week if nothing yet this week) with a match.</summary>
	private static int WeeklyStreak(List<PlayedMatch> matches) {
		static int Week(DateTime d) => (int)Math.Floor((d.Date - new DateTime(2000, 1, 3)).TotalDays / 7);
		HashSet<int> weeks = matches.Select(x => Week(x.At)).ToHashSet();
		int week = Week(DateTime.UtcNow);
		if (!weeks.Contains(week)) week--;
		int streak = 0;
		while (weeks.Contains(week)) {
			streak++;
			week--;
		}

		return streak;
	}

	public async Task<UResponse<PlayerStatsResponse?>> ReadPlayerStats(PlayerStatsParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		Guid? userId = p.UserId ?? userData?.Id;
		if (userId == null) return new UResponse<PlayerStatsResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		UserEntity? user = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == userId, ct);
		if (user == null) return new UResponse<PlayerStatsResponse?>(null, Usc.NotFound, ls.Get("userNotFound"));

		List<PlayedMatch> played = await PlayedMatches(user.Id, p.SportId, ct);
		List<PlayerAchievementEntity> placements = await db.Set<PlayerAchievementEntity>()
			.Where(x => x.UserId == user.Id && x.Tags.Contains(TagPlayerAchievement.Placement) && (p.SportId == null || x.SportId == p.SportId)).ToListAsync(ct);

		PlayerStatsResponse stats = new() {
			User = await db.Set<UserEntity>().Where(x => x.Id == user.Id).Select(Projections.PublicUserSelector()).FirstOrDefaultAsync(ct),
			MatchesPlayed = played.Count,
			Wins = played.Count(x => x.Result == 1),
			Losses = played.Count(x => x.Result == -1),
			Draws = played.Count(x => x.Result == 0),
			CurrentWinStreak = WinStreak(played, false),
			BestWinStreak = WinStreak(played, true),
			WeeklyStreak = WeeklyStreak(played),
			TournamentsPlayed = await db.Set<TournamentEntryEntity>().CountAsync(x => x.Users.Any(u => u.Id == user.Id) && x.Tags.Contains(TagTournamentEntry.Approved) && (p.SportId == null || x.Tournament.SportId == p.SportId), ct),
			TournamentWins = placements.Count(x => x.Rank == 1),
			Podiums = placements.Count(x => x.Rank <= 3),
			RankingPoints = placements.Sum(x => x.Points),
			Followers = await db.Set<FollowEntity>().CountAsync(x => x.UserId == user.Id, ct),
			Following = await db.Set<FollowEntity>().CountAsync(x => x.CreatorId == user.Id && x.UserId != null, ct),
			ReferralCount = await db.Set<UserEntity>().CountAsync(x => x.JsonData.ReferrerId == user.Id, ct)
		};
		stats.WinRate = stats.MatchesPlayed == 0 ? 0 : (int)Math.Round(100.0 * stats.Wins / stats.MatchesPlayed);

		// The invite code is private; it is made the first time its owner asks for it.
		if (userData?.Id == user.Id) {
			if (user.JsonData.ReferralCode.IsNullOrEmpty()) {
				string code;
				do code = Convert.ToHexString(RandomNumberGenerator.GetBytes(4));
				while (await db.Set<UserEntity>().AnyAsync(x => x.JsonData.ReferralCode == code, ct));
				user.JsonData.ReferralCode = code;
				await db.SaveChangesAsync(ct);
			}

			stats.ReferralCode = user.JsonData.ReferralCode;
			if (stats.ReferralCount >= 3) await AwardBadges([user.Id], ct);
		}

		return new UResponse<PlayerStatsResponse?>(stats);
	}

	/// <summary>Ranking points for a final rank.</summary>
	private static int RankingPoints(int rank) => rank switch { 1 => 100, 2 => 70, 3 => 50, 4 => 40, <= 8 => 25, _ => 10 };

	/// <summary>Final ranks of a finished tournament: knockout placings, the box order, or the table.</summary>
	private static List<TournamentStandingResponse> FinalRanks(TournamentEntity t, List<TournamentStandingResponse> rows, List<TournamentMatchEntity> matches) {
		switch (FormatOf(t)) {
			case TagTournament.GroupsKnockout: {
				List<TournamentMatchEntity> knockout = matches.Where(TournamentEngine.IsKnockout).ToList();
				HashSet<Guid> inKnockout = knockout.SelectMany(x => new[] { x.EntryAId, x.EntryBId }).OfType<Guid>().ToHashSet();
				List<TournamentStandingResponse> placed = TournamentEngine.Placements(rows.Where(x => inKnockout.Contains(x.EntryId)).ToList(), knockout);
				List<TournamentStandingResponse> rest = rows.Where(x => !inKnockout.Contains(x.EntryId)).ToList();
				foreach (TournamentStandingResponse r in rest) r.Rank = placed.Count + 1;
				return placed.Concat(rest).ToList();
			}
			case TagTournament.Ladder: {
				List<TournamentStandingResponse> boxes = ComputeStandings(t, rows, matches);
				for (int i = 0; i < boxes.Count; i++) boxes[i].Rank = i + 1;
				return boxes;
			}
			default:
				return ComputeStandings(t, rows, matches);
		}
	}

	/// <summary>Turns the final ranks into trophies and ranking points (again from scratch if the tournament is finished again).</summary>
	private async Task AwardPlacements(TournamentEntity t, List<TournamentMatchEntity> matches, Guid creatorId, CancellationToken ct) {
		await db.Set<PlayerAchievementEntity>().Where(x => x.TournamentId == t.Id).ExecuteDeleteAsync(ct);
		List<TournamentStandingResponse> ranks = FinalRanks(t, await StandingRows(t.Id, ct), matches);
		DateTime now = DateTime.UtcNow;
		foreach (TournamentStandingResponse r in ranks)
		foreach (UserResponse u in r.Users)
			await db.Set<PlayerAchievementEntity>().AddAsync(new PlayerAchievementEntity {
				Id = Guid.CreateVersion7(),
				CreatorId = creatorId,
				CreatedAt = now,
				Tags = [TagPlayerAchievement.Placement],
				UserId = u.Id,
				SportId = t.SportId,
				TournamentId = t.Id,
				Rank = r.Rank,
				Points = RankingPoints(r.Rank),
				JsonData = new PlayerAchievementJson { Title = t.Title, EntryCount = ranks.Count }
			}, ct);

		List<Guid> players = ranks.SelectMany(x => x.Users.Select(u => u.Id)).ToList();
		await db.AddNotifications(players, creatorId, "notifTournamentFinished", t.Title, "tournament", t.Id, ct);
		await db.SaveChangesAsync(ct);
		await AwardBadges(players, ct);
	}

	/// <summary>Badge keys; the apps translate them.</summary>
	public static class Badges {
		public const string FirstMatch = "firstMatch";
		public const string TenMatches = "tenMatches";
		public const string FiftyMatches = "fiftyMatches";
		public const string FirstWin = "firstWin";
		public const string WinStreak5 = "winStreak5";
		public const string Champion = "champion";
		public const string Podium = "podium";
		public const string Organizer = "organizer";
		public const string Recruiter = "recruiter";
		public const string Regular = "regular"; // four weeks in a row
	}

	/// <summary>Gives the badges the players have earned and don't have yet.</summary>
	private async Task AwardBadges(List<Guid> userIds, CancellationToken ct) {
		DateTime now = DateTime.UtcNow;
		foreach (Guid userId in userIds.Distinct()) {
			HashSet<string?> owned = (await db.Set<PlayerAchievementEntity>().Where(x => x.UserId == userId && x.Tags.Contains(TagPlayerAchievement.Badge)).Select(x => x.JsonData.Badge).ToListAsync(ct)).ToHashSet();
			List<PlayedMatch> played = await PlayedMatches(userId, null, ct);
			List<int?> ranks = await db.Set<PlayerAchievementEntity>().Where(x => x.UserId == userId && x.Tags.Contains(TagPlayerAchievement.Placement)).Select(x => x.Rank).ToListAsync(ct);

			Dictionary<string, bool> earned = new() {
				[Badges.FirstMatch] = played.Count >= 1,
				[Badges.TenMatches] = played.Count >= 10,
				[Badges.FiftyMatches] = played.Count >= 50,
				[Badges.FirstWin] = played.Any(x => x.Result == 1),
				[Badges.WinStreak5] = WinStreak(played, true) >= 5,
				[Badges.Champion] = ranks.Any(x => x == 1),
				[Badges.Podium] = ranks.Any(x => x <= 3),
				[Badges.Organizer] = await db.Set<TournamentEntity>().AnyAsync(x => x.CreatorId == userId, ct),
				[Badges.Recruiter] = await db.Set<UserEntity>().CountAsync(x => x.JsonData.ReferrerId == userId, ct) >= 3,
				[Badges.Regular] = WeeklyStreak(played) >= 4
			};

			List<string> fresh = earned.Where(x => x.Value && !owned.Contains(x.Key)).Select(x => x.Key).ToList();
			foreach (string badge in fresh) {
				await db.Set<PlayerAchievementEntity>().AddAsync(new PlayerAchievementEntity {
					Id = Guid.CreateVersion7(),
					CreatorId = userId,
					CreatedAt = now,
					Tags = [TagPlayerAchievement.Badge],
					UserId = userId,
					JsonData = new PlayerAchievementJson { Badge = badge }
				}, ct);
				await db.AddNotifications([userId], Core.App.Users.SystemAdmin.Id, "notifNewBadge", badge, "achievement", null, ct, TagNotification.Achievement);
			}

			if (fresh.Count > 0) {
				await db.SaveChangesAsync(ct);
				await rt.ToUsers([userId], "notification");
			}
		}
	}

	// ---------------- Open match ----------------
	// The creator organizes; AdminUserIds help. Public games are joined directly, private ones after approval (invited players skip it).

	private static bool CanManage(JwtClaimData u, OpenMatchEntity m) => u.IsAdmin || m.CreatorId == u.Id || m.AdminUserIds.Contains(u.Id);

	private static void SetStatus(OpenMatchEntity m, TagOpenMatch status) => m.Tags = m.Tags.Where(x => (int)x / 100 != 1).Append(status).ToList();

	/// <summary>Open or full, from the number of players (finished and cancelled games stay as they are).</summary>
	private static void RefreshFull(OpenMatchEntity m) {
		if (m.Tags.Contains(TagOpenMatch.Finished) || m.Tags.Contains(TagOpenMatch.Cancelled)) return;
		SetStatus(m, m.Users.Count >= m.Capacity ? TagOpenMatch.Full : TagOpenMatch.Open);
	}

	public async Task<UResponse<Guid?>> CreateOpenMatch(OpenMatchCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		SportEntity? sport = await db.Set<SportEntity>().FirstOrDefaultAsync(x => x.Id == p.SportId, ct);
		if (sport == null || !sport.Tags.Contains(TagSport.Active)) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("sportNotFound"));
		if (p.Capacity < 2) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("capacityMustBeAtLeastTwo"));
		if (p.MinLevel > p.MaxLevel) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("levelIsOutOfRange"));
		if (p.StartAt < DateTime.UtcNow) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("theStartTimeIsInThePast"));
		if (p.VenueId.HasValue && !await db.Set<VenueEntity>().AnyAsync(x => x.Id == p.VenueId && x.Tags.Contains(TagVenue.Approved), ct)) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("venueNotFound"));

		UserEntity creator = await db.Set<UserEntity>().AsTracking().FirstAsync(x => x.Id == userData.Id, ct);
		bool challenge = p.Tags.Contains(TagOpenMatch.Challenge);
		OpenMatchEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = [
				TagOpenMatch.Open,
				challenge || p.Tags.Contains(TagOpenMatch.Private) ? TagOpenMatch.Private : TagOpenMatch.Public,
				p.Tags.Contains(TagOpenMatch.Friendly) ? TagOpenMatch.Friendly : TagOpenMatch.Competitive,
				..challenge ? [TagOpenMatch.Challenge] : Array.Empty<TagOpenMatch>()
			],
			StartAt = p.StartAt,
			DurationMinutes = Math.Clamp(p.DurationMinutes, 15, 600),
			Capacity = p.Capacity,
			MinLevel = p.MinLevel,
			MaxLevel = p.MaxLevel,
			PricePerPlayer = p.PricePerPlayer,
			SportId = p.SportId,
			VenueId = p.VenueId,
			AdminUserIds = p.AdminUserIds ?? [],
			Users = [creator],
			JsonData = new OpenMatchJson {
				Detail1 = p.Detail1,
				Detail2 = p.Detail2,
				Title = p.Title,
				Description = p.Description,
				Place = p.Place,
				Latitude = p.Latitude,
				Longitude = p.Longitude,
				BookingId = p.BookingId,
				InvitedUserIds = p.InvitedUserIds.Where(x => x != userData.Id).Distinct().ToList()
			}
		};

		await db.Set<OpenMatchEntity>().AddAsync(e, ct);
		await db.AddNotifications(e.JsonData.InvitedUserIds, userData.Id, challenge ? "notifChallenge" : "notifGameInvite", userData.FullName, "openMatch", e.Id, ct);
		await db.SaveChangesAsync(ct);
		await rt.ToUsers(e.JsonData.InvitedUserIds, "notification");
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<OpenMatchResponse>?>> ReadOpenMatches(OpenMatchReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		Guid uid = userData?.Id ?? Guid.Empty;
		IQueryable<OpenMatchEntity> q = db.Set<OpenMatchEntity>().ApplyReadParams(p);
		// Private games are listed only for their players, the invited and the organizers.
		if (userData is not { IsAdmin: true })
			q = q.Where(x => x.Tags.Contains(TagOpenMatch.Public) || x.CreatorId == uid || x.AdminUserIds.Contains(uid) || x.Users.Any(u => u.Id == uid));
		if (p.SportId.HasValue) q = q.Where(x => x.SportId == p.SportId);
		if (p.VenueId.HasValue) q = q.Where(x => x.VenueId == p.VenueId);
		if (p.UserId.HasValue) q = q.Where(x => x.CreatorId == p.UserId || x.Users.Any(u => u.Id == p.UserId));
		if (p.ForMyLevel && userData != null) {
			// Games the player fits: their level in that sport is inside the game's range.
			q = q.Where(x => db.Set<PlayerSportProfileEntity>().Any(pr => pr.UserId == uid && pr.SportId == x.SportId && (x.MinLevel == null || pr.Level >= x.MinLevel) && (x.MaxLevel == null || pr.Level <= x.MaxLevel)));
		}

		if (p is { Latitude: not null, Longitude: not null, RadiusKm: not null }) {
			double dLat = p.RadiusKm.Value / 111.0, dLng = p.RadiusKm.Value / (111.0 * Math.Max(0.01, Math.Cos(p.Latitude.Value * Math.PI / 180)));
			double lat = p.Latitude.Value, lng = p.Longitude.Value;
			q = q.Where(x => (x.Venue != null ? x.Venue.Latitude : x.JsonData.Latitude) >= lat - dLat && (x.Venue != null ? x.Venue.Latitude : x.JsonData.Latitude) <= lat + dLat &&
			                 (x.Venue != null ? x.Venue.Longitude : x.JsonData.Longitude) >= lng - dLng && (x.Venue != null ? x.Venue.Longitude : x.JsonData.Longitude) <= lng + dLng);
		}

		if (p.Upcoming == true) q = q.Where(x => x.StartAt >= DateTime.UtcNow && !x.Tags.Contains(TagOpenMatch.Cancelled) && !x.Tags.Contains(TagOpenMatch.Finished)).OrderBy(x => x.StartAt);
		if (p.Upcoming == false) q = q.Where(x => x.StartAt < DateTime.UtcNow || x.Tags.Contains(TagOpenMatch.Finished)).OrderByDescending(x => x.StartAt);

		return await q.Select(Projections.OpenMatchSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<OpenMatchResponse?>> ReadOpenMatchById(IdParams<OpenMatchSelectorArgs> p, CancellationToken ct) {
		OpenMatchResponse? e = await db.Set<OpenMatchEntity>().Select(Projections.OpenMatchSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<OpenMatchResponse?>(null, Usc.NotFound, ls.Get("gameNotFound")) : new UResponse<OpenMatchResponse?>(e);
	}

	public async Task<UResponse> UpdateOpenMatch(OpenMatchUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		OpenMatchEntity? e = await db.Set<OpenMatchEntity>().AsTracking().Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("gameNotFound"));
		if (!CanManage(userData, e)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if ((p.MinLevel ?? e.MinLevel) > (p.MaxLevel ?? e.MaxLevel)) return new UResponse(Usc.BadRequest, ls.Get("levelIsOutOfRange"));
		if (p.Capacity.HasValue && (p.Capacity < 2 || p.Capacity < e.Users.Count)) return new UResponse(Usc.BadRequest, ls.Get("capacityMustBeAtLeastTwo"));
		bool wasCancelled = e.Tags.Contains(TagOpenMatch.Cancelled);

		if (p.StartAt.HasValue) e.StartAt = p.StartAt.Value;
		if (p.DurationMinutes.HasValue) e.DurationMinutes = Math.Clamp(p.DurationMinutes.Value, 15, 600);
		if (p.Capacity.HasValue) e.Capacity = p.Capacity.Value;
		if (p.MinLevel.HasValue) e.MinLevel = p.MinLevel;
		if (p.MaxLevel.HasValue) e.MaxLevel = p.MaxLevel;
		if (p.PricePerPlayer.HasValue) e.PricePerPlayer = p.PricePerPlayer.Value;
		if (p.VenueId.HasValue) e.VenueId = p.VenueId == Guid.Empty ? null : p.VenueId;
		if (p.Title.IsNotNull()) e.JsonData.Title = p.Title;
		if (p.Description.IsNotNull()) e.JsonData.Description = p.Description;
		if (p.Place.IsNotNull()) e.JsonData.Place = p.Place;
		if (p.Latitude.HasValue) e.JsonData.Latitude = p.Latitude;
		if (p.Longitude.HasValue) e.JsonData.Longitude = p.Longitude;
		e.ApplyUpdateParam<OpenMatchEntity, TagOpenMatch, OpenMatchJson>(p);

		List<Guid> approved = [], notify = [];
		if (p.ApproveUserIds != null) {
			List<Guid> ids = p.ApproveUserIds.Where(x => e.JsonData.PendingUserIds.Contains(x) && e.Users.All(u => u.Id != x)).ToList();
			if (e.Users.Count + ids.Count > e.Capacity) return new UResponse(Usc.Conflict, ls.Get("thisGameIsFull"));
			e.Users = e.Users.Concat(await db.Set<UserEntity>().AsTracking().Where(x => ids.Contains(x.Id)).ToListAsync(ct)).ToList();
			e.JsonData.PendingUserIds = e.JsonData.PendingUserIds.Except(ids).ToList();
			approved.AddRange(ids);
		}

		if (p.RemoveUserIds != null) {
			List<Guid> removed = p.RemoveUserIds.Where(x => x != e.CreatorId).ToList();
			e.Users = e.Users.Where(u => !removed.Contains(u.Id)).ToList();
			e.JsonData.PendingUserIds = e.JsonData.PendingUserIds.Except(removed).ToList();
			e.JsonData.InvitedUserIds = e.JsonData.InvitedUserIds.Except(removed).ToList();
		}

		if (p.InviteUserIds != null) {
			List<Guid> invited = p.InviteUserIds.Except(e.JsonData.InvitedUserIds).Where(x => e.Users.All(u => u.Id != x)).ToList();
			e.JsonData.InvitedUserIds.AddRange(invited);
			await db.AddNotifications(invited, userData.Id, "notifGameInvite", userData.FullName, "openMatch", e.Id, ct);
			notify.AddRange(invited);
		}

		RefreshFull(e);
		await db.AddNotifications(approved, userData.Id, "notifJoinApproved", e.JsonData.Title, "openMatch", e.Id, ct);
		notify.AddRange(approved);
		if (!wasCancelled && e.Tags.Contains(TagOpenMatch.Cancelled)) {
			List<Guid> players = e.Users.Select(x => x.Id).ToList();
			await db.AddNotifications(players, userData.Id, "notifGameCancelled", e.JsonData.Title, "openMatch", e.Id, ct);
			notify.AddRange(players);
		}

		await db.SaveChangesAsync(ct);
		await rt.ToUsers(notify, "notification");
		return new UResponse();
	}

	public async Task<UResponse> DeleteOpenMatch(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		OpenMatchEntity? e = await db.Set<OpenMatchEntity>().AsTracking().Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("gameNotFound"));
		if (!CanManage(userData, e)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		// A played competitive game is part of the players' history.
		if (!userData.IsAdmin && e.Tags.Contains(TagOpenMatch.Finished)) return new UResponse(Usc.Conflict, ls.Get("aFinishedGameCannotBeDeleted"));

		await RevertRating(e.Id, ct);
		await db.AddNotifications(e.Users.Select(x => x.Id), userData.Id, "notifGameCancelled", e.JsonData.Title, null, null, ct);
		db.Set<OpenMatchEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> JoinOpenMatch(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		OpenMatchEntity? e = await db.Set<OpenMatchEntity>().AsTracking().Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("gameNotFound"));
		if (!e.Tags.Contains(TagOpenMatch.Open) || e.StartAt < DateTime.UtcNow) return new UResponse(Usc.Conflict, ls.Get("thisGameIsFull"));
		if (e.Users.Any(x => x.Id == userData.Id) || e.JsonData.PendingUserIds.Contains(userData.Id)) return new UResponse(Usc.Conflict, ls.Get("youHaveAlreadyJoined"));
		if (await db.Set<BlockEntity>().AnyAsync(x => x.CreatorId == e.CreatorId && x.BlockedUserId == userData.Id, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		bool invited = e.JsonData.InvitedUserIds.Contains(userData.Id);
		if (!invited) {
			PlayerSportProfileEntity? profile = await db.Set<PlayerSportProfileEntity>().FirstOrDefaultAsync(x => x.UserId == userData.Id && x.SportId == e.SportId, ct);
			if (profile == null) return new UResponse(Usc.BadRequest, ls.Get("addThisSportToYourProfileFirst"));
			if (profile.Level < (e.MinLevel ?? decimal.MinValue) || profile.Level > (e.MaxLevel ?? decimal.MaxValue)) return new UResponse(Usc.BadRequest, ls.Get("levelIsNotInTheGameRange"));
		}

		List<Guid> organizers = [e.CreatorId, ..e.AdminUserIds];
		if (e.Tags.Contains(TagOpenMatch.Private) && !invited) {
			e.JsonData.PendingUserIds.Add(userData.Id);
			await db.AddNotifications(organizers, userData.Id, "notifJoinRequest", userData.FullName, "openMatch", e.Id, ct);
		}
		else {
			e.Users.Add(await db.Set<UserEntity>().AsTracking().FirstAsync(x => x.Id == userData.Id, ct));
			e.JsonData.InvitedUserIds.Remove(userData.Id);
			RefreshFull(e);
			await db.AddNotifications(organizers, userData.Id, "notifPlayerJoined", userData.FullName, "openMatch", e.Id, ct);
		}

		await db.SaveChangesAsync(ct);
		await rt.ToUsers(organizers, "notification");
		return new UResponse();
	}

	public async Task<UResponse> LeaveOpenMatch(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		OpenMatchEntity? e = await db.Set<OpenMatchEntity>().AsTracking().Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("gameNotFound"));
		if (e.CreatorId == userData.Id) return new UResponse(Usc.Conflict, ls.Get("theOrganizerCannotLeaveCancelInstead"));
		if (e.Tags.Contains(TagOpenMatch.Finished)) return new UResponse(Usc.Conflict, ls.Get("aFinishedGameCannotBeDeleted"));

		e.Users = e.Users.Where(x => x.Id != userData.Id).ToList();
		e.JsonData.PendingUserIds.Remove(userData.Id);
		e.JsonData.InvitedUserIds.Remove(userData.Id);
		RefreshFull(e);
		await db.AddNotifications([e.CreatorId], userData.Id, "notifPlayerLeft", userData.FullName, "openMatch", e.Id, ct);
		await db.SaveChangesAsync(ct);
		await rt.ToUsers([e.CreatorId], "notification");
		return new UResponse();
	}

	public async Task<UResponse> SetOpenMatchResult(OpenMatchResultParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		OpenMatchEntity? e = await db.Set<OpenMatchEntity>().AsTracking().Include(x => x.Users).Include(x => x.Sport).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("gameNotFound"));
		if (!CanManage(userData, e)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (e.Tags.Contains(TagOpenMatch.Cancelled)) return new UResponse(Usc.Conflict, ls.Get("gameNotFound"));
		if (e.StartAt > DateTime.UtcNow) return new UResponse(Usc.Conflict, ls.Get("theGameHasNotStartedYet"));

		await RevertRating(e.Id, ct);
		if (p.Sets.Count == 0) {
			e.JsonData.Sets = [];
			e.JsonData.TeamA = [];
			e.JsonData.TeamB = [];
			SetStatus(e, TagOpenMatch.Open);
			RefreshFull(e);
			await db.SaveChangesAsync(ct);
			return new UResponse();
		}

		// Both teams are players of this game, nobody on both sides.
		HashSet<Guid> players = e.Users.Select(x => x.Id).ToHashSet();
		List<Guid> teamA = p.TeamA.Distinct().ToList(), teamB = p.TeamB.Distinct().ToList();
		if (teamA.Count == 0 || teamB.Count == 0 || teamA.Intersect(teamB).Any() || !teamA.Concat(teamB).All(players.Contains)) return new UResponse(Usc.BadRequest, ls.Get("teamsAreNotValid"));
		string? error = TournamentEngine.ValidateScore(SportTypeOf(e.Sport), false, false, p.Sets, new TournamentJson());
		if (error != null) return new UResponse(Usc.BadRequest, ls.Get(error));

		e.JsonData.TeamA = teamA;
		e.JsonData.TeamB = teamB;
		e.JsonData.Sets = p.Sets;
		SetStatus(e, TagOpenMatch.Finished);
		if (e.Tags.Contains(TagOpenMatch.Competitive)) {
			int balance = p.Sets.Count(x => x.A > x.B) - p.Sets.Count(x => x.B > x.A);
			await ApplyRating(e.Sport, e.Id, teamA, teamB, balance > 0 ? 1 : balance < 0 ? 0 : 0.5, ct);
		}

		await db.AddNotifications(players, userData.Id, "notifMatchResult", e.JsonData.Title, "openMatch", e.Id, ct);
		await db.SaveChangesAsync(ct);
		await AwardBadges(players.ToList(), ct);
		await rt.ToUsers(players, "notification");
		return new UResponse();
	}

	public async Task NotifyUpcomingGames(CancellationToken ct) {
		DateTime now = DateTime.UtcNow, until = now + TimeSpan.FromHours(1);
		Guid system = Core.App.Users.SystemAdmin.Id;
		HashSet<Guid> notified = [];

		List<TournamentMatchEntity> matches = await db.Set<TournamentMatchEntity>().AsTracking().Include(x => x.Tournament)
			.Where(x => x.ScheduledAt > now && x.ScheduledAt <= until && !x.Tags.Contains(TagTournamentMatch.Finished) && !x.Tags.Contains(TagTournamentMatch.Bye) && !x.JsonData.Reminded)
			.ToListAsync(ct);
		foreach (TournamentMatchEntity m in matches) {
			List<Guid> sides = new[] { m.EntryAId, m.EntryBId, m.PartnerAId, m.PartnerBId }.OfType<Guid>().ToList();
			List<Guid> players = await db.Set<TournamentEntryEntity>().Where(x => sides.Contains(x.Id)).SelectMany(x => x.Users.Select(u => u.Id)).ToListAsync(ct);
			await db.AddNotifications(players, system, "notifMatchSoon", m.Tournament.Title, "tournament", m.TournamentId, ct, TagNotification.Reminder);
			m.JsonData.Reminded = true;
			notified.UnionWith(players);
		}

		List<BookingEntity> bookings = await db.Set<BookingEntity>().AsTracking().Include(x => x.Venue)
			.Where(x => x.StartAt > now && x.StartAt <= until && x.Tags.Contains(TagBooking.Confirmed) && !x.JsonData.Reminded)
			.ToListAsync(ct);
		foreach (BookingEntity b in bookings) {
			await db.AddNotifications(b.ParticipantIds, system, "notifBookingSoon", b.Venue.Title, "booking", b.Id, ct, TagNotification.Reminder);
			b.JsonData.Reminded = true;
			notified.UnionWith(b.ParticipantIds);
		}

		List<OpenMatchEntity> games = await db.Set<OpenMatchEntity>().AsTracking().Include(x => x.Users)
			.Where(x => x.StartAt > now && x.StartAt <= until && (x.Tags.Contains(TagOpenMatch.Open) || x.Tags.Contains(TagOpenMatch.Full)) && !x.JsonData.Reminded)
			.ToListAsync(ct);
		foreach (OpenMatchEntity g in games) {
			List<Guid> players = g.Users.Select(x => x.Id).ToList();
			await db.AddNotifications(players, system, "notifGameSoon", g.JsonData.Title, "openMatch", g.Id, ct, TagNotification.Reminder);
			g.JsonData.Reminded = true;
			notified.UnionWith(players);
		}

		if (notified.Count == 0) return;
		await db.SaveChangesAsync(ct);
		await rt.ToUsers(notified, "notification");
	}

	private const string SeedNamespace = "sportopia-seed";
	private const string DefaultPassword = "Sport@1234";
	private const string EmailDomain = "sportopia.app";

	private sealed record SeedUser(string Key, string First, string Last, bool Female, string Country, string City, string Bio, string Role, decimal Balance, (TagSport Sport, decimal Level)[] Sports, bool Photo = true);

	private sealed record SeedCourt(string Key, string Title, TagSport Sport, bool Indoor, decimal Price, int Slot, string Surface, int Players, List<CourtPriceRule>? Rules = null);

	private sealed record SeedVenue(
		string Key, string Owner, string Title, TagVenue Kind, TagVenue Status, double Lat, double Lng, string City, string Address, string Phone, string Instagram,
		string Description, string[] Amenities, string Open, string Close, bool PayAtVenue, int FreeHours, int Penalty, string[] Staff, SeedCourt[] Courts,
		(byte R, byte G, byte B) Color, string? RejectionReason = null);

	private static readonly SeedUser[] Users = [
		new("demo", "آرمان", "کیانی", false, "IR", "Tehran", "عاشق پدل، آخر هفته‌ها تنیس. صاحب باغ پدل کیانی 🎾", "Demo: player + club owner + organizer", 20_000_000,
			[(TagSport.Padel, 4.2m), (TagSport.Tennis, 3.6m), (TagSport.Squash, 3.0m), (TagSport.Billiards, 3.5m), (TagSport.Snooker, 2.8m)]),
		new("owner", "مهرداد", "علوی", false, "IR", "Tehran", "مؤسس پدل آرنا تهران و باشگاه راکتی انقلاب", "Club owner", 2_000_000, [(TagSport.Padel, 3.8m), (TagSport.Tennis, 4.0m)]),
		new("organizer", "کیمیا", "صادقی", true, "IR", "Tehran", "مدیر برگزاری مسابقات پدل و اسکواش", "Organizer + staff of Padel Arena", 5_000_000, [(TagSport.Padel, 3.2m), (TagSport.Squash, 3.5m)]),
		new("neda", "ندا", "شریفی", true, "IR", "Tehran", "پدل هر روز صبح ☀️", "Player", 12_000_000, [(TagSport.Padel, 4.6m), (TagSport.Tennis, 4.1m)]),
		new("kaveh", "کاوه", "مرادی", false, "IR", "Tehran", "دست راست، بک‌هند قوی", "Player", 12_000_000, [(TagSport.Padel, 5.1m), (TagSport.Squash, 4.4m)]),
		new("sara", "سارا", "احمدی", true, "IR", "Tehran", "تنیس‌باز سابق، تازه پدل رو شروع کردم", "Player", 12_000_000, [(TagSport.Tennis, 4.8m), (TagSport.Padel, 3.9m)]),
		new("reza", "رضا", "توکلی", false, "IR", "Tehran", "فوتسال، پدل، بیلیارد — هرچی توپ داره", "Player", 12_000_000, [(TagSport.Padel, 4.4m), (TagSport.Football, 4.5m), (TagSport.Billiards, 4.0m)]),
		new("mina", "مینا", "رحیمی", true, "IR", "Isfahan", "صاحب مرکز راکتی اصفهان", "Club owner (Isfahan)", 12_000_000, [(TagSport.Padel, 3.3m), (TagSport.Tennis, 3.8m)]),
		new("babak", "بابک", "جعفری", false, "IR", "Tehran", "کاپیتان شیرهای آزادی", "Player", 12_000_000, [(TagSport.Football, 5.0m), (TagSport.Padel, 4.0m), (TagSport.Snooker, 4.2m)]),
		new("leila", "لیلا", "کریمی", true, "IR", "Shiraz", "شیراز، زمین فوتبال پنج نفره و اسکواش", "Club owner (Shiraz)", 12_000_000, [(TagSport.Squash, 3.9m), (TagSport.Padel, 3.6m)]),
		new("omid", "امید", "حسینی", false, "IR", "Tehran", "", "Player", 12_000_000, [(TagSport.Padel, 4.8m), (TagSport.Tennis, 4.4m), (TagSport.Football, 4.0m)], false),
		new("shirin", "شیرین", "نجفی", true, "IR", "Tehran", "تازه‌کار ولی پرانرژی", "Player (invited by demo)", 12_000_000, [(TagSport.Padel, 2.9m), (TagSport.Tennis, 3.1m)]),
		new("pouya", "پویا", "ابراهیمی", false, "IR", "Mashhad", "بیلیارد و اسنوکر، مشهد", "Player (pending venue)", 12_000_000, [(TagSport.Billiards, 5.2m), (TagSport.Snooker, 4.9m), (TagSport.Padel, 3.7m)]),
		new("yasaman", "یاسمن", "قاسمی", true, "IR", "Tehran", "اسکواش صبح، پدل عصر", "Player", 12_000_000, [(TagSport.Padel, 4.3m), (TagSport.Squash, 4.1m)]),
		new("farhad", "فرهاد", "زند", false, "IR", "Tehran", "مربی پدل · هم‌تیمی آرمان", "Player", 12_000_000, [(TagSport.Padel, 5.4m), (TagSport.Tennis, 5.0m)]),
		new("niloufar", "نیلوفر", "باقری", true, "IR", "Kish", "ساحل کیش، پدل کنار دریا 🌊", "Club owner (Kish)", 12_000_000, [(TagSport.Padel, 3.5m), (TagSport.Tennis, 3.4m)]),
		new("amir", "امیر", "صالحی", false, "IR", "Tehran", "", "Player", 12_000_000, [(TagSport.Padel, 4.1m), (TagSport.Football, 4.2m), (TagSport.Squash, 3.7m)], false),
		new("parisa", "پریسا", "موسوی", true, "IR", "Tehran", "بیلیارد رو جدی گرفتم", "Player", 12_000_000, [(TagSport.Padel, 3.8m), (TagSport.Billiards, 3.6m)]),
		new("hamed", "حامد", "رستمی", false, "IR", "Tabriz", "تبریز · فوتبال و اسنوکر", "Player", 12_000_000, [(TagSport.Football, 4.8m), (TagSport.Padel, 3.4m), (TagSport.Snooker, 3.8m)]),
		new("tara", "تارا", "عزیزی", true, "IR", "Tehran", "تنیس و پدل، چهارشنبه‌ها پایه‌ام", "Player", 12_000_000, [(TagSport.Padel, 4.5m), (TagSport.Tennis, 4.6m)]),
		new("siavash", "سیاوش", "کمالی", false, "IR", "Tehran", "استخر و میز اسنوکر", "Player (rejected venue)", 12_000_000, [(TagSport.Billiards, 4.6m), (TagSport.Snooker, 4.4m), (TagSport.Padel, 3.1m)]),
		new("ghazal", "غزل", "فراهانی", true, "IR", "Isfahan", "", "Player", 12_000_000, [(TagSport.Squash, 3.2m), (TagSport.Padel, 3.0m)], false),
		new("ali", "علی", "دانشور", false, "IR", "Tehran", "کاپیتان ستاره‌های شمال", "Player", 12_000_000, [(TagSport.Padel, 4.7m), (TagSport.Football, 4.6m), (TagSport.Tennis, 3.9m)]),
		new("mahsa", "مهسا", "نیک‌پور", true, "IR", "Tehran", "دعوت‌شده توسط آرمان 🙌", "Player (invited by demo)", 12_000_000, [(TagSport.Padel, 3.6m), (TagSport.Tennis, 3.3m), (TagSport.Billiards, 3.0m)]),
		new("lucas", "Lucas", "Fernández", false, "ES", "Barcelona", "Padel coach from Barcelona. Here for the winter season.", "Player (Spain)", 12_000_000, [(TagSport.Padel, 5.6m), (TagSport.Tennis, 4.5m)]),
		new("emma", "Emma", "Schulz", true, "DE", "Berlin", "Tennis first, padel second.", "Player (Germany)", 12_000_000, [(TagSport.Tennis, 5.2m), (TagSport.Padel, 4.0m), (TagSport.Squash, 4.0m)]),
		new("omar", "Omar", "Haddad", false, "AE", "Dubai", "Dubai Falcons captain · squash & padel", "Player (UAE)", 12_000_000, [(TagSport.Padel, 5.0m), (TagSport.Squash, 4.6m), (TagSport.Football, 4.4m)]),
		new("elif", "Elif", "Yılmaz", true, "TR", "Istanbul", "Istanbul ↔ Tehran", "Player (Turkey)", 12_000_000, [(TagSport.Padel, 3.9m), (TagSport.Tennis, 4.2m)]),
		new("james", "James", "Carter", false, "GB", "London", "Squash and snooker. Tea after every match.", "Player (UK)", 12_000_000, [(TagSport.Squash, 5.3m), (TagSport.Tennis, 4.3m), (TagSport.Snooker, 5.0m), (TagSport.Billiards, 4.4m)]),
		new("sofia", "Sofia", "Rossi", true, "IT", "Milan", "", "Player (Italy)", 12_000_000, [(TagSport.Padel, 4.4m), (TagSport.Tennis, 4.0m)], false),
		new("behrooz", "بهروز", "نصیری", false, "IR", "Tehran", "فروش راکت ارزان، دایرکت بدید!!!", "Spammer: blocked and reported", 500_000, [(TagSport.Padel, 2.5m)], false)
	];

	// Who registered with whose referral code (three or more earn the recruiter badge).
	private static readonly Dictionary<string, string> ReferredBy = new() {
		["shirin"] = "demo", ["mahsa"] = "demo", ["niloufar"] = "demo", ["behrooz"] = "demo", ["parisa"] = "neda", ["ghazal"] = "mina"
	};

	private static readonly string[] AllAmenities = ["parking", "shower", "locker", "cafe", "light", "indoor", "rent"];

	private static List<CourtPriceRule> PeakRules(decimal evening, decimal weekend) => [
		new() { From = "17:00", To = "24:00", PricePerHour = evening },
		new() { Days = [4, 5], From = "00:00", To = "24:00", PricePerHour = weekend }
	];

	private static readonly SeedVenue[] Venues = [
		new("arena", "owner", "پدل آرنا تهران", TagVenue.Club, TagVenue.Approved, 35.7915, 51.4105, "Tehran", "تهران، ولیعصر، بالاتر از پارک‌وی، کوچه سپیده، پلاک ۸", "02122001100", "padelarena.tehran",
			"بزرگ‌ترین مجموعه پدل تهران با دو زمین سرپوشیده‌ی پانوراما و دو زمین روباز. اجاره‌ی راکت، کلاس‌های گروهی و تورنمنت‌های ماهانه.",
			AllAmenities, "07:00", "24:00", true, 24, 50, ["organizer"], [
				new("arena-1", "زمین ۱ · پانوراما", TagSport.Padel, true, 900_000, 90, "چمن مصنوعی آبی", 4, PeakRules(1_200_000, 1_300_000)),
				new("arena-2", "زمین ۲ · پانوراما", TagSport.Padel, true, 900_000, 90, "چمن مصنوعی آبی", 4, PeakRules(1_200_000, 1_300_000)),
				new("arena-3", "زمین ۳ · روباز", TagSport.Padel, false, 700_000, 90, "چمن مصنوعی سبز", 4, PeakRules(900_000, 1_000_000)),
				new("arena-4", "زمین ۴ · روباز", TagSport.Padel, false, 700_000, 90, "چمن مصنوعی سبز", 4)
			], (30, 90, 200)),
		new("enghelab", "owner", "باشگاه راکتی انقلاب", TagVenue.Club, TagVenue.Approved, 35.7012, 51.3952, "Tehran", "تهران، خیابان انقلاب، مجموعه ورزشی انقلاب، درب شمالی", "02166001200", "enghelab.racket",
			"زمین‌های تنیس خاک رس و هارد، به‌علاوه‌ی دو سالن اسکواش استاندارد. پرداخت در محل هم ممکنه.",
			["parking", "shower", "locker", "cafe", "light"], "06:00", "23:00", true, 12, 100, [], [
				new("enghelab-t1", "تنیس ۱ · خاک رس", TagSport.Tennis, false, 600_000, 60, "خاک رس", 4, [new() { From = "16:00", To = "23:00", PricePerHour = 750_000 }]),
				new("enghelab-t2", "تنیس ۲ · هارد", TagSport.Tennis, false, 550_000, 60, "هارد کورت", 4),
				new("enghelab-t3", "تنیس ۳ · سرپوشیده", TagSport.Tennis, true, 800_000, 60, "هارد کورت", 4),
				new("enghelab-s1", "اسکواش ۱", TagSport.Squash, true, 400_000, 60, "پارکت", 2),
				new("enghelab-s2", "اسکواش ۲", TagSport.Squash, true, 400_000, 60, "پارکت", 2)
			], (200, 90, 40)),
		new("cue", "owner", "کیو کلاب ونک", TagVenue.Club, TagVenue.Approved, 35.7575, 51.4097, "Tehran", "تهران، میدان ونک، برج نگار، طبقه منفی یک", "02188001300", "cueclub.vanak",
			"میزهای بیلیارد آمریکایی و اسنوکر استاندارد، با کافه و فضای آرام.",
			["parking", "cafe", "indoor", "rent"], "12:00", "24:00", false, 48, 50, [], [
				new("cue-b1", "میز بیلیارد ۱", TagSport.Billiards, true, 250_000, 60, "ماهوت سبز", 2),
				new("cue-b2", "میز بیلیارد ۲", TagSport.Billiards, true, 250_000, 60, "ماهوت سبز", 2),
				new("cue-b3", "میز بیلیارد ۳", TagSport.Billiards, true, 250_000, 60, "ماهوت آبی", 2),
				new("cue-s1", "میز اسنوکر ۱", TagSport.Snooker, true, 350_000, 60, "ماهوت سبز", 2),
				new("cue-s2", "میز اسنوکر ۲", TagSport.Snooker, true, 350_000, 60, "ماهوت سبز", 2)
			], (20, 110, 60)),
		new("garden", "demo", "باغ پدل کیانی", TagVenue.Club, TagVenue.Approved, 35.7570, 51.3700, "Tehran", "تهران، شهرک غرب، بلوار دریا، باغ کیانی", "02188002400", "kiani.padel",
			"دو زمین پدل وسط باغ؛ یکی سرپوشیده و یکی روباز زیر درخت‌ها. پارکینگ رایگان.",
			["parking", "shower", "light", "cafe"], "08:00", "23:00", true, 24, 100, [], [
				new("garden-1", "زمین سرپوشیده", TagSport.Padel, true, 850_000, 90, "چمن مصنوعی آبی", 4, PeakRules(1_050_000, 1_100_000)),
				new("garden-2", "زمین روباز", TagSport.Padel, false, 650_000, 90, "چمن مصنوعی سبز", 4)
			], (40, 140, 90)),
		new("kish", "niloufar", "پدل ساحلی کیش", TagVenue.Club, TagVenue.Approved, 26.5337, 53.9805, "Kish", "کیش، بلوار ساحل، روبروی اسکله تفریحی", "07644001500", "kish.beach.padel",
			"پدل کنار دریا با غروب‌های دیدنی. زمین‌ها روباز و با نورپردازی شبانه.",
			["parking", "shower", "light", "rent"], "08:00", "23:00", false, 24, 100, [], [
				new("kish-1", "زمین ساحلی ۱", TagSport.Padel, false, 650_000, 90, "چمن مصنوعی", 4),
				new("kish-2", "زمین ساحلی ۲", TagSport.Padel, false, 650_000, 90, "چمن مصنوعی", 4)
			], (20, 150, 190)),
		new("isfahan", "mina", "مرکز راکتی اصفهان", TagVenue.Club, TagVenue.Approved, 32.6539, 51.6660, "Isfahan", "اصفهان، خیابان چهارباغ بالا، کوچه ۱۲", "03136001600", "isfahan.racket",
			"پدل و تنیس در قلب اصفهان.",
			["parking", "shower", "locker"], "07:00", "22:00", true, 24, 100, [], [
				new("isfahan-p1", "پدل ۱", TagSport.Padel, true, 600_000, 90, "چمن مصنوعی", 4),
				new("isfahan-p2", "پدل ۲", TagSport.Padel, false, 500_000, 90, "چمن مصنوعی", 4),
				new("isfahan-t1", "تنیس", TagSport.Tennis, false, 450_000, 60, "خاک رس", 4)
			], (180, 120, 50)),
		new("shiraz", "leila", "فوتبال پنج‌نفره شیراز", TagVenue.Club, TagVenue.Approved, 29.6100, 52.5310, "Shiraz", "شیراز، بلوار چمران، جنب پارک آزادی", "07136001700", "shiraz.fivesside",
			"دو زمین چمن مصنوعی استاندارد پنج‌نفره با رختکن و دوش.",
			["parking", "shower", "locker", "light"], "08:00", "24:00", true, 24, 100, [], [
				new("shiraz-1", "زمین A", TagSport.Football, false, 1_500_000, 60, "چمن مصنوعی نسل ۴", 10),
				new("shiraz-2", "زمین B", TagSport.Football, false, 1_300_000, 60, "چمن مصنوعی نسل ۳", 10)
			], (60, 130, 50)),
		new("shop", "owner", "فروشگاه راکت تهران", TagVenue.Shop, TagVenue.Approved, 35.7600, 51.4150, "Tehran", "تهران، خیابان گاندی، پلاک ۲۲", "02188001800", "racketshop.tehran",
			"راکت پدل و تنیس، توپ، کفش و زه‌کشی حرفه‌ای. نمایندگی برندهای معتبر.",
			["parking"], "10:00", "21:00", false, 24, 100, [], [], (120, 60, 160)),
		new("mashhad", "pouya", "پدل هاب مشهد", TagVenue.Club, TagVenue.Pending, 36.2970, 59.6060, "Mashhad", "مشهد، بلوار وکیل‌آباد، نبش وکیل‌آباد ۲۰", "05138001900", "padelhub.mashhad",
			"مجموعه‌ی تازه‌ی پدل در مشهد؛ منتظر تأیید ادمین.",
			["parking", "shower"], "08:00", "23:00", false, 24, 100, [], [
				new("mashhad-1", "زمین ۱", TagSport.Padel, true, 550_000, 90, "چمن مصنوعی", 4)
			], (150, 70, 70)),
		new("lavasan", "siavash", "دهکده ورزشی لواسان", TagVenue.Club, TagVenue.Rejected, 35.8240, 51.6330, "Tehran", "لواسان", "", "",
			"", [], "08:00", "22:00", false, 24, 100, [], [], (90, 90, 90), "عکس‌ها و نشانی دقیق مجموعه کامل نیست. لطفاً اطلاعات را تکمیل کنید.")
	];

	private readonly Dictionary<string, UserEntity> _users = new();
	private readonly Dictionary<string, string> _tokens = new();
	private readonly Dictionary<Guid, string> _keyOf = new();
	private readonly Dictionary<TagSport, SportEntity> _sports = new();
	private readonly Dictionary<string, (SeedVenue Venue, SeedCourt Court)> _courts = new();
	private readonly HashSet<(string Court, int Day, int Slot)> _usedSlots = [];
	private readonly Random _random = new(20261003);
	private readonly SportSeedResponse _result = new();
	private List<Guid> _userIds = [];
	private string _adminToken = "";
	private int _tempDay = 150;
	private CancellationToken _ct;

	private static Guid SeedId(string key) => new(MD5.HashData(Encoding.UTF8.GetBytes($"{SeedNamespace}:{key}")));

	private static string Email(string key) => $"{key}@{EmailDomain}";

	private static readonly TimeZoneInfo Tehran = FindZone("Asia/Tehran");

	private static TimeZoneInfo FindZone(string id) {
		try {
			return TimeZoneInfo.FindSystemTimeZoneById(id);
		}
		catch {
			return TimeZoneInfo.Utc;
		}
	}

	private Guid U(string key) => _users[key].Id;
	private string T(string key) => _tokens[key];
	private string Name(string key) => $"{_users[key].FirstName} {_users[key].LastName}";

	private decimal LevelOf(string key, TagSport sport) => Users.First(x => x.Key == key).Sports.FirstOrDefault(x => x.Sport == sport).Level;

	private List<string> Roster(TagSport sport) => Users.Where(x => x.Key != "behrooz" && x.Sports.Any(s => s.Sport == sport)).Select(x => x.Key).ToList();

	/// One-call demo data for Sportopia: about thirty players (levels, followers, wallets, photos), clubs with courts,
	/// photos and reviews, finished / running / open tournaments, open games, bookings, posts, stories, chats and the
	/// notifications they cause. The sport, venue and booking actions go through the real services signed in as the seed
	/// users, so levels, trophies, badges, wallet payments and notifications come out exactly as the app makes them; each
	/// past action is then moved back to its date. Every seed user has a fixed id, so Reset removes what an earlier run made
	/// and nothing else. Sign in to the app with demo@sportopia.app / Sport@1234.
	public async Task<UResponse<SportSeedResponse?>> SeedSportopia(SportSeedParams p, CancellationToken ct) {
		if (ts.ExtractClaims(p.Token) is not { IsSystemAdmin: true }) return new UResponse<SportSeedResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		_ct = ct;
		_userIds = Users.Select(x => SeedId($"user:{x.Key}")).ToList();
		DateTime now = DateTime.UtcNow;

		if (!await db.Set<UserEntity>().AnyAsync(x => x.Id == Core.App.Users.SystemAdmin.Id, ct))
			return new UResponse<SportSeedResponse?>(null, Usc.NotFound, "The system admin user doesn't exist yet; call DataSeeder/Users first.");
		if (p.Reset) await Wipe();
		else if (await db.Set<VenueEntity>().AnyAsync(x => x.Id == SeedId("venue:arena"), ct))
			return new UResponse<SportSeedResponse?>(null, Usc.Conflict, "Already seeded; call again with Reset = true.");

		string? conflict = await FindConflict();
		if (conflict != null) return new UResponse<SportSeedResponse?>(null, Usc.Conflict, conflict);

		await SeedSports(now);
		await SeedAccounts(now);
		await SeedProfiles(now);
		await SeedFollows(now);
		await SeedVenues(now);
		await SeedHistory(now);
		await SeedUpcoming(now);
		await SeedBookings(now);
		await SeedPosts(now);
		await SeedChats(now);
		await FinishNotifications(now);
		await Count();

		ULog.Success($"Sportopia seed: {_result.Users} users, {_result.Venues} venues, {_result.Tournaments} tournaments, {_result.OpenMatches} games, {_result.Bookings} bookings, {_result.Warnings.Count} warnings.");
		return new UResponse<SportSeedResponse?>(_result, Usc.Created);
	}

	// ---------------- Helpers ----------------

	/// <summary>Checks a service answer; a refusal is kept as a warning and the seed goes on.</summary>
	private bool Ok(UResponse r, string step) {
		db.ChangeTracker.Clear();
		if ((int)r.Status < 300) return true;
		_result.Warnings.Add($"{step}: {r.Message} ({r.Status})");
		ULog.Warning($"Sportopia seed: {step}: {r.Message} ({r.Status})");
		return false;
	}

	/// <summary>Runs [action] now and then moves everything it created for the seed users back (or forward) to [when].</summary>
	private async Task At(DateTime when, Func<Task> action) {
		DateTime before = DateTime.UtcNow;
		await action();
		db.ChangeTracker.Clear();
		TimeSpan shift = when - before;
		List<Guid> ids = _userIds;
		CancellationToken ct = _ct;
		await db.Set<NotificationEntity>().Where(x => x.CreatedAt >= before && ids.Contains(x.UserId)).ExecuteUpdateAsync(u => u.SetProperty(x => x.CreatedAt, x => x.CreatedAt + shift), ct);
		await db.Set<WalletTxnEntity>().Where(x => x.CreatedAt >= before && (ids.Contains(x.SenderId) || ids.Contains(x.ReceiverId))).ExecuteUpdateAsync(u => u.SetProperty(x => x.CreatedAt, x => x.CreatedAt + shift), ct);
		await db.Set<PlayerAchievementEntity>().Where(x => x.CreatedAt >= before && ids.Contains(x.UserId)).ExecuteUpdateAsync(u => u.SetProperty(x => x.CreatedAt, x => x.CreatedAt + shift), ct);
		await db.Set<PlayerRatingHistoryEntity>().Where(x => x.CreatedAt >= before && ids.Contains(x.UserId)).ExecuteUpdateAsync(u => u.SetProperty(x => x.CreatedAt, x => x.CreatedAt + shift), ct);
		await db.Set<TournamentEntity>().Where(x => x.CreatedAt >= before && ids.Contains(x.CreatorId)).ExecuteUpdateAsync(u => u.SetProperty(x => x.CreatedAt, x => x.CreatedAt + shift), ct);
		await db.Set<TournamentEntryEntity>().Where(x => x.CreatedAt >= before && ids.Contains(x.CreatorId)).ExecuteUpdateAsync(u => u.SetProperty(x => x.CreatedAt, x => x.CreatedAt + shift), ct);
		await db.Set<TournamentMatchEntity>().Where(x => x.CreatedAt >= before && ids.Contains(x.CreatorId)).ExecuteUpdateAsync(u => u.SetProperty(x => x.CreatedAt, x => x.CreatedAt + shift), ct);
		await db.Set<OpenMatchEntity>().Where(x => x.CreatedAt >= before && ids.Contains(x.CreatorId)).ExecuteUpdateAsync(u => u.SetProperty(x => x.CreatedAt, x => x.CreatedAt + shift), ct);
		await db.Set<BookingEntity>().Where(x => x.CreatedAt >= before && ids.Contains(x.CreatorId)).ExecuteUpdateAsync(u => u.SetProperty(x => x.CreatedAt, x => x.CreatedAt + shift), ct);
		await db.Set<VenueEntity>().Where(x => x.CreatedAt >= before && ids.Contains(x.CreatorId)).ExecuteUpdateAsync(u => u.SetProperty(x => x.CreatedAt, x => x.CreatedAt + shift), ct);
		await db.Set<CourtEntity>().Where(x => x.CreatedAt >= before && ids.Contains(x.CreatorId)).ExecuteUpdateAsync(u => u.SetProperty(x => x.CreatedAt, x => x.CreatedAt + shift), ct);
	}

	private T Pick<T>(IReadOnlyList<T> list) => list[_random.Next(list.Count)];

	private List<T> Shuffle<T>(IEnumerable<T> items) => items.OrderBy(_ => _random.Next()).ToList();

	/// <summary>The level on the sport's own scale (the seed data uses 1-7).</summary>
	private decimal Scale(TagSport sport, decimal level) {
		SportEntity s = _sports[sport];
		decimal scaled = s.MinLevel + (level - 1) / 6 * (s.MaxLevel - s.MinLevel);
		return Math.Round(Math.Clamp(scaled, s.MinLevel, s.MaxLevel), 2);
	}

	private static TimeSpan ParseTime(string value) => value == "24:00" ? TimeSpan.FromHours(24) : TimeSpan.Parse(value, CultureInfo.InvariantCulture);

	/// <summary>The UTC start of the [slot]th slot of a court on a day (days from today, venue time).</summary>
	private static DateTime SlotUtc(SeedVenue v, SeedCourt c, int day, int slot) {
		DateTime today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Tehran).Date;
		DateTime local = today.AddDays(day) + ParseTime(v.Open) + TimeSpan.FromMinutes(slot * c.Slot);
		return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), Tehran);
	}

	private static int SlotsPerDay(SeedVenue v, SeedCourt c) => (int)((ParseTime(v.Close) - ParseTime(v.Open)).TotalMinutes / c.Slot);

	/// <summary>Today in Tehran at [hour], plus [days].</summary>
	private static DateTime LocalAt(int days, double hour) {
		DateTime today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Tehran).Date;
		return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(today.AddDays(days).AddHours(hour), DateTimeKind.Unspecified), Tehran);
	}

	// ---------------- Reset ----------------

	/// <summary>
	/// Removes what an earlier run made. The seed users stay (other tables, like ApiLogs, may point at them) and are refreshed;
	/// what the platform still holds for them is taken back out of the system admin's wallet so its balance stays right.
	/// </summary>
	private async Task Wipe() {
		List<Guid> ids = _userIds;
		Guid admin = Core.App.Users.SystemAdmin.Id;
		CancellationToken ct = _ct;

		decimal held = await db.Set<WalletTxnEntity>().Where(x => ids.Contains(x.SenderId) && x.ReceiverId == admin && !x.Tags.Contains(TagWalletTxn.Charge)).SumAsync(x => x.Amount, ct)
		               - await db.Set<WalletTxnEntity>().Where(x => x.SenderId == admin && ids.Contains(x.ReceiverId) && !x.Tags.Contains(TagWalletTxn.Charge)).SumAsync(x => x.Amount, ct);
		if (held != 0) await db.Set<WalletEntity>().Where(x => x.CreatorId == admin).ExecuteUpdateAsync(u => u.SetProperty(x => x.Balance, x => x.Balance - held), ct);
		await db.Set<WalletTxnEntity>().Where(x => ids.Contains(x.SenderId) || ids.Contains(x.ReceiverId)).ExecuteDeleteAsync(ct);

		await db.Set<NotificationEntity>().Where(x => ids.Contains(x.UserId) || ids.Contains(x.CreatorId)).ExecuteDeleteAsync(ct);

		List<Guid> conversations = await db.Set<ConversationEntity>().Where(x => ids.Contains(x.CreatorId)).Select(x => x.Id).ToListAsync(ct);
		await db.Set<MessageEntity>().Where(x => conversations.Contains(x.ConversationId) || ids.Contains(x.CreatorId)).ExecuteDeleteAsync(ct);
		await db.Set<ConversationEntity>().Where(x => conversations.Contains(x.Id)).ExecuteDeleteAsync(ct);

		await db.Set<BlockEntity>().Where(x => ids.Contains(x.CreatorId) || ids.Contains(x.BlockedUserId)).ExecuteDeleteAsync(ct);
		List<Guid> posts = await db.Set<PostEntity>().Where(x => ids.Contains(x.CreatorId)).Select(x => x.Id).ToListAsync(ct);
		// Replies of other users to the seed posts go with them, deepest first.
		List<Guid> level = posts;
		for (int depth = 0; depth < 6 && level.Count > 0; depth++) {
			List<Guid> parents = level;
			level = await db.Set<PostEntity>().Where(x => x.ParentId != null && parents.Contains(x.ParentId.Value) && !posts.Contains(x.Id)).Select(x => x.Id).ToListAsync(ct);
			posts.AddRange(level);
		}

		await db.Set<ReportEntity>().Where(x => ids.Contains(x.CreatorId) || ids.Contains(x.TargetId) || posts.Contains(x.TargetId)).ExecuteDeleteAsync(ct);
		await db.Set<MediaEntity>().Where(x => x.PostId != null && posts.Contains(x.PostId.Value)).ExecuteDeleteAsync(ct);
		for (int depth = 0; depth < 8; depth++) {
			int deleted = await db.Set<PostEntity>().Where(x => posts.Contains(x.Id) && !x.Children.Any()).ExecuteDeleteAsync(ct);
			if (deleted == 0) break;
		}

		await db.Set<FollowEntity>().Where(x => ids.Contains(x.CreatorId) || x.UserId != null && ids.Contains(x.UserId.Value)).ExecuteDeleteAsync(ct);

		List<Guid> venueIds = await db.Set<VenueEntity>().Where(x => ids.Contains(x.CreatorId)).Select(x => x.Id).ToListAsync(ct);
		await db.Set<BookingEntity>().Where(x => venueIds.Contains(x.VenueId) || ids.Contains(x.UserId)).ExecuteDeleteAsync(ct);
		await db.Set<CommentEntity>().Where(x => ids.Contains(x.CreatorId) || x.VenueId != null && venueIds.Contains(x.VenueId.Value)).ExecuteDeleteAsync(ct);
		await db.Set<MediaEntity>().Where(x => x.VenueId != null && venueIds.Contains(x.VenueId.Value) || x.UserId != null && ids.Contains(x.UserId.Value)).ExecuteDeleteAsync(ct);
		await db.Set<OpenMatchEntity>().Where(x => x.VenueId != null && venueIds.Contains(x.VenueId.Value) && !ids.Contains(x.CreatorId)).ExecuteUpdateAsync(u => u.SetProperty(x => x.VenueId, (Guid?)null), ct);
		await db.Set<OpenMatchEntity>().Where(x => ids.Contains(x.CreatorId)).ExecuteDeleteAsync(ct);

		List<Guid> tournaments = await db.Set<TournamentEntity>().Where(x => ids.Contains(x.CreatorId)).Select(x => x.Id).ToListAsync(ct);
		await db.Set<PlayerAchievementEntity>().Where(x => ids.Contains(x.UserId) || x.TournamentId != null && tournaments.Contains(x.TournamentId.Value)).ExecuteDeleteAsync(ct);
		await db.Set<PlayerRatingHistoryEntity>().Where(x => ids.Contains(x.UserId)).ExecuteDeleteAsync(ct);
		await db.Set<TournamentMatchEntity>().Where(x => tournaments.Contains(x.TournamentId)).ExecuteDeleteAsync(ct);
		await db.Set<TournamentEntryEntity>().Where(x => tournaments.Contains(x.TournamentId)).ExecuteDeleteAsync(ct);
		await db.Set<TournamentEntity>().Where(x => tournaments.Contains(x.Id)).ExecuteDeleteAsync(ct);

		await db.Set<CourtEntity>().Where(x => venueIds.Contains(x.VenueId)).ExecuteDeleteAsync(ct);
		await db.Set<VenueEntity>().Where(x => venueIds.Contains(x.Id)).ExecuteDeleteAsync(ct);
		await db.Set<PlayerSportProfileEntity>().Where(x => ids.Contains(x.UserId)).ExecuteDeleteAsync(ct);
	}

	/// <summary>A seed email or user name that already belongs to another account would make the insert fail.</summary>
	private async Task<string?> FindConflict() {
		List<string> emails = Users.Select(x => Email(x.Key)).ToList();
		List<string> userNames = Users.Select(x => $"sp.{x.Key}").ToList();
		List<Guid> ids = _userIds;
		var taken = await db.Set<UserEntity>()
			.Where(x => !ids.Contains(x.Id) && (x.Email != null && emails.Contains(x.Email) || userNames.Contains(x.UserName)))
			.Select(x => new { x.Id, x.Email, x.UserName })
			.FirstOrDefaultAsync(_ct);
		return taken == null ? null : $"User {taken.Id} already uses {taken.Email ?? taken.UserName}; remove it or change the seed's email domain.";
	}

	// ---------------- Catalog, accounts, profiles, follows ----------------

	private async Task SeedSports(DateTime now) {
		(TagSport Type, string Title)[] catalog = [
			(TagSport.Padel, ULocalizedConstants.Padel), (TagSport.Tennis, ULocalizedConstants.Tennis), (TagSport.Squash, ULocalizedConstants.Squash),
			(TagSport.Billiards, ULocalizedConstants.Billiards), (TagSport.Snooker, ULocalizedConstants.Snooker), (TagSport.Football, ULocalizedConstants.Football),
			(TagSport.Karate, ULocalizedConstants.Karate)
		];
		for (int i = 0; i < catalog.Length; i++) {
			(TagSport type, string title) = catalog[i];
			SportEntity? s = await db.Set<SportEntity>().AsTracking().FirstOrDefaultAsync(x => x.Tags.Contains(type), _ct);
			if (s == null) {
				// Karate stays "coming soon" so that state shows up too.
				s = new SportEntity {
					Id = SeedId($"sport:{type}"),
					CreatedAt = now.AddDays(-90),
					CreatorId = Core.App.Users.SystemAdmin.Id,
					Tags = [type, type == TagSport.Karate ? TagSport.ComingSoon : TagSport.Active],
					Title = title,
					Order = i + 1,
					JsonData = new SportJson()
				};
				await db.Set<SportEntity>().AddAsync(s, _ct);
			}
			// The seed plays every sport but karate, so they have to be open.
			else if (type != TagSport.Karate && !s.Tags.Contains(TagSport.Active)) s.Tags = s.Tags.Where(x => (int)x < 200).Append(TagSport.Active).ToList();

			_sports[type] = s;
		}

		await db.SaveChangesAsync(_ct);
		db.ChangeTracker.Clear();
	}

	private async Task SeedAccounts(DateTime now) {
		Guid admin = Core.App.Users.SystemAdmin.Id;
		for (int i = 0; i < Users.Length; i++) {
			SeedUser s = Users[i];
			Guid id = SeedId($"user:{s.Key}");
			UserJson json = new() {
				Country = s.Country,
				City = s.City,
				ReferralCode = $"{s.Key.ToUpperInvariant()}{(i * 37 + 11) % 90 + 10}",
				ReferrerId = ReferredBy.TryGetValue(s.Key, out string? referrer) ? SeedId($"user:{referrer}") : null
			};
			UserEntity? e = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == id, _ct);
			if (e == null) {
				e = new UserEntity {
					Id = id,
					CreatedAt = now.AddDays(-75 + i),
					CreatorId = id,
					JsonData = json,
					Tags = [s.Female ? TagUser.Female : TagUser.Male, TagUser.Verified],
					UserName = $"sp.{s.Key}",
					Password = UPasswordHasher.Hash(DefaultPassword),
					RefreshToken = ts.GenerateRefreshToken(),
					Email = Email(s.Key)
				};
				await db.Set<UserEntity>().AddAsync(e, _ct);
			}
			else {
				e.JsonData.Country = json.Country;
				e.JsonData.City = json.City;
				e.JsonData.ReferralCode = json.ReferralCode;
				e.JsonData.ReferrerId = json.ReferrerId;
				e.Tags = [s.Female ? TagUser.Female : TagUser.Male, TagUser.Verified];
				e.UserName = $"sp.{s.Key}";
				e.Password = UPasswordHasher.Hash(DefaultPassword);
				e.Email = Email(s.Key);
			}

			e.FirstName = s.First;
			e.LastName = s.Last;
			e.Bio = s.Bio.IsNullOrEmpty() ? null : s.Bio;
			e.Birthdate = new DateTime(1985 + i % 17, i % 12 + 1, i % 27 + 1, 0, 0, 0, DateTimeKind.Utc);
			_users[s.Key] = e;
			_keyOf[id] = s.Key;
			_result.Accounts.Add(new SportSeedAccountResponse { Email = Email(s.Key), Password = DefaultPassword, FullName = $"{s.First} {s.Last}", Role = s.Role });
		}

		await db.SaveChangesAsync(_ct);

		// Wallets: the seed users get their starting balance (shown as a top-up), and the platform's escrow wallet must exist.
		foreach (SeedUser s in Users) {
			Guid id = U(s.Key);
			WalletEntity? w = await db.Set<WalletEntity>().AsTracking().FirstOrDefaultAsync(x => x.CreatorId == id, _ct);
			if (w == null) await db.Set<WalletEntity>().AddAsync(new WalletEntity { Id = SeedId($"wallet:{s.Key}"), CreatorId = id, CreatedAt = now.AddDays(-60), JsonData = new WalletJson(), Tags = [TagWallet.Primary], Balance = s.Balance }, _ct);
			else w.Balance = s.Balance;
			await db.Set<WalletTxnEntity>().AddAsync(new WalletTxnEntity {
				Id = SeedId($"charge:{s.Key}"),
				CreatedAt = now.AddDays(-45),
				CreatorId = id,
				SenderId = admin,
				ReceiverId = id,
				Amount = s.Balance,
				Tags = [TagWalletTxn.Charge],
				JsonData = new WalletTxnJson()
			}, _ct);
		}

		if (!await db.Set<WalletEntity>().AnyAsync(x => x.CreatorId == admin, _ct))
			await db.Set<WalletEntity>().AddAsync(new WalletEntity { Id = Guid.CreateVersion7(), CreatorId = admin, CreatedAt = now, JsonData = new WalletJson(), Tags = [TagWallet.Primary], Balance = 0 }, _ct);

		// Profile photos: drawn here, so the seed needs no files of its own.
		for (int i = 0; i < Users.Length; i++) {
			SeedUser s = Users[i];
			if (!s.Photo) continue;
			string path = $"users/seed-{s.Key}.jpg";
			DrawAvatar(path, i);
			await db.Set<MediaEntity>().AddAsync(new MediaEntity { Id = SeedId($"avatar:{s.Key}"), CreatedAt = now.AddDays(-40), CreatorId = U(s.Key), UserId = U(s.Key), Path = path, Tags = [TagMedia.Image, TagMedia.Profile], JsonData = new MediaJson() }, _ct);
		}

		await db.SaveChangesAsync(_ct);
		db.ChangeTracker.Clear();

		foreach (SeedUser s in Users) _tokens[s.Key] = ts.GenerateJwt(_users[s.Key]);
		_adminToken = ts.GenerateJwt(Core.App.Users.SystemAdmin);
	}

	private async Task SeedProfiles(DateTime now) {
		foreach (SeedUser s in Users) {
			for (int i = 0; i < s.Sports.Length; i++) {
				(TagSport sport, decimal level) = s.Sports[i];
				if (!_sports[sport].Tags.Contains(TagSport.Active)) continue;
				await db.Set<PlayerSportProfileEntity>().AddAsync(new PlayerSportProfileEntity {
					Id = SeedId($"profile:{s.Key}:{sport}"),
					CreatedAt = now.AddDays(-60),
					CreatorId = U(s.Key),
					Tags = i == 0 ? [TagPlayerSportProfile.Active, TagPlayerSportProfile.Primary] : [TagPlayerSportProfile.Active],
					Level = Scale(sport, level),
					UserId = U(s.Key),
					SportId = _sports[sport].Id,
					JsonData = new PlayerSportProfileJson()
				}, _ct);
			}
		}

		await db.SaveChangesAsync(_ct);
		db.ChangeTracker.Clear();
	}

	private async Task SeedFollows(DateTime now) {
		HashSet<(string From, string To)> pairs = [];
		string[] demoFollows = ["farhad", "neda", "tara", "kaveh", "yasaman", "lucas", "sara", "omid", "owner", "organizer", "james", "elif", "reza", "ali", "mahsa"];
		foreach (string to in demoFollows) pairs.Add(("demo", to));
		foreach (SeedUser s in Users.Where(x => x.Key is not ("demo" or "behrooz" or "ghazal" or "hamed"))) pairs.Add((s.Key, "demo"));
		List<string> keys = Users.Select(x => x.Key).Where(x => x != "behrooz").ToList();
		foreach (string from in keys)
		foreach (string to in Shuffle(keys).Take(_random.Next(4, 9)))
			if (from != to)
				pairs.Add((from, to));
		pairs.Add(("behrooz", "demo"));
		pairs.Add(("behrooz", "neda"));

		foreach ((string from, string to) in pairs)
			await db.Set<FollowEntity>().AddAsync(new FollowEntity {
				Id = Guid.CreateVersion7(),
				CreatedAt = now.AddDays(-_random.Next(5, 55)).AddMinutes(-_random.Next(0, 1440)),
				CreatorId = U(from),
				UserId = U(to),
				Tags = [TagFollow.User],
				JsonData = new FollowJson()
			}, _ct);

		await db.Set<BlockEntity>().AddAsync(new BlockEntity { Id = Guid.CreateVersion7(), CreatedAt = now.AddDays(-4), CreatorId = U("demo"), BlockedUserId = U("behrooz"), Tags = [TagBlock.User], JsonData = new BlockJson() }, _ct);
		await db.Set<BlockEntity>().AddAsync(new BlockEntity { Id = Guid.CreateVersion7(), CreatedAt = now.AddDays(-9), CreatorId = U("kaveh"), BlockedUserId = U("behrooz"), Tags = [TagBlock.User], JsonData = new BlockJson() }, _ct);
		await db.SaveChangesAsync(_ct);
		db.ChangeTracker.Clear();
	}

	// ---------------- Venues ----------------

	private async Task SeedVenues(DateTime now) {
		for (int i = 0; i < Venues.Length; i++) {
			SeedVenue v = Venues[i];
			Guid venueId = SeedId($"venue:{v.Key}");
			await At(now.AddDays(-58 + i * 2), async () => {
				UResponse<Guid?> created = await venues.CreateVenue(new VenueCreateParams {
					Token = T(v.Owner),
					Id = venueId,
					Tags = v.PayAtVenue ? [v.Kind, TagVenue.PayAtVenue] : [v.Kind],
					Title = v.Title,
					Latitude = v.Lat,
					Longitude = v.Lng,
					Address = v.Address,
					PhoneNumber = v.Phone.IsNullOrEmpty() ? null : v.Phone,
					Country = "IR",
					City = v.City,
					Description = v.Description.IsNullOrEmpty() ? null : v.Description,
					Instagram = v.Instagram.IsNullOrEmpty() ? null : v.Instagram,
					Website = v.Instagram.IsNullOrEmpty() ? null : $"https://{v.Instagram.Replace('.', '-')}.ir",
					Whatsapp = v.Phone.IsNullOrEmpty() ? null : $"+98{v.Phone[1..]}",
					TimeZone = "Asia/Tehran",
					Currency = "IRT",
					Amenities = [..v.Amenities],
					OpeningHours = Enumerable.Range(0, 7).Select(d => new VenueOpeningHour { Day = d, Open = v.Open, Close = v.Close }).ToList(),
					CancellationFreeHours = v.FreeHours,
					CancellationPenaltyPercent = v.Penalty,
					AdminUserIds = v.Staff.Select(U).ToList()
				}, _ct);
				if (!Ok(created, $"venue {v.Key}")) return;

				foreach (SeedCourt c in v.Courts) {
					bool court = Ok(await venues.CreateCourt(new CourtCreateParams {
						Token = T(v.Owner),
						Id = SeedId($"court:{c.Key}"),
						Tags = [c.Indoor ? TagCourt.Indoor : TagCourt.Outdoor],
						Title = c.Title,
						VenueId = venueId,
						SportId = _sports[c.Sport].Id,
						PricePerHour = c.Price,
						SlotMinutes = c.Slot,
						Surface = c.Surface,
						Players = c.Players,
						PriceRules = c.Rules
					}, _ct), $"court {c.Key}");
					if (court) _courts[c.Key] = (v, c);
				}

				if (v.Status != TagVenue.Pending)
					Ok(await venues.UpdateVenue(new VenueUpdateParams {
						Token = _adminToken,
						Id = venueId,
						RemoveTags = [TagVenue.Pending],
						AddTags = [v.Status],
						RejectionReason = v.RejectionReason
					}, _ct), $"approve venue {v.Key}");
			});

			// Photos: a cover and two more of the place.
			if (v.Status == TagVenue.Rejected) continue;
			TagMedia[] shots = [TagMedia.Cover, TagMedia.Exterior, TagMedia.Interior];
			for (int n = 0; n < shots.Length; n++) {
				string path = $"venues/seed-{v.Key}-{n}.jpg";
				DrawVenue(path, v, n);
				await db.Set<MediaEntity>().AddAsync(new MediaEntity {
					Id = SeedId($"venue-media:{v.Key}:{n}"),
					CreatedAt = now.AddDays(-57 + i * 2),
					CreatorId = U(v.Owner),
					VenueId = venueId,
					Path = path,
					Tags = [TagMedia.Image, shots[n]],
					JsonData = new MediaJson()
				}, _ct);
			}
		}

		// Reviews
		(string Venue, string User, int Score, string Text)[] reviews = [
			("arena", "neda", 5, "بهترین زمین‌های پدل تهران. شیشه‌ها تمیز و نور عالی."),
			("arena", "kaveh", 4, "زمین‌ها عالی‌ان ولی ساعت‌های عصر خیلی شلوغه."),
			("arena", "lucas", 5, "Panoramic courts as good as Barcelona. Great staff!"),
			("arena", "tara", 4, "کافه‌اش خیلی خوبه، فقط پارکینگ کمه."),
			("arena", "sara", 5, "راکت اجاره‌ای هم کیفیت خوبی داشت."),
			("arena", "demo", 4, "برای تورنمنت‌ها بهترین جاست."),
			("enghelab", "emma", 5, "Lovely clay court, well maintained."),
			("enghelab", "james", 4, "Squash courts are proper size. Showers could be warmer."),
			("enghelab", "sara", 4, "خاک رس خیلی خوب نگهداری میشه."),
			("enghelab", "demo", 3, "رزرو در محل گاهی شلوغ میشه."),
			("cue", "pouya", 5, "میزهای اسنوکر استاندارد و ماهوت نو."),
			("cue", "parisa", 4, "فضای آرومی داره، قهوه‌اش هم خوبه."),
			("cue", "james", 5, "Best snooker tables I found in Tehran."),
			("garden", "farhad", 5, "بازی زیر درخت‌ها حال دیگه‌ای داره 🌳"),
			("garden", "yasaman", 4, "زمین روباز عصرها خنکه، عالیه."),
			("garden", "neda", 5, "میزبانی آرمان عالیه!"),
			("kish", "niloufar", 5, "غروب کیش و پدل؛ دیگه چی می‌خواین؟"),
			("kish", "omar", 4, "Beautiful sunset games. A bit windy."),
			("shiraz", "babak", 5, "چمن نسل ۴ واقعاً فرق داره."),
			("isfahan", "ghazal", 4, "زمین سرپوشیده‌اش خیلی خوبه.")
		];
		foreach ((string venueKey, string user, int score, string text) in reviews)
			await db.Set<CommentEntity>().AddAsync(new CommentEntity {
				Id = Guid.CreateVersion7(),
				CreatedAt = now.AddDays(-_random.Next(2, 40)),
				CreatorId = U(user),
				Tags = [TagComment.Released],
				Score = score,
				Description = text,
				VenueId = SeedId($"venue:{venueKey}"),
				JsonData = new CommentJson()
			}, _ct);

		await db.SaveChangesAsync(_ct);
		db.ChangeTracker.Clear();
	}

	// ---------------- Scores ----------------

	private static readonly (int Hi, int Lo)[] TennisSets = [(6, 0), (6, 1), (6, 2), (6, 3), (6, 3), (6, 4), (6, 4), (7, 5), (7, 6)];

	/// <summary>Chance that side A wins, from the levels (plus a bonus for the seed's favourites).</summary>
	private double ChanceA(double strengthA, double strengthB) => 1 / (1 + Math.Exp(-(strengthA - strengthB) * 1.3));

	/// <summary>A valid result for the sport's rules: A wins when [aWins], a draw only where draws are allowed.</summary>
	private List<MatchSetScore> Score(TagSport sport, bool aWins, bool draw, bool pointsFormat, int pointsPerMatch, bool knockout) {
		MatchSetScore Set(bool aTakes, int hi, int lo) => aTakes ? new MatchSetScore { A = hi, B = lo } : new MatchSetScore { A = lo, B = hi };

		if (pointsFormat) {
			int winner = _random.Next(13, 19);
			return [Set(aWins, winner, pointsPerMatch - winner)];
		}

		switch (sport) {
			case TagSport.Padel:
			case TagSport.Tennis: {
				List<bool> order = _random.NextDouble() < 0.65 ? [true, true] : [true, false, true];
				return order.Select(w => {
					(int hi, int lo) = Pick(TennisSets);
					return Set(w == aWins, hi, lo);
				}).ToList();
			}
			case TagSport.Squash: {
				List<bool> order = _random.Next(3) switch { 0 => [true, true, true], 1 => [true, false, true, true], _ => [false, true, true, false, true] };
				return order.Select(w => _random.NextDouble() < 0.2 ? Set(w == aWins, 12, 10) : Set(w == aWins, 11, _random.Next(2, 10))).ToList();
			}
			case TagSport.Billiards:
				return [Set(aWins, 5, _random.Next(0, 5))];
			case TagSport.Snooker:
				return [Set(aWins, 3, _random.Next(0, 3))];
			default: {
				int goals = _random.Next(1, 6);
				if (draw && !knockout) return [new MatchSetScore { A = goals - 1, B = goals - 1 }];
				if (draw) return [new MatchSetScore { A = goals, B = goals }, Set(aWins, 5, 4)];
				return [Set(aWins, goals, _random.Next(0, goals))];
			}
		}
	}

	// ---------------- Tournaments ----------------

	private async Task<bool> CreateTournament(string key, string creator, TagSport sport, TagTournament format, TagTournament type, string title, DateTime start, int capacity, decimal fee,
		bool autoApprove, Action<TournamentCreateParams>? options = null, bool draft = false) {
		TournamentCreateParams p = new() {
			Token = T(creator),
			Id = SeedId($"tournament:{key}"),
			Tags = [format, type, ..autoApprove ? [TagTournament.AutoApprove] : Array.Empty<TagTournament>(), ..draft ? [TagTournament.Draft] : Array.Empty<TagTournament>()],
			Title = title,
			SportId = _sports[sport].Id,
			StartDate = start,
			Capacity = capacity,
			EntryFee = fee
		};
		options?.Invoke(p);
		return Ok(await CreateTournament(p, _ct), $"tournament {key}");
	}

	private async Task<Guid?> Register(string tournament, string player, string? partner = null, string? team = null, bool pay = true) {
		UResponse<Guid?> r = await RegisterTournamentEntry(new TournamentRegisterParams {
			Token = T(player),
			TournamentId = SeedId($"tournament:{tournament}"),
			PartnerEmail = partner == null ? null : Email(partner),
			Title = team,
			PayFromWallet = pay
		}, _ct);
		return Ok(r, $"register {player} in {tournament}") ? r.Result : null;
	}

	private async Task<bool> Generate(string tournament, string creator) =>
		Ok(await GenerateTournamentMatches(new IdParams { Token = T(creator), Id = SeedId($"tournament:{tournament}") }, _ct), $"draw {tournament}");

	/// <summary>Plays the ready matches in order (new rounds appear as earlier ones finish) until [limit] results are in or none is left.</summary>
	private async Task Play(string tournament, string creator, TagSport sport, DateTime start, int? limit = null, string[]? favourites = null, int pointsPerMatch = 24) {
		Guid id = SeedId($"tournament:{tournament}");
		TournamentEntity t = await db.Set<TournamentEntity>().FirstAsync(x => x.Id == id, _ct);
		bool pointsFormat = t.Tags.Contains(TagTournament.Americano) || t.Tags.Contains(TagTournament.Mexicano);
		HashSet<string> fav = [..favourites ?? []];
		Dictionary<Guid, List<string>> players = (await db.Set<TournamentEntryEntity>().Where(x => x.TournamentId == id).Select(x => new { x.Id, Users = x.Users.Select(u => u.Id).ToList() }).ToListAsync(_ct))
			.ToDictionary(x => x.Id, x => x.Users.Where(_keyOf.ContainsKey).Select(u => _keyOf[u]).ToList());
		double Strength(Guid? entry) => entry == null || !players.TryGetValue(entry.Value, out List<string>? keys) || keys.Count == 0
			? 0
			: keys.Average(k => (double)LevelOf(k, sport)) + (keys.Any(fav.Contains) ? 2.5 : 0);

		int played = 0;
		while (limit == null || played < limit) {
			TournamentMatchEntity? m = await db.Set<TournamentMatchEntity>()
				.Where(x => x.TournamentId == id && x.EntryAId != null && x.EntryBId != null && !x.Tags.Contains(TagTournamentMatch.Finished) && !x.Tags.Contains(TagTournamentMatch.Bye))
				.OrderBy(x => x.Round).ThenBy(x => x.Order)
				.FirstOrDefaultAsync(_ct);
			if (m == null) break;

			double a = Strength(m.EntryAId) + Strength(m.PartnerAId), b = Strength(m.EntryBId) + Strength(m.PartnerBId);
			double chance = ChanceA(a, b);
			bool knockout = TournamentEngine.IsKnockout(m);
			bool draw = !pointsFormat && sport == TagSport.Football && Math.Abs(chance - 0.5) < 0.2 && _random.NextDouble() < 0.35;
			List<MatchSetScore> sets = Score(sport, _random.NextDouble() < chance, draw, pointsFormat, pointsPerMatch, knockout);

			bool ok = Ok(await UpdateTournamentMatch(new TournamentMatchUpdateParams {
				Token = T(creator),
				Id = m.Id,
				Sets = sets,
				ScheduledAt = start.AddMinutes(played / 2 * 50),
				Court = sport is TagSport.Padel or TagSport.Tennis or TagSport.Squash ? $"{played % 2 + 1}" : null
			}, _ct), $"result in {tournament}");
			if (!ok) break;
			played++;
		}

		_result.TournamentMatches += played;
	}

	/// <summary>The first ready match goes live; the rest are given times from [from] on.</summary>
	private async Task ScheduleRest(string tournament, string creator, DateTime from, bool live) {
		Guid id = SeedId($"tournament:{tournament}");
		var waiting = await db.Set<TournamentMatchEntity>()
			.Where(x => x.TournamentId == id && !x.Tags.Contains(TagTournamentMatch.Finished) && !x.Tags.Contains(TagTournamentMatch.Bye))
			.OrderBy(x => x.Round).ThenBy(x => x.Order)
			.Select(x => new { x.Id, Ready = x.EntryAId != null && x.EntryBId != null })
			.ToListAsync(_ct);
		Guid? liveId = live ? waiting.FirstOrDefault(x => x.Ready)?.Id : null;
		for (int i = 0; i < waiting.Count; i++) {
			bool goLive = waiting[i].Id == liveId;
			Ok(await UpdateTournamentMatch(new TournamentMatchUpdateParams {
				Token = T(creator),
				Id = waiting[i].Id,
				ScheduledAt = goLive ? DateTime.UtcNow.AddMinutes(-25) : from.AddMinutes(i * 60),
				Tags = goLive ? [TagTournamentMatch.Live] : null
			}, _ct), $"schedule {tournament}");
		}
	}

	/// <summary>The sport's past: tournaments and open games over the last six weeks, in date order so the levels move as they would have.</summary>
	private async Task SeedHistory(DateTime now) {
		List<(DateTime When, Func<Task> Action)> timeline = [];

		// Open games, two or three a day in the last six weeks, the demo player in at least one each week.
		(TagSport Sport, double Weight, int Capacity)[] mix = [(TagSport.Padel, 0.5, 4), (TagSport.Tennis, 0.18, 2), (TagSport.Squash, 0.12, 2), (TagSport.Billiards, 0.1, 2), (TagSport.Snooker, 0.05, 2), (TagSport.Football, 0.05, 6)];
		for (int day = 42; day >= 1; day--) {
			int games = _random.NextDouble() < 0.35 ? 2 : 1;
			for (int g = 0; g < games; g++) {
				double roll = _random.NextDouble(), sum = 0;
				(TagSport sport, double _, int capacity) = mix.First(x => (sum += x.Weight) >= roll || x == mix[^1]);
				if (sport == TagSport.Tennis && _random.NextDouble() < 0.4) capacity = 4;
				bool withDemo = LevelOf("demo", sport) > 0 && (day % 7 is 1 or 4 || _random.NextDouble() < 0.3);
				DateTime when = LocalAt(-day, 17 + g * 2.5 + _random.Next(0, 3) * 0.5);
				timeline.Add((when, () => PastOpenMatch(sport, capacity, withDemo, when)));
			}
		}

		timeline.Add((LocalAt(-30, 9), () => FinishedTennisLeague(LocalAt(-30, 9))));
		timeline.Add((LocalAt(-21, 15), () => FinishedPadelOpen(LocalAt(-21, 15))));
		timeline.Add((LocalAt(-16, 18), () => FinishedSwiss(LocalAt(-16, 18))));
		timeline.Add((LocalAt(-10, 17), () => FinishedAmericano(LocalAt(-10, 17))));
		timeline.Add((LocalAt(-7, 16), () => FinishedFootball(LocalAt(-7, 16))));
		timeline.Add((LocalAt(-5, 18), () => RunningSnookerLeague(LocalAt(-5, 18))));
		timeline.Add((LocalAt(-3, 10), () => RunningSquashMasters(LocalAt(-3, 10))));
		timeline.Add((LocalAt(-1, 19), () => RunningMexicano(LocalAt(-1, 19))));
		timeline.Add((DateTime.UtcNow.AddHours(-2), () => RunningPadelNightCup(DateTime.UtcNow.AddHours(-2))));

		foreach ((DateTime _, Func<Task> action) in timeline.OrderBy(x => x.When)) await action();
	}

	private async Task PastOpenMatch(TagSport sport, int capacity, bool withDemo, DateTime when) {
		List<string> roster = Roster(sport).Where(x => x != "demo").ToList();
		List<string> players = [..withDemo ? ["demo"] : Array.Empty<string>(), ..Shuffle(roster).Take(withDemo ? capacity - 1 : capacity)];
		if (players.Count < capacity) return;
		Guid id = Guid.CreateVersion7();
		string?[] titles = ["بازی عصرگاهی", "تمرین دوستانه", "بازی رقابتی", "دوبل آخر هفته", null, null];
		bool friendly = _random.NextDouble() < 0.15;
		SeedVenue? venue = Venues.Where(v => v.Status == TagVenue.Approved && v.Courts.Any(c => c.Sport == sport) && v.City == "Tehran").OrderBy(_ => _random.Next()).FirstOrDefault()
		                   ?? Venues.FirstOrDefault(v => v.Status == TagVenue.Approved && v.Courts.Any(c => c.Sport == sport));

		await At(when.AddDays(-2), async () => {
			if (!Ok(await CreateOpenMatch(new OpenMatchCreateParams {
				    Token = T(players[0]),
				    Id = id,
				    Tags = [TagOpenMatch.Public, friendly ? TagOpenMatch.Friendly : TagOpenMatch.Competitive],
				    SportId = _sports[sport].Id,
				    StartAt = DateTime.UtcNow.AddHours(3),
				    DurationMinutes = sport == TagSport.Football ? 60 : 90,
				    Capacity = capacity,
				    PricePerPlayer = sport switch { TagSport.Padel => 300_000, TagSport.Football => 150_000, TagSport.Tennis => 250_000, _ => 120_000 },
				    VenueId = venue == null ? null : SeedId($"venue:{venue.Key}"),
				    Title = Pick(titles)
			    }, _ct), "past game")) return;
			foreach (string player in players.Skip(1)) Ok(await JoinOpenMatch(new IdParams { Token = T(player), Id = id }, _ct), "join past game");
		});

		await db.Set<OpenMatchEntity>().Where(x => x.Id == id).ExecuteUpdateAsync(u => u.SetProperty(x => x.StartAt, when), _ct);
		await At(when.AddMinutes(100), async () => {
			int half = capacity / 2;
			List<string> teamA = players.Take(half).ToList(), teamB = players.Skip(half).ToList();
			double chance = ChanceA(teamA.Average(k => (double)LevelOf(k, sport)), teamB.Average(k => (double)LevelOf(k, sport)));
			bool draw = sport == TagSport.Football && _random.NextDouble() < 0.2;
			Ok(await SetOpenMatchResult(new OpenMatchResultParams {
				Token = T(players[0]),
				Id = id,
				TeamA = teamA.Select(U).ToList(),
				TeamB = teamB.Select(U).ToList(),
				Sets = Score(sport, _random.NextDouble() < chance, draw, false, 0, false)
			}, _ct), "past game result");
		});
	}

	private async Task FinishedTennisLeague(DateTime start) {
		await At(start.AddDays(-12), async () => {
			if (!await CreateTournament("tennis-league", "owner", TagSport.Tennis, TagTournament.RoundRobin, TagTournament.Singles, "لیگ بهاره تنیس", start, 6, 0, true, p => {
				    p.Description = "شش بازیکن، دور رفت. هر برد ۳ امتیاز.";
				    p.Venue = "باشگاه راکتی انقلاب";
				    p.Latitude = 35.7012;
				    p.Longitude = 51.3952;
				    p.Prize = "کاپ و راکت ویلسون";
			    })) return;
			foreach (string player in new[] { "demo", "sara", "omid", "farhad", "tara", "emma" }) await Register("tennis-league", player);
		});
		await At(start, async () => {
			if (await Generate("tennis-league", "owner")) await Play("tennis-league", "owner", TagSport.Tennis, start);
		});
	}

	private async Task FinishedPadelOpen(DateTime start) {
		await At(start.AddDays(-14), async () => {
			if (!await CreateTournament("padel-open", "organizer", TagSport.Padel, TagTournament.SingleElimination, TagTournament.Doubles, "اوپن پدل تهران", start, 8, 500_000, true, p => {
				    p.Description = "حذفی دونفره با بازی رده‌بندی. ورودیه از کیف پول پرداخت میشه.";
				    p.Venue = "پدل آرنا تهران";
				    p.Address = "تهران، ولیعصر، بالاتر از پارک‌وی";
				    p.Latitude = 35.7915;
				    p.Longitude = 51.4105;
				    p.Prize = "۲۰ میلیون تومان + کاپ";
				    p.ThirdPlaceMatch = true;
				    p.SuperTiebreak = true;
			    })) return;
			(string, string)[] pairs = [("demo", "farhad"), ("lucas", "kaveh"), ("omid", "tara"), ("neda", "yasaman"), ("reza", "ali"), ("sara", "amir"), ("babak", "parisa"), ("niloufar", "mina")];
			foreach ((string a, string b) in pairs) await Register("padel-open", a, b);
		});
		await At(start, async () => {
			if (await Generate("padel-open", "organizer")) await Play("padel-open", "organizer", TagSport.Padel, start, favourites: ["demo"]);
		});
	}

	private async Task FinishedSwiss(DateTime start) {
		await At(start.AddDays(-8), async () => {
			if (!await CreateTournament("cue-swiss", "owner", TagSport.Billiards, TagTournament.Swiss, TagTournament.Singles, "سوئیسی کیو کلاب", start, 6, 150_000, true, p => {
				    p.Description = "سه دور سوئیسی، هر بازی تا ۵ رک.";
				    p.Venue = "کیو کلاب ونک";
				    p.Rounds = 3;
			    })) return;
			foreach (string player in new[] { "demo", "reza", "pouya", "parisa", "siavash", "james" }) await Register("cue-swiss", player);
		});
		await At(start, async () => {
			if (await Generate("cue-swiss", "owner")) await Play("cue-swiss", "owner", TagSport.Billiards, start);
		});
	}

	private async Task FinishedAmericano(DateTime start) {
		await At(start.AddDays(-5), async () => {
			if (!await CreateTournament("americano", "organizer", TagSport.Padel, TagTournament.Americano, TagTournament.Singles, "آمریکانوی جمعه", start, 8, 0, true, p => {
				    p.Description = "هر دور با هم‌تیمی جدید. ۲۴ امتیاز در هر بازی.";
				    p.Venue = "باغ پدل کیانی";
				    p.PointsPerMatch = 24;
			    })) return;
			foreach (string player in new[] { "demo", "neda", "sara", "shirin", "yasaman", "tara", "mahsa", "sofia" }) await Register("americano", player);
		});
		await At(start, async () => {
			if (await Generate("americano", "organizer")) await Play("americano", "organizer", TagSport.Padel, start);
		});
	}

	private async Task FinishedFootball(DateTime start) {
		await At(start.AddDays(-10), async () => {
			if (!await CreateTournament("football", "organizer", TagSport.Football, TagTournament.RoundRobin, TagTournament.Team, "جام فوتبال پنج‌نفره", start, 4, 1_000_000, true, p => {
				    p.Description = "چهار تیم، دوره‌ای. برد ۳، مساوی ۱.";
				    p.Venue = "فوتبال پنج‌نفره شیراز";
				    p.Prize = "کاپ قهرمانی";
			    })) return;
			(string, string)[] teams = [("reza", "مهاجمان تهران"), ("babak", "شیرهای آزادی"), ("ali", "ستاره‌های شمال"), ("omar", "Dubai Falcons")];
			foreach ((string captain, string team) in teams) await Register("football", captain, team: team);
		});
		await At(start, async () => {
			if (await Generate("football", "organizer")) await Play("football", "organizer", TagSport.Football, start);
		});
	}

	private async Task RunningSnookerLeague(DateTime start) {
		await At(start.AddDays(-6), async () => {
			if (!await CreateTournament("snooker-box", "owner", TagSport.Snooker, TagTournament.Ladder, TagTournament.Singles, "لیگ جعبه‌ای اسنوکر · پاییز", start, 6, 0, true, p => {
				    p.Description = "جعبه‌های سه‌نفره؛ اول هر جعبه بالا می‌ره.";
				    p.Venue = "کیو کلاب ونک";
				    p.BoxSize = 3;
			    })) return;
			foreach (string player in new[] { "demo", "babak", "pouya", "hamed", "siavash", "james" }) await Register("snooker-box", player);
		});
		await At(start, async () => {
			if (!await Generate("snooker-box", "owner")) return;
			await Play("snooker-box", "owner", TagSport.Snooker, start, 4);
			await ScheduleRest("snooker-box", "owner", LocalAt(1, 19), false);
		});
	}

	private async Task RunningSquashMasters(DateTime start) {
		await At(start.AddDays(-9), async () => {
			if (!await CreateTournament("squash-masters", "organizer", TagSport.Squash, TagTournament.GroupsKnockout, TagTournament.Singles, "مسترز اسکواش", start, 8, 300_000, true, p => {
				    p.Description = "دو گروه چهارنفره، دو نفر اول هر گروه به نیمه‌نهایی می‌رسن.";
				    p.Venue = "باشگاه راکتی انقلاب";
				    p.GroupCount = 2;
				    p.AdvancePerGroup = 2;
			    })) return;
			foreach (string player in new[] { "demo", "kaveh", "leila", "yasaman", "amir", "ghazal", "omar", "james" }) await Register("squash-masters", player);
		});
		await At(start, async () => {
			if (!await Generate("squash-masters", "organizer")) return;
			await Play("squash-masters", "organizer", TagSport.Squash, start, 13, ["demo"]);
			await ScheduleRest("squash-masters", "organizer", LocalAt(1, 18), true);
		});
	}

	private async Task RunningMexicano(DateTime start) {
		await At(start.AddDays(-4), async () => {
			if (!await CreateTournament("mexicano", "organizer", TagSport.Padel, TagTournament.Mexicano, TagTournament.Singles, "مکزیکانوی دوشنبه‌ها", start, 8, 0, true, p => {
				    p.Description = "چهار دور؛ هر دور بر اساس جدول، هم‌تیمی‌ها عوض می‌شن.";
				    p.Venue = "پدل آرنا تهران";
				    p.Rounds = 4;
				    p.PointsPerMatch = 24;
			    })) return;
			foreach (string player in new[] { "demo", "kaveh", "mina", "babak", "shirin", "niloufar", "parisa", "ghazal" }) await Register("mexicano", player);
		});
		await At(start, async () => {
			if (!await Generate("mexicano", "organizer")) return;
			await Play("mexicano", "organizer", TagSport.Padel, start, 3);
			await ScheduleRest("mexicano", "organizer", DateTime.UtcNow.AddMinutes(40), true);
		});
	}

	private async Task RunningPadelNightCup(DateTime start) {
		await At(start.AddDays(-6), async () => {
			if (!await CreateTournament("night-cup", "owner", TagSport.Padel, TagTournament.DoubleElimination, TagTournament.Doubles, "جام شبانه پدل", start, 6, 400_000, true, p => {
				    p.Description = "حذفی دوگانه: هر تیم با دو باخت حذف میشه. نتایج زنده.";
				    p.Venue = "پدل آرنا تهران";
				    p.Latitude = 35.7915;
				    p.Longitude = 51.4105;
				    p.Prize = "۱۲ میلیون تومان";
			    })) return;
			(string, string)[] pairs = [("neda", "omid"), ("kaveh", "yasaman"), ("lucas", "sofia"), ("reza", "tara"), ("ali", "sara"), ("omar", "elif")];
			foreach ((string a, string b) in pairs) await Register("night-cup", a, b);
		});
		await At(start, async () => {
			if (!await Generate("night-cup", "owner")) return;
			await Play("night-cup", "owner", TagSport.Padel, start, 5);
			await ScheduleRest("night-cup", "owner", DateTime.UtcNow.AddMinutes(50), true);
		});
	}

	// ---------------- What's coming ----------------

	private async Task SeedUpcoming(DateTime now) {
		// Registration open, with approval: some entries approved, two waiting, one rejected (and refunded).
		if (await CreateTournament("autumn-cup", "organizer", TagSport.Padel, TagTournament.SingleElimination, TagTournament.Doubles, "جام پاییزه پدل", LocalAt(9, 16), 16, 600_000, false, p => {
			    p.Description = "حذفی ۱۶ تیمی با بازی رده‌بندی. ثبت‌نام با تأیید برگزارکننده.";
			    p.Venue = "پدل آرنا تهران";
			    p.Address = "تهران، ولیعصر، بالاتر از پارک‌وی";
			    p.Latitude = 35.7915;
			    p.Longitude = 51.4105;
			    p.Prize = "۳۰ میلیون تومان + کاپ";
			    p.ThirdPlaceMatch = true;
			    p.MinLevel = Scale(TagSport.Padel, 2.5m);
			    p.MaxLevel = Scale(TagSport.Padel, 6.5m);
		    })) {
			(string, string)[] pairs = [("demo", "yasaman"), ("lucas", "farhad"), ("neda", "tara"), ("kaveh", "omid"), ("sara", "reza"), ("elif", "sofia"), ("mahsa", "shirin")];
			List<Guid?> entries = [];
			foreach ((string a, string b) in pairs) entries.Add(await Register("autumn-cup", a, b));
			for (int i = 0; i < entries.Count; i++) {
				if (entries[i] == null || i is 4 or 5) continue;
				Ok(await UpdateTournamentEntry(new TournamentEntryUpdateParams { Token = T("organizer"), Id = entries[i]!.Value, Tags = [i == 6 ? TagTournamentEntry.Rejected : TagTournamentEntry.Approved], Seed = i < 3 ? i + 1 : null }, _ct), "approve entry");
			}
		}

		if (await CreateTournament("tennis-day", "owner", TagSport.Tennis, TagTournament.RoundRobin, TagTournament.Singles, "روز باز تنیس", LocalAt(5, 9), 8, 0, true, p => {
			    p.Description = "رایگان و دوستانه، برای همه‌ی سطح‌ها.";
			    p.Venue = "باشگاه راکتی انقلاب";
			    p.Unrated = true;
		    }))
			foreach (string player in new[] { "sara", "emma", "elif", "shirin" }) await Register("tennis-day", player);

		if (await CreateTournament("squash-beginners", "owner", TagSport.Squash, TagTournament.SingleElimination, TagTournament.Singles, "جام نوآموزان اسکواش", LocalAt(12, 17), 8, 200_000, true, p => {
			    p.Description = "فقط برای سطح ۴٫۵ و پایین‌تر.";
			    p.Venue = "باشگاه راکتی انقلاب";
			    p.MaxLevel = Scale(TagSport.Squash, 4.5m);
		    }))
			foreach (string player in new[] { "ghazal", "organizer", "leila" }) await Register("squash-beginners", player);

		// The demo player's own draft.
		await CreateTournament("garden-weekend", "demo", TagSport.Padel, TagTournament.RoundRobin, TagTournament.Doubles, "جام آخر هفته باغ کیانی", LocalAt(20, 10), 6, 250_000, true, p => {
			p.Description = "پیش‌نویس — هنوز منتشر نشده.";
			p.Venue = "باغ پدل کیانی";
		}, true);

		// Open games: one starting within the hour (its reminder comes from the background service), some to join, a full one,
		// a private one waiting for approval, an invitation and a challenge for the demo player.
		DateTime soon = DateTime.UtcNow.AddMinutes(55);
		await OpenGame("p-soon", "neda", TagSport.Padel, soon, 4, "arena", "بازی امروز عصر", ["demo", "tara", "farhad"]);
		Guid? booking = await Book("demo", "arena-1", 1, 8, split: ["tara", "neda", "farhad"], notes: "بازی دوستانه با بچه‌ها");
		if (booking != null) Ok(await venues.PayBookingShare(new IdParams { Token = T("tara"), Id = booking.Value }, _ct), "pay share");
		await OpenGame("p-demo", "demo", TagSport.Padel, SlotUtc(Venues[0], Venues[0].Courts[0], 1, 8), 4, "arena", "دوبل فردا شب · زمین پانوراما", ["tara"], booking: booking,
			min: Scale(TagSport.Padel, 3.5m), max: Scale(TagSport.Padel, 5.5m));
		await OpenGame("p-garden", "kaveh", TagSport.Padel, LocalAt(2, 19.5), 4, "garden", "دنبال یک نفر برای دوبل", ["yasaman", "omid"]);
		await OpenGame("p-private", "farhad", TagSport.Padel, LocalAt(3, 20), 4, "arena", "بازی سطح بالا (خصوصی)", [], priv: true, requests: ["demo", "tara"]);
		await OpenGame("t-challenge", "lucas", TagSport.Tennis, LocalAt(4, 10), 2, "enghelab", "Challenge: best of three?", [], challenge: true, invited: ["demo"]);
		await OpenGame("t-full", "omid", TagSport.Tennis, LocalAt(2, 18), 4, "enghelab", "دوبل تنیس دوستانه", ["sara", "tara", "mahsa"], friendly: true);
		await OpenGame("b-open", "pouya", TagSport.Billiards, LocalAt(1, 21), 2, "cue", null, []);
		await OpenGame("s-invite", "yasaman", TagSport.Squash, LocalAt(2, 8), 2, "enghelab", "اسکواش صبح زود", [], invited: ["demo"]);
		await OpenGame("f-open", "babak", TagSport.Football, LocalAt(5, 20), 10, "shiraz", "فوتبال پنج‌نفره شب جمعه", ["reza", "ali", "amir", "hamed", "omid"]);
		await OpenGame("p-park", "amir", TagSport.Padel, LocalAt(6, 8), 4, null, "پدل صبح جمعه در پارک", [], place: "پارک ملت، زمین‌های عمومی", lat: 35.7792, lng: 51.4210);
		await OpenGame("p-kish", "niloufar", TagSport.Padel, LocalAt(3, 18), 4, "kish", "غروب کیش 🌅", ["mina"]);
	}

	private async Task<Guid> OpenGame(string key, string creator, TagSport sport, DateTime start, int capacity, string? venue, string? title, string[] joiners, bool priv = false,
		bool challenge = false, bool friendly = false, string[]? invited = null, string[]? requests = null, Guid? booking = null, decimal? min = null, decimal? max = null,
		string? place = null, double? lat = null, double? lng = null) {
		Guid id = SeedId($"game:{key}");
		if (!Ok(await CreateOpenMatch(new OpenMatchCreateParams {
			    Token = T(creator),
			    Id = id,
			    Tags = [priv ? TagOpenMatch.Private : TagOpenMatch.Public, friendly ? TagOpenMatch.Friendly : TagOpenMatch.Competitive, ..challenge ? [TagOpenMatch.Challenge] : Array.Empty<TagOpenMatch>()],
			    SportId = _sports[sport].Id,
			    StartAt = start,
			    DurationMinutes = sport == TagSport.Football ? 60 : 90,
			    Capacity = capacity,
			    MinLevel = min,
			    MaxLevel = max,
			    PricePerPlayer = sport switch { TagSport.Padel => 300_000, TagSport.Football => 150_000, TagSport.Tennis => 250_000, _ => 120_000 },
			    VenueId = venue == null ? null : SeedId($"venue:{venue}"),
			    BookingId = booking,
			    Title = title,
			    Description = challenge ? "Loser buys the coffee ☕" : null,
			    Place = place,
			    Latitude = lat,
			    Longitude = lng,
			    InvitedUserIds = (invited ?? []).Select(U).ToList()
		    }, _ct), $"game {key}"))
			return id;
		foreach (string player in joiners.Concat(requests ?? [])) Ok(await JoinOpenMatch(new IdParams { Token = T(player), Id = id }, _ct), $"join game {key}");
		return id;
	}

	// ---------------- Bookings ----------------

	private async Task<Guid?> Book(string user, string court, int day, int slot, int slots = 1, bool wallet = true, string[]? split = null, string? notes = null) {
		if (!_courts.TryGetValue(court, out (SeedVenue Venue, SeedCourt Court) vc)) return null;
		if (slot < 0 || slot + slots > SlotsPerDay(vc.Venue, vc.Court) || !_usedSlots.Add((court, day, slot))) return null;
		UResponse<BookingResponse?> r = await venues.CreateBooking(new BookingCreateParams {
			Token = T(user),
			CourtId = SeedId($"court:{court}"),
			StartAt = SlotUtc(vc.Venue, vc.Court, day, slot),
			DurationMinutes = vc.Court.Slot * slots,
			PayFromWallet = wallet,
			SplitWithUserIds = (split ?? []).Select(U).ToList(),
			Notes = notes
		}, _ct);
		return Ok(r, $"booking {court} by {user}") ? r.Result?.Id : null;
	}

	/// <summary>A booking in the past: made for a free future slot, moved to its day, then closed by the venue.</summary>
	private async Task PastBooking(string user, string court, int daysAgo, int slot, TagBooking end = TagBooking.Completed, string[]? split = null) {
		if (!_courts.TryGetValue(court, out (SeedVenue Venue, SeedCourt Court) vc) || !_usedSlots.Add((court, -daysAgo, slot))) return;
		DateTime start = SlotUtc(vc.Venue, vc.Court, -daysAgo, slot);
		Guid? id = null;
		await At(start.AddDays(-2), async () => {
			id = await Book(user, court, _tempDay++, 0, split: split);
			if (id == null) return;
			foreach (string friend in split ?? []) Ok(await venues.PayBookingShare(new IdParams { Token = T(friend), Id = id.Value }, _ct), "pay share");
		});
		if (id == null) return;
		await db.Set<BookingEntity>().Where(x => x.Id == id).ExecuteUpdateAsync(u => u.SetProperty(x => x.StartAt, start).SetProperty(x => x.EndAt, start.AddMinutes(vc.Court.Slot)), _ct);
		await At(start.AddMinutes(vc.Court.Slot + 30), async () =>
			Ok(await venues.UpdateBooking(new BookingUpdateParams { Token = T(vc.Venue.Owner), Id = id.Value, Tags = [end] }, _ct), "close booking"));
	}

	private async Task SeedBookings(DateTime now) {
		// The demo player's own: a pay-at-venue one, one shared with them (their share unpaid), two cancelled (one late, with a penalty).
		await Book("demo", "enghelab-t1", 3, 2, wallet: false, notes: "تمرین سرویس");
		await Book("farhad", "arena-2", 2, 9, split: ["demo"], notes: "آرمان سهمت رو بزن 😄");
		Guid? early = await Book("demo", "arena-4", 5, 6);
		if (early != null) Ok(await venues.CancelBooking(new BookingCancelParams { Token = T("demo"), Id = early.Value, Reason = "برنامه‌ام عوض شد" }, _ct), "cancel booking");
		Guid? late = await Book("demo", "cue-b1", 1, 2);
		if (late != null) Ok(await venues.CancelBooking(new BookingCancelParams { Token = T("demo"), Id = late.Value, Reason = "دیر رسیدم" }, _ct), "late cancel");

		await PastBooking("demo", "arena-3", 6, 7, split: ["farhad"]);
		await PastBooking("demo", "enghelab-s1", 13, 12);
		await PastBooking("demo", "cue-s1", 20, 6, TagBooking.NoShow);
		await PastBooking("demo", "arena-1", 27, 8);

		// Busy weeks at every club (for the owners' calendars and dashboards), the demo player's garden included.
		string[] players = Users.Select(x => x.Key).Where(x => x is not ("demo" or "behrooz")).ToArray();
		foreach ((string court, (SeedVenue venue, SeedCourt c)) in _courts.ToList()) {
			if (venue.Status != TagVenue.Approved) continue;
			int slots = SlotsPerDay(venue, c);
			int future = venue.Key is "arena" or "garden" ? 6 : 3, past = venue.Key is "arena" or "garden" ? 7 : 3;
			for (int i = 0; i < future; i++) {
				string user = Pick(players);
				if (user == venue.Owner) continue;
				string[]? split = c.Players > 2 && _random.NextDouble() < 0.4 ? Shuffle(players.Where(x => x != user && x != venue.Owner)).Take(_random.Next(1, 4)).ToArray() : null;
				await Book(user, court, _random.Next(1, 9), _random.Next(slots / 3, slots), wallet: !venue.PayAtVenue || _random.NextDouble() < 0.7, split: split);
			}

			for (int i = 0; i < past; i++) {
				string user = Pick(players);
				if (user == venue.Owner) continue;
				await PastBooking(user, court, _random.Next(1, 29), _random.Next(slots / 3, slots), _random.NextDouble() < 0.1 ? TagBooking.NoShow : TagBooking.Completed);
			}
		}

		// A booking at the demo player's garden that its player cancelled.
		Guid? cancelled = await Book("neda", "garden-1", 4, 6);
		if (cancelled != null) Ok(await venues.CancelBooking(new BookingCancelParams { Token = T("neda"), Id = cancelled.Value, Reason = "مسافرت" }, _ct), "cancel garden booking");
	}

	// ---------------- Posts, stories, reports ----------------

	private async Task SeedPosts(DateTime now) {
		Guid padelOpen = SeedId("tournament:padel-open");
		(string User, double HoursAgo, string Text, string? Image, string? LinkType, Guid? LinkId, bool Followers)[] posts = [
			("demo", 500, "قهرمان اوپن پدل تهران شدیم! 🏆 مرسی فرهاد، بهترین هم‌تیمی دنیا.", "trophy", "tournament", padelOpen, false),
			("farhad", 498, "چه فینالی بود! آرمان امروز ترکوند 🔥", null, "tournament", padelOpen, false),
			("organizer", 480, "عکس‌های اوپن پدل تهران رو گذاشتیم. ماه بعد جام پاییزه، ثبت‌نام بازه.", "padel", "tournament", SeedId("tournament:autumn-cup"), false),
			("lucas", 300, "First week in Tehran. The padel scene here is amazing. Who wants to play?", "padel", null, null, false),
			("neda", 260, "صبح ساعت ۷ و زمین خالی. بهترین حس دنیا ☀️", "court", null, null, false),
			("demo", 230, "باغ پدل کیانی آماده‌ست! زمین روباز زیر درخت‌ها 🌳 رزرو از توی اپ.", "court", "venue", SeedId("venue:garden"), false),
			("tara", 200, "کسی برای چهارشنبه عصر دوبل هست؟", null, null, null, false),
			("james", 180, "Snooker box league started. Tough group!", "table", "tournament", SeedId("tournament:snooker-box"), false),
			("kaveh", 150, "بک‌هند رو بالاخره درست کردم. مرسی از کلاس فرهاد.", null, null, null, true),
			("owner", 140, "زمین‌های ۱ و ۲ پدل آرنا نورپردازی جدید گرفتن 💡", "court", "venue", SeedId("venue:arena"), false),
			("sara", 120, "اولین تورنمنت پدلم بود، کلی یاد گرفتم 💪", "padel", null, null, false),
			("demo", 96, "سه هفته پشت سر هم بازی! نشان «همیشگی» گرفتم 😎", null, null, null, false),
			("omar", 90, "Dubai Falcons are ready for the next five-a-side cup ⚽️", "pitch", "tournament", SeedId("tournament:football"), false),
			("yasaman", 72, "اسکواش صبح زود + قهوه = بهترین شروع روز", null, null, null, false),
			("pouya", 60, "مجموعه‌ی پدل مشهد به زودی! منتظر تأیید هستیم 🙏", "court", null, null, false),
			("elif", 50, "Tehran sunsets after a padel match 🧡", "padel", null, null, false),
			("mahsa", 40, "مرسی آرمان که منو با پدل آشنا کردی!", null, null, null, false),
			("babak", 30, "تیم شیرهای آزادی دنبال یک بازیکن دفاع هست. پیام بدید.", null, null, null, false),
			("demo", 20, "کی پایه‌ست فردا شب؟ یک جا خالیه 👇", null, "openMatch", SeedId("game:p-demo"), false),
			("farhad", 12, "نکته‌ی امروز: توی پدل، بالای سر بازی کن نه جلوی سینه.", null, null, null, false),
			("behrooz", 10, "راکت اصل نصف قیمت!!! فقط امروز!!! دایرکت", null, null, null, false),
			("neda", 5, "جام شبانه پدل امشب زنده‌ست! بیاید تماشا 🎾", "padel", "tournament", SeedId("tournament:night-cup"), false),
			("emma", 3, "Booked my first court through the app, super easy.", null, null, null, false)
		];

		Dictionary<int, Guid> postIds = new();
		string[] likers = Users.Select(x => x.Key).Where(x => x != "behrooz").ToArray();
		string[] replies = ["عالیه! 👏", "تبریک 🎉", "منم پایه‌ام", "کی دوباره بازی کنیم؟", "Great job!", "👏👏👏", "دمت گرم", "Let's play next week!", "چه خوب 😍", "منو هم خبر کن"];
		for (int i = 0; i < posts.Length; i++) {
			(string user, double hoursAgo, string text, string? image, string? linkType, Guid? linkId, bool followers) = posts[i];
			DateTime at = now.AddHours(-hoursAgo);
			Guid id = SeedId($"post:{i}");
			postIds[i] = id;
			List<string> liked = user == "behrooz" ? ["shirin"] : Shuffle(likers.Where(x => x != user)).Take(_random.Next(2, 14)).ToList();
			await db.Set<PostEntity>().AddAsync(new PostEntity {
				Id = id,
				CreatedAt = at,
				CreatorId = U(user),
				Tags = [TagPost.Post, followers ? TagPost.Followers : TagPost.Public, ..user == "behrooz" ? [TagPost.Hidden] : Array.Empty<TagPost>()],
				Text = text,
				JsonData = new PostJson {
					LinkType = linkType,
					LinkId = linkId,
					Reactions = liked.Select(x => new PostReaction { UserId = U(x), Tag = TagReaction.Like }).ToList()
				}
			}, _ct);
			if (image != null) {
				string path = $"posts/seed-{i}.jpg";
				DrawPost(path, image, i);
				await db.Set<MediaEntity>().AddAsync(new MediaEntity { Id = SeedId($"post-media:{i}"), CreatedAt = at, CreatorId = U(user), PostId = id, Path = path, Tags = [TagMedia.Image], JsonData = new MediaJson() }, _ct);
			}

			int replyCount = user == "behrooz" ? 0 : _random.Next(0, 5);
			for (int r = 0; r < replyCount; r++) {
				string replier = Pick(likers.Where(x => x != user).ToList());
				DateTime replyAt = at.AddMinutes(_random.Next(5, (int)Math.Max(10, Math.Min(hoursAgo * 60 - 5, 600))));
				await db.Set<PostEntity>().AddAsync(new PostEntity {
					Id = SeedId($"post:{i}:reply:{r}"),
					CreatedAt = replyAt,
					CreatorId = U(replier),
					Tags = [TagPost.Comment, TagPost.Public],
					Text = Pick(replies),
					ParentId = id,
					JsonData = new PostJson { Reactions = _random.NextDouble() < 0.5 ? [new PostReaction { UserId = U(user), Tag = TagReaction.Like }] : [] }
				}, _ct);
				if (user == "demo") await Notify("demo", replier, "notifNewReply", Name(replier), "post", id, replyAt, TagNotification.Social);
			}

			if (user == "demo")
				foreach (string liker in liked.Take(4))
					await Notify("demo", liker, "notifNewReaction", Name(liker), "post", id, at.AddMinutes(_random.Next(3, 300)), TagNotification.Social);
		}

		// Stories: some the demo player has seen, some not, and their own.
		(string User, double HoursAgo, string Image, bool Seen)[] stories = [
			("farhad", 2, "padel", false), ("neda", 4, "court", true), ("tara", 6, "padel", false), ("lucas", 9, "court", false),
			("kaveh", 13, "padel", true), ("organizer", 18, "trophy", false), ("demo", 3, "court", false)
		];
		for (int i = 0; i < stories.Length; i++) {
			(string user, double hoursAgo, string image, bool seen) = stories[i];
			DateTime at = now.AddHours(-hoursAgo);
			Guid id = SeedId($"story:{i}");
			List<Guid> viewers = Shuffle(likers.Where(x => x != user && x != "demo")).Take(_random.Next(3, 12)).Select(U).ToList();
			if (seen) viewers.Add(U("demo"));
			await db.Set<PostEntity>().AddAsync(new PostEntity {
				Id = id,
				CreatedAt = at,
				CreatorId = U(user),
				Tags = [TagPost.Story, TagPost.Public],
				ExpiresAt = at.AddHours(24),
				JsonData = new PostJson { ViewerIds = viewers }
			}, _ct);
			string path = $"posts/seed-story-{i}.jpg";
			DrawPost(path, image, 40 + i);
			await db.Set<MediaEntity>().AddAsync(new MediaEntity { Id = SeedId($"story-media:{i}"), CreatedAt = at, CreatorId = U(user), PostId = id, Path = path, Tags = [TagMedia.Image], JsonData = new MediaJson() }, _ct);
		}

		// Reports for the moderators: the spam post and its author waiting, older ones handled.
		Guid spamPost = postIds[20];
		(string User, TagReport Kind, TagReport Status, Guid Target, string Reason, string? Note, double HoursAgo)[] reports = [
			("demo", TagReport.Post, TagReport.Pending, spamPost, "تبلیغات و اسپم", null, 9),
			("neda", TagReport.Post, TagReport.Pending, spamPost, "کلاهبرداری", null, 8),
			("kaveh", TagReport.User, TagReport.Pending, U("behrooz"), "پیام‌های تبلیغاتی مکرر", null, 30),
			("sara", TagReport.User, TagReport.Resolved, U("behrooz"), "حساب جعلی", "هشدار داده شد.", 200),
			("tara", TagReport.Post, TagReport.Dismissed, postIds[9], "تبلیغ مجموعه", "پست مجموعه‌ی ورزشی، مشکلی نداره.", 120),
			("omid", TagReport.Venue, TagReport.Pending, SeedId("venue:lavasan"), "نشانی اشتباه", null, 50)
		];
		foreach ((string user, TagReport kind, TagReport status, Guid target, string reason, string? note, double hoursAgo) in reports)
			await db.Set<ReportEntity>().AddAsync(new ReportEntity {
				Id = Guid.CreateVersion7(),
				CreatedAt = now.AddHours(-hoursAgo),
				CreatorId = U(user),
				Tags = [kind, status],
				TargetId = target,
				Reason = reason,
				JsonData = new ReportJson { Note = note }
			}, _ct);

		await db.SaveChangesAsync(_ct);
		db.ChangeTracker.Clear();
	}

	private async Task Notify(string to, string from, string key, string? subject, string? linkType, Guid? linkId, DateTime at, TagNotification kind) =>
		await db.Set<NotificationEntity>().AddAsync(new NotificationEntity {
			Id = Guid.CreateVersion7(),
			CreatedAt = at,
			CreatorId = U(from),
			UserId = U(to),
			Tags = [kind, TagNotification.Unread],
			JsonData = new NotificationJson { Detail1 = key, Detail2 = subject ?? "", LinkType = linkType, LinkId = linkId }
		}, _ct);

	// ---------------- Chats ----------------

	private async Task SeedChats(DateTime now) {
		List<(string Key, string? Title, string[] Members, (string From, string Text, double MinutesAgo, string? LinkType, Guid? LinkId)[] Messages, int UnreadForDemo)> chats = [
			("farhad", null, ["demo", "farhad"], [
				("farhad", "سلام آرمان، فردا شب هستی؟", 2900, null, null),
				("demo", "سلام! آره، زمین ۱ رو رزرو کردم", 2890, null, null),
				("farhad", "عالیه. سهم منو از کیف پول پرداخت می‌کنم", 2880, null, null),
				("demo", "مرسی 🙏 تارا و ندا هم میان", 2870, null, null),
				("farhad", "پس مسترز اسکواش رو چی‌کار می‌کنی؟ نیمه‌نهایی فرداست", 600, null, null),
				("demo", "می‌رم ببینم چی میشه 😅", 590, null, null),
				("farhad", "این بازی رو دیدی؟", 45, "openMatch", SeedId("game:p-private")),
				("farhad", "خصوصیه ولی درخواستت رو قبول می‌کنم", 44, null, null),
				("farhad", "فقط زودتر بیا گرم کنیم", 12, null, null)
			], 3),
			("tara", null, ["demo", "tara"], [
				("tara", "سهمم رو برای رزرو فردا زدم ✅", 1500, null, null),
				("demo", "دمت گرم!", 1490, null, null),
				("tara", "راستی امروز عصر هم بازی هست، بیا", 300, "openMatch", SeedId("game:p-soon")),
				("demo", "اومدم توش 👍", 280, null, null)
			], 0),
			("lucas", null, ["demo", "lucas"], [
				("lucas", "Hey Arman! Saw your Padel Open win. Congrats!", 6000, null, null),
				("demo", "Thanks Lucas! You guys were tough in the semis.", 5990, null, null),
				("lucas", "Up for a tennis challenge this week?", 200, "openMatch", SeedId("game:t-challenge")),
				("lucas", "Best of three, loser buys the coffee ☕", 199, null, null)
			], 2),
			("owner", null, ["demo", "owner"], [
				("demo", "سلام آقای علوی، برای جام پاییزه زمین ۱ و ۲ رو از ساعت ۱۶ نگه می‌دارید؟", 4000, null, null),
				("owner", "سلام، حتماً. کیمیا هماهنگ می‌کنه.", 3950, null, null),
				("owner", "این هم لینک تورنمنت", 3940, "tournament", SeedId("tournament:autumn-cup"))
			], 0),
			("crew", "اکیپ پدل جمعه‌ها", ["demo", "neda", "yasaman", "tara", "farhad"], [
				("neda", "بچه‌ها این جمعه کجا بازی کنیم؟", 3000, null, null),
				("yasaman", "باغ کیانی؟ 🌳", 2990, null, null),
				("demo", "قدمتون روی چشم، زمین روباز رو نگه می‌دارم", 2980, "venue", SeedId("venue:garden")),
				("tara", "من ساعت ۸ صبح هستم", 2970, null, null),
				("farhad", "منم", 2960, null, null),
				("neda", "امشب جام شبانه هم هست، کسی میاد تماشا؟", 120, "tournament", SeedId("tournament:night-cup")),
				("yasaman", "من میام!", 100, null, null),
				("tara", "منم بعد از بازی میام", 60, null, null)
			], 3),
			("open", "بازیکنان اوپن پدل تهران", ["organizer", "demo", "farhad", "lucas", "kaveh", "omid", "tara", "neda", "yasaman"], [
				("organizer", "سلام به همه! قرعه‌کشی انجام شد، جدول توی اپ هست.", 31000, "tournament", padelOpenId),
				("organizer", "لطفاً ۳۰ دقیقه قبل از بازی برسید.", 30900, null, null),
				("kaveh", "زمین ۱ یا ۲؟", 30800, null, null),
				("organizer", "برنامه‌ی زمین‌ها توی جدول هست", 30790, null, null),
				("organizer", "تبریک به آرمان و فرهاد، قهرمان‌های اوپن! 🏆", 29000, null, null)
			], 0),
			("kaveh-yasaman", null, ["kaveh", "yasaman"], [
				("kaveh", "بازی پنجشنبه رو ساختم، بیا", 900, "openMatch", SeedId("game:p-garden")),
				("yasaman", "اومدم ✌️", 880, null, null)
			], 0)
		];

		foreach ((string key, string? title, string[] members, var messages, int unread) in chats) {
			Guid id = SeedId($"chat:{key}");
			List<Guid> memberIds = members.Select(U).ToList();
			List<UserEntity> users = await db.Set<UserEntity>().AsTracking().Where(x => memberIds.Contains(x.Id)).ToListAsync(_ct);
			DateTime last = now.AddMinutes(-messages[^1].MinutesAgo);
			List<ConversationRead> reads = members.Select(m => new ConversationRead {
				UserId = U(m),
				// The demo player hasn't read the last [unread] messages; everyone else is up to date.
				At = m == "demo" && unread > 0 ? now.AddMinutes(-messages[^(unread + 1)].MinutesAgo) : last
			}).ToList();
			await db.Set<ConversationEntity>().AddAsync(new ConversationEntity {
				Id = id,
				CreatedAt = now.AddMinutes(-messages[0].MinutesAgo - 5),
				CreatorId = U(members[0]),
				Tags = [title == null ? TagConversation.Direct : TagConversation.Group],
				Title = title,
				DirectKey = title == null ? DirectKey(U(members[0]), U(members[1])) : null,
				LastMessageAt = last,
				Users = users,
				JsonData = new ConversationJson { LastMessageText = messages[^1].Text, LastMessageUserId = U(messages[^1].From), Reads = reads }
			}, _ct);
			for (int i = 0; i < messages.Length; i++) {
				(string from, string text, double minutesAgo, string? linkType, Guid? linkId) = messages[i];
				await db.Set<MessageEntity>().AddAsync(new MessageEntity {
					Id = SeedId($"chat:{key}:{i}"),
					CreatedAt = now.AddMinutes(-minutesAgo),
					CreatorId = U(from),
					Tags = [linkType == null ? TagMessage.Text : TagMessage.Shared],
					Text = text,
					ConversationId = id,
					JsonData = new MessageJson { LinkType = linkType, LinkId = linkId }
				}, _ct);
			}

			await db.SaveChangesAsync(_ct);
			db.ChangeTracker.Clear();
		}
	}

	private static readonly Guid padelOpenId = SeedId("tournament:padel-open");

	private static string DirectKey(Guid a, Guid b) => string.CompareOrdinal(a.ToString(), b.ToString()) < 0 ? $"{a}:{b}" : $"{b}:{a}";

	// ---------------- Wrap-up ----------------

	/// <summary>Everything older than a day and a half has been read; the newest stay unread.</summary>
	private async Task FinishNotifications(DateTime now) {
		await db.SaveChangesAsync(_ct);
		db.ChangeTracker.Clear();
		List<Guid> ids = _userIds;
		DateTime readBefore = now.AddHours(-36);
		foreach (TagNotification kind in Enum.GetValues<TagNotification>().Where(x => (int)x < 200)) {
			List<TagNotification> read = [kind, TagNotification.Read];
			await db.Set<NotificationEntity>()
				.Where(x => ids.Contains(x.UserId) && x.CreatedAt < readBefore && x.Tags.Contains(kind) && x.Tags.Contains(TagNotification.Unread))
				.ExecuteUpdateAsync(u => u.SetProperty(x => x.Tags, read), _ct);
		}
	}

	private async Task Count() {
		List<Guid> ids = _userIds;
		_result.Users = ids.Count;
		_result.Venues = await db.Set<VenueEntity>().CountAsync(x => ids.Contains(x.CreatorId), _ct);
		_result.Courts = await db.Set<CourtEntity>().CountAsync(x => ids.Contains(x.CreatorId), _ct);
		_result.Bookings = await db.Set<BookingEntity>().CountAsync(x => ids.Contains(x.UserId), _ct);
		_result.Tournaments = await db.Set<TournamentEntity>().CountAsync(x => ids.Contains(x.CreatorId), _ct);
		_result.OpenMatches = await db.Set<OpenMatchEntity>().CountAsync(x => ids.Contains(x.CreatorId), _ct);
		_result.Posts = await db.Set<PostEntity>().CountAsync(x => ids.Contains(x.CreatorId), _ct);
		_result.Conversations = await db.Set<ConversationEntity>().CountAsync(x => ids.Contains(x.CreatorId), _ct);
		_result.Messages = await db.Set<MessageEntity>().CountAsync(x => ids.Contains(x.CreatorId), _ct);
		_result.Notifications = await db.Set<NotificationEntity>().CountAsync(x => ids.Contains(x.UserId), _ct);
	}

	// ---------------- Pictures ----------------
	// Drawn pixel by pixel (ImageSharp has no drawing package here): a court, table or pitch seen from above for venues,
	// a ball for posts and a silhouette for profile photos.

	private static readonly (byte R, byte G, byte B)[] Palette = [
		(30, 90, 200), (220, 80, 60), (40, 150, 110), (150, 70, 200), (230, 150, 30), (20, 140, 170), (200, 60, 130), (90, 110, 130), (60, 60, 160), (170, 120, 60)
	];

	private void Save(string path, Image<Rgb24> image) {
		string full = Path.Combine(env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot"), "Media", path);
		Directory.CreateDirectory(Path.GetDirectoryName(full)!);
		image.SaveAsJpeg(full, new JpegEncoder { Quality = 82 });
	}

	private static Rgb24 Mix((byte R, byte G, byte B) a, (byte R, byte G, byte B) b, float t) {
		t = Math.Clamp(t, 0, 1);
		return new Rgb24((byte)(a.R + (b.R - a.R) * t), (byte)(a.G + (b.G - a.G) * t), (byte)(a.B + (b.B - a.B) * t));
	}

	private static Rgb24 Blend(Rgb24 c, (byte R, byte G, byte B) over, float alpha) => Mix((c.R, c.G, c.B), over, alpha);

	private static (byte, byte, byte) Darker((byte R, byte G, byte B) c, float f) => ((byte)(c.R * f), (byte)(c.G * f), (byte)(c.B * f));

	private void Draw(string path, int width, int height, Func<int, int, Rgb24> pixel) {
		using Image<Rgb24> image = new(width, height);
		image.ProcessPixelRows(rows => {
			for (int y = 0; y < rows.Height; y++) {
				Span<Rgb24> row = rows.GetRowSpan(y);
				for (int x = 0; x < row.Length; x++) row[x] = pixel(x, y);
			}
		});
		Save(path, image);
	}

	private void DrawAvatar(string path, int index) {
		(byte, byte, byte) a = Palette[index % Palette.Length], b = Palette[(index * 3 + 4) % Palette.Length];
		const int size = 256;
		Draw(path, size, size, (x, y) => {
			Rgb24 c = Mix(a, b, (x + y) / (2f * size));
			float hx = x - size * 0.5f, hy = y - size * 0.4f, sx = (x - size * 0.5f) / (size * 0.36f), sy = (y - size * 0.98f) / (size * 0.34f);
			bool head = hx * hx + hy * hy < size * 0.17f * size * 0.17f, body = sx * sx + sy * sy < 1;
			return head || body ? Blend(c, (255, 255, 255), 0.82f) : c;
		});
	}

	private void DrawVenue(string path, SeedVenue v, int shot) {
		(byte R, byte G, byte B) baseColor = v.Color, surround = Darker(v.Color, 0.45f);
		TagSport? sport = v.Courts.FirstOrDefault()?.Sport;
		const int w = 960, h = 600;
		Random noise = new(v.Key.GetHashCode() ^ shot);
		float[] lights = Enumerable.Range(0, 14).Select(_ => (float)noise.NextDouble()).ToArray();

		// Shot 1 and 2: the place around it (warm light spots); shot 0: the court itself from above.
		if (shot > 0 || sport == null) {
			(byte, byte, byte) warm = shot == 1 ? ((byte)250, (byte)200, (byte)120) : ((byte)255, (byte)240, (byte)210);
			Draw(path, w, h, (x, y) => {
				Rgb24 c = Mix(surround, baseColor, y / (float)h * 0.8f + x / (float)w * 0.2f);
				for (int i = 0; i < lights.Length; i += 2) {
					float dx = x - lights[i] * w, dy = y - lights[i + 1] * h * 0.6f;
					float d = MathF.Sqrt(dx * dx + dy * dy);
					if (d < 140) c = Blend(c, warm, (1 - d / 140) * 0.45f);
				}
				return c;
			});
			return;
		}

		Draw(path, w, h, (x, y) => {
			float mx = 120, my = 70;
			bool inside = x >= mx && x <= w - mx && y >= my && y <= h - my;
			Rgb24 c = inside ? Mix(baseColor, Darker(baseColor, 0.8f), y / (float)h) : Mix(surround, Darker(surround, 0.7f), x / (float)w);
			if (!inside) return c;
			float fx = (x - mx) / (w - 2 * mx), fy = (y - my) / (h - 2 * my);
			float lw = 3f / (w - 2 * mx), lh = 3f / (h - 2 * my);
			bool edge = fx < lw || fx > 1 - lw || fy < lh || fy > 1 - lh;
			bool line = sport switch {
				TagSport.Football => FootballLine(fx, fy, lw, lh),
				TagSport.Billiards or TagSport.Snooker => false,
				TagSport.Squash => MathF.Abs(fy - 0.55f) < lh || fy > 0.55f && MathF.Abs(fx - 0.5f) < lw,
				_ => MathF.Abs(fy - 0.5f) < lh || (MathF.Abs(fy - 0.2f) < lh || MathF.Abs(fy - 0.8f) < lh) || fy > 0.2f && fy < 0.8f && MathF.Abs(fx - 0.5f) < lw
			};
			bool net = sport is TagSport.Padel or TagSport.Tennis && MathF.Abs(fy - 0.5f) < lh * 2.5f;
			if (sport is TagSport.Billiards or TagSport.Snooker) {
				bool cushion = fx < 0.05f || fx > 0.95f || fy < 0.08f || fy > 0.92f;
				float[][] pockets = [[0.03f, 0.05f], [0.5f, 0.03f], [0.97f, 0.05f], [0.03f, 0.95f], [0.5f, 0.97f], [0.97f, 0.95f]];
				bool pocket = pockets.Any(p => MathF.Pow((fx - p[0]) * 1.6f, 2) + MathF.Pow(fy - p[1], 2) < 0.0022f);
				if (pocket) return new Rgb24(15, 15, 15);
				if (cushion) return Mix((110, 60, 25), (80, 40, 15), fy);
			}

			if (net) return new Rgb24(240, 240, 240);
			return edge || line ? Blend(c, (255, 255, 255), 0.9f) : c;
		});
	}

	/// <summary>Halfway line, centre circle and the two penalty boxes of a pitch (fx, fy in 0-1).</summary>
	private static bool FootballLine(float fx, float fy, float lw, float lh) {
		bool halfway = MathF.Abs(fx - 0.5f) < lw;
		float cx = (fx - 0.5f) * 1.6f, cy = fy - 0.5f;
		bool circle = MathF.Abs(MathF.Sqrt(cx * cx + cy * cy) - 0.18f) < lh * 1.5f;
		bool inBoxRows = MathF.Abs(fy - 0.5f) < 0.25f;
		bool boxSide = (MathF.Abs(fx - 0.15f) < lw || MathF.Abs(fx - 0.85f) < lw) && inBoxRows;
		bool boxEdge = (fx < 0.15f || fx > 0.85f) && MathF.Abs(MathF.Abs(fy - 0.5f) - 0.25f) < lh;
		return halfway || circle || boxSide || boxEdge;
	}

	private void DrawPost(string path, string kind, int index) {
		(byte, byte, byte) a = Palette[index % Palette.Length], b = Palette[(index + 5) % Palette.Length];
		const int size = 900;
		(byte, byte, byte) ball = kind switch { "table" => ((byte)230, (byte)40, (byte)40), "pitch" => ((byte)250, (byte)250, (byte)250), "trophy" => ((byte)250, (byte)200, (byte)60), _ => ((byte)220, (byte)240, (byte)60) };
		Draw(path, size, size, (x, y) => {
			Rgb24 c = Mix(a, b, (x * 0.7f + y * 0.3f) / size);
			// Soft stripes, then the ball with its seam.
			if ((x + y) / 60 % 2 == 0) c = Blend(c, (255, 255, 255), 0.05f);
			float dx = x - size * 0.58f, dy = y - size * 0.45f, r = size * 0.24f, d = MathF.Sqrt(dx * dx + dy * dy);
			float sx = x - size * 0.62f, sy = y - size * 0.78f;
			if (d < r) {
				float shade = 1 - d / r * 0.35f;
				c = new Rgb24((byte)(ball.Item1 * shade), (byte)(ball.Item2 * shade), (byte)(ball.Item3 * shade));
				float seam1 = MathF.Abs(MathF.Sqrt((dx + r * 1.15f) * (dx + r * 1.15f) + dy * dy) - r * 0.95f);
				float seam2 = MathF.Abs(MathF.Sqrt((dx - r * 1.15f) * (dx - r * 1.15f) + dy * dy) - r * 0.95f);
				if (kind != "trophy" && (seam1 < 5 || seam2 < 5)) c = Blend(c, (255, 255, 255), 0.85f);
			}
			else if (sx * sx / 4 + sy * sy < 30 * 30) c = Blend(c, (0, 0, 0), 0.12f);
			return c;
		});
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

/// <summary>
/// Every few minutes: notifies players of tournament matches, court bookings and open games that start within the hour (once each).
/// A host without the sport tables just skips it.
/// </summary>
public sealed class SportReminderService(IServiceScopeFactory scopeFactory) : BackgroundService {
	private static readonly TimeSpan Every = TimeSpan.FromMinutes(5);
	private bool _failureLogged;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
		using PeriodicTimer timer = new(Every);
		try {
			do {
				try {
					using IServiceScope scope = scopeFactory.CreateScope();
					await scope.ServiceProvider.GetRequiredService<ISportService>().NotifyUpcomingGames(stoppingToken);
				}
				catch (OperationCanceledException) {
					throw;
				}
				catch (Exception e) {
					if (!_failureLogged) ULog.Error(e, "Sport reminders are off (are the sport tables migrated?)");
					_failureLogged = true;
				}
			} while (await timer.WaitForNextTickAsync(stoppingToken));
		}
		catch (OperationCanceledException) {
			// The server is stopping.
		}
	}
}
