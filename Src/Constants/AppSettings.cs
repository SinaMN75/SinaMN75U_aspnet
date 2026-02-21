namespace SinaMN75U.Constants;

public class AppSettings {
	public required ConnectionStrings ConnectionStrings { get; init; }
	public required Jwt Jwt { get; init; }
	public required Middleware Middleware { get; init; }
	public required SmsPanel SmsPanel { get; init; }
	public required string BaseUrl { get; init; }
	public required string ApiKey { get; init; }

	public static AppSettings Instance { get; private set; } = null!;

	public static void Initialize(AppSettings settings) => Instance = settings;
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

public class SmsPanel {
	public required TagSmsPanel Tag { get; init; }
	public required string Pattern { get; init; }
	public required string ApiKey { get; init; }
	public required string OtpPattern { get; init; }
	public required string UserName { get; set; }
	public required string Password { get; set; }
}