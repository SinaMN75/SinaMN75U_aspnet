namespace SinaMN75U.Data.Params;

public sealed class SimCardCreateParams : BaseCreateParams<TagSimOperator> {
	[UValidationRequired("NumberRequired")]
	public string Number { get; set; } = null!;

	public string? Serial { get; set; }
}

public sealed class SimCardUpdateParams : BaseUpdateParams<TagSimOperator> {
	public string? Number { get; set; }
	public string? Serial { get; set; }
}

public sealed class SimCardReadParams : BaseReadParams<TagSimOperator> {
	public SimCardSelectorArgs SelectorArgs { get; set; } = new();
}