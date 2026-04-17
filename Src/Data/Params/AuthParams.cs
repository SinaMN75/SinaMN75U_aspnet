namespace SinaMN75U.Data.Params;

public sealed class RefreshTokenParams : BaseParams {
	[UValidationRequired("IdRequired")]
	public string RefreshToken { get; set; } = null!;
}

public sealed class GetMobileVerificationCodeForLoginParams : BaseParams {
	[UValidationRequired("PhoneNumberRequired"), UValidationStringLength(9, 12, "PhoneNumberNotValid")]
	public string PhoneNumber { get; set; } = null!;
}

public sealed class LoginParams : BaseParams {
	public string? UserName { get; set; }
	public string? Email { get; set; }

	[UValidationRequired("PasswordRequired"), UValidationStringLength(4, 100, "PasswordMinLength")]
	public string Password { get; set; } = null!;
}

public sealed class RegisterParams : BaseParams {
	[UValidationRequired("UserNameRequired"), UValidationStringLength(2, 100, "UserNameMinLenght")]
	public string UserName { get; set; } = null!;

	[UValidationRequired("PasswordRequired"), UValidationStringLength(4, 100, "PasswordMinLength")]
	public string Password { get; set; } = null!;

	[UValidationMinCollectionLength(1, "TagsRequired")]
	public List<TagUser> Tags { get; set; } = null!;
	
	public string? Email { get; set; }
	public string? PhoneNumber { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
}

public sealed class VerifyMobileForLoginParams : BaseParams {
	[UValidationRequired("PhoneNumberRequired"), UValidationStringLength(9, 12, "PhoneNumberNotValid")]
	public string PhoneNumber { get; set; } = null!;

	[UValidationRequired("OtpRequired")]
	public string Otp { get; set; } = null!;
}

public sealed class AuthCompleteProfileParams : BaseParams {
	[UValidationRequired("FirstNameRequired"), UValidationStringLength(2, 40, "FirstNameInvalid")]
	public string FirstName { get; set; } = null!;

	[UValidationRequired("LastNameRequired"), UValidationStringLength(2, 40, "LastNameInvalid")]
	public string LastName { get; set; } = null!;

	[UValidationRequired("NationalCodeRequired"), UValidationStringLength(10, 10, "NationalCodeInvalid")]
	public string NationalCode { get; set; } = null!;
}