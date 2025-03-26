namespace SinaMN75U.Data.Params;

using FluentValidation.Results;

public class ValidationFilter<T> : IEndpointFilter {
	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next) {
		IValidator<T>? validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
		if (validator is null) return await next(context);

		T? model = context.Arguments.OfType<T>().FirstOrDefault();
		if (model is null) return new UResponse(USC.BadRequest);

		ValidationResult? validationResult = await validator.ValidateAsync(model);
		if (validationResult.IsValid) return await next(context);
		ValidationFailure? firstError = validationResult.Errors.FirstOrDefault();
		if (firstError != null) return new UResponse(USC.BadRequest, firstError.ErrorMessage);

		return await next(context);
	}
}