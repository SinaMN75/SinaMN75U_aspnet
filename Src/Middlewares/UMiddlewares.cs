namespace SinaMN75U.Middlewares;

public class UMiddleware(RequestDelegate next, IConfiguration config) {
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

			string decodedRequestBody;
			using (StreamReader reader = new(context.Request.Body, Encoding.UTF8, leaveOpen: true)) {
				rawRequestBody = await reader.ReadToEndAsync();
				context.Request.Body.Position = 0;

				// Decode request body
				if (bool.Parse(config["MiddlewareDecryptParams"] ?? "false")) {
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

				// API Key Validation
				if (bool.Parse(config["MiddlewareRequireApiKey"] ?? "false")) {
					try {
						JsonElement json = JsonSerializer.Deserialize<JsonElement>(decodedRequestBody);
						if (!json.TryGetProperty("apiKey", out JsonElement apiKey) ||
						    apiKey.GetString() != config["ApiKey"]) {
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
			}

			// Replace the request body with the decoded content
			MemoryStream decodedBodyStream = new(Encoding.UTF8.GetBytes(decodedRequestBody));
			context.Request.Body = decodedBodyStream;

			// Intercept response
			Stream originalBody = context.Response.Body;
			using (MemoryStream memStream = new()) {
				context.Response.Body = memStream;

				try {
					await next(context);

					memStream.Position = 0;
					using StreamReader resReader = new(memStream);
					responseBody = await resReader.ReadToEndAsync();
					memStream.Position = 0;

					// Encode response to base64 if configured
					if (bool.Parse(config["MiddlewareEncryptResponse"] ?? "false")) {
						string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(responseBody));
						byte[] base64Bytes = Encoding.UTF8.GetBytes(base64);
						context.Response.ContentLength = base64Bytes.Length;
						await originalBody.WriteAsync(base64Bytes);
					}
					else {
						await memStream.CopyToAsync(originalBody);
					}
				}
				catch (BadHttpRequestException ex) when (ex.Message.Contains("Failed to read parameter")) {
					exception = ex;
					context.Response.StatusCode = StatusCodes.Status400BadRequest;
					await context.Response.WriteAsync("Invalid request format");
					responseBody = "Invalid request format";
				}
				finally {
					context.Response.Body = originalBody;
					await decodedBodyStream.DisposeAsync();
				}
			}
		}
		catch (Exception ex) {
			exception = ex;
			context.Response.StatusCode = StatusCodes.Status500InternalServerError;
			await context.Response.WriteAsync("Internal server error");
			responseBody = "Internal server error";
		}
		finally {
			if (bool.Parse(config["MiddlewareLog"] ?? "false")) {
				LogToFile(DateTime.UtcNow,
					context.Request.Method,
					context.Request.Path,
					context.Response.StatusCode,
					sw.ElapsedMilliseconds,
					rawRequestBody,
					responseBody,
					exception
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
		DateTime now = DateTime.Now;
		string logDir = Path.Combine("wwwroot", "Logs", now.Year.ToString(), now.Month.ToString("00"));
		Directory.CreateDirectory(logDir);

		bool isSuccess = statusCode is >= 200 and <= 299;
		string logFileName = $"{now:dd}_{(isSuccess ? "success" : "failed")}.json";
		string logFilePath = Path.Combine(logDir, logFileName);

		var logEntry = new {
			summary = $"{timestamp:yyyy-MM-dd HH:mm:ss} | {method} {path} | {statusCode} | {elapsedMs}ms",
			requestBody = TryParseJson(requestBody.EncodeJson()) ?? requestBody,
			responseBody = TryParseJson(responseBody.EncodeJson()) ?? responseBody,
			exception = exception != null
				? new {
					type = exception.GetType().Name,
					message = exception.Message,
					stackTrace = exception.StackTrace
				}
				: null
		};

		try {
			List<object> logs;

			if (File.Exists(logFilePath)) {
				string existingJson = File.ReadAllText(logFilePath);
				logs = JsonSerializer.Deserialize<List<object>>(existingJson, UJsonOptions.Default) ?? [];
			}
			else {
				logs = [];
			}

			logs.Add(logEntry);
			string updatedJson = JsonSerializer.Serialize(logs, UJsonOptions.Default);
			File.WriteAllText(logFilePath, updatedJson);
		}
		catch {
			// ignored
		}
	}

	private static object? TryParseJson(string json) {
		try {
			return JsonElementToDynamic(JsonDocument.Parse(json).RootElement);
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