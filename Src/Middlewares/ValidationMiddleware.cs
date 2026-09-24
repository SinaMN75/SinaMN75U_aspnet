namespace SinaMN75U.Middlewares;

public class UValidationFilter : IEndpointFilter {
	private const int MaxNestedDepth = 4;

	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next) {
		foreach (object? argument in context.Arguments) {
			if (argument is not (BaseParams or IEnumerable<BaseParams>)) continue;

			string? error = Validate(argument, context.HttpContext.RequestServices, 0);
			if (error == null) continue;

			if (error.Length == 0) error = context.HttpContext.RequestServices.GetRequiredService<ILocalizationService>().Get("validationFailedPleaseCheckYourInput");
			return new UResponse(Usc.BadRequest, error).ToResult();
		}

		return await next(context);
	}

	private static string? Validate(object instance, IServiceProvider services, int depth) {
		if (instance is IEnumerable<BaseParams> items) return items.Select(item => Validate(item, services, depth)).OfType<string>().FirstOrDefault();
		SelectorArgsGuard.Limit(instance);
		List<ValidationResult> results = [];
		if (!Validator.TryValidateObject(instance, new ValidationContext(instance, services, null), results, true)) return results.FirstOrDefault()?.ErrorMessage ?? "";
		return depth >= MaxNestedDepth ? null : NestedParamsProperties(instance.GetType()).Select(p => p.GetValue(instance)).OfType<object>().Select(value => Validate(value, services, depth + 1)).OfType<string>().FirstOrDefault();
	}

	private static readonly ConcurrentDictionary<Type, PropertyInfo[]> NestedProperties = new();

	private static PropertyInfo[] NestedParamsProperties(Type type) => NestedProperties.GetOrAdd(type, t => t.GetProperties()
		.Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
		.Where(p => typeof(BaseParams).IsAssignableFrom(p.PropertyType) || typeof(IEnumerable<BaseParams>).IsAssignableFrom(p.PropertyType))
		.ToArray());
}

public abstract class UValidationAttribute(string key) : ValidationAttribute {
	protected string GetErrorMessage(ValidationContext context) => context.GetRequiredService<ILocalizationService>().Get(key);

	public override string FormatErrorMessage(string name) => key;

	protected static DateTime ToUtc(DateTime date) => date.Kind == DateTimeKind.Local ? date.ToUniversalTime() : date;
}

public sealed class UValidationRequiredAttribute(string key) : UValidationAttribute(key) {
	protected override ValidationResult? IsValid(object? value, ValidationContext context) => value switch {
		null => new ValidationResult(GetErrorMessage(context)),
		string str when string.IsNullOrWhiteSpace(str) => new ValidationResult(GetErrorMessage(context)),
		Guid guid when guid == Guid.Empty => new ValidationResult(GetErrorMessage(context)),
		_ => ValidationResult.Success
	};
}

public sealed class UValidationStringLengthAttribute(int min, int max, string key) : UValidationAttribute(key) {
	protected override ValidationResult? IsValid(object? value, ValidationContext context) {
		if (value is not string str) return ValidationResult.Success;
		return str.Length < min || str.Length > max ? new ValidationResult(GetErrorMessage(context)) : ValidationResult.Success;
	}
}

public sealed class UValidationEmailAttribute(string key) : UValidationAttribute(key) {
	private static readonly EmailAddressAttribute Email = new();
	protected override ValidationResult? IsValid(object? value, ValidationContext context) => value is string str && !Email.IsValid(str) ? new ValidationResult(GetErrorMessage(context)) : ValidationResult.Success;
}

public sealed class UValidationGuidAttribute(string key) : UValidationAttribute(key) {
	protected override ValidationResult? IsValid(object? value, ValidationContext context) => value is string str && !str.IsGuid() ? new ValidationResult(GetErrorMessage(context)) : ValidationResult.Success;
}

public sealed class UValidationRegexAttribute(string pattern, string key) : UValidationAttribute(key) {
	protected override ValidationResult? IsValid(object? value, ValidationContext context) => value is string str && !Regex.IsMatch(str, pattern) ? new ValidationResult(GetErrorMessage(context)) : ValidationResult.Success;
}

public sealed class UValidationCompareAttribute(string otherProperty, string key) : UValidationAttribute(key) {
	protected override ValidationResult? IsValid(object? value, ValidationContext context) => !Equals(value, context.ObjectType.GetProperty(otherProperty)?.GetValue(context.ObjectInstance)) ? new ValidationResult(GetErrorMessage(context)) : ValidationResult.Success;
}

public sealed class UValidationFutureDateAttribute(string key) : UValidationAttribute(key) {
	protected override ValidationResult? IsValid(object? value, ValidationContext context) => value is DateTime date && ToUtc(date) < DateTime.UtcNow ? new ValidationResult(GetErrorMessage(context)) : ValidationResult.Success;
}

public sealed class UValidationBeforeDateAttribute(string key) : UValidationAttribute(key) {
	protected override ValidationResult? IsValid(object? value, ValidationContext context) => value is DateTime date && ToUtc(date) > DateTime.UtcNow ? new ValidationResult(GetErrorMessage(context)) : ValidationResult.Success;
}

public sealed class UValidationMinCollectionLengthAttribute(int min, string key) : UValidationAttribute(key) {
	protected override ValidationResult? IsValid(object? value, ValidationContext context) {
		if (value is not IEnumerable collection) return ValidationResult.Success;
		int count = collection.Cast<object?>().Count();
		return count < min ? new ValidationResult(GetErrorMessage(context)) : ValidationResult.Success;
	}
}