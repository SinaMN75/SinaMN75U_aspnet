namespace SinaMN75U.Middlewares;

using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

public sealed class ApiRequestLoggingMiddleware(
	RequestDelegate next,
	ILogger<ApiRequestLoggingMiddleware> logger,
	IServiceProvider serviceProvider
) {
	public async Task InvokeAsync(HttpContext context) {
		HttpRequest request = context.Request;

		if (!request.Path.StartsWithSegments("/api")) {
			await next(context);
			return;
		}

		Stopwatch stopwatch = Stopwatch.StartNew();
		List<string> efCoreQueries = new();

		// Set up EF Core query logging
		using (IServiceScope scope = serviceProvider.CreateScope()) {
			IEnumerable<DbContext> dbContexts = scope.ServiceProvider.GetServices<DbContext>();
			foreach (DbContext dbContext in dbContexts) {
				dbContext.Database.SetCommandTimeout(TimeSpan.FromSeconds(30));
				ILoggerFactory loggerFactory = dbContext.GetService<ILoggerFactory>();
				loggerFactory.AddProvider(new EfCoreQueryLoggerProvider(efCoreQueries));
			}
		}

		string requestBody = "";
		if (request.Body.CanRead) {
			try {
				request.EnableBuffering();
				using StreamReader reader = new(
					request.Body,
					Encoding.UTF8,
					true,
					1024,
					true);

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
				responseBody,
				efCoreQueries
			);
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
		string responseBody,
		List<string> efCoreQueries) {
		try {
			DateTime now = DateTime.Now;
			string logDir = Path.Combine("Logs", now.Year.ToString(), now.Month.ToString("00"));
			Directory.CreateDirectory(logDir);

			// Determine log file name based on status code
			string logFileName = statusCode == 200 ? "success.txt" : $"{now:dd}.txt";

			string logEntry = $"{timestamp:yyyy-MM-dd HH:mm:ss} - {method} - {path} - {statusCode} - {elapsedMs}ms\n" +
			                  $"{SerializeHeaders(requestHeaders)}\n" +
			                  $"{SerializeHeaders(responseHeaders)}\n" +
			                  $"{TryMinifyJson(requestBody)}\n" +
			                  $"{TryMinifyJson(responseBody)}\n" +
			                  $"EF Core Queries ({efCoreQueries.Count}):\n{string.Join("\n", efCoreQueries)}\n" +
			                  new string('-', 50) + "\n";

			File.AppendAllText(Path.Combine(logDir, logFileName), logEntry);
		}
		catch (Exception ex) {
			logger.LogError(ex, "Log write failed");
		}
	}

	private static string SerializeHeaders(IHeaderDictionary headers) {
		try {
			return JsonSerializer.Serialize(
				headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
				UJsonOptions.Default
			);
		}
		catch {
			return "{}";
		}
	}

	private static string TryMinifyJson(string json) {
		try {
			return JsonSerializer.Serialize(JsonDocument.Parse(json).RootElement, UJsonOptions.Default);
		}
		catch {
			return json;
		}
	}
}

// EF Core query logger provider and logger implementation
public class EfCoreQueryLoggerProvider(List<string> queries) : ILoggerProvider {
	public ILogger CreateLogger(string categoryName) => new EfCoreQueryLogger(queries);

	public void Dispose() { }
}

public class EfCoreQueryLogger(List<string> queries) : ILogger {
	public IDisposable BeginScope<TState>(TState state) => null;

	public bool IsEnabled(LogLevel logLevel) => true;

	public void Log<TState>(
		LogLevel logLevel,
		EventId eventId,
		TState state,
		Exception exception,
		Func<TState, Exception, string> formatter) {
		if (eventId.Name != "Microsoft.EntityFrameworkCore.Database.Command.CommandExecuting") return;
		string message = formatter(state, exception);
		queries.Add(message);
	}
}