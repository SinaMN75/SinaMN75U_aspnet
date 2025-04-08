namespace SinaMN75U.Data.Params;

using FluentValidation.Results;

public interface IValidatable<T> {
	AbstractValidator<T> GetValidator(ILocalizationService localization);
}

public class ValidationFilter<T> : IEndpointFilter {
	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next) {
		T? model = context.Arguments.OfType<T>().FirstOrDefault();
		if (model is null) return new UResponse(USC.BadRequest);

		if (model is not IValidatable<T> validatableModel) return await next(context);
		ILocalizationService localization = context.HttpContext.RequestServices.GetRequiredService<ILocalizationService>();
		AbstractValidator<T> validator = validatableModel.GetValidator(localization);

		ValidationResult? validationResult = await validator.ValidateAsync(model);
		if (validationResult.IsValid) return await next(context);
		ValidationFailure? firstError = validationResult.Errors.FirstOrDefault();
		return firstError != null ? new UResponse(USC.BadRequest, firstError.ErrorMessage) : new UResponse(USC.BadRequest);
	}
}