namespace SinaMN75U.InnerServices;

public interface ILocalizationService {
	string Get(string key, string? locale = null);
}

public class LocalizationService(IHttpContextAccessor httpContext) : ILocalizationService {
	public string Get(string key, string? locale) {
		try {
			return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "LocalizedMessages.json")))!
				.GetValueOrDefault(locale ?? httpContext.HttpContext?.Request.Headers["Locale"].FirstOrDefault() ?? "en")?
				.GetValueOrDefault(key, "Error") ?? "Error";
		}
		catch (Exception) {
			return string.Empty;
		}
	}
}