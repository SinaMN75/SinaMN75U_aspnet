namespace SinaMN75U.Data.Params;

public sealed class SimCardCreateParams : BaseCreateParams<TagSimCard> {
	public required string Number { get; set; }
	public string? Serial { get; set; }
	public required Guid UserId { get; set; }
}

public sealed class SimCardUpdateParams : BaseUpdateParams<TagSimCard> {
	public string? Number { get; set; }
	public string? Serial { get; set; }
	public Guid UserId { get; set; }
}

public sealed class SimCardReadParams : BaseReadParams<TagSimCard> {
	public SimCardSelectorArgs SelectorArgs { get; set; } = new();
}