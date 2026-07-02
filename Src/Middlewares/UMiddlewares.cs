using BadHttpRequestException = Microsoft.AspNetCore.Http.BadHttpRequestException;

namespace SinaMN75U.Middlewares;

public sealed class UMiddleware(
	RequestDelegate next,
	ILocalizationService ls,
	ITokenService ts,
	IRequestLogger logger
) {
	private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase) {
		"Authorization", "Cookie", "Set-Cookie", "X-Api-Key", "apiKey", "Proxy-Authorization"
	};

	public async Task InvokeAsync(HttpContext context) {
		if (!ShouldHandle(context)) {
			await next(context);
			return;
		}

		Stopwatch sw = Stopwatch.StartNew();
		string rawRequestBody;
		string decodedRequestBodyForLog = string.Empty;
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
		
		context.Response.OnStarting(() => {
			if (!context.Response.Headers.ContainsKey("X-Trace-Id"))
				context.Response.Headers["X-Trace-Id"] = context.TraceIdentifier;
			return Task.CompletedTask;
		});

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
			string responseBody;
			try {
				captureStream.Seek(0, SeekOrigin.Begin);
				responseBody = await new StreamReader(captureStream, leaveOpen: true).ReadToEndAsync();
			}
			catch {
				responseBody = "<response unreadable>";
			}
			
			string? requestHeadersJson = null;
			string? responseHeadersJson = null;
			if (Core.App.Middleware.LogHeaders) {
				requestHeadersJson = SerializeHeaders(context.Request.Headers);
				responseHeadersJson = SerializeHeaders(context.Response.Headers);
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

			(Guid? UserId, string? UserName, string? Email, string? Roles) userData = TryExtractUserData(context, decodedRequestBodyForLog);
			logger.TryLog(new RequestLogDto {
				Timestamp = DateTime.UtcNow,
				Method = context.Request.Method,
				Path = context.Request.Path,
				QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
				StatusCode = context.Response.StatusCode,
				DurationMs = sw.ElapsedMilliseconds,
				RawRequest = rawRequestBody,
				DecodedRequest = decodedRequestBodyForLog,
				Response = responseBody,
				RequestHeaders = requestHeadersJson,
				ResponseHeaders = responseHeadersJson,
				RequestSizeBytes = Encoding.UTF8.GetByteCount(rawRequestBody),
				ResponseSizeBytes = Encoding.UTF8.GetByteCount(responseBody),
				UserAgent = context.Request.Headers["User-Agent"].FirstOrDefault(),
				TraceId = context.TraceIdentifier,
				Host = Environment.MachineName,
				Exception = exception,
				UserId = userData.UserId,
				UserName = userData.UserName,
				UserEmail = userData.Email,
				UserRoles = userData.Roles,
				IpAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim()
				            ?? context.Connection.RemoteIpAddress?.ToString()
			});
		}
	}

	private static bool ShouldHandle(HttpContext ctx) {
		string? path = ctx.Request.Path.Value;
		if (path == null || !ctx.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)) return false;
		return path.Contains("media", StringComparison.OrdinalIgnoreCase) != true &&
		       path.Contains("download", StringComparison.OrdinalIgnoreCase) != true;
	}

	private static string SerializeHeaders(IHeaderDictionary headers) {
		try {
			Dictionary<string, string> map = headers.ToDictionary(
				h => h.Key,
				h => SensitiveHeaders.Contains(h.Key) ? "***REDACTED***" : h.Value.ToString(),
				StringComparer.OrdinalIgnoreCase);
			return JsonSerializer.Serialize(map, Core.Default);
		}
		catch {
			return "{}";
		}
	}

	private async Task<(string? Processed, string? Decoded)> PreProcessRequestAsync(HttpContext ctx, string raw) {

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
		
		string jsonSource = string.IsNullOrWhiteSpace(processed) ? "{}" : processed;

		if (Core.App.Middleware.RequireApiKey)
			try {
				JsonElement json = JsonSerializer.Deserialize<JsonElement>(jsonSource);
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
				JsonElement json = JsonSerializer.Deserialize<JsonElement>(jsonSource);
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

	private (Guid? UserId, string? UserName, string? Email, string? Roles) TryExtractUserData(HttpContext ctx, string decodedBody) {
		string? token = null;

		try {
			JsonElement json = JsonSerializer.Deserialize<JsonElement>(decodedBody);
			token = json.GetStringOrNull("token");
		}
		catch {
			// not a JSON body - fall through to header/query lookups below
		}

		if (string.IsNullOrWhiteSpace(token)) {
			string? authHeader = ctx.Request.Headers.Authorization.FirstOrDefault();
			if (authHeader != null && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
				token = authHeader["Bearer ".Length..].Trim();
		}

		if (string.IsNullOrWhiteSpace(token))
			token = ctx.Request.Query["token"].FirstOrDefault();

		if (string.IsNullOrWhiteSpace(token)) return (null, null, null, null);

		try {
			JwtClaimData? claims = ts.ExtractClaims(token);
			if (claims == null) return (null, null, null, null);
			string? roles = claims.Tags.Any() ? string.Join(",", claims.Tags) : null;
			return (claims.Id, claims.UserName ?? claims.FullName, claims.Email, roles);
		}
		catch {
			return (null, null, null, null);
		}
	}
}