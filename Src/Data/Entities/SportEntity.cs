namespace SinaMN75U.Data.Entities;

// ---------------- Sport ----------------

[Table("Sports")]
public sealed class SportEntity : BaseEntity<TagSport, SportJson> {
	// The sport type is a 1xx tag (TagSport.Padel, ...), one sport per type.

	[Required, MaxLength(100)]
	public required string Title { get; set; } // a ULocalizedConstants key, e.g. ULocalizedConstants.Padel; clients translate it

	public int Order { get; set; }

	[Column(TypeName = "decimal(4,2)")]
	public decimal MinLevel { get; set; } = 1;

	[Column(TypeName = "decimal(4,2)")]
	public decimal MaxLevel { get; set; } = 7;

	public ICollection<PlayerSportProfileEntity> PlayerProfiles { get; set; } = [];
}

public sealed class SportJson : BaseJson {
	public string? Icon { get; set; }
}

// ---------------- PlayerSportProfile ----------------

[Table("PlayerSportProfiles")]
[Microsoft.EntityFrameworkCore.Index(nameof(UserId), nameof(SportId), IsUnique = true, Name = "IX_PlayerSportProfiles_UserId_SportId")]
public sealed class PlayerSportProfileEntity : BaseEntity<TagPlayerSportProfile, PlayerSportProfileJson> {
	[Column(TypeName = "decimal(4,2)")]
	public required decimal Level { get; set; }

	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;

	public required Guid SportId { get; set; }
	public SportEntity Sport { get; set; } = null!;
}

public sealed class PlayerSportProfileJson : BaseJson {
	// Glicko-2 state behind Level; empty until the first rated match.
	public double? Rating { get; set; }
	public double? Deviation { get; set; }
	public double? Volatility { get; set; }
	public int MatchesPlayed { get; set; }
}

// ---------------- PlayerRatingHistory ----------------

/// <summary>One level change caused by one match; keeps the state before it so a corrected result can be undone.</summary>
[Table("PlayerRatingHistories")]
[Microsoft.EntityFrameworkCore.Index(nameof(UserId), nameof(SportId), Name = "IX_PlayerRatingHistories_UserId_SportId")]
[Microsoft.EntityFrameworkCore.Index(nameof(MatchId), Name = "IX_PlayerRatingHistories_MatchId")]
public sealed class PlayerRatingHistoryEntity : BaseEntity<TagPlayerRatingHistory, PlayerRatingHistoryJson> {
	public required Guid UserId { get; set; }
	public required Guid SportId { get; set; }
	public required Guid MatchId { get; set; }

	[Column(TypeName = "decimal(4,2)")]
	public required decimal LevelBefore { get; set; }

	[Column(TypeName = "decimal(4,2)")]
	public required decimal LevelAfter { get; set; }
}

public sealed class PlayerRatingHistoryJson : BaseJson {
	public double? RatingBefore { get; set; }
	public double? DeviationBefore { get; set; }
	public double? VolatilityBefore { get; set; }
	public int MatchesPlayedBefore { get; set; }
}

// ---------------- Tournament ----------------

[Table("Tournaments")]
[Microsoft.EntityFrameworkCore.Index(nameof(SportId), Name = "IX_Tournaments_SportId")]
public sealed class TournamentEntity : BaseEntity<TagTournament, TournamentJson> {
	// Tags: one format (1xx), one participant type (2xx), one status (3xx), optional AutoApprove (401).

	[Required, MaxLength(200)]
	public required string Title { get; set; }

	public required DateTime StartDate { get; set; }

	public required int Capacity { get; set; }

	[Column(TypeName = "decimal(18,2)")]
	public decimal EntryFee { get; set; }

	[Column(TypeName = "decimal(4,2)")]
	public decimal? MinLevel { get; set; }

	[Column(TypeName = "decimal(4,2)")]
	public decimal? MaxLevel { get; set; }

	public required Guid SportId { get; set; }
	public SportEntity Sport { get; set; } = null!;

	public ICollection<TournamentEntryEntity> Entries { get; set; } = [];
	public ICollection<TournamentMatchEntity> Matches { get; set; } = [];
}

public sealed class TournamentJson : BaseJson {
	public string? Description { get; set; }
	public string? Prize { get; set; }
	public string? Venue { get; set; }
	public string? Address { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public int PointsForWin { get; set; } = 3;
	public int PointsForDraw { get; set; } = 1;
	public int PointsForLoss { get; set; }

	// Format options
	public int? GroupCount { get; set; } // groups + knockout; empty = about 4 per group
	public int AdvancePerGroup { get; set; } = 2;
	public bool ThirdPlaceMatch { get; set; } // single elimination
	public int? Rounds { get; set; } // mexicano / swiss; empty = a sensible default
	public int PointsPerMatch { get; set; } = 24; // americano / mexicano
	public int BoxSize { get; set; } = 5; // box league

	// Scoring options; empty = the sport's default
	public int? SetsToWin { get; set; }
	public int? RaceTo { get; set; } // billiards racks / snooker frames
	public bool SuperTiebreak { get; set; } // padel / tennis deciding set to 10

	public bool Unrated { get; set; } // results don't change the players' levels
	public Guid? PreviousSeasonId { get; set; } // box league: the period this one continues
}

// ---------------- TournamentEntry ----------------

[Table("TournamentEntries")]
[Microsoft.EntityFrameworkCore.Index(nameof(TournamentId), Name = "IX_TournamentEntries_TournamentId")]
public sealed class TournamentEntryEntity : BaseEntity<TagTournamentEntry, TournamentEntryJson> {
	[MaxLength(100)]
	public string? Title { get; set; } // team name; empty = the players' names

