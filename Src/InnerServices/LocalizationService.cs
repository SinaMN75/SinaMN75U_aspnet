namespace SinaMN75U.InnerServices;

public interface ILocalizationService {
	string Get(string key, string locale = "en");
}

public class LocalizationService : ILocalizationService {
	public string Get(string key, string locale = "en") {
		try {
			return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "LocalizedMessages.json")))!
				.GetValueOrDefault(locale)?
				.GetValueOrDefault(key, "Error") ?? "Error";
		}
		catch (Exception) {
			return string.Empty;
		}
	}
}