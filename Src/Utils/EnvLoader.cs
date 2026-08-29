namespace SinaMN75U.Utils;

public static class EnvLoader {
	public static void Load() {
		string envPath = Path.Combine(AppContext.BaseDirectory, ".env");
		if (!File.Exists(envPath)) return;

		foreach (string line in File.ReadAllLines(envPath)) {
			if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;

			string[] parts = line.Split('=', 2);
			if (parts.Length != 2) continue;

			string key = parts[0].Trim();
			string value = parts[1].Trim();
			Environment.SetEnvironmentVariable(key, value);
		}
	}

	public static string Get(string key, string defaultValue = "") => Environment.GetEnvironmentVariable(key) ?? defaultValue;
}