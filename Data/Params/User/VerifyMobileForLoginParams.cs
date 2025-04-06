namespace SinaMN75U.Data.Params.User;

public class VerifyMobileForLoginParams : BaseParams {
	public required string PhoneNumber { get; set; }
	public required string Otp { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
}