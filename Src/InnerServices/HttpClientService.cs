namespace SinaMN75U.InnerServices;

public interface IHttpClientService {
	Task<string> Get(string uri, Dictionary<string, string>? headers = null);
	Task<string> Post(string uri, object? body, Dictionary<string, string>? headers = null);
	Task<string> Put(string uri, object? body, Dictionary<string, string>? headers = null);
	Task<string> Delete(string uri, Dictionary<string, string>? headers = null);
	Task<string> Upload(string uri, IFormFile file, Dictionary<string, string>? headers = null);
	Task<string> Upload(string uri, IFormFile file, string fileName, Dictionary<string, string>? headers = null);
}

public class HttpClientService(HttpClient httpClient) : IHttpClientService {
	private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

	public async Task<string> Get(string uri, Dictionary<string, string>? headers = null) => await Send(HttpMethod.Get, uri, null, headers);

	public async Task<string> Post(string uri, object? body, Dictionary<string, string>? headers = null) => await Send(HttpMethod.Post, uri, body, headers);

	public async Task<string> Put(string uri, object? body, Dictionary<string, string>? headers = null) => await Send(HttpMethod.Put, uri, body, headers);

	public async Task<string> Delete(string uri, Dictionary<string, string>? headers = null) => await Send(HttpMethod.Delete, uri, null, headers);

	public async Task<string> Upload(string uri, IFormFile file, Dictionary<string, string>? headers = null) => await Upload(uri, file, file.FileName, headers);

	public async Task<string> Upload(string uri, IFormFile file, string fileName, Dictionary<string, string>? headers = null) {
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

		using HttpResponseMessage response = await _httpClient.SendAsync(request);
		if (!response.IsSuccessStatusCode) throw new HttpRequestException($"Request failed with status code {response.StatusCode}");

		return await response.Content.ReadAsStringAsync();
	}

	private async Task<string> Send(HttpMethod method, string uri, object? body = null, Dictionary<string, string>? headers = null) {
		if (string.IsNullOrEmpty(uri)) throw new ArgumentException("URI cannot be null or empty.", nameof(uri));

		using HttpRequestMessage request = new(method, uri);
		if (body != null) {
			string json = JsonSerializer.Serialize(body);
			request.Content = new StringContent(json, Encoding.UTF8, "application/json");
		}

		if (headers != null)
			foreach (KeyValuePair<string, string> header in headers)
				request.Headers.Add(header.Key, header.Value);


		using HttpResponseMessage response = await _httpClient.SendAsync(request);
		if (!response.IsSuccessStatusCode) throw new HttpRequestException($"Request failed with status code {response.StatusCode}");

		string responseContent = await response.Content.ReadAsStringAsync();

		return responseContent;
	}
}