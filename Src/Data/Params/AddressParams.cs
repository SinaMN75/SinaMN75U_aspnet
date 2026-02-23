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

	public AddressEntity MapToEntity() => new() {
		Id = Id ?? Guid.CreateVersion7(),
		Title = Title,
		Tags = Tags,
		JsonData = new AddressJson {
			Province = Province,
			Township = Township,
			Street = Street,
			Street2 = Street2,
			LocalityName = LocalityName,
			HouseNumber = HouseNumber,
			Floor = Floor,
			ZipCode = ZipCode,
			Description = Description
		}
	};
}

public sealed class AddressUpdateParams : BaseUpdateParams<TagAddress> {
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
		if (Province != null) e.JsonData.Province = Province;
		if (Township != null) e.JsonData.Township = Township;
		if (Street != null) e.JsonData.Street = Street;
		if (Street2 != null) e.JsonData.Street2 = Street2;
		if (LocalityName != null) e.JsonData.LocalityName = LocalityName;
		if (HouseNumber != null) e.JsonData.HouseNumber = HouseNumber;
		if (Floor != null) e.JsonData.Floor = Floor;
		if (ZipCode != null) e.JsonData.ZipCode = ZipCode;
		if (Description != null) e.JsonData.Description = Description;
		if (Tags != null) e.Tags = Tags;
		return e;
	}
}

public sealed class AddressReadParams : BaseReadParams<TagAddress> {
	public bool OrderByOrder { get; set; }
	public bool OrderByOrderDesc { get; set; }
	public AddressSelectorArgs SelectorArgs { get; set; } = new();
}