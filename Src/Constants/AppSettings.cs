namespace SinaMN75U.Constants;

public class AppSettings {
	public required ConnectionStrings ConnectionStrings { get; init; }
	public required Jwt Jwt { get; init; }
	public required Middleware Middleware { get; init; }
	public required string BaseUrl { get; init; }
	public required string ApiKey { get; init; }

	private static AppSettings _instance = null!;

	public static AppSettings Instance {
		get {
			if (_instance == null) {
				throw new InvalidOperationException(
					"AppSettings has not been initialized yet. " +
					"Call AppSettings.Initialize(...) once at startup.");
			}

			return _instance;
		}
	}

	public static void Initialize(AppSettings settings) {
		if (_instance != null) {
			throw new InvalidOperationException("AppSettings is already initialized.");
		}

		_instance = settings ?? throw new ArgumentNullException(nameof(settings));
	}
}

public class ConnectionStrings {
	public required string Server { get; init; }
}

public class Jwt {
	public required string Key { get; init; }
	public required string Issuer { get; init; }
	public required string Audience { get; init; }
	public required string Expires { get; init; }
}
public class Middleware {
	public required bool DecryptParams { get; init; }
	public required bool EncryptResponse { get; init; }
	public required bool RequireApiKey { get; init; }
	public required bool Log { get; init; }
	public required bool LogSuccess { get; init; }
}