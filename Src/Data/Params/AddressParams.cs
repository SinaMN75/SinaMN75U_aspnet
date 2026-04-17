namespace SinaMN75U.Data.Params;

public sealed class AddressCreateParams : BaseCreateParams<TagAddress> {
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string? Province { get; set; }
	public string? Township { get; set; }
	public string? Street { get; set; }
	public string? Street2 { get; set; }
	public string? LocalityName { get; set; }
	public string? HouseNumber { get; set; }
	public string? Floor { get; set; }
	public string? ZipCode { get; set; }
	public string? BuildingName { get; set; }
	public string? LocalityType { get; set; }
	public string? SideFloor { get; set; }
	public string? SubLocality { get; set; }
	public string? Village { get; set; }
}

public sealed class AddressUpdateParams : BaseUpdateParams<TagAddress> {
	public string? Title { get; set; }
	public string? Description { get; set; }
	public string? Province { get; set; }
	public string? Township { get; set; }
	public string? Street { get; set; }
	public string? Street2 { get; set; }
	public string? LocalityName { get; set; }
	public string? HouseNumber { get; set; }
	public string? Floor { get; set; }
	public string? ZipCode { get; set; }
	public string? BuildingName { get; set; }
	public string? LocalityType { get; set; }
	public string? SideFloor { get; set; }
	public string? SubLocality { get; set; }
	public string? Village { get; set; }
}

public sealed class AddressReadParams : BaseReadParams<TagAddress> {
	public string? ZipCode { get; set; }
	public AddressSelectorArgs SelectorArgs { get; set; } = new();
}