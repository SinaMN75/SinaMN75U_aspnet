namespace SinaMN75U.Data.Responses;

public sealed class BankAccountResponse : BaseResponse<TagBankAccount, BaseJsonData> {
	public string? CardNumber { get; set; }
	public string? AccountNumber { get; set; }
	public string? IBanNumber { get; set; }
	public string? BankName { get; set; }
	public string? OwnerName { get; set; }
}