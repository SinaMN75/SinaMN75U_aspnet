namespace SinaMN75U.Middlewares;

public class UValidationFilter : IEndpointFilter {
	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next) {
		foreach (object? argument in context.Arguments) {
			if (argument is null) continue;
			ILocalizationService l = context.HttpContext.RequestServices.GetRequiredService<ILocalizationService>();
			ValidationContext validationContext = new(argument,
				context.HttpContext.RequestServices,
				null);

			List<ValidationResult> validationResults = [];
			bool isValid = Validator.TryValidateObject(
				argument,
				validationContext,
				validationResults,
				true
			);

			if (isValid) continue;
			string errorMessage = validationResults.FirstOrDefault()?.ErrorMessage ?? l.Get("ValidationError");

			return new UResponse(Usc.BadRequest, errorMessage).ToResult();
		}

		return await next(context);
	}
}

public abstract class UValidationAttribute(string key) : ValidationAttribute {
	protected string GetErrorMessage(ValidationContext context) => context.GetRequiredService<ILocalizationService>().Get(key);
	public override string FormatErrorMessage(string name) => key;
}

public sealed class URequiredAttribute(string key) : UValidationAttribute(key) {
	protected override ValidationResult? IsValid(object? value, ValidationContext context) => value == null || value is string str && string.IsNullOrWhiteSpace(str) ? new ValidationResult(GetErrorMessage(context)) : ValidationResult.Success;
}

public sealed class UStringLengthAttribute(int min, int max, string key) : UValidationAttribute(key) {
	protected override ValidationResult? IsValid(object? value, ValidationContext context) {
		if (value is not string str) return ValidationResult.Success;
		return str.Length < min || str.Length > max ? new ValidationResult(GetErrorMessage(context)) : ValidationResult.Success;
	}
}

public sealed class UEmailAttribute(string key) : UValidationAttribute(key) {
	protected override ValidationResult? IsValid(object? value, ValidationContext context) => value is string str && !new EmailAddressAttribute().IsValid(str) ? new ValidationResult(GetErrorMessage(context)) : ValidationResult.Success;
}

public sealed class URegexAttribute(string pattern, string key) : UValidationAttribute(key) {
	protected override ValidationResult? IsValid(object? value, ValidationContext context) => value is string str && !new Regex(pattern).IsMatch(str) ? new ValidationResult(GetErrorMessage(context)) : ValidationResult.Success;
}

public sealed class UCompareAttribute(string otherProperty, string key) : UValidationAttribute(key) {
	protected override ValidationResult? IsValid(object? value, ValidationContext context) => !Equals(value, context.ObjectType.GetProperty(otherProperty)?.GetValue(context.ObjectInstance)) ? new ValidationResult(GetErrorMessage(context)) : ValidationResult.Success;
}

public sealed class UFutureDateAttribute(string key) : UValidationAttribute(key) {
	protected override ValidationResult? IsValid(object? value, ValidationContext context) => value is DateTime date && date < DateTime.Now ? new ValidationResult(GetErrorMessage(context)) : ValidationResult.Success;
}

public sealed class UBeforeDateAttribute(string key) : UValidationAttribute(key) {
	protected override ValidationResult? IsValid(object? value, ValidationContext context) => value is DateTime date && date > DateTime.Now ? new ValidationResult(GetErrorMessage(context)) : ValidationResult.Success;
}

public sealed class UMinCollectionLengthAttribute(int min, string key) : UValidationAttribute(key) {
	protected override ValidationResult? IsValid(object? value, ValidationContext context) {
		if (value is not IEnumerable collection) return ValidationResult.Success;
		int count = collection.Cast<object?>().Count();
		return count < min ? new ValidationResult(GetErrorMessage(context)) : ValidationResult.Success;
	}
}