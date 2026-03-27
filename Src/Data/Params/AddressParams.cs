namespace SinaMN75U.Data.Params;

public sealed class AddressCreateParams : BaseCreateParams<TagAddress> {
	[UValidationRequired("TitleRequired")]
	public required string Title { get; set; }

	public string? Province { get; set; }
	public string? Township { get; set; }
	public string? Street { get; set; }
	public string? Street2 { get; set; }
	public string? LocalityName { get; set; }
	public string? HouseNumber { get; set; }
	public string? Floor { get; set; }
	public string? ZipCode { get; set; }
	public string? Description { get; set; }
}

public sealed class AddressUpdateParams : BaseUpdateParams<TagAddress> {
	public string? Title { get; set; }
	public string? Province { get; set; }
	public string? Township { get; set; }
	public string? Street { get; set; }
	public string? Street2 { get; set; }
	public string? LocalityName { get; set; }
	public string? HouseNumber { get; set; }
	public string? Floor { get; set; }
	public string? ZipCode { get; set; }
	public string? Description { get; set; }
}

public sealed class AddressReadParams : BaseReadParams<TagAddress> {
	public AddressSelectorArgs SelectorArgs { get; set; } = new();
}