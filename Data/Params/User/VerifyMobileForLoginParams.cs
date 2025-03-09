namespace SinaMN75U.Data.Params.User;

public class VerifyMobileForLoginParams {
	public required string PhoneNumber { get; set; }
	public required string Otp { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Instagram { get; set; }
	public string? FcmToken { get; set; }
}