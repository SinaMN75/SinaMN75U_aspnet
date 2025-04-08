namespace SinaMN75U.Data.Params;

public class RefreshTokenParams : BaseParams {
	public required string RefreshToken { get; set; }
}

public class GetMobileVerificationCodeForLoginParams : BaseParams {
	[LocalizedEmail("AuthorizationRequired")]
	public required string PhoneNumber { get; set; }
}

public class LoginWithEmailPasswordParams : BaseParams {
	public required string Email { get; set; }
	public required string Password { get; set; }
}

public class RegisterParams : BaseParams {
	public required string UserName { get; set; }
	public required string Email { get; set; }
	public required string PhoneNumber { get; set; }
	public required string Password { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public required List<TagUser> Tags { get; set; }
}

public class VerifyMobileForLoginParams : BaseParams {
	public required string PhoneNumber { get; set; }
	public required string Otp { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
}