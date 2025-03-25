using Microsoft.Extensions.Logging;

namespace SinaMN75U.Middlewares;

public class ApiRequestLoggingMiddleware(
	RequestDelegate next,
	ILogger<ApiRequestLoggingMiddleware> logger,
	string logFilePath = "api_logs.txt") {
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
			using StreamReader reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true);
			requestBody = await reader.ReadToEndAsync();
			request.Body.Position = 0;
		}
		catch (Exception ex) {
			logger.LogError(ex, "Failed to read request body");
		}

		Stream originalResponseBodyStream = context.Response.Body;
		using MemoryStream responseBodyStream = new MemoryStream();
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

		StringBuilder logEntry = new StringBuilder();
		logEntry.AppendLine($"{request.Method} - {request.Path} - {context.Response.StatusCode} | {elapsedMs}ms");
		logEntry.AppendLine("Headers:");
		foreach (KeyValuePair<string, StringValues> header in request.Headers) {
			logEntry.AppendLine($"  {header.Key}: {header.Value}");
		}

		logEntry.AppendLine($"Request Body: {requestBody}");
		logEntry.AppendLine($"Response Body: {responseBody}");
		logEntry.AppendLine(new string('-', 50));

		try {
			await File.AppendAllTextAsync(logFilePath, logEntry.ToString());
		}
		catch (Exception ex) {
			logger.LogError(ex, "Failed to write to log file");
		}
	}
}