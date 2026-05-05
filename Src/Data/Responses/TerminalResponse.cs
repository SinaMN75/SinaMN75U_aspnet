namespace SinaMN75U.Data.Responses;

public class TerminalResponse : BaseResponse<TagTerminal, BaseJsonData> {
	public required string Serial { get; set; }
	public string? SimCardNumber { get; set; }
	public string? SimCardSerial { get; set; }
	public string? Imei { get; set; }

	public Guid? MerchantId { get; set; }
	public MerchantResponse? Merchant { get; set; }
}