namespace SinaMN75U.Data.ServiceParams;

public sealed class AddressCreateServiceParams : BaseCreateServiceParams<TagAddress> {
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
	public required Guid CreatorId { get; set; }
}

public sealed class AddressBulkCreateServiceParams {
	public required IEnumerable<AddressCreateServiceParams> List { get; set; }
}

public sealed class AddressUpdateServiceParams : BaseUpdateServiceParams<TagAddress> {
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

	public AddressEntity MapToEntity(AddressEntity e) {
		if (Title != null) e.Title = Title;
		if (ZipCode != null) e.ZipCode = ZipCode;
		if (Province != null) e.JsonData.Province = Province;
		if (Township != null) e.JsonData.Township = Township;
		if (Street != null) e.JsonData.Street = Street;
		if (Street2 != null) e.JsonData.Street2 = Street2;
		if (LocalityName != null) e.JsonData.LocalityName = LocalityName;
		if (HouseNumber != null) e.JsonData.HouseNumber = HouseNumber;
		if (Floor != null) e.JsonData.Floor = Floor;
		if (Description != null) e.JsonData.Description = Description;
		if (Tags != null) e.Tags = Tags;
		return e;
	}
}

public sealed class AddressReadServiceParams : BaseReadServiceParams<TagAddress> {
	public Guid? CreatorId { get; set; }
	public AddressSelectorArgs SelectorArgs { get; set; } = new();
}