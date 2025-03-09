using SinaMN75U.InnerServices;

namespace SinaMN75U.Data.Params.User;

public class LoginWithEmailPasswordParams : BaseParam {
	[Required]
	[MinLength(5)]
	[MaxLength(100)]
	[EmailAddress]
	public required string Email { get; set; }

	[Required]
	[MinLength(6)]
	[MaxLength(100)]
	public required string Password { get; set; }
}

public class LoginParamsValidator : AbstractValidator<LoginWithEmailPasswordParams> {
	public LoginParamsValidator(ILocalizationService l) {
		RuleFor(x => x.Email)
			.NotEmpty().WithMessage(l.Get("EmailRequired"))
			.EmailAddress().WithMessage(l.Get("EmailInvalid"));

		RuleFor(x => x.Password)
			.NotEmpty().WithMessage(l.Get("PasswordRequired"))
			.MinimumLength(6).WithMessage(l.Get("PasswordMinLength"))
			.MaximumLength(100).WithMessage(l.Get("PasswordMaxLength"));
	}
}