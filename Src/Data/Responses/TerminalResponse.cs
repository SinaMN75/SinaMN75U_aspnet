namespace SinaMN75U.Data.Responses;

public class TerminalResponse : BaseResponse<TagTerminal, BaseJsonData> {
	public required string Serial { get; set; }
	public string? SimCardNumber { get; set; }
	public string? SimCardSerial { get; set; }
	public string? Imei { get; set; }
	public string? MerchantId { get; set; }
	public string? TerminalId { get; set; }
	
	public Guid? AddressId { get; set; }
	public AddressResponse? Address { get; set; }
}