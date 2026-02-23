namespace SinaMN75U.Data.Responses;

public class AddressResponse : BaseResponse<TagAddress, AddressJson> {
	public required string Title { get; set; }
}