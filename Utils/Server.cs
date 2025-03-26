namespace SinaMN75U.Utils;

public class Server {
	private static IHttpContextAccessor? _httpContextAccessor;
	private static string? _serverAddress;

	public static string ServerAddress {
		get {
			if (_serverAddress != null) return _serverAddress;
			HttpRequest? request = _httpContextAccessor?.HttpContext?.Request;
			_serverAddress = $"{request?.Scheme}://{request?.Host.ToUriComponent()}{request?.PathBase.ToUriComponent()}";
			return _serverAddress;
		}
	}

	public static void Configure(IHttpContextAccessor? accessor) => _httpContextAccessor = accessor;


	public static string GetAppLanguage() {
		HttpContext? httpContext = _httpContextAccessor?.HttpContext;
		if (httpContext == null) return "en";
		if (!httpContext.Request.Headers.TryGetValue("locale", out StringValues headerValue)) return "en";
		return StringValues.IsNullOrEmpty(headerValue) ? "en" : headerValue.ToString();
	}

	public static async Task RunCommand(string command, string args) {
		Console.WriteLine("COMMAND Started");
		Console.WriteLine(command);
		try {
			Process process = new() {
				StartInfo = new ProcessStartInfo {
					FileName = command,
					Arguments = args,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = true,
					CreateNoWindow = true
				}
			};
			process.Start();
			string output = await process.StandardOutput.ReadToEndAsync();
			Console.WriteLine("COMMAND LOG");
			Console.WriteLine(output);
			await process.WaitForExitAsync();
		}
		catch (Exception e) {
			Console.WriteLine("COMMAND Exception");
			Console.WriteLine(e.Message);
		}
	}
}