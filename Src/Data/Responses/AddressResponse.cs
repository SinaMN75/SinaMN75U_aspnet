namespace SinaMN75U.Data.Responses;

public sealed class AddressResponse : BaseResponse<TagAddress, AddressJson> {
	public string? ZipCode { get; set; }
}