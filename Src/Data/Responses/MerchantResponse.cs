namespace SinaMN75U.Data.Responses;

public class MerchantResponse : BaseResponse<TagMerchant, MerchantJson> {
	public required string ZipCode { get; set; }
	public required string CityCode { get; set; }
	public required string PhoneNumber { get; set; }
	public required string Title { get; set; }
	public required string Landline { get; set; }
	public required string NationalCode { get; set; }
	public required string BankAccountId { get; set; }
	public required string Mcc { get; set; }
	public required string MerchantId { get; set; }
	public required string InsId { get; set; }
	
	public required Guid UserId { get; set; }
	public UserResponse? User { get; set; }
	
	public ICollection<TerminalResponse>? Terminals { get; set; }
	public ICollection<AgreementResponse>? Agreements { get; set; }
}