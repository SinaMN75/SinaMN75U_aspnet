namespace SinaMN75U.Data.Params.User;

public class GetMobileVerificationCodeForLoginParams {
	public required string PhoneNumber { get; set; }
	public required List<TagUser> Tags { get; set; }
}