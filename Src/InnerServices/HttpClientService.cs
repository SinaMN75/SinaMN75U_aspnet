namespace SinaMN75U.InnerServices;

public interface IHttpClientService {
	Task<HttpResponseMessage?> Get(string uri, Dictionary<string, string>? headers = null);
	Task<HttpResponseMessage?> Post(string uri, object? body, Dictionary<string, string>? headers = null);
	Task<HttpResponseMessage?> Put(string uri, object? body, Dictionary<string, string>? headers = null);
	Task<HttpResponseMessage?> Delete(string uri, Dictionary<string, string>? headers = null);
	Task<HttpResponseMessage?> Upload(string uri, IFormFile file, Dictionary<string, string>? headers = null);
	Task<HttpResponseMessage?> Upload(string uri, IFormFile file, string fileName, Dictionary<string, string>? headers = null);
}

public class HttpClientService(
	HttpClient httpClient,
	IApiLogQueue apiLogQueue
) : IHttpClientService {
	private static readonly Lazy<string> ServerIpAddress = new(ResolveServerIpAddress);

	// ULog keeps its last 5000 lines in memory and queued API logs wait in memory before being written, so bodies are capped.
	private const int MaxConsoleBody = 4_000;
	private const int MaxLoggedBody = 20_000;

	private static string Cap(string s, int max) => s.Length <= max ? s : s[..max] + "...<truncated>";

	public async Task<HttpResponseMessage?> Get(string uri, Dictionary<string, string>? headers = null) => await Send(HttpMethod.Get, uri, null, headers);
	public async Task<HttpResponseMessage?> Post(string uri, object? body, Dictionary<string, string>? headers = null) => await Send(HttpMethod.Post, uri, body, headers);
	public async Task<HttpResponseMessage?> Put(string uri, object? body, Dictionary<string, string>? headers = null) => await Send(HttpMethod.Put, uri, body, headers);
	public async Task<HttpResponseMessage?> Delete(string uri, Dictionary<string, string>? headers = null) => await Send(HttpMethod.Delete, uri, null, headers);
	public async Task<HttpResponseMessage?> Upload(string uri, IFormFile file, Dictionary<string, string>? headers = null) => await Upload(uri, file, file.FileName, headers);

	public async Task<HttpResponseMessage?> Upload(string uri, IFormFile file, string fileName, Dictionary<string, string>? headers = null) {
		if (string.IsNullOrEmpty(uri)) throw new ArgumentException("URI cannot be null or empty.", nameof(uri));
		ArgumentNullException.ThrowIfNull(file);
		if (string.IsNullOrEmpty(fileName)) throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));

		using MultipartFormDataContent content = new();
		await using Stream stream = file.OpenReadStream();
		StreamContent fileContent = new(stream);
		if (MediaTypeHeaderValue.TryParse(file.ContentType, out MediaTypeHeaderValue? contentType)) fileContent.Headers.ContentType = contentType;
		content.Add(fileContent, "file", fileName);

		using HttpRequestMessage request = new(HttpMethod.Post, uri);
		request.Content = content;

		AddHeaders(request, headers);

		return await httpClient.SendAsync(request);
	}
	
	private static string ResolveServerIpAddress() {
		try {
			IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());
			IPAddress? ipv4 = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
			return ipv4?.ToString() ?? addresses.FirstOrDefault()?.ToString() ?? "N/A";
		}
		catch {
			return "N/A";
		}
	}

	private static HttpContent? BuildContent(object? body, string json) => body switch {
		null => null,
		Dictionary<string, string> formData => new FormUrlEncodedContent(formData),
		_ => new StringContent(json, Encoding.UTF8, "application/json")
	};

	private static void AddHeaders(HttpRequestMessage request, Dictionary<string, string>? headers) {
		if (headers == null) return;
		foreach (KeyValuePair<string, string> header in headers.Where(header => !request.Headers.TryAddWithoutValidation(header.Key, header.Value))) request.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value);
	}

	private async Task<HttpResponseMessage?> Send(HttpMethod method, string uri, object? body = null, Dictionary<string, string>? headers = null) {
		Stopwatch sw = Stopwatch.StartNew();
		string requestBody = body == null ? "" : JsonSerializer.Serialize(body);
		try {
			using HttpRequestMessage request = new(method, uri);

			request.Content = BuildContent(body, requestBody);
			// Outbound bodies can carry third-party credentials (e.g. OAuth client_secret/password); redact before logging.
			requestBody = ApiLogMiddleware.RedactBody(requestBody);
			AddHeaders(request, headers);

			HttpResponseMessage response = await httpClient.SendAsync(request);
			// Only used for logging (callers read the response themselves), so it is redacted too.
			string responseBody = ApiLogMiddleware.RedactBody(await response.Content.ReadAsStringAsync());
			string logMessage = $"{method} - {uri} - {(int)response.StatusCode} \nPARAMS: {(body != null ? Cap(requestBody, MaxConsoleBody) : "null")} \nRESPONSE: {Cap(responseBody, MaxConsoleBody)}";
			if ((int)response.StatusCode >= 400) ULog.Error(logMessage);
			else ULog.Info(logMessage);

			sw.Stop();
			apiLogQueue.Enqueue(new ApiLogCreateParams {
				Method = method.ToString(),
				Path = uri,
				StatusCode = (int)response.StatusCode,
				DurationMs = sw.ElapsedMilliseconds,
				RequestBody = Cap(requestBody, MaxLoggedBody),
				ResponseBody = Cap(responseBody, MaxLoggedBody),
				RequestSizeBytes = body == null ? 0 : Encoding.UTF8.GetByteCount(requestBody),
				ResponseSizeBytes = Encoding.UTF8.GetByteCount(responseBody),
				IpAddress = ServerIpAddress.Value
			});

			return response;
		}
		catch (Exception ex) {
			sw.Stop();
			apiLogQueue.Enqueue(new ApiLogCreateParams {
				Method = method.ToString(),
				Path = uri,
				StatusCode = 500,
				DurationMs = sw.ElapsedMilliseconds,
				RequestBody = Cap(requestBody, MaxLoggedBody),
				ResponseBody = "",
				RequestSizeBytes = body == null ? 0 : Encoding.UTF8.GetByteCount(requestBody),
				ResponseSizeBytes = 0,
				ExceptionType = ex.GetType().Name,
				ExceptionMessage = ex.Message,
				StackTrace = ex.StackTrace,
				IpAddress = ServerIpAddress.Value
			});

			ULog.Error($"{method} - {uri} - ERROR \nPARAMS: {(body != null ? Cap(requestBody, MaxConsoleBody) : "null")} \nRESPONSE: {ex.Message}");
			return null;
		}
	}
}