namespace SinaMN75U.Data.Params;

public class RefreshTokenParams : BaseParams {
	public required string RefreshToken { get; set; }
}

public class GetMobileVerificationCodeForLoginParams : BaseParams, IValidatable<GetMobileVerificationCodeForLoginParams> {
	public required string PhoneNumber { get; set; }

	public AbstractValidator<GetMobileVerificationCodeForLoginParams> GetValidator(ILocalizationService l) {
		return new InlineValidator<GetMobileVerificationCodeForLoginParams> {
			v => v.RuleFor(x => x.PhoneNumber)
				.Must(username => username.MinMaxLength(9, 15))
				.WithMessage(l.Get("PhoneNumberInvalid"))
		};
	}
}

public class LoginWithEmailPasswordParams : BaseParams, IValidatable<LoginWithEmailPasswordParams> {
	public required string Email { get; set; }
	public required string Password { get; set; }

	public AbstractValidator<LoginWithEmailPasswordParams> GetValidator(ILocalizationService l) {
		return new InlineValidator<LoginWithEmailPasswordParams> {
			v => v.RuleFor(x => x.Email)
				.NotEmpty().WithMessage(l.Get("EmailRequired"))
				.EmailAddress().WithMessage(l.Get("EmailInvalid")),

			v => v.RuleFor(x => x.Password)
				.NotEmpty().WithMessage(l.Get("PasswordRequired"))
				.MinimumLength(6).WithMessage(l.Get("PasswordMinLength"))
				.MaximumLength(100).WithMessage(l.Get("PasswordMaxLength"))
		};
	}
}

public class RegisterParams : BaseParams, IValidatable<RegisterParams> {
	public required string UserName { get; set; }
	public required string Email { get; set; }
	public required string PhoneNumber { get; set; }
	public required string Password { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public required List<TagUser> Tags { get; set; }

	public AbstractValidator<RegisterParams> GetValidator(ILocalizationService l) {
		return new InlineValidator<RegisterParams> {
			v => v.RuleFor(x => x.Email)
				.NotEmpty().WithMessage(l.Get("EmailRequired"))
				.EmailAddress().WithMessage(l.Get("EmailInvalid")),

			v => v.RuleFor(x => x.Password)
				.NotEmpty().WithMessage(l.Get("PasswordRequired"))
				.MinimumLength(6).WithMessage(l.Get("PasswordMinLength"))
				.MaximumLength(100).WithMessage(l.Get("PasswordMaxLength")),

			v => v.RuleFor(x => x.UserName)
				.Must(username => username.MinMaxLength(2, 40))
				.WithMessage(l.Get("UserNameInvalid")),

			v => v.RuleFor(x => x.PhoneNumber)
				.Must(username => username.MinMaxLength(9, 15))
				.WithMessage(l.Get("PhoneNumberInvalid"))
		};
	}
}

public class VerifyMobileForLoginParams : BaseParams {
	public required string PhoneNumber { get; set; }
	public required string Otp { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
}

public class RegisterParamsValidator : AbstractValidator<RegisterParams> {
	public RegisterParamsValidator(ILocalizationService l) {
		RuleFor(x => x.Email)
			.NotEmpty().WithMessage(l.Get("EmailRequired"))
			.EmailAddress().WithMessage(l.Get("EmailInvalid"));

		RuleFor(x => x.Password)
			.NotEmpty().WithMessage(l.Get("PasswordRequired"))
			.MinimumLength(6).WithMessage(l.Get("PasswordMinLength"))
			.MaximumLength(100).WithMessage(l.Get("PasswordMaxLength"));

		RuleFor(x => x.UserName)
			.Must(username => username.MinMaxLength(2, 40))
			.WithMessage(l.Get("UserNameInvalid"));

		RuleFor(x => x.PhoneNumber)
			.Must(username => username.MinMaxLength(9, 15))
			.WithMessage(l.Get("PhoneNumberInvalid"));
	}
}
