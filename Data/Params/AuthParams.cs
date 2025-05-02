namespace SinaMN75U.Data.Params;

public class RefreshTokenParams : BaseParams {
	public required string RefreshToken { get; set; }
}

public class GetMobileVerificationCodeForLoginParams : BaseParams {
	[URequired("PhoneNumberRequired")]
	[UStringLength(9, 12, "PhoneNumberNotValid")]
	public required string PhoneNumber { get; set; }
}

public class LoginWithEmailPasswordParams : BaseParams {
	[URequired("EmailRequired")]
	[UEmail("EmailInvalid")]
	public required string Email { get; set; }

	[URequired("PasswordRequired")]
	[UStringLength(4, 100, "PasswordMinLength")]
	public required string Password { get; set; }
}

public class LoginWithUserNamePasswordParams : BaseParams {
	[URequired("UserNameRequired")]
	[UStringLength(2, 100, "UserNameMinLenght")]
	public required string UserName { get; set; }
	
	[URequired("PasswordRequired")]
	[UStringLength(4, 100, "PasswordMinLength")]
	public required string Password { get; set; }
}

public class RegisterParams : BaseParams {
	[URequired("UserNameRequired")]
	[UStringLength(2, 100, "UserNameMinLenght")]
	public required string UserName { get; set; }
	
	public string? Email { get; set; }
	
	public string? PhoneNumber { get; set; }

	[URequired("PasswordRequired")]
	[UStringLength(4, 100, "PasswordMinLength")]
	public required string Password { get; set; }

	public string? FirstName { get; set; }
	public string? LastName { get; set; }

	[UMinCollectionLength(1, "TagsRequired")]
	public required List<TagUser> Tags { get; set; }
}

public class VerifyMobileForLoginParams : BaseParams {
	[URequired("PhoneNumberRequired")]
	[UStringLength(9, 12, "PhoneNumberNotValid")]
	public required string PhoneNumber { get; set; }

	[URequired("OtpRequired")]
	public required string Otp { get; set; }

	public string? FirstName { get; set; }
	public string? LastName { get; set; }
}