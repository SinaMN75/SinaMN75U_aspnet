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
