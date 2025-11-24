namespace SinaMN75U.Data.Params;

public sealed class RefreshTokenParams : BaseParams {
	public required string RefreshToken { get; set; }
}

public sealed class GetMobileVerificationCodeForLoginParams : BaseParams {
	[UValidationRequired("PhoneNumberRequired")]
	[UValidationStringLength(9, 12, "PhoneNumberNotValid")]
	public required string PhoneNumber { get; set; }
}

public sealed class LoginWithEmailPasswordParams : BaseParams {
	[UValidationRequired("EmailRequired")]
	[UValidationEmail("EmailInvalid")]
	public required string Email { get; set; }

	[UValidationRequired("PasswordRequired")]
	[UValidationStringLength(4, 100, "PasswordMinLength")]
	public required string Password { get; set; }
}

public sealed class LoginWithUserNamePasswordParams : BaseParams {
	[UValidationRequired("UserNameRequired")]
	[UValidationStringLength(2, 100, "UserNameMinLenght")]
	public required string UserName { get; set; }

	[UValidationRequired("PasswordRequired")]
	[UValidationStringLength(4, 100, "PasswordMinLength")]
	public required string Password { get; set; }
}

public sealed class RegisterParams : BaseParams {
	[UValidationRequired("UserNameRequired")]
	[UValidationStringLength(2, 100, "UserNameMinLenght")]
	public required string UserName { get; set; }

	public string? Email { get; set; }

	public string? PhoneNumber { get; set; }

	[UValidationRequired("PasswordRequired")]
	[UValidationStringLength(4, 100, "PasswordMinLength")]
	public required string Password { get; set; }

	public string? FirstName { get; set; }
	public string? LastName { get; set; }

	[UValidationMinCollectionLength(1, "TagsRequired")]
	public required List<TagUser> Tags { get; set; }
}

public sealed class VerifyMobileForLoginParams : BaseParams {
	[UValidationRequired("PhoneNumberRequired")]
	[UValidationStringLength(9, 12, "PhoneNumberNotValid")]
	public required string PhoneNumber { get; set; }

	[UValidationRequired("OtpRequired")]
	public required string Otp { get; set; }

	public string? FirstName { get; set; }
	public string? LastName { get; set; }
}