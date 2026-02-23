namespace SinaMN75U.Data.Entities;

public class AddressEntity : BaseEntity<TagAddress, AddressJson> {
	public required string Title { get; set; }
	public string? ZipCode { get; set; }

	public AddressResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		DeletedAt = DeletedAt,
		JsonData = JsonData,
		Tags = Tags,
		Title = Title,
		ZipCode = ZipCode
	};
}

public class AddressJson {
	public string? Province { get; set; }
	public string? Township { get; set; }
	public string? Street { get; set; }
	public string? Street2 { get; set; }
	public string? LocalityName { get; set; }
	public string? HouseNumber { get; set; }
	public string? Floor { get; set; }
	public string? Description { get; set; }
}