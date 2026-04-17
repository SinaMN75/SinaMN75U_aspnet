namespace SinaMN75U.Data.Entities;

[Table("Addresses")]
public sealed class AddressEntity : BaseEntity<TagAddress, AddressJson> {
	[MaxLength(20)]
	public string? ZipCode { get; set; }
}

public sealed class AddressJson: BaseJsonData {
	public string? Title { get; set; }
	public string? Province { get; set; }
	public string? Township { get; set; }
	public string? Street { get; set; }
	public string? Street2 { get; set; }
	public string? LocalityName { get; set; }
	public string? HouseNumber { get; set; }
	public string? Floor { get; set; }
	public string? Description { get; set; }
	public string? BuildingName { get; set; }
	public string? LocalityType { get; set; }
	public string? SideFloor { get; set; }
	public string? SubLocality { get; set; }
	public string? Village { get; set; }
}