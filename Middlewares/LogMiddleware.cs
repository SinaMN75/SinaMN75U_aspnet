using Microsoft.Extensions.Logging;

namespace SinaMN75U.Middlewares;

public class ApiRequestLoggingMiddleware(
	RequestDelegate next,
	ILogger<ApiRequestLoggingMiddleware> logger) {
	public async Task InvokeAsync(HttpContext context) {
		HttpRequest request = context.Request;

		if (!request.Path.StartsWithSegments("/api")) {
			await next(context);
			return;
		}

		Stopwatch stopwatch = Stopwatch.StartNew();
		Stream originalRequestBodyStream = request.Body;

		string requestBody = "";
		try {
			request.EnableBuffering();
			using StreamReader reader = new(request.Body, Encoding.UTF8, true, 1024, true);
			requestBody = await reader.ReadToEndAsync();
			request.Body.Position = 0;
		}
		catch (Exception ex) {
			logger.LogError(ex, "Failed to read request body");
		}

		Stream originalResponseBodyStream = context.Response.Body;
		using MemoryStream responseBodyStream = new();
		context.Response.Body = responseBodyStream;
		await next(context);

		string responseBody = "";
		try {
			responseBodyStream.Position = 0;
			responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
			responseBodyStream.Position = 0;
			await responseBodyStream.CopyToAsync(originalResponseBodyStream);
		}
		catch (Exception ex) {
			logger.LogError(ex, "Failed to read response body");
		}

		stopwatch.Stop();
		long elapsedMs = stopwatch.ElapsedMilliseconds;

		string cleanRequestBody = TryMinifyJson(requestBody);
		string cleanResponseBody = TryMinifyJson(responseBody);

		string logEntry =
			$"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {request.Method} - {request.Path} - {context.Response.StatusCode} - {elapsedMs}ms\n" +
			$"{JsonSerializer.Serialize(request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()))}\n" +
			$"{JsonSerializer.Serialize(context.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()))}\n" +
			$"{cleanRequestBody}\n" +
			$"{cleanResponseBody}\n" +
			new string('-', 50) + "\n";

		try {
			DateTime now = DateTime.Now;
			string logDirectory = Path.Combine("Logs", now.Year.ToString(), now.Month.ToString("00"));
			string logFileName = $"{now:dd}.txt";
			string logFilePath = Path.Combine(logDirectory, logFileName);

			Directory.CreateDirectory(logDirectory);
			await File.AppendAllTextAsync(logFilePath, logEntry);
		}
		catch (Exception ex) {
			logger.LogError(ex, "Failed to write to log file");
		}
	}

	private static string TryMinifyJson(string json) {
		try {
			using JsonDocument doc = JsonDocument.Parse(json);
			return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions {
				WriteIndented = false,
				Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			});
		}
		catch {
			return json;
		}
	}
}