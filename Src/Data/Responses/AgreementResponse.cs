namespace SinaMN75U.Data.Responses;

public sealed class AgreementResponse : BaseResponse<TagAgreement, GeneralJsonData> {
	public required string Agreement { get; set; }
	
	public TerminalResponse? Terminal { get; set; }
	public Guid? TerminalId { get; set; }
}