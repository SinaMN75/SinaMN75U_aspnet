namespace SinaMN75U.InnerServices;

public interface IHttpClientService {
	Task<HttpResponseMessage?> Get(string uri, Dictionary<string, string>? headers = null);
	Task<HttpResponseMessage?> Post(string uri, object? body, Dictionary<string, string>? headers = null);
	Task<HttpResponseMessage?> PostForm(string uri, Dictionary<string, string> formData, Dictionary<string, string>? headers = null);
	Task<HttpResponseMessage?> Put(string uri, object? body, Dictionary<string, string>? headers = null);
	Task<HttpResponseMessage?> Delete(string uri, Dictionary<string, string>? headers = null);
	Task<HttpResponseMessage?> Upload(string uri, IFormFile file, Dictionary<string, string>? headers = null);
	Task<HttpResponseMessage?> Upload(string uri, IFormFile file, string fileName, Dictionary<string, string>? headers = null);
}

public class HttpClientService(HttpClient httpClient) : IHttpClientService {
	public async Task<HttpResponseMessage?> Get(string uri, Dictionary<string, string>? headers = null) => await Send(HttpMethod.Get, uri, null, headers);
	public async Task<HttpResponseMessage?> Post(string uri, object? body, Dictionary<string, string>? headers = null) => await Send(HttpMethod.Post, uri, body, headers);
	public async Task<HttpResponseMessage?> Put(string uri, object? body, Dictionary<string, string>? headers = null) => await Send(HttpMethod.Put, uri, body, headers);
	public async Task<HttpResponseMessage?> Delete(string uri, Dictionary<string, string>? headers = null) => await Send(HttpMethod.Delete, uri, null, headers);
	public async Task<HttpResponseMessage?> Upload(string uri, IFormFile file, Dictionary<string, string>? headers = null) => await Upload(uri, file, file.FileName, headers);

	public async Task<HttpResponseMessage?> PostForm(string uri, Dictionary<string, string> formData, Dictionary<string, string>? headers = null) {
		try {
			using HttpRequestMessage request = new(HttpMethod.Post, uri);
			request.Content = new FormUrlEncodedContent(formData);

			if (headers != null)
				foreach (KeyValuePair<string, string> h in headers)
					request.Headers.Add(h.Key, h.Value);

			HttpResponseMessage response = await httpClient.SendAsync(request);
			string responseBody = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"POST - {uri} - {(int)response.StatusCode} \nPARAMS: {JsonSerializer.Serialize(formData)} \nRESPONSE: {responseBody}");

			return response;
		}
		catch (Exception ex) {
			Console.WriteLine($"POST - {uri} - ERROR \nPARAMS: {JsonSerializer.Serialize(formData)} \nRESPONSE: {ex.Message}");
			return null;
		}
	}

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

	private async Task<HttpResponseMessage?> Send(HttpMethod method, string uri, object? body = null, Dictionary<string, string>? headers = null) {
		try {
			using HttpRequestMessage request = new(method, uri);
			string paramsLog = body != null ? JsonSerializer.Serialize(body) : "null";

			if (body != null) request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
			if (headers != null)
				foreach (KeyValuePair<string, string> header in headers)
					request.Headers.Add(header.Key, header.Value);

			HttpResponseMessage response = await httpClient.SendAsync(request);
			string responseBody = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"{method} - {uri} - {(int)response.StatusCode} \nPARAMS: {paramsLog} \nRESPONSE: {responseBody}");

			return response;
		}
		catch (Exception ex) {
			Console.WriteLine($"{method} - {uri} - ERROR \nPARAMS: {(body != null ? JsonSerializer.Serialize(body) : "null")} \nRESPONSE: {ex.Message}");
			return null;
		}
	}
}