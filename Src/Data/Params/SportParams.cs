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
