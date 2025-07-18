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
}