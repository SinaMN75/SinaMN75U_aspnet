namespace SinaMN75U.Data.Responses;

public sealed class AddressResponse : BaseResponse<TagAddress, AddressJson> {
	public required string Title { get; set; }
	public string? ZipCode { get; set; }
	
	public UserResponse? Creator { get; set; }
	public required Guid CreatorId { get; set; }
}