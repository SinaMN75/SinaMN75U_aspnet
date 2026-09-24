namespace SinaMN75U.Middlewares;

public sealed class ApiLogMiddleware(RequestDelegate next, ITokenService ts, IApiLogQueue queue) {
	private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase) {
		"Authorization", "Cookie", "Set-Cookie", "X-Api-Key", "apiKey", "Proxy-Authorization"
	};

	private static readonly HashSet<string> SensitiveFields = new(StringComparer.OrdinalIgnoreCase) {
		"password", "newPassword", "oldPassword", "currentPassword", "confirmPassword", "repeatPassword",
		"token", "refreshToken", "accessToken", "apiKey", "apiToken", "otp", "verificationCode", "clientSecret", "secret",
		"client_secret", "access_token", "refresh_token", "api_key", "id_token"
	};

	private static readonly string[] SecretBodyPaths = ["/api/AppSettings/ReadAll", "/api/AppSettings/Update"];
	private const string Redacted = "***REDACTED***";
	private const int MaxBodyLength = 20_000;

	public async Task InvokeAsync(HttpContext context) {
		if (!ShouldHandle(context)) {
			await next(context);
			return;
		}

		context.Response.OnStarting(() => {
			if (!context.Response.Headers.ContainsKey("X-Trace-Id")) context.Response.Headers["X-Trace-Id"] = context.TraceIdentifier;
			return Task.CompletedTask;
		});

		string requestBody = IsTextual(context.Request.ContentType) ? await context.ReadBodyOnceAsync() : "";

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
			
			bool shouldLog = context.Response.StatusCode is not (>= 200 and <= 299) || Core.App.Middleware.LogSuccess;

			string responseBody = "";
			if (shouldLog && IsTextual(context.Response.ContentType))
				try {
					captureStream.Seek(0, SeekOrigin.Begin);
					using StreamReader reader = new(captureStream, leaveOpen: true);
					responseBody = await reader.ReadToEndAsync();
				}
				catch {
					responseBody = "";
				}

			context.Response.Body = originalResponseStream;
			captureStream.Seek(0, SeekOrigin.Begin);
			await captureStream.CopyToAsync(originalResponseStream);

			if (shouldLog) EnqueueLog(context, requestBody, responseBody, captureStream.Length, sw.ElapsedMilliseconds, exception);
		}
	}

	private void EnqueueLog(HttpContext context, string requestBody, string responseBody, long responseLength, long durationMs, Exception? exception) {
		(Guid? UserId, string? UserName, string? Email, string? Roles, string? FirstName, string? LastName, string? PhoneNumber) userData = TryExtractUserData(context, requestBody);
		int requestSize = context.Request.ContentLength is { } reqLen ? (int)Math.Min(reqLen, int.MaxValue) : Encoding.UTF8.GetByteCount(requestBody);
		int responseSize = (int)Math.Min(responseLength, int.MaxValue);
		bool secretBodies = SecretBodyPaths.Any(x => context.Request.Path.StartsWithSegments(x, StringComparison.OrdinalIgnoreCase));
		string? requestHeaders = null;
		string? responseHeaders = null;
		if (Core.App.Middleware.LogHeaders) {
			requestHeaders = SerializeHeaders(context.Request.Headers);
			responseHeaders = SerializeHeaders(context.Response.Headers);
		}

		queue.Enqueue(new ApiLogCreateParams {
			Method = context.Request.Method,
			Path = context.Request.Path,
			StatusCode = context.Response.StatusCode,
			DurationMs = durationMs,
			UserId = userData.UserId,
			UserName = userData.UserName,
			UserEmail = userData.Email,
			UserRoles = userData.Roles,
			UserFirstName = userData.FirstName,
			UserLastName = userData.LastName,
			UserPhoneNumber = userData.PhoneNumber,
			IpAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim() ?? context.Connection.RemoteIpAddress?.ToString(),
			QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
			RequestBody = secretBodies ? Redacted : Truncate(RedactBody(requestBody)),
			ResponseBody = secretBodies ? Redacted : Truncate(RedactBody(responseBody)),
			RequestHeaders = requestHeaders,
			ResponseHeaders = responseHeaders,
			UserAgent = context.Request.Headers.UserAgent.FirstOrDefault(),
			Host = Environment.MachineName,
			RequestSizeBytes = requestSize,
			ResponseSizeBytes = responseSize,
			ExceptionType = exception?.GetType().Name,
			ExceptionMessage = exception?.Message,
			StackTrace = exception?.StackTrace
		});
	}

	private static bool ShouldHandle(HttpContext ctx) {
		if (!Core.App.Middleware.Log) return false;
		string? path = ctx.Request.Path.Value;
		if (path == null || !ctx.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)) return false;
		if (ctx.Request.Path.StartsWithSegments("/" + RouteTags.Log.TrimEnd('/'), StringComparison.OrdinalIgnoreCase) ||
		    ctx.Request.Path.StartsWithSegments("/" + RouteTags.Dashboard.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)) return false;
		return !path.Contains("media", StringComparison.OrdinalIgnoreCase) && !path.Contains("download", StringComparison.OrdinalIgnoreCase);
	}

	private static bool IsTextual(string? contentType) =>
		string.IsNullOrEmpty(contentType) ||
		contentType.Contains("json", StringComparison.OrdinalIgnoreCase) ||
		contentType.Contains("xml", StringComparison.OrdinalIgnoreCase) ||
		contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase) ||
		contentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase);

	private static string Truncate(string body) => body.Length <= MaxBodyLength ? body : body[..MaxBodyLength] + "...<truncated>";

	internal static string RedactBody(string body) {
		if (string.IsNullOrWhiteSpace(body)) return body;
		try {
			JsonNode? node = JsonNode.Parse(body);
			if (node == null || !RedactNode(node)) return body;
			return node.ToJsonString(Core.Default);
		}
		catch {
			return body;
		}
	}

	private static bool RedactNode(JsonNode node) {
		bool changed = false;
		switch (node) {
			case JsonObject obj:
				foreach (string key in obj.Select(kv => kv.Key).ToList()) {
					if (SensitiveFields.Contains(key) || key.Contains("password", StringComparison.OrdinalIgnoreCase)) {
						if (obj[key] is JsonValue) {
							obj[key] = Redacted;
							changed = true;
						}
					}
					else if (obj[key] is { } child)
						changed |= RedactNode(child);
				}

				break;
			case JsonArray arr:
				changed = arr.OfType<JsonNode>().Aggregate(changed, (current, item) => current | RedactNode(item));
				break;
		}

		return changed;
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

	private (Guid? UserId, string? UserName, string? Email, string? Roles, string? FirstName, string? LastName, string? PhoneNumber) TryExtractUserData(HttpContext ctx, string requestBody) {
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

		if (token.IsNullOrEmpty()) return (null, null, null, null, null, null, null);

		try {
			JwtClaimData? claims = ts.ExtractClaims(token);
			if (claims == null) return (null, null, null, null, null, null, null);
			string? roles = claims.Tags.Any() ? string.Join(",", claims.Tags) : null;
			return (claims.Id, claims.UserName ?? claims.FullName, claims.Email, roles, claims.FirstName, claims.LastName, claims.PhoneNumber);
		}
		catch {
			return (null, null, null, null, null, null, null);
		}
	}
}