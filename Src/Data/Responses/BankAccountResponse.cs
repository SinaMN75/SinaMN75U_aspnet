namespace SinaMN75U.Data.Responses;

public sealed class BankAccountResponse : BaseResponse<TagBankAccount, GeneralJsonData> {
	public string? CardNumber { get; set; }
	public string? AccountNumber { get; set; }
	public string? IBanNumber { get; set; }
	public string? BankName { get; set; }
	public string? OwnerName { get; set; }

	public Guid? UserId { get; set; }
	public UserResponse? User { get; set; }
}