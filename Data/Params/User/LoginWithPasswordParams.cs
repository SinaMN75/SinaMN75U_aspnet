namespace SinaMN75U.Data.Params.User;

public class LoginWithEmailPasswordParams: BaseParams {
	public required string Email { get; set; }
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