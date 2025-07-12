namespace SinaMN75U.Middlewares;

public class UMiddleware(RequestDelegate next, IConfiguration config) {
	public async Task InvokeAsync(HttpContext context) {
		// Only process POST requests
		if (!context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase)) {
			await next(context);
			return;
		}

		Stopwatch sw = Stopwatch.StartNew();
		string decodedRequestBody;
		string responseBody = string.Empty;
		Exception? exception = null;

		// Enable buffering to read the request body once
		context.Request.EnableBuffering();

		using StreamReader reader = new(context.Request.Body, Encoding.UTF8, leaveOpen: true);
		string rawRequestBody = await reader.ReadToEndAsync();
		context.Request.Body.Position = 0;

		// 🔓 Decode request body from Base64 if configured
		if (bool.TryParse(config["MiddlewareDecryptParams"], out bool decrypt) && decrypt) {
			try {
				byte[] bytes = Convert.FromBase64String(rawRequestBody);
				decodedRequestBody = Encoding.UTF8.GetString(bytes);
			}
			catch {
				context.Response.StatusCode = StatusCodes.Status400BadRequest;
				await context.Response.WriteAsync("Invalid base64 request body");
				return;
			}
		}
		else {
			decodedRequestBody = rawRequestBody;
		}

		// 🔑 API Key Validation
		if (bool.TryParse(config["MiddlewareRequireApiKey"], out bool requireApiKey) && requireApiKey) {
			try {
				JsonElement json = JsonSerializer.Deserialize<JsonElement>(decodedRequestBody);
				if (!json.TryGetProperty("apiKey", out JsonElement apiKey) ||
				    apiKey.GetString() != config["ApiKey"]) {
					context.Response.StatusCode = StatusCodes.Status401Unauthorized;
					await context.Response.WriteAsync("Invalid API key");
					return;
				}
			}
			catch {
				context.Response.StatusCode = StatusCodes.Status400BadRequest;
				await context.Response.WriteAsync("Invalid JSON body");
				return;
			}
		}

		// 🧼 Intercept response
		Stream originalBody = context.Response.Body;
		using MemoryStream memStream = new MemoryStream();
		context.Response.Body = memStream;

		try {
			await next(context);

			memStream.Position = 0;
			using StreamReader resReader = new StreamReader(memStream);
			responseBody = await resReader.ReadToEndAsync();
			memStream.Position = 0;

			// 📦 Encode response to base64 if configured
			if (bool.TryParse(config["MiddlewareEncryptResponse"], out bool encrypt) && encrypt) {
				string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(responseBody));
				byte[] base64Bytes = Encoding.UTF8.GetBytes(base64);
				context.Response.ContentLength = base64Bytes.Length;
				await originalBody.WriteAsync(base64Bytes);
			}
			else {
				await memStream.CopyToAsync(originalBody);
			}
		}
		catch (Exception ex) {
			exception = ex;
			context.Response.StatusCode = 500;
			await context.Response.WriteAsync("Internal server error");
		}
		finally {
			context.Response.Body = originalBody;

			// 🧾 Logging (only base64-encoded request, raw response)
			if (bool.TryParse(config["MiddlewareLog"], out bool log) && log) {
				LogToFile(
					DateTime.UtcNow,
					context.Request.Method,
					context.Request.Path,
					context.Response.StatusCode,
					sw.ElapsedMilliseconds,
					rawRequestBody, // log original base64-encoded request
					responseBody, // log unencoded raw response
					exception
				);
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
			requestBody = requestBody.EncodeJson(),
			responseBody = responseBody.EncodeJson(),
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
		catch {
			// fail silently
		}
	}
}