namespace SinaMN75U.Routes;

public static class SportRoutes {
	public static void MapSportRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();

		r.MapPost("Sport/Create", async (SportCreateParams p, ISportService s, CancellationToken c) => (await s.CreateSport(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Sport/Read", async (SportReadParams p, ISportService s, CancellationToken c) => (await s.ReadSports(p, c)).ToResult()).Produces<UResponse<IEnumerable<SportResponse>>>();
		r.MapPost("Sport/ReadById", async (IdParams<SportSelectorArgs> p, ISportService s, CancellationToken c) => (await s.ReadSportById(p, c)).ToResult()).Produces<UResponse<SportResponse>>();
		r.MapPost("Sport/Update", async (SportUpdateParams p, ISportService s, CancellationToken c) => (await s.UpdateSport(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Sport/Delete", async (IdParams p, ISportService s, CancellationToken c) => (await s.DeleteSport(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("PlayerSportProfile/Create", async (PlayerSportProfileCreateParams p, ISportService s, CancellationToken c) => (await s.CreatePlayerSportProfile(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("PlayerSportProfile/Read", async (PlayerSportProfileReadParams p, ISportService s, CancellationToken c) => (await s.ReadPlayerSportProfiles(p, c)).ToResult()).Produces<UResponse<IEnumerable<PlayerSportProfileResponse>>>();
		r.MapPost("PlayerSportProfile/Update", async (PlayerSportProfileUpdateParams p, ISportService s, CancellationToken c) => (await s.UpdatePlayerSportProfile(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("PlayerSportProfile/Delete", async (IdParams p, ISportService s, CancellationToken c) => (await s.DeletePlayerSportProfile(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("Tournament/Create", async (TournamentCreateParams p, ISportService s, CancellationToken c) => (await s.CreateTournament(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Tournament/Read", async (TournamentReadParams p, ISportService s, CancellationToken c) => (await s.ReadTournaments(p, c)).ToResult()).Produces<UResponse<IEnumerable<TournamentResponse>>>();
		r.MapPost("Tournament/ReadById", async (IdParams<TournamentSelectorArgs> p, ISportService s, CancellationToken c) => (await s.ReadTournamentById(p, c)).ToResult()).Produces<UResponse<TournamentResponse>>();
		r.MapPost("Tournament/Update", async (TournamentUpdateParams p, ISportService s, CancellationToken c) => (await s.UpdateTournament(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Tournament/Delete", async (IdParams p, ISportService s, CancellationToken c) => (await s.DeleteTournament(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Tournament/Standings", async (IdParams p, ISportService s, CancellationToken c) => (await s.ReadTournamentStandings(p, c)).ToResult()).Produces<UResponse<IEnumerable<TournamentStandingResponse>>>();
		r.MapPost("Tournament/GenerateMatches", async (IdParams p, ISportService s, CancellationToken c) => (await s.GenerateTournamentMatches(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Tournament/NextSeason", async (IdParams p, ISportService s, CancellationToken c) => (await s.CreateNextTournamentSeason(p, c)).ToResult()).Produces<UResponse<Guid?>>();

		r.MapPost("TournamentEntry/Register", async (TournamentRegisterParams p, ISportService s, CancellationToken c) => (await s.RegisterTournamentEntry(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("TournamentEntry/Update", async (TournamentEntryUpdateParams p, ISportService s, CancellationToken c) => (await s.UpdateTournamentEntry(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("TournamentEntry/Delete", async (IdParams p, ISportService s, CancellationToken c) => (await s.DeleteTournamentEntry(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("TournamentMatch/Update", async (TournamentMatchUpdateParams p, ISportService s, CancellationToken c) => (await s.UpdateTournamentMatch(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("TournamentMatch/Read", async (TournamentMatchReadParams p, ISportService s, CancellationToken c) => (await s.ReadTournamentMatches(p, c)).ToResult()).Produces<UResponse<IEnumerable<TournamentMatchResponse>>>();

		r.MapPost("PlayerRatingHistory/Read", async (PlayerRatingHistoryReadParams p, ISportService s, CancellationToken c) => (await s.ReadPlayerRatingHistory(p, c)).ToResult()).Produces<UResponse<IEnumerable<PlayerRatingHistoryResponse>>>();
		r.MapPost("PlayerAchievement/Read", async (PlayerAchievementReadParams p, ISportService s, CancellationToken c) => (await s.ReadPlayerAchievements(p, c)).ToResult()).Produces<UResponse<IEnumerable<PlayerAchievementResponse>>>();
		r.MapPost("PlayerAchievement/Update", async (PlayerAchievementUpdateParams p, ISportService s, CancellationToken c) => (await s.UpdatePlayerAchievement(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Leaderboard/Read", async (LeaderboardParams p, ISportService s, CancellationToken c) => (await s.ReadLeaderboard(p, c)).ToResult()).Produces<UResponse<IEnumerable<LeaderboardRowResponse>>>();
		r.MapPost("Player/Stats", async (PlayerStatsParams p, ISportService s, CancellationToken c) => (await s.ReadPlayerStats(p, c)).ToResult()).Produces<UResponse<PlayerStatsResponse>>();

		r.MapPost("OpenMatch/Create", async (OpenMatchCreateParams p, ISportService s, CancellationToken c) => (await s.CreateOpenMatch(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("OpenMatch/Read", async (OpenMatchReadParams p, ISportService s, CancellationToken c) => (await s.ReadOpenMatches(p, c)).ToResult()).Produces<UResponse<IEnumerable<OpenMatchResponse>>>();
		r.MapPost("OpenMatch/ReadById", async (IdParams<OpenMatchSelectorArgs> p, ISportService s, CancellationToken c) => (await s.ReadOpenMatchById(p, c)).ToResult()).Produces<UResponse<OpenMatchResponse>>();
		r.MapPost("OpenMatch/Update", async (OpenMatchUpdateParams p, ISportService s, CancellationToken c) => (await s.UpdateOpenMatch(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("OpenMatch/Delete", async (IdParams p, ISportService s, CancellationToken c) => (await s.DeleteOpenMatch(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("OpenMatch/Join", async (IdParams p, ISportService s, CancellationToken c) => (await s.JoinOpenMatch(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("OpenMatch/Leave", async (IdParams p, ISportService s, CancellationToken c) => (await s.LeaveOpenMatch(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("OpenMatch/Result", async (OpenMatchResultParams p, ISportService s, CancellationToken c) => (await s.SetOpenMatchResult(p, c)).ToResult()).Produces<UResponse>();
	}
}
