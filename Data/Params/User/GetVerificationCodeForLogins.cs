namespace SinaMN75U.Data.Params.User;

public class GetMobileVerificationCodeForLoginParams : BaseParams {
	public required string PhoneNumber { get; set; }
}

public class GetMobileVerificationCodeForLoginParamsValidator : AbstractValidator<GetMobileVerificationCodeForLoginParams> {
	public GetMobileVerificationCodeForLoginParamsValidator(ILocalizationService l) => RuleFor(x => x.PhoneNumber)
		.Must(username => username.MinMaxLenght(9, 15))
		.WithMessage(l.Get("PhoneNumberInvalid"));
}