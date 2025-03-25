using Microsoft.Extensions.Logging;

namespace SinaMN75U.Middlewares;

public sealed class ApiRequestLoggingMiddleware(
	RequestDelegate next,
	ILogger<ApiRequestLoggingMiddleware> logger
) {
	public async Task InvokeAsync(HttpContext context) {
		HttpRequest request = context.Request;

		if (!request.Path.StartsWithSegments("/api")) {
			await next(context);
			return;
		}

		Stopwatch stopwatch = Stopwatch.StartNew();

		string requestBody = "";
		if (request.Body.CanRead) {
			try {
				request.EnableBuffering();
				using StreamReader reader = new(
					request.Body,
					Encoding.UTF8,
					detectEncodingFromByteOrderMarks: true,
					bufferSize: 1024,
					leaveOpen: true);

				requestBody = await reader.ReadToEndAsync();
				request.Body.Position = 0;
			}
			catch (Exception) {
				// ignored
			}
		}

		Stream originalResponseBody = context.Response.Body;
		using MemoryStream responseBuffer = new();
		context.Response.Body = responseBuffer;

		try {
			await next(context);

			responseBuffer.Position = 0;
			string responseBody = await new StreamReader(responseBuffer).ReadToEndAsync();
			responseBuffer.Position = 0;
			await responseBuffer.CopyToAsync(originalResponseBody);

			LogToFile(
				DateTime.UtcNow,
				request.Method,
				request.Path,
				context.Response.StatusCode,
				stopwatch.ElapsedMilliseconds,
				request.Headers,
				context.Response.Headers,
				requestBody,
				responseBody);
		}
		finally {
			context.Response.Body = originalResponseBody;
		}
	}

	private void LogToFile(
		DateTime timestamp,
		string method,
		string path,
		int statusCode,
		long elapsedMs,
		IHeaderDictionary requestHeaders,
		IHeaderDictionary responseHeaders,
		string requestBody,
		string responseBody) {
		try {
			DateTime now = DateTime.Now;
			string logDir = Path.Combine("Logs", now.Year.ToString(), now.Month.ToString("00"));
			Directory.CreateDirectory(logDir);

			string logEntry = $"{timestamp:yyyy-MM-dd HH:mm:ss} - {method} - {path} - {statusCode} - {elapsedMs}ms\n" +
			                  $"{SerializeHeaders(requestHeaders)}\n" +
			                  $"{SerializeHeaders(responseHeaders)}\n" +
			                  $"{TryMinifyJson(requestBody)}\n" +
			                  $"{TryMinifyJson(responseBody)}\n" +
			                  new string('-', 50) + "\n";

			File.AppendAllText(Path.Combine(logDir, $"{now:dd}.txt"), logEntry);
		}
		catch (Exception ex) {
			logger.LogError(ex, "Log write failed");
		}
	}

	private static string SerializeHeaders(IHeaderDictionary headers) {
		try {
			return JsonSerializer.Serialize(
				headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
				UJsonOptions.JsonOptions
			);
		}
		catch {
			return "{}";
		}
	}

	private static string TryMinifyJson(string json) {
		try {
			return JsonSerializer.Serialize(JsonDocument.Parse(json).RootElement, UJsonOptions.JsonOptions);
		}
		catch {
			return json;
		}
	}
}