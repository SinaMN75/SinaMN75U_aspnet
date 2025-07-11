namespace SinaMN75U.Middlewares;

public sealed class UMiddleware(IConfiguration config, RequestDelegate next) : IMiddleware {
	public async Task InvokeAsync(HttpContext context, RequestDelegate _) {
		if (!HttpMethods.IsPost(context.Request.Method)) {
			await next(context);
			return;
		}

		// Check if this request is loggable
		bool shouldLog = bool.TryParse(config["EnableLog"], out bool enableLogging) && enableLogging;
		bool isApiRequest = shouldLog &&
		                    context.Request.Path.StartsWithSegments("/api") &&
		                    !context.Request.Path.StartsWithSegments("/api/media") &&
		                    !context.Request.Path.StartsWithSegments("/api/practino");

		Stopwatch? stopwatch = isApiRequest ? Stopwatch.StartNew() : null;
		string requestBody = "";
		string responseBody = "";
		Exception? exception = null;

		// ========== 1. Decrypt base64 body if configured ==========
		if (bool.TryParse(config["DecryptParams"], out bool decrypt) && decrypt && context.Request.ContentLength > 0) {
			try {
				context.Request.EnableBuffering();
				using StreamReader reader = new(context.Request.Body, Encoding.UTF8, true, 1024, leaveOpen: true);
				string base64 = await reader.ReadToEndAsync();
				string decoded = base64.DecodeBase64();
				context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(decoded));
				context.Request.ContentLength = decoded.Length;
				context.Request.Body.Seek(0, SeekOrigin.Begin);
			}
			catch (FormatException) {
				context.Response.StatusCode = StatusCodes.Status400BadRequest;
				await context.Response.WriteAsync("Invalid Base64 input.");
				return;
			}
		}

		// ========== 2. Read body for logging and API Key ==========
		if (context.Request.Body.CanRead) {
			try {
				context.Request.EnableBuffering();
				using StreamReader reader = new(context.Request.Body, Encoding.UTF8, true, 1024, leaveOpen: true);
				requestBody = await reader.ReadToEndAsync();
				context.Request.Body.Seek(0, SeekOrigin.Begin);
			}
			catch {
				requestBody = "";
			}
		}

		// ========== 3. Validate API Key if required ==========
		if (bool.TryParse(config["RequireApiKey"], out bool requireApiKey) && requireApiKey) {
			if (!context.Request.ContentType?.Contains("application/json") ?? true) {
				context.Response.StatusCode = StatusCodes.Status400BadRequest;
				await context.Response.WriteAsync("Invalid content type");
				return;
			}

			try {
				JsonElement json = JsonSerializer.Deserialize<JsonElement>(requestBody);
				if (!json.TryGetProperty("apiKey", out JsonElement apiKey) || apiKey.GetString() != config["ApiKey"]) {
					context.Response.StatusCode = StatusCodes.Status401Unauthorized;
					await context.Response.WriteAsync("Invalid API key");
					return;
				}

				if (json.TryGetProperty("token", out JsonElement token))
					context.Items["JwtToken"] = token.GetString();
			}
			catch {
				context.Response.StatusCode = StatusCodes.Status400BadRequest;
				await context.Response.WriteAsync("Malformed JSON.");
				return;
			}
		}

		// ========== 4. Capture Response ==========
		Stream originalBody = context.Response.Body;
		await using MemoryStream responseStream = new();
		context.Response.Body = responseStream;

		try {
			await next(context);

			responseStream.Seek(0, SeekOrigin.Begin);
			responseBody = await new StreamReader(responseStream).ReadToEndAsync();
			responseStream.Seek(0, SeekOrigin.Begin);

			// Encrypt response if needed
			if (bool.TryParse(config["EncryptResponse"], out bool encrypt) && encrypt &&
			    context.Response.StatusCode == StatusCodes.Status200OK) {
				string encrypted = responseBody.EncodeBase64();
				responseStream.SetLength(0);
				await responseStream.WriteAsync(Encoding.UTF8.GetBytes(encrypted));
				responseStream.Seek(0, SeekOrigin.Begin);
			}

			await responseStream.CopyToAsync(originalBody);
		}
		catch (Exception ex) {
			exception = ex;
			context.Response.StatusCode = StatusCodes.Status500InternalServerError;
			responseBody = ex.ToString();
			throw;
		}
		finally {
			context.Response.Body = originalBody;

			if (isApiRequest && stopwatch is not null) {
				LogToFile(
					DateTime.UtcNow,
					context.Request.Method,
					context.Request.Path,
					context.Response.StatusCode,
					stopwatch.ElapsedMilliseconds,
					requestBody,
					responseBody.FirstChars(1000),
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
		try {
			DateTime now = DateTime.Now;
			string logDir = Path.Combine("wwwroot", "Logs", now.Year.ToString(), now.Month.ToString("00"));
			Directory.CreateDirectory(logDir);

			string fileName = $"{now:dd}_{(statusCode is >= 200 and <= 299 ? "success" : "failed")}.json";
			string logPath = Path.Combine(logDir, fileName);

			var log = new {
				timestamp = timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
				method,
				path,
				statusCode,
				elapsedMs,
				requestBody = TryMinifyJson(requestBody),
				responseBody = TryMinifyJson(responseBody),
				exception = exception != null
					? new {
						type = exception.GetType().Name,
						message = exception.Message,
						stackTrace = exception.StackTrace
					}
					: null
			};

			string json = JsonSerializer.Serialize(log, UJsonOptions.Default);
			File.AppendAllText(logPath, json + "," + Environment.NewLine);
		}
		catch {
			// Ignore log write failures
		}
	}

	private static string TryMinifyJson(string json) {
		if (string.IsNullOrWhiteSpace(json)) return string.Empty;
		try {
			using JsonDocument doc = JsonDocument.Parse(json);
			return JsonSerializer.Serialize(doc.RootElement, UJsonOptions.Default);
		}
		catch {
			return json;
		}
	}
}