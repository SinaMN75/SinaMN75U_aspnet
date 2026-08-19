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
	IDashboardService dashboardService
) : IHttpClientService {
	private static readonly Lazy<string> ServerIpAddress = new(ResolveServerIpAddress);

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
		fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
		content.Add(fileContent, "file", fileName);

		using HttpRequestMessage request = new(HttpMethod.Post, uri);
		request.Content = content;

		if (headers != null)
			foreach (KeyValuePair<string, string> header in headers)
				request.Headers.Add(header.Key, header.Value);

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

	private static HttpContent? BuildContent(object? body) => body switch {
		null => null,
		Dictionary<string, string> formData => new FormUrlEncodedContent(formData),
		_ => new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
	};

	private async Task<HttpResponseMessage?> Send(HttpMethod method, string uri, object? body = null, Dictionary<string, string>? headers = null) {
		Stopwatch sw = Stopwatch.StartNew();
		string requestBody = body == null ? "" : JsonSerializer.Serialize(body);
		try {
			using HttpRequestMessage request = new(method, uri);

			request.Content = BuildContent(body);
			request.Content = BuildContent(body);
			if (headers != null)
				foreach (KeyValuePair<string, string> header in headers)
					request.Headers.Add(header.Key, header.Value);

			HttpResponseMessage response = await httpClient.SendAsync(request);
			string responseBody = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"{method} - {uri} - {(int)response.StatusCode} \nPARAMS: {(body != null ? requestBody : "null")} \nRESPONSE: {responseBody}");

			sw.Stop();
			await dashboardService.CreateApiLog(new ApiLogCreateParams {
				Method = method.ToString(),
				Path = uri,
				StatusCode = (int)response.StatusCode,
				DurationMs = sw.ElapsedMilliseconds,
				RequestBody = requestBody,
				ResponseBody = responseBody,
				RequestSizeBytes = body == null ? 0 : Encoding.UTF8.GetByteCount(requestBody),
				ResponseSizeBytes = Encoding.UTF8.GetByteCount(responseBody),
				IpAddress = ServerIpAddress.Value
			}, CancellationToken.None);

			return response;
		}
		catch (Exception ex) {
			sw.Stop();
			await dashboardService.CreateApiLog(new ApiLogCreateParams {
				Method = method.ToString(),
				Path = uri,
				StatusCode = 500,
				DurationMs = sw.ElapsedMilliseconds,
				RequestBody = requestBody,
				ResponseBody = "",
				RequestSizeBytes = body == null ? 0 : Encoding.UTF8.GetByteCount(requestBody),
				ResponseSizeBytes = 0,
				ExceptionType = ex.GetType().Name,
				ExceptionMessage = ex.Message,
				StackTrace = ex.StackTrace,
				IpAddress = ServerIpAddress.Value
			}, CancellationToken.None);

			Console.WriteLine($"{method} - {uri} - ERROR \nPARAMS: {(body != null ? requestBody : "null")} \nRESPONSE: {ex.Message}");
			return null;
		}
	}
}