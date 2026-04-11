namespace SinaMN75U.Data.Responses;

public class TerminalResponse : BaseResponse<TagTerminal, GeneralJsonData> {
	public string? SimCardNumber { get; set; }
	public string? SimCardSerial { get; set; }
	public string? Imei { get; set; }

	public UserResponse? Creator { get; set; }
	public required Guid CreatorId { get; set; }
}