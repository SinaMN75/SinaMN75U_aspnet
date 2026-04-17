namespace SinaMN75U.Data.Params;

public sealed class SimCardCreateParams : BaseCreateParams<TagSimCard> {
	[UValidationRequired("NumberRequired")]
	public string Number { get; set; } = null!;

	public string? Serial { get; set; }
}

public sealed class SimCardUpdateParams : BaseUpdateParams<TagSimCard> {
	public string? Number { get; set; }
	public string? Serial { get; set; }
}

public sealed class SimCardReadParams : BaseReadParams<TagSimCard> {
	public SimCardSelectorArgs SelectorArgs { get; set; } = new();
}