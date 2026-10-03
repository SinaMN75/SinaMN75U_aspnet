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
