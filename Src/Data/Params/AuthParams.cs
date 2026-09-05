namespace SinaMN75U.Data.Params;

public sealed class RefreshTokenParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public string RefreshToken { get; set; } = null!;
}

public sealed class GetMobileVerificationCodeForLoginParams : BaseParams {
	[UValidationRequired("phoneNumberIsRequired"), UValidationStringLength(9, 15, "phoneNumberIsNotValid")]
	public string PhoneNumber { get; set; } = null!;
}

public sealed class LoginParams : BaseParams {
	public string? UserName { get; set; }
	public string? Email { get; set; }

	[UValidationRequired("pleaseEnterAPassword"), UValidationStringLength(4, 100, "passwordMustBeAtLeast6Characters")]
	public string Password { get; set; } = null!;
}

public sealed class RegisterParams : BaseParams {
	[UValidationRequired("userNameIsRequired"), UValidationStringLength(2, 100, "userNameIsNotValid")]
	public string UserName { get; set; } = null!;

	[UValidationRequired("pleaseEnterAPassword"), UValidationStringLength(4, 100, "passwordMustBeAtLeast6Characters")]
	public string Password { get; set; } = null!;

	[UValidationMinCollectionLength(1, "tagsIsRequired")]
	public ICollection<TagUser> Tags { get; set; } = null!;
	
	public string? Email { get; set; }
	public string? PhoneNumber { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? NationalCode { get; set; }
}

public sealed class VerifyMobileForLoginParams : BaseParams {
	[UValidationRequired("phoneNumberIsRequired"), UValidationStringLength(9, 15, "phoneNumberIsNotValid")]
	public string PhoneNumber { get; set; } = null!;

	[UValidationRequired("otpIsRequired")]
	public string Otp { get; set; } = null!;
}

public sealed class AuthCompleteProfileParams : BaseParams {
	[UValidationRequired("firstNameIsRequired"), UValidationStringLength(2, 40, "firstNameIsInvalid")]
	public string FirstName { get; set; } = null!;

	[UValidationRequired("lastNameIsRequired"), UValidationStringLength(2, 40, "lastNameIsInvalid")]
	public string LastName { get; set; } = null!;

	[UValidationRequired("nationalCodeIsRequired"), UValidationStringLength(10, 10, "nationalCodeIsInvalid")]
	public string NationalCode { get; set; } = null!;
}