namespace SinaMN75U.Data.ServiceResponses;

public class AddressServiceResponse : BaseServiceResponse<TagAddress, AddressJson> {
	public required string Title { get; set; }
	public string? ZipCode { get; set; }

	public UserServiceResponse? Creator { get; set; }
	public required Guid CreatorId { get; set; }

	public AddressResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		DeletedAt = DeletedAt,
		JsonData = JsonData,
		Tags = Tags,
		Title = Title,
		ZipCode = ZipCode,
		CreatorId = CreatorId
	};
}