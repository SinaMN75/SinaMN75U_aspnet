namespace SinaMN75U.Data.Responses;

public class MerchantResponse : BaseResponse<TagMerchant, BaseJsonData> {

	public required string ZipCode { get; set; }
	
	public required Guid UserId { get; set; }
	public UserResponse? User { get; set; }
	
	public ICollection<TerminalResponse>? Terminals { get; set; }
	public ICollection<AgreementResponse>? Agreements { get; set; }
}