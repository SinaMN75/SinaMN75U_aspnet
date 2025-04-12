namespace SinaMN75U.Middlewares;

using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

public sealed class ApiRequestLoggingMiddleware(
	RequestDelegate next,
	ILogger<ApiRequestLoggingMiddleware> logger,
	IServiceProvider serviceProvider) {
	public async Task InvokeAsync(HttpContext context) {
		HttpRequest request = context.Request;

		if (!request.Path.StartsWithSegments("/api") || request.Path.StartsWithSegments("/api/media")) {
			await next(context);
			return;
		}

		Stopwatch stopwatch = Stopwatch.StartNew();
		List<string> efCoreQueries = [];
		string requestBody = "";
		string responseBody = "";
		Exception? exception = null;

		try {
			// Setup EF Core query logging
			using (IServiceScope scope = serviceProvider.CreateScope()) {
				IEnumerable<DbContext> dbContexts = scope.ServiceProvider.GetServices<DbContext>();
				foreach (DbContext dbContext in dbContexts) {
					ILoggerFactory loggerFactory = dbContext.GetService<ILoggerFactory>();
					loggerFactory.AddProvider(new EfCoreQueryLoggerProvider(efCoreQueries));
				}
			}

			// Read request body
			if (request.Body.CanRead) {
				try {
					request.EnableBuffering();
					using StreamReader reader = new StreamReader(
						request.Body,
						Encoding.UTF8,
						true,
						1024,
						true);
					requestBody = await reader.ReadToEndAsync();
					request.Body.Position = 0;
				}
				catch (Exception ex) {
					logger.LogError(ex, "Failed to read request body");
				}
			}

			// Capture response
			Stream originalResponseBody = context.Response.Body;
			using MemoryStream responseBuffer = new MemoryStream();
			context.Response.Body = responseBuffer;

			try {
				await next(context);
				responseBuffer.Position = 0;
				responseBody = await new StreamReader(responseBuffer).ReadToEndAsync();
				responseBuffer.Position = 0;
				await responseBuffer.CopyToAsync(originalResponseBody);
			}
			finally {
				context.Response.Body = originalResponseBody;
			}
		}
		catch (Exception ex) {
			exception = ex;
			context.Response.StatusCode = StatusCodes.Status500InternalServerError;
			responseBody = ex.ToString();
			throw; // Re-throw after logging
		}
		finally {
			try {
				LogToFile(
					timestamp: DateTime.UtcNow,
					method: request.Method,
					path: request.Path,
					statusCode: context.Response.StatusCode,
					elapsedMs: stopwatch.ElapsedMilliseconds,
					requestHeaders: request.Headers,
					responseHeaders: context.Response.Headers,
					requestBody: requestBody,
					responseBody: responseBody,
					efCoreQueries: efCoreQueries,
					exception: exception);
			}
			catch (Exception loggingEx) {
				logger.LogError(loggingEx, "CRITICAL: Failed to log request");
			}
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
		string responseBody,
		List<string> efCoreQueries,
		Exception? exception = null) {
		DateTime now = DateTime.Now;
		string logDir = Path.Combine("wwwroot", "Logs", now.Year.ToString(), now.Month.ToString("00"));
		Directory.CreateDirectory(logDir);

		bool isSuccess = statusCode is >= 200 and <= 299;
		string logFileName = $"{now:dd}_{(isSuccess ? "success" : "failed")}.txt";

		StringBuilder logEntry = new StringBuilder()
			.AppendLine($"{timestamp:yyyy-MM-dd HH:mm:ss} {method} - {path} - {statusCode} - {elapsedMs}ms")
			.AppendLine(SerializeHeaders(requestHeaders))
			.AppendLine(SerializeHeaders(responseHeaders));

		if (exception != null)
			logEntry.AppendLine($"Type: {exception.GetType().Name}")
				.AppendLine($"Message: {exception.Message}")
				.AppendLine($"Stack Trace: {exception.StackTrace}");
		else
			logEntry.AppendLine(TryMinifyJson(requestBody)).AppendLine(TryMinifyJson(responseBody));

		logEntry
			.AppendJoin("", efCoreQueries)
			.AppendLine("\n" + new string('=', 50));

		try {
			File.AppendAllText(Path.Combine(logDir, logFileName), logEntry.ToString());
			logger.LogDebug("Logged to {file}", logFileName);
		}
		catch (Exception ex) {
			logger.LogError(ex, "Failed to write to log file {file}", logFileName);
		}
	}

	private static string SerializeHeaders(IHeaderDictionary headers) {
		try {
			return JsonSerializer.Serialize(
				headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
				UJsonOptions.Default);
		}
		catch {
			return "{}";
		}
	}

	private static string TryMinifyJson(string json) {
		if (string.IsNullOrWhiteSpace(json))
			return string.Empty;

		try {
			using JsonDocument doc = JsonDocument.Parse(json);
			return JsonSerializer.Serialize(doc.RootElement, UJsonOptions.Default);
		}
		catch {
			return json;
		}
	}
}

public class EfCoreQueryLoggerProvider(List<string> queries) : ILoggerProvider {
	public ILogger CreateLogger(string categoryName) {
		return new EfCoreQueryLogger(queries);
	}

	public void Dispose() { }
}

public class EfCoreQueryLogger(List<string> queries) : ILogger {
	public IDisposable BeginScope<TState>(TState state) => null;

	public bool IsEnabled(LogLevel logLevel) => true;

	public void Log<TState>(
		LogLevel logLevel,
		EventId eventId,
		TState state,
		Exception? exception,
		Func<TState, Exception, string> formatter) {
		if (eventId.Name != "Microsoft.EntityFrameworkCore.Database.Command.CommandExecuted") return;
		string message = formatter(state, exception);
		queries.Add($"Query {queries.Count + 1}: {message}");
		Debug.WriteLine($"EF Core Query: {message}");
	}
}