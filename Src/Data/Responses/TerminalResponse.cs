namespace SinaMN75U.Data.Responses;

public class TerminalResponse : BaseResponse<TagTerminal, BaseJson> {
	public required string Serial { get; set; }
	public string? SimCardNumber { get; set; }
	public string? SimCardSerial { get; set; }
	public string? Imei { get; set; }
	public string? TerminalId { get; set; }
	public string? Agreement { get; set; }

	public Guid? MerchantId { get; set; }
	public MerchantResponse? Merchant { get; set; }
}

public class TerminalSupportPasswordResponse {
	public string? Password { get; set; }
}