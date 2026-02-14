using BadHttpRequestException = Microsoft.AspNetCore.Http.BadHttpRequestException;

namespace SinaMN75U.Middlewares;

public sealed class UMiddleware(RequestDelegate next) {
	private static readonly Lock LogLock = new();

	public async Task InvokeAsync(HttpContext context) {
		if (!ShouldHandle(context)) {
			await next(context);
			return;
		}

		Stopwatch sw = Stopwatch.StartNew();
		string rawRequestBody;
		string decodedRequestBodyForLog = string.Empty;
		string responseBody;
		Exception? exception = null;
		bool earlyError = false;

		context.Request.EnableBuffering();

		try {
			using StreamReader reader = new(context.Request.Body, Encoding.UTF8, leaveOpen: true);
			rawRequestBody = await reader.ReadToEndAsync();
		}
		finally {
			// Always seek, even if read fails
			try {
				context.Request.Body.Seek(0, SeekOrigin.Begin);
			}
			catch {
				/* ignore non-seekable streams */
			}
		}

		Stream originalResponseStream = context.Response.Body;
		using MemoryStream captureStream = new();
		context.Response.Body = captureStream;

		try {
			(string? Processed, string? Decoded) result = await PreProcessRequestAsync(context, rawRequestBody);
			decodedRequestBodyForLog = result.Decoded ?? rawRequestBody;

			if (result.Processed == null) {
				earlyError = true;
				return;
			}

			byte[] bodyBytes = Encoding.UTF8.GetBytes(result.Processed);
			context.Request.Body = new MemoryStream(bodyBytes);
			context.Request.ContentLength = bodyBytes.Length;

			await next(context);
		}
		catch (BadHttpRequestException ex) when (ex.Message.Contains("Failed to read parameter")) {
			exception = ex;
			await WriteErrorAsync(context, 400, "Invalid request format");
		}
		catch (Exception ex) {
			exception = ex;
			if (!context.Response.HasStarted && !earlyError)
				await WriteErrorAsync(context, 500, "Internal server error");
		}
		finally {
			try {
				captureStream.Seek(0, SeekOrigin.Begin);
				responseBody = await new StreamReader(captureStream, leaveOpen: true).ReadToEndAsync();
			}
			catch {
				responseBody = "<response unreadable>";
			}

			context.Response.Body = originalResponseStream;

			bool encrypt = AppSettings.Instance.Middleware.EncryptResponse;
			if (encrypt && responseBody.Length > 0) {
				byte[] payload = Encoding.UTF8.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes(responseBody)));
				context.Response.ContentLength = payload.Length;
				await originalResponseStream.WriteAsync(payload);
			}
			else {
				captureStream.Seek(0, SeekOrigin.Begin);
				await captureStream.CopyToAsync(originalResponseStream);
			}

			sw.Stop();

