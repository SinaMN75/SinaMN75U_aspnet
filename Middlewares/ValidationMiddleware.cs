namespace SinaMN75U.Middlewares;

using System.Collections;

public class ValidationFilter : IEndpointFilter {
	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next) {
		foreach (object? argument in context.Arguments) {
			if (argument is null) continue;
			ILocalizationService localization = context.HttpContext.RequestServices.GetRequiredService<ILocalizationService>();
			ValidationContext validationContext = new ValidationContext(argument,
				serviceProvider: context.HttpContext.RequestServices,
				items: null);

			List<ValidationResult> validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
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
	protected override ValidationResult? IsValid(object? value, ValidationContext context) {
		if (value is null || value is string str && string.IsNullOrWhiteSpace(str)) return new ValidationResult(GetErrorMessage(context));
		return ValidationResult.Success;
	}
}

public sealed class LocalizedStringLengthAttribute(int min, int max, string key) : LocalizedValidationAttribute(key) {
	private int MinimumLength { get; } = min;
	private int MaximumLength { get; } = max;

	protected override ValidationResult? IsValid(object? value, ValidationContext context) {
		if (value is not string str) return ValidationResult.Success;

		return str.Length < MinimumLength || str.Length > MaximumLength
			? new ValidationResult(GetErrorMessage(context))
			: ValidationResult.Success;
	}
}

public sealed class LocalizedEmailAttribute : LocalizedValidationAttribute {
	private static readonly EmailAddressAttribute _validator = new();

	public LocalizedEmailAttribute(string key) : base(key) { }

	protected override ValidationResult? IsValid(object? value, ValidationContext context) {
		return value is string str && !_validator.IsValid(str)
			? new ValidationResult(GetErrorMessage(context))
			: ValidationResult.Success;
	}
}

public sealed class LocalizedRegexAttribute : LocalizedValidationAttribute {
	private readonly Regex _regex;

	public LocalizedRegexAttribute(string pattern, string key) : base(key) => _regex = new Regex(pattern);

	protected override ValidationResult? IsValid(object? value, ValidationContext context) {
		return value is string str && !_regex.IsMatch(str)
			? new ValidationResult(GetErrorMessage(context))
			: ValidationResult.Success;
	}
}

public sealed class LocalizedCompareAttribute : LocalizedValidationAttribute {
	public string OtherProperty { get; }

	public LocalizedCompareAttribute(string otherProperty, string key) : base(key) => OtherProperty = otherProperty;

	protected override ValidationResult? IsValid(object? value, ValidationContext context) {
		var otherValue = context.ObjectType.GetProperty(OtherProperty)?.GetValue(context.ObjectInstance);
		return !Equals(value, otherValue)
			? new ValidationResult(GetErrorMessage(context))
			: ValidationResult.Success;
	}
}

public sealed class LocalizedFutureDateAttribute : LocalizedValidationAttribute {
	public LocalizedFutureDateAttribute(string key) : base(key) { }

	protected override ValidationResult? IsValid(object? value, ValidationContext context) {
		return value is DateTime date && date <= DateTime.Now
			? new ValidationResult(GetErrorMessage(context))
			: ValidationResult.Success;
	}
}

public sealed class LocalizedMinCollectionLengthAttribute : LocalizedValidationAttribute {
	public int MinLength { get; }

	public LocalizedMinCollectionLengthAttribute(int min, string key) : base(key) => MinLength = min;

	protected override ValidationResult? IsValid(object? value, ValidationContext context) {
		if (value is not IEnumerable collection) return ValidationResult.Success;

		var count = 0;
		foreach (var _ in collection) count++;

		return count < MinLength
			? new ValidationResult(GetErrorMessage(context))
			: ValidationResult.Success;
	}
}