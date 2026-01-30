namespace SinaMN75U.Utils;

public static class Server {
	private static string? _baseUrl;

	public static string BaseUrl => _baseUrl ?? throw new InvalidOperationException("Server not configured. Call Configure() at startup.");

	public static void Configure(IConfiguration config) {
		_baseUrl = config["BaseUrl"]?.TrimEnd('/');
	}
}