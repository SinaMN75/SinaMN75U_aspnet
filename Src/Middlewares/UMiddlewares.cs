using BadHttpRequestException = Microsoft.AspNetCore.Http.BadHttpRequestException;

namespace SinaMN75U.Middlewares;

public class UMiddleware(RequestDelegate next, IConfiguration config) {
	private static readonly Lock LogLock = new();

	public async Task InvokeAsync(HttpContext context) {
		if (!context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase)) {
			await next(context);
			return;
		}

		Stopwatch sw = Stopwatch.StartNew();
		string responseBody = string.Empty;
		Exception? exception = null;
		string rawRequestBody = string.Empty;

		try {
			context.Request.EnableBuffering();

			// Read entire request body into memory
			using StreamReader requestReader = new(context.Request.Body, Encoding.UTF8, leaveOpen: true);
			rawRequestBody = await requestReader.ReadToEndAsync();

			string decodedRequestBody;

			// Decode Base64 if enabled
			bool decryptParams = bool.Parse(config["MiddlewareDecryptParams"] ?? "false");
			if (decryptParams) {
				try {
					byte[] bytes = Convert.FromBase64String(rawRequestBody);
					decodedRequestBody = Encoding.UTF8.GetString(bytes);
				}
				catch (FormatException) {
					context.Response.StatusCode = StatusCodes.Status400BadRequest;
					await context.Response.WriteAsync("Invalid base64 request body");
					return;
				}
			}
			else {
				decodedRequestBody = rawRequestBody;
			}

			// API Key validation
			bool requireApiKey = bool.Parse(config["MiddlewareRequireApiKey"] ?? "false");
			if (requireApiKey) {
				try {
					JsonElement json = JsonSerializer.Deserialize<JsonElement>(decodedRequestBody);
					if (!json.TryGetProperty("apiKey", out JsonElement apiKeyElement) ||
					    apiKeyElement.GetString() != config["ApiKey"]) {
						context.Response.StatusCode = StatusCodes.Status401Unauthorized;
						await context.Response.WriteAsync("Invalid API key");
						return;
					}
				}
				catch (JsonException) {
					context.Response.StatusCode = StatusCodes.Status400BadRequest;
					await context.Response.WriteAsync("Invalid JSON body");
					return;
				}
			}

			// Replace request body with decoded version
			byte[] decodedBodyBytes = Encoding.UTF8.GetBytes(decodedRequestBody);
			context.Request.Body = new MemoryStream(decodedBodyBytes);

			// Capture response
			Stream originalBody = context.Response.Body;
			await using MemoryStream responseCapture = new();
			context.Response.Body = responseCapture;

			try {
				await next(context);
			}
			catch (BadHttpRequestException ex) when (ex.Message.Contains("Failed to read parameter")) {
				exception = ex;
				context.Response.StatusCode = StatusCodes.Status400BadRequest;
				await context.Response.WriteAsync("Invalid request format");
				responseBody = "Invalid request format";
			}
			finally {
				// Read captured response
				responseCapture.Position = 0;
				using StreamReader resReader = new(responseCapture, leaveOpen: true);
				responseBody = await resReader.ReadToEndAsync();

				// Restore original stream BEFORE writing
				context.Response.Body = originalBody;

				// Encode response if enabled
				bool encryptResponse = bool.Parse(config["MiddlewareEncryptResponse"] ?? "false");
				if (encryptResponse) {
					string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(responseBody));
					byte[] base64Bytes = Encoding.UTF8.GetBytes(base64);
					context.Response.ContentLength = base64Bytes.Length;
					await originalBody.WriteAsync(base64Bytes);
				}
				else {
					responseCapture.Position = 0;
					await responseCapture.CopyToAsync(originalBody);
				}
			}
		}
		catch (Exception ex) {
			exception = ex;
			if (context.Response.HasStarted)
				return;

			context.Response.StatusCode = StatusCodes.Status500InternalServerError;
			await context.Response.WriteAsync("Internal server error");
			responseBody = "Internal server error";
		}
		finally {
			sw.Stop();

			// Fixed logging condition: log errors (400+) or success (200-299) based on config
			bool logEnabled = bool.Parse(config["MiddlewareLog"] ?? "false");
			bool isSuccess = context.Response.StatusCode is >= 200 and <= 299;
			bool shouldLog = logEnabled && (!isSuccess || bool.Parse(config["MiddlewareLogSuccess"] ?? "false"));

			if (shouldLog) {
				LogToFile(
					timestamp: DateTime.UtcNow,
					method: context.Request.Method,
					path: context.Request.Path,
					statusCode: context.Response.StatusCode,
					elapsedMs: sw.ElapsedMilliseconds,
					requestBody: rawRequestBody,
					responseBody: responseBody,
					exception: exception
				);
			}
		}
	}

	private static void LogToFile(
		DateTime timestamp,
		string method,
		string path,
		int statusCode,
		long elapsedMs,
		string requestBody,
		string responseBody,
		Exception? exception = null) {
		try {
			DateTime now = DateTime.Now;
			string logDir = Path.Combine("wwwroot", "Logs", now.Year.ToString(), now.Month.ToString("00"));
			Directory.CreateDirectory(logDir);

			bool isSuccess = statusCode >= 200 && statusCode <= 299;
			string logFileName = $"{now:dd}_{(isSuccess ? "success" : "failed")}.json";
			string logFilePath = Path.Combine(logDir, logFileName);

			var logEntry = new {
				summary = $"{timestamp:yyyy-MM-dd HH:mm:ss} | {method} {path} | {statusCode} | {elapsedMs}ms",
				requestBody = TryParseJson(requestBody) ?? requestBody,
				responseBody = TryParseJson(responseBody) ?? responseBody,
				exception = exception != null
					? new {
						type = exception.GetType().Name,
						message = exception.Message,
						stackTrace = exception.StackTrace
					}
					: null
			};

			lock (LogLock) {
				List<object> logs;

				if (File.Exists(logFilePath)) {
					string existingJson = File.ReadAllText(logFilePath);
					logs = JsonSerializer.Deserialize<List<object>>(existingJson, UJsonOptions.Default) ?? new List<object>();
				}
				else {
					logs = new List<object>();
				}

				logs.Add(logEntry);
				string updatedJson = JsonSerializer.Serialize(logs, UJsonOptions.Default);
				File.WriteAllText(logFilePath, updatedJson);
			}
		}
		catch {
			// Silently ignore logging failures to avoid crashing request
		}
	}

	private static object? TryParseJson(string json) {
		try {
			JsonDocument doc = JsonDocument.Parse(json);
			return JsonElementToDynamic(doc.RootElement);
		}
		catch {
			return null;
		}
	}

	private static object? JsonElementToDynamic(JsonElement element) => element.ValueKind switch {
		JsonValueKind.Object => element.EnumerateObject().ToDictionary(
			prop => prop.Name,
			prop => JsonElementToDynamic(prop.Value)
		),
		JsonValueKind.Array => element.EnumerateArray().Select(JsonElementToDynamic).ToList(),
		JsonValueKind.String => element.GetString(),
		JsonValueKind.Number => element.TryGetInt64(out long l) ? l : element.TryGetDouble(out double d) ? d : null,
		JsonValueKind.True => true,
		JsonValueKind.False => false,
		_ => null
	};
}