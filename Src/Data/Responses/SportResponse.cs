namespace SinaMN75U.Data.Responses;

public sealed class SportResponse : BaseResponse<TagSport, SportJson> {
	public required string Title { get; set; }
	public int Order { get; set; }
	public decimal MinLevel { get; set; }
	public decimal MaxLevel { get; set; }
}

public sealed class PlayerSportProfileResponse : BaseResponse<TagPlayerSportProfile, PlayerSportProfileJson> {
	public required decimal Level { get; set; }
	public decimal? LastLevelChange { get; set; } // from the latest rated match
	public required Guid UserId { get; set; }
	public required Guid SportId { get; set; }

	public UserResponse? User { get; set; }
	public SportResponse? Sport { get; set; }
}

public sealed class TournamentResponse : BaseResponse<TagTournament, TournamentJson> {
	public required string Title { get; set; }
	public required DateTime StartDate { get; set; }
	public required int Capacity { get; set; }
	public decimal EntryFee { get; set; }
	public decimal? MinLevel { get; set; }
	public decimal? MaxLevel { get; set; }
	public required Guid SportId { get; set; }
	public int EntryCount { get; set; } // entries that are not rejected

	public SportResponse? Sport { get; set; }
	public ICollection<TournamentEntryResponse>? Entries { get; set; }
	public ICollection<TournamentMatchResponse>? Matches { get; set; }
}

public sealed class TournamentEntryResponse : BaseResponse<TagTournamentEntry, TournamentEntryJson> {
	public string? Title { get; set; }
	public int? Seed { get; set; }
	public int? GroupNumber { get; set; }
	public required Guid TournamentId { get; set; }

	/// <summary>Only public fields (id and names).</summary>
	public ICollection<UserResponse>? Users { get; set; }

	public TournamentResponse? Tournament { get; set; }
}

public sealed class TournamentMatchResponse : BaseResponse<TagTournamentMatch, TournamentMatchJson> {
	public required int Round { get; set; }
	public required int Order { get; set; }
	public DateTime? ScheduledAt { get; set; }
	public required Guid TournamentId { get; set; }
	public Guid? EntryAId { get; set; }
	public Guid? EntryBId { get; set; }
	public Guid? WinnerEntryId { get; set; }
	public int? GroupNumber { get; set; }
	public Guid? PartnerAId { get; set; }
	public Guid? PartnerBId { get; set; }
	public Guid? NextMatchId { get; set; }
	public int? NextMatchSlot { get; set; }
	public Guid? LoserNextMatchId { get; set; }
	public int? LoserNextMatchSlot { get; set; }

	public TournamentResponse? Tournament { get; set; }
	public TournamentEntryResponse? EntryA { get; set; }
	public TournamentEntryResponse? EntryB { get; set; }
	public TournamentEntryResponse? PartnerA { get; set; }
	public TournamentEntryResponse? PartnerB { get; set; }
}

public sealed class TournamentStandingResponse {
	public required Guid EntryId { get; set; }
	public string? Title { get; set; }
	public ICollection<UserResponse> Users { get; set; } = [];
	public int? GroupNumber { get; set; } // group or box; empty for a single table
	public int Promotion { get; set; } // box league: 1 moves up a box, -1 moves down
	public int Rank { get; set; }
	public int Played { get; set; }
	public int Won { get; set; }
	public int Drawn { get; set; }
	public int Lost { get; set; }
	public int SetsFor { get; set; }
	public int SetsAgainst { get; set; }
	public int ScoreFor { get; set; }
	public int ScoreAgainst { get; set; }
	public int Points { get; set; }
}

public sealed class PlayerRatingHistoryResponse : BaseResponse<TagPlayerRatingHistory, PlayerRatingHistoryJson> {
	public required Guid UserId { get; set; }
	public required Guid SportId { get; set; }
	public required Guid MatchId { get; set; }
	public required decimal LevelBefore { get; set; }
	public required decimal LevelAfter { get; set; }
}

public sealed class PlayerAchievementResponse : BaseResponse<TagPlayerAchievement, PlayerAchievementJson> {
	public required Guid UserId { get; set; }
	public Guid? SportId { get; set; }
	public Guid? TournamentId { get; set; }
	public int? Rank { get; set; }
	public int Points { get; set; }

	public UserResponse? User { get; set; }
	public SportResponse? Sport { get; set; }
}

public sealed class LeaderboardRowResponse {
	public required int Rank { get; set; }
	public required UserResponse User { get; set; }
	public decimal Level { get; set; }
	public int Points { get; set; }
	public int MatchesPlayed { get; set; }
}

public sealed class PlayerStatsResponse {
	public UserResponse? User { get; set; } // public fields only
	public int MatchesPlayed { get; set; }
	public int Wins { get; set; }
	public int Losses { get; set; }
	public int Draws { get; set; }
	public int WinRate { get; set; } // percent
	public int CurrentWinStreak { get; set; }
	public int BestWinStreak { get; set; }
	public int WeeklyStreak { get; set; } // weeks in a row with at least one match
	public int TournamentsPlayed { get; set; }
	public int TournamentWins { get; set; }
	public int Podiums { get; set; }
	public int RankingPoints { get; set; }
	public int Followers { get; set; }
	public int Following { get; set; }
	public string? ReferralCode { get; set; } // the signed-in user's own only
	public int ReferralCount { get; set; }
}

public sealed class OpenMatchResponse : BaseResponse<TagOpenMatch, OpenMatchJson> {
	public required DateTime StartAt { get; set; }
	public int DurationMinutes { get; set; }
	public required int Capacity { get; set; }
	public decimal? MinLevel { get; set; }
	public decimal? MaxLevel { get; set; }
	public decimal PricePerPlayer { get; set; }
	public required Guid SportId { get; set; }
	public Guid? VenueId { get; set; }
	public int PlayerCount { get; set; }

	public SportResponse? Sport { get; set; }
	public VenueResponse? Venue { get; set; }

	/// <summary>Only public fields (id and names).</summary>
	public ICollection<UserResponse>? Users { get; set; }
}

public sealed class SportSeedAccountResponse {
	public required string Email { get; set; }
	public required string Password { get; set; }
	public required string FullName { get; set; }
	public required string Role { get; set; }
}

public sealed class SportSeedResponse {
	public ICollection<SportSeedAccountResponse> Accounts { get; set; } = [];
	public int Users { get; set; }
	public int Venues { get; set; }
	public int Courts { get; set; }
	public int Bookings { get; set; }
	public int Tournaments { get; set; }
	public int TournamentMatches { get; set; }
	public int OpenMatches { get; set; }
	public int Posts { get; set; }
	public int Conversations { get; set; }
	public int Messages { get; set; }
	public int Notifications { get; set; }

	/// Steps a service refused; the rest of the seed still ran.
	public ICollection<string> Warnings { get; set; } = [];
}
