namespace SinaMN75U.Data.Params;

public sealed class SportCreateParams : BaseCreateParams<TagSport> {
	/// <summary>A ULocalizedConstants key, e.g. "padel".</summary>
	[UValidationRequired("titleIsRequired"), UValidationStringLength(2, 100, "titleIsRequired")]
	public string Title { get; set; } = null!;

	public int Order { get; set; }
	public decimal MinLevel { get; set; } = 1;
	public decimal MaxLevel { get; set; } = 7;
	public string? Icon { get; set; }
}

public sealed class SportUpdateParams : BaseUpdateParams<TagSport> {
	public string? Title { get; set; }
	public int? Order { get; set; }
	public decimal? MinLevel { get; set; }
	public decimal? MaxLevel { get; set; }
	public string? Icon { get; set; }
}

public sealed class SportReadParams : BaseReadParams<TagSport> {
	public SportSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class PlayerSportProfileCreateParams : BaseCreateParams<TagPlayerSportProfile> {
	[UValidationRequired("sportIsRequired")]
	public Guid SportId { get; set; }

	public decimal Level { get; set; }

	/// <summary>Admins only; everybody else creates their own profile.</summary>
	public Guid? UserId { get; set; }
}

public sealed class PlayerSportProfileUpdateParams : BaseUpdateParams<TagPlayerSportProfile> {
	public decimal? Level { get; set; }
}

public sealed class PlayerSportProfileReadParams : BaseReadParams<TagPlayerSportProfile> {
	public Guid? UserId { get; set; }
	public Guid? SportId { get; set; }
	public PlayerSportProfileSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class TournamentCreateParams : BaseCreateParams<TagTournament> {
	[UValidationRequired("titleIsRequired"), UValidationStringLength(2, 200, "titleIsRequired")]
	public string Title { get; set; } = null!;

	[UValidationRequired("sportIsRequired")]
	public Guid SportId { get; set; }

	[UValidationRequired("startDateIsRequired")]
	public DateTime StartDate { get; set; }

	public int Capacity { get; set; }
	public decimal EntryFee { get; set; }
	public decimal? MinLevel { get; set; }
	public decimal? MaxLevel { get; set; }
	public string? Description { get; set; }
	public string? Prize { get; set; }
	public string? Venue { get; set; }
	public string? Address { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public int? PointsForWin { get; set; }
	public int? PointsForDraw { get; set; }
	public int? PointsForLoss { get; set; }
	public int? GroupCount { get; set; }
	public int? AdvancePerGroup { get; set; }
	public bool? ThirdPlaceMatch { get; set; }
	public int? Rounds { get; set; }
	public int? PointsPerMatch { get; set; }
	public int? BoxSize { get; set; }
	public int? SetsToWin { get; set; }
	public int? RaceTo { get; set; }
	public bool? SuperTiebreak { get; set; }
	public bool? Unrated { get; set; }
}

public sealed class TournamentUpdateParams : BaseUpdateParams<TagTournament> {
	public string? Title { get; set; }
	public DateTime? StartDate { get; set; }
	public int? Capacity { get; set; }
	public decimal? EntryFee { get; set; }
	public decimal? MinLevel { get; set; }
	public decimal? MaxLevel { get; set; }
	public string? Description { get; set; }
	public string? Prize { get; set; }
	public string? Venue { get; set; }
	public string? Address { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public int? PointsForWin { get; set; }
	public int? PointsForDraw { get; set; }
	public int? PointsForLoss { get; set; }
	public int? GroupCount { get; set; }
	public int? AdvancePerGroup { get; set; }
	public bool? ThirdPlaceMatch { get; set; }
	public int? Rounds { get; set; }
	public int? PointsPerMatch { get; set; }
	public int? BoxSize { get; set; }
	public int? SetsToWin { get; set; }
	public int? RaceTo { get; set; }
	public bool? SuperTiebreak { get; set; }
	public bool? Unrated { get; set; }
}

public sealed class TournamentReadParams : BaseReadParams<TagTournament> {
	public Guid? SportId { get; set; }

	/// <summary>Only tournaments this user has an entry in.</summary>
	public Guid? UserId { get; set; }

	public string? Title { get; set; }
	public TournamentSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class TournamentRegisterParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid TournamentId { get; set; }

	public string? Title { get; set; }

	/// <summary>Doubles only: the partner's account email.</summary>
	public string? PartnerEmail { get; set; }

	/// <summary>Pays the entry fee from the wallet now; otherwise it is paid to the organizer at the venue.</summary>
	public bool PayFromWallet { get; set; }
}

public sealed class TournamentEntryUpdateParams : BaseUpdateParams<TagTournamentEntry> {
	public string? Title { get; set; }
	public int? Seed { get; set; }
	public int? GroupNumber { get; set; }
}

public sealed class TournamentMatchUpdateParams : BaseUpdateParams<TagTournamentMatch> {
	/// <summary>The result; an empty list clears it.</summary>
	public List<MatchSetScore>? Sets { get; set; }

	public DateTime? ScheduledAt { get; set; }
	public string? Court { get; set; }
}

public sealed class TournamentMatchReadParams : BaseReadParams<TagTournamentMatch> {
	/// <summary>Matches this user plays in.</summary>
	public Guid? UserId { get; set; }

	public Guid? TournamentId { get; set; }
	public Guid? SportId { get; set; }

	/// <summary>true: not played yet, soonest first; false: played, latest first.</summary>
	public bool? Upcoming { get; set; }

	public TournamentMatchSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class PlayerRatingHistoryReadParams : BaseReadParams<TagPlayerRatingHistory> {
	public Guid? UserId { get; set; }
	public Guid? SportId { get; set; }
}

public sealed class PlayerAchievementReadParams : BaseReadParams<TagPlayerAchievement> {
	public Guid? UserId { get; set; }
	public Guid? SportId { get; set; }
	public Guid? TournamentId { get; set; }
	public PlayerAchievementSelectorArgs SelectorArgs { get; set; } = new();
}

/// <summary>Only the Hidden tag changes: a player shows or hides a trophy.</summary>
public sealed class PlayerAchievementUpdateParams : BaseUpdateParams<TagPlayerAchievement>;

public sealed class LeaderboardParams : BaseParams {
	[UValidationRequired("sportIsRequired")]
	public Guid SportId { get; set; }

	/// <summary>Ranking points from tournament placements; otherwise the level.</summary>
	public bool ByPoints { get; set; }

	/// <summary>Points of this year only (the season); empty = all time.</summary>
	public int? Year { get; set; }

	public string? Country { get; set; }
	public string? City { get; set; }
	public int PageSize { get; set; } = 50;
	public int PageNumber { get; set; } = 1;
}

public sealed class PlayerStatsParams : BaseParams {
	/// <summary>Empty = the signed-in user.</summary>
	public Guid? UserId { get; set; }

	public Guid? SportId { get; set; }
}

public sealed class OpenMatchCreateParams : BaseCreateParams<TagOpenMatch> {
	[UValidationRequired("sportIsRequired")]
	public Guid SportId { get; set; }

	[UValidationRequired("startDateIsRequired")]
	public DateTime StartAt { get; set; }

	public int DurationMinutes { get; set; } = 90;
	public int Capacity { get; set; } = 4;
	public decimal? MinLevel { get; set; }
	public decimal? MaxLevel { get; set; }
	public decimal PricePerPlayer { get; set; }
	public Guid? VenueId { get; set; }
	public Guid? BookingId { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string? Place { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }

	/// <summary>They may join without approval (a challenge invites its opponents).</summary>
	public List<Guid> InvitedUserIds { get; set; } = [];
}

public sealed class OpenMatchUpdateParams : BaseUpdateParams<TagOpenMatch> {
	public DateTime? StartAt { get; set; }
	public int? DurationMinutes { get; set; }
	public int? Capacity { get; set; }
	public decimal? MinLevel { get; set; }
	public decimal? MaxLevel { get; set; }
	public decimal? PricePerPlayer { get; set; }
	public Guid? VenueId { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string? Place { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }

	/// <summary>Players who asked to join a private game.</summary>
	public List<Guid>? ApproveUserIds { get; set; }

	/// <summary>Players or requests the organizer removes.</summary>
	public List<Guid>? RemoveUserIds { get; set; }

	public List<Guid>? InviteUserIds { get; set; }
}

public sealed class OpenMatchReadParams : BaseReadParams<TagOpenMatch> {
	public Guid? SportId { get; set; }

	/// <summary>Games this user plays in or organizes.</summary>
	public Guid? UserId { get; set; }

	public Guid? VenueId { get; set; }

	/// <summary>true: not started yet, soonest first.</summary>
	public bool? Upcoming { get; set; }

	/// <summary>Games whose level range fits the signed-in player's level.</summary>
	public bool ForMyLevel { get; set; }

	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public double? RadiusKm { get; set; }
	public OpenMatchSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class OpenMatchResultParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid Id { get; set; }

	public List<Guid> TeamA { get; set; } = [];
	public List<Guid> TeamB { get; set; } = [];

	/// <summary>An empty list clears the result.</summary>
	public List<MatchSetScore> Sets { get; set; } = [];
}
