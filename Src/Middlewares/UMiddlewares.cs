using BadHttpRequestException = Microsoft.AspNetCore.Http.BadHttpRequestException;

namespace SinaMN75U.Middlewares;

public sealed class UMiddleware(
	RequestDelegate next,
	ILocalizationService ls,
	ITokenService ts
) {
	public async Task InvokeAsync(HttpContext context) {
		context.Request.EnableBuffering();

		string rawRequestBody;
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

		Stream? originalResponseStream = null;
		MemoryStream? captureStream = null;
		if (Core.App.Middleware.EncryptResponse) {
			originalResponseStream = context.Response.Body;
			captureStream = new MemoryStream();
			context.Response.Body = captureStream;
		}

		try {
			(string? Processed, string? Decoded) result = await PreProcessRequestAsync(context, rawRequestBody);

			if (result.Processed == null) return;

			byte[] bodyBytes = Encoding.UTF8.GetBytes(result.Processed);
			context.Request.Body = new MemoryStream(bodyBytes);
			context.Request.ContentLength = bodyBytes.Length;

			await next(context);
		}
		catch (BadHttpRequestException ex) when (ex.Message.Contains("Failed to read parameter")) {
			context.Items["ApiLogException"] = ex;
			await WriteErrorAsync(context, Usc.BadRequest, ls.Get("InvalidRequestFormat"));
		}
		catch (Exception ex) {
			context.Items["ApiLogException"] = ex;
			if (!context.Response.HasStarted)
				await WriteErrorAsync(context, Usc.InternalServerError, ls.Get("InternalServerError"));
		}
		finally {
			if (captureStream != null && originalResponseStream != null) {
				context.Response.Body = originalResponseStream;

				captureStream.Seek(0, SeekOrigin.Begin);
				string responseBody = await new StreamReader(captureStream, leaveOpen: true).ReadToEndAsync();

				if (responseBody.Length > 0) {
					byte[] payload = Encoding.UTF8.GetBytes(SimpleCrypto.Encrypt(responseBody));
					context.Response.ContentLength = payload.Length;
					await originalResponseStream.WriteAsync(payload);
				}
				else {
					captureStream.Seek(0, SeekOrigin.Begin);
					await captureStream.CopyToAsync(originalResponseStream);
				}

				captureStream.Dispose();
			}
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
}
