namespace SinaMN75U.Data.Params.User;

public class RegisterParams: BaseParams {
	public required string UserName { get; set; }
	public required string Email { get; set; }
	public required string PhoneNumber { get; set; }
	public required string Password { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public required List<TagUser> Tags { get; set; }
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
			.Must(username => username.MinMaxLenght(2, 40))
			.WithMessage(l.Get("UserNameInvalid"));
		
		RuleFor(x => x.PhoneNumber)
			.Must(username => username.MinMaxLenght(9, 15))
			.WithMessage(l.Get("UserNameInvalid"));
	}
}