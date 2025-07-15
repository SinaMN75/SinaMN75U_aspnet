namespace SinaMN75U.Middlewares;

public sealed class ApiRequestLoggingMiddleware(RequestDelegate next, IServiceProvider serviceProvider) {
	public async Task InvokeAsync(HttpContext context) {
		HttpRequest request = context.Request;

		if (!request.Path.StartsWithSegments("/api") || request.Path.StartsWithSegments("/api/media") || request.Path.StartsWithSegments("/api/practino")) {
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

			if (request.Body.CanRead)
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

			// Capture response
			Stream originalResponseBody = context.Response.Body;
			using MemoryStream responseBuffer = new();
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
					DateTime.UtcNow,
					request.Method,
					request.Path,
					context.Response.StatusCode,
					stopwatch.ElapsedMilliseconds,
					requestBody,
					responseBody.FirstChars(1000),
					efCoreQueries,
					exception);
			}
			catch (Exception) {
				// ignored
			}
		}
	}

	private void LogToFile(
		DateTime timestamp,
		string method,
		string path,
		int statusCode,
		long elapsedMs,
		string requestBody,
		string responseBody,
		List<string> efCoreQueries,
		Exception? exception = null) {
		DateTime now = DateTime.Now;
		string logDir = Path.Combine("wwwroot", "Logs", now.Year.ToString(), now.Month.ToString("00"));
		Directory.CreateDirectory(logDir);

		bool isSuccess = statusCode is >= 200 and <= 299;
		string logFileName = $"{now:dd}_{(isSuccess ? "success" : "failed")}.json";

		var logEntry = new {
			timestamp = timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
			method,
			path,
			statusCode,
			elapsedMs,
			requestBody = TryMinifyJson(requestBody),
			responseBody = TryMinifyJson(responseBody),
			efCoreQueries = efCoreQueries.ToArray(),
			exception = exception != null
				? new {
					type = exception.GetType().Name,
					message = exception.Message,
					stackTrace = exception.StackTrace
				}
				: null
		};

		try {
			string json = JsonSerializer.Serialize(logEntry, UJsonOptions.Default);
			File.AppendAllText(Path.Combine(logDir, logFileName), json + "," + Environment.NewLine);
		}
		catch (Exception) {
			// ignored
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
	public ILogger CreateLogger(string categoryName) => new EfCoreQueryLogger(queries);

	public void Dispose() { }
}

public class EfCoreQueryLogger(List<string> queries) : ILogger {
	public IDisposable? BeginScope<TState>(TState state) => null;

	public bool IsEnabled(LogLevel logLevel) => true;

	public void Log<TState>(
		LogLevel logLevel,
		EventId eventId,
		TState state,
		Exception? exception,
		Func<TState, Exception?, string> formatter) {
		if (eventId.Name != "Microsoft.EntityFrameworkCore.Database.Command.CommandExecuted") return;
		string message = formatter(state, exception);
		queries.Add($"Query {queries.Count + 1}: {message}");
		Debug.WriteLine($"EF Core Query: {message}");
	}
}