	public int? Seed { get; set; }

	public int? GroupNumber { get; set; } // group (groups + knockout) or box (box league)

	public required Guid TournamentId { get; set; }
	public TournamentEntity Tournament { get; set; } = null!;

	public ICollection<UserEntity> Users { get; set; } = [];
}

public sealed class TournamentEntryJson : BaseJson {
	// The entry fee paid from the wallet: held by the platform, refunded if the entry leaves, paid to the organizer when the tournament starts.
	public decimal PaidAmount { get; set; }
	public bool Refunded { get; set; }
	public bool Settled { get; set; }
}

// ---------------- TournamentMatch ----------------

[Table("TournamentMatches")]
[Microsoft.EntityFrameworkCore.Index(nameof(TournamentId), Name = "IX_TournamentMatches_TournamentId")]
public sealed class TournamentMatchEntity : BaseEntity<TagTournamentMatch, TournamentMatchJson> {
	public required int Round { get; set; }
	public required int Order { get; set; }

	public DateTime? ScheduledAt { get; set; }

	public required Guid TournamentId { get; set; }
	public TournamentEntity Tournament { get; set; } = null!;

	public Guid? EntryAId { get; set; }
	public TournamentEntryEntity? EntryA { get; set; }

	public Guid? EntryBId { get; set; }
	public TournamentEntryEntity? EntryB { get; set; }

	public Guid? WinnerEntryId { get; set; } // null on a draw or before the result

	public int? GroupNumber { get; set; }

	// Americano / Mexicano: each side is two individual entries.
	public Guid? PartnerAId { get; set; }
	public TournamentEntryEntity? PartnerA { get; set; }

	public Guid? PartnerBId { get; set; }
	public TournamentEntryEntity? PartnerB { get; set; }

	// Brackets: where the winner / loser goes (slot 1 = side A, 2 = side B).
	public Guid? NextMatchId { get; set; }
	public int? NextMatchSlot { get; set; }
	public Guid? LoserNextMatchId { get; set; }
	public int? LoserNextMatchSlot { get; set; }
}

public sealed class TournamentMatchJson : BaseJson {
	public string? Court { get; set; }
	public List<MatchSetScore> Sets { get; set; } = [];
	public List<MatchScoreChange> History { get; set; } = [];
	public bool Reminded { get; set; } // the players got the "starts soon" notification
}

/// <summary>Who changed a result and to what.</summary>
public sealed class MatchScoreChange {
	public required Guid UserId { get; set; }
	public required DateTime At { get; set; }
	public List<MatchSetScore> Sets { get; set; } = [];
}

/// <summary>One set / game / frame / half — whatever the sport counts.</summary>
public sealed class MatchSetScore {
	public required int A { get; set; }
	public required int B { get; set; }
}


// ---------------- PlayerAchievement ----------------

/// <summary>A trophy on a player's profile: a final tournament rank (with ranking points) or a badge.</summary>
[Table("PlayerAchievements")]
[Microsoft.EntityFrameworkCore.Index(nameof(UserId), Name = "IX_PlayerAchievements_UserId")]
[Microsoft.EntityFrameworkCore.Index(nameof(TournamentId), Name = "IX_PlayerAchievements_TournamentId")]
public sealed class PlayerAchievementEntity : BaseEntity<TagPlayerAchievement, PlayerAchievementJson> {
	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;

	public Guid? SportId { get; set; }
	public SportEntity? Sport { get; set; }

	public Guid? TournamentId { get; set; }
	public TournamentEntity? Tournament { get; set; }

	public int? Rank { get; set; }

	public int Points { get; set; } // ranking points for the leaderboard
}

public sealed class PlayerAchievementJson : BaseJson {
	public string? Title { get; set; } // the tournament's title when it was won
	public string? Badge { get; set; } // a badge key, e.g. "firstWin"; clients translate it
	public int EntryCount { get; set; }
}

// ---------------- OpenMatch ----------------

/// <summary>A game a player opens for others to join; a finished competitive one changes the players' levels.</summary>
[Table("OpenMatches")]
[Microsoft.EntityFrameworkCore.Index(nameof(SportId), nameof(StartAt), Name = "IX_OpenMatches_SportId_StartAt")]
public sealed class OpenMatchEntity : BaseEntity<TagOpenMatch, OpenMatchJson> {
	public required DateTime StartAt { get; set; }

	public int DurationMinutes { get; set; } = 90;

	public required int Capacity { get; set; }

	[Column(TypeName = "decimal(4,2)")]
	public decimal? MinLevel { get; set; }

	[Column(TypeName = "decimal(4,2)")]
	public decimal? MaxLevel { get; set; }

	[Column(TypeName = "decimal(18,2)")]
	public decimal PricePerPlayer { get; set; } // paid at the venue

	public required Guid SportId { get; set; }
	public SportEntity Sport { get; set; } = null!;

	public Guid? VenueId { get; set; }
	public VenueEntity? Venue { get; set; }

	public ICollection<UserEntity> Users { get; set; } = [];
}

public sealed class OpenMatchJson : BaseJson {
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string? Place { get; set; } // when it isn't at a listed venue
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public Guid? BookingId { get; set; }
	public List<Guid> PendingUserIds { get; set; } = []; // asked to join a private game
	public List<Guid> InvitedUserIds { get; set; } = []; // may join without approval
	public List<Guid> TeamA { get; set; } = [];
	public List<Guid> TeamB { get; set; } = [];
	public List<MatchSetScore> Sets { get; set; } = [];
	public bool Reminded { get; set; }
}
