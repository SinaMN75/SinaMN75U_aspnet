namespace SinaMN75U.Middlewares;

using System.Collections;

public class ValidationFilter : IEndpointFilter {
	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next) {
		foreach (object? argument in context.Arguments) {
			if (argument is null) continue;
			ILocalizationService localization = context.HttpContext.RequestServices.GetRequiredService<ILocalizationService>();
			ValidationContext validationContext = new(argument,
				serviceProvider: context.HttpContext.RequestServices,
				items: null);

			List<ValidationResult> validationResults = [];
			bool isValid = Validator.TryValidateObject(
				argument,
				validationContext,
				validationResults,
				validateAllProperties: true
			);

			if (isValid) continue;
			ValidationResult? firstError = validationResults.FirstOrDefault();
			string errorMessage = firstError?.ErrorMessage ?? localization.Get("ValidationError");

			return new UResponse(USC.BadRequest, errorMessage);
		}

		return await next(context);
	}
}

public abstract class LocalizedValidationAttribute(string key) : ValidationAttribute {
	protected string GetErrorMessage(ValidationContext context) => context.GetRequiredService<ILocalizationService>().Get(key);
	public override string FormatErrorMessage(string name) => key;
}

public sealed class LocalizedRequiredAttribute(string key) : LocalizedValidationAttribute(key) {
	protected override ValidationResult? IsValid(object? value, ValidationContext context) => value == null || value is string str && string.IsNullOrWhiteSpace(str) ? new ValidationResult(GetErrorMessage(context)) : ValidationResult.Success;
}

public sealed class LocalizedStringLengthAttribute(int min, int max, string key) : LocalizedValidationAttribute(key) {
	protected override ValidationResult? IsValid(object? value, ValidationContext context) {
		if (value is not string str) return ValidationResult.Success;
		return str.Length < min || str.Length > max ? new ValidationResult(GetErrorMessage(context)) : ValidationResult.Success;
	}
}

public sealed class LocalizedEmailAttribute(string key) : LocalizedValidationAttribute(key) {
	protected override ValidationResult? IsValid(object? value, ValidationContext context) => value is string str && !new EmailAddressAttribute().IsValid(str) ? new ValidationResult(GetErrorMessage(context)) : ValidationResult.Success;
}

public sealed class LocalizedRegexAttribute(string pattern, string key) : LocalizedValidationAttribute(key) {
	protected override ValidationResult? IsValid(object? value, ValidationContext context) => value is string str && !new Regex(pattern).IsMatch(str) ? new ValidationResult(GetErrorMessage(context)) : ValidationResult.Success;
}

public sealed class LocalizedCompareAttribute(string otherProperty, string key) : LocalizedValidationAttribute(key) {
	protected override ValidationResult? IsValid(object? value, ValidationContext context) => !Equals(value, context.ObjectType.GetProperty(otherProperty)?.GetValue(context.ObjectInstance)) ? new ValidationResult(GetErrorMessage(context)) : ValidationResult.Success;
}

public sealed class LocalizedFutureDateAttribute(string key) : LocalizedValidationAttribute(key) {
	protected override ValidationResult? IsValid(object? value, ValidationContext context) => value is DateTime date && date <= DateTime.Now ? new ValidationResult(GetErrorMessage(context)) : ValidationResult.Success;
}

public sealed class LocalizedMinCollectionLengthAttribute(int min, string key) : LocalizedValidationAttribute(key) {
	protected override ValidationResult? IsValid(object? value, ValidationContext context) {
		if (value is not IEnumerable collection) return ValidationResult.Success;
		int count = collection.Cast<object?>().Count();
		return count < min ? new ValidationResult(GetErrorMessage(context)) : ValidationResult.Success;
	}
}