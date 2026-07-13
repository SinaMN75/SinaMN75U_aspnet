namespace SinaMN75U.Middlewares;

public sealed class ApiLogMiddleware(RequestDelegate next, ITokenService ts) {
	private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase) {
		"Authorization", "Cookie", "Set-Cookie", "X-Api-Key", "apiKey", "Proxy-Authorization"
	};

	public async Task InvokeAsync(HttpContext context, IApiLogService service) {
		if (!ShouldHandle(context)) {
			await next(context);
			return;
		}

		context.Response.OnStarting(() => {
			if (!context.Response.Headers.ContainsKey("X-Trace-Id")) context.Response.Headers["X-Trace-Id"] = context.TraceIdentifier;
			return Task.CompletedTask;
		});

		string requestBody = await context.ReadBodyOnceAsync();

		Stream originalResponseStream = context.Response.Body;
		using MemoryStream captureStream = new();
		context.Response.Body = captureStream;

		Stopwatch sw = Stopwatch.StartNew();
		Exception? exception = null;

		try {
			await next(context);
		}
		catch (Exception ex) {
			exception = ex;
			throw;
		}
		finally {
			sw.Stop();
			exception ??= context.Items["ApiLogException"] as Exception;

			string responseBody;
			try {
				captureStream.Seek(0, SeekOrigin.Begin);
				responseBody = await new StreamReader(captureStream, leaveOpen: true).ReadToEndAsync();
			}
			catch {
				responseBody = "";
			}

			context.Response.Body = originalResponseStream;
			captureStream.Seek(0, SeekOrigin.Begin);
			await captureStream.CopyToAsync(originalResponseStream);

			(Guid? UserId, string? UserName, string? Email, string? Roles) userData = TryExtractUserData(context, requestBody);
			string? requestHeaders = null;
			string? responseHeaders = null;
			if (Core.App.Middleware.LogHeaders) {
				requestHeaders = SerializeHeaders(context.Request.Headers);
				responseHeaders = SerializeHeaders(context.Response.Headers);
			}

			await service.Create(new ApiLogCreateParams {
				Method = context.Request.Method,
				Path = context.Request.Path,
				StatusCode = context.Response.StatusCode,
				DurationMs = sw.ElapsedMilliseconds,
				UserId = userData.UserId,
				UserName = userData.UserName,
				UserEmail = userData.Email,
				UserRoles = userData.Roles,
				IpAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim() ?? context.Connection.RemoteIpAddress?.ToString(),
				TraceId = context.TraceIdentifier,
				QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
				RequestBody = requestBody,
				ResponseBody = responseBody,
				RequestHeaders = requestHeaders,
				ResponseHeaders = responseHeaders,
				UserAgent = context.Request.Headers["User-Agent"].FirstOrDefault(),
				Host = Environment.MachineName,
				RequestSizeBytes = Encoding.UTF8.GetByteCount(requestBody),
				ResponseSizeBytes = Encoding.UTF8.GetByteCount(responseBody),
				ExceptionType = exception?.GetType().Name,
				ExceptionMessage = exception?.Message,
				StackTrace = exception?.StackTrace
			}, CancellationToken.None);
		}
	}

	private static bool ShouldHandle(HttpContext ctx) {
		string? path = ctx.Request.Path.Value;
		if (path == null || !ctx.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)) return false;
		return path.Contains("media", StringComparison.OrdinalIgnoreCase) != true && path.Contains("download", StringComparison.OrdinalIgnoreCase) != true;
	}

	private static string SerializeHeaders(IHeaderDictionary headers) {
		try {
			Dictionary<string, string> map = headers.ToDictionary(h => h.Key, h => SensitiveHeaders.Contains(h.Key) ? "***REDACTED***" : h.Value.ToString(), StringComparer.OrdinalIgnoreCase);
			return JsonSerializer.Serialize(map, Core.Default);
		}
		catch {
			return "{}";
		}
	}

	private (Guid? UserId, string? UserName, string? Email, string? Roles) TryExtractUserData(HttpContext ctx, string requestBody) {
		string? token = null;

		try {
			JsonElement json = JsonSerializer.Deserialize<JsonElement>(requestBody);
			token = json.GetStringOrNull("token");
		}
		catch {
			// ignored
		}

		if (string.IsNullOrWhiteSpace(token)) {
			string? authHeader = ctx.Request.Headers.Authorization.FirstOrDefault();
			if (authHeader != null && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
				token = authHeader["Bearer ".Length..].Trim();
		}

		if (string.IsNullOrWhiteSpace(token)) token = ctx.Request.Query["token"].FirstOrDefault();
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