			// Fire-and-forget logging
			_ = Task.Run(() => TryLog(
				context, sw.ElapsedMilliseconds, rawRequestBody, decodedRequestBodyForLog, responseBody, exception));
		}
	}

	private static bool ShouldHandle(HttpContext ctx) =>
		ctx.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) &&
		ctx.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase) &&
		ctx.Request.Path.Value?.Contains("media", StringComparison.OrdinalIgnoreCase) != true;

	private async Task<(string? Processed, string? Decoded)> PreProcessRequestAsync(HttpContext ctx, string raw) {
		// Optional: limit request size
		if (raw.Length > 100_000) // 100 KB
		{
			await WriteErrorAsync(ctx, 413, "Request too large");
			return (null, raw);
		}

		bool decrypt = AppSettings.Instance.Middleware.DecryptParams;
		string decoded = raw;
		string processed = raw;

		if (decrypt) {
			if (!TryDecodeBase64(raw, out byte[] decodedBytes)) {
				await WriteErrorAsync(ctx, 400, "Invalid base64 request body");
				return (null, raw);
			}

			decoded = Encoding.UTF8.GetString(decodedBytes);
			processed = decoded;
		}

		bool needKey = AppSettings.Instance.Middleware.RequireApiKey;
		if (needKey) {
			try {
				JsonElement json = JsonSerializer.Deserialize<JsonElement>(processed);
				if (!json.TryGetProperty("apiKey", out JsonElement token) || token.GetString() != AppSettings.Instance.ApiKey) {
					await WriteErrorAsync(ctx, 401, "Invalid API key");
					return (null, decoded);
				}
			}
			catch {
				await WriteErrorAsync(ctx, 400, "Invalid JSON body");
				return (null, decoded);
			}
		}

		return (processed, decoded);
	}

	private static bool TryDecodeBase64(string input, out byte[] result) {
		try {
			string clean = input.Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "");
			if (clean.Length % 4 != 0)
				clean += new string('=', 4 - clean.Length % 4);
			result = Convert.FromBase64String(clean);
			return true;
		}
		catch {
			result = Array.Empty<byte>();
			return false;
		}
	}

	private static async Task WriteErrorAsync(HttpContext ctx, int status, string msg) {
		if (ctx.Response.HasStarted) return;

		ctx.Response.StatusCode = status;
		ctx.Response.ContentType = "application/json";
		byte[] payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { error = msg }));
		ctx.Response.ContentLength = payload.Length;
		await ctx.Response.Body.WriteAsync(payload);
		await ctx.Response.Body.FlushAsync();
	}

	private void TryLog(HttpContext ctx, long ms, string rawReq, string decodedReq, string res, Exception? ex) {
		if (!AppSettings.Instance.Middleware.Log) return;
		if (ctx.Response.StatusCode is >= 200 and <= 299 && !AppSettings.Instance.Middleware.LogSuccess) return;

		// Truncate large bodies in logs
		const int maxLen = 10_000;
		if (rawReq.Length > maxLen) rawReq = rawReq[..maxLen] + "...<truncated>";
		if (decodedReq.Length > maxLen) decodedReq = decodedReq[..maxLen] + "...<truncated>";
		if (res.Length > maxLen) res = res[..maxLen] + "...<truncated>";

		LogToFile(DateTime.UtcNow, ctx.Request.Method, ctx.Request.Path, ctx.Response.StatusCode, ms, rawReq, decodedReq, res, ex);
	}

	private static void LogToFile(DateTime ts, string method, string path, int status, long ms, string rawReq, string decodedReq, string res, Exception? ex) {
		try {
			DateTime now = DateTime.Now;
			string dir = Path.Combine("wwwroot", "Logs", now.Year.ToString(), $"{now.Month:00}");
			Directory.CreateDirectory(dir);
			string file = Path.Combine(dir, $"{now:dd}_{(status < 300 ? "success" : "failed")}.json");

			var entry = new {
				summary = $"{ts:yyyy-MM-dd HH:mm:ss} | {method} {path} | {status} | {ms}ms",
				requestBodyRaw = TryParseJson(rawReq) ?? rawReq,
				requestBody = TryParseJson(decodedReq) ?? decodedReq,
				responseBody = TryParseJson(res) ?? res,
				exception = ex is null ? null : new { type = ex.GetType().Name, message = ex.Message, stackTrace = ex.StackTrace }
			};

			lock (LogLock) {
				List<object> list = File.Exists(file)
					? JsonSerializer.Deserialize<List<object>>(File.ReadAllText(file), UJsonOptions.Default) ?? new()
					: [];
				list.Add(entry);
				File.WriteAllText(file, JsonSerializer.Serialize(list, UJsonOptions.Default));
			}
		}
		catch {
			/* ignore */
		}
	}

	private static object? TryParseJson(string s) {
		try {
			return JsonElementToDynamic(JsonDocument.Parse(s).RootElement);
		}
		catch {
			return null;
		}
	}

	private static object? JsonElementToDynamic(JsonElement e) => e.ValueKind switch {
		JsonValueKind.Object => e.EnumerateObject().ToDictionary(p => p.Name, p => JsonElementToDynamic(p.Value)),
		JsonValueKind.Array => e.EnumerateArray().Select(JsonElementToDynamic).ToList(),
		JsonValueKind.String => e.GetString(),
		JsonValueKind.Number => e.TryGetInt64(out long l) ? l : e.GetDouble(),
		JsonValueKind.True => true,
		JsonValueKind.False => false,
		_ => null
	};
}