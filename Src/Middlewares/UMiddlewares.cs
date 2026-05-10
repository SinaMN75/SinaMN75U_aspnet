using BadHttpRequestException = Microsoft.AspNetCore.Http.BadHttpRequestException;

namespace SinaMN75U.Middlewares;

public sealed class UMiddleware(
	RequestDelegate next,
	ILocalizationService ls,
	ITokenService ts,
	IRequestLogger logger
) {
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
			try {
				context.Request.Body.Seek(0, SeekOrigin.Begin);
			}
			catch {
				// ignored
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
			await WriteErrorAsync(context, Usc.BadRequest, ls.Get("InvalidRequestFormat"));
		}
		catch (Exception ex) {
			exception = ex;
			if (!context.Response.HasStarted && !earlyError)
				await WriteErrorAsync(context, Usc.InternalServerError, ls.Get("InternalServerError"));
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

			if (Core.App.Middleware.EncryptResponse && responseBody.Length > 0) {
				byte[] payload = Encoding.UTF8.GetBytes(SimpleCrypto.Encrypt(responseBody));
				context.Response.ContentLength = payload.Length;
				await originalResponseStream.WriteAsync(payload);
			}
			else {
				captureStream.Seek(0, SeekOrigin.Begin);
				await captureStream.CopyToAsync(originalResponseStream);
			}

			sw.Stop();

			_ = Task.Run(() => logger.TryLog(new RequestLogDto {
				Timestamp = DateTime.UtcNow,
				Method = context.Request.Method,
				Path = context.Request.Path,
				StatusCode = context.Response.StatusCode,
				DurationMs = sw.ElapsedMilliseconds,
				RawRequest = rawRequestBody,
				DecodedRequest = decodedRequestBodyForLog,
				Response = responseBody,
				Exception = exception
			}));
		}
	}

	private static bool ShouldHandle(HttpContext ctx) {
		return ctx.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) &&
		       ctx.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase) &&
		       ctx.Request.Path.Value?.Contains("media", StringComparison.OrdinalIgnoreCase) != true;
	}

	private async Task<(string? Processed, string? Decoded)> PreProcessRequestAsync(HttpContext ctx, string raw) {
		if (raw.Length > 100_000) {
			await WriteErrorAsync(ctx, Usc.PayloadTooLarge, "RequestTooLarge");
			return (null, raw);
		}

		string decoded = raw;
		string processed = raw;

		if (Core.App.Middleware.DecryptParams) {
			try {
				string decrypted = SimpleCrypto.Decrypt(raw);
				decoded = decrypted;
				processed = decrypted;
			}
			catch {
				await WriteErrorAsync(ctx, Usc.BadRequest, ls.Get("InvalidBase64RequestBody"));
				return (null, raw);
			}
		}

		if (Core.App.Middleware.RequireApiKey)
			try {
				JsonElement json = JsonSerializer.Deserialize<JsonElement>(processed);
				if (!json.TryGetProperty("apiKey", out JsonElement token) || token.GetString() != Core.App.ApiKey) {
					await WriteErrorAsync(ctx, Usc.UnAuthorized, ls.Get("InvalidAPIKey"));
					return (null, decoded);
				}
			}
			catch {
				await WriteErrorAsync(ctx, Usc.BadRequest, ls.Get("InvalidJsonBody"));
				return (null, decoded);
			}

		if (Core.App.Middleware.RequireRefreshToken)
			try {
				JsonElement json = JsonSerializer.Deserialize<JsonElement>(processed);
				JwtClaimData? userData = ts.ExtractClaims(json.GetStringOrNull("token"));
				if (userData != null && userData.IsExpired) {
					await WriteErrorAsync(ctx, Usc.ExpiredToken, ls.Get("TokenExpired"));
					return (null, decoded);
				}
			}
			catch {
				await WriteErrorAsync(ctx, Usc.BadRequest, ls.Get("InvalidJsonBody"));
				return (null, decoded);
			}

		return (processed, decoded);
	}

	private static async Task WriteErrorAsync(HttpContext ctx, Usc status, string msg) {
		if (ctx.Response.HasStarted) return;
		await new UResponse(status, msg).ToResult().ExecuteAsync(ctx);
	}
}
