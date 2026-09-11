namespace SinaMN75U.Middlewares;

public sealed class ApiKeyMiddleware(RequestDelegate next, ILocalizationService ls) {
	public async Task InvokeAsync(HttpContext context) {
		if (ShouldSkip(context.Request.Path)) {
			await next(context);
			return;
		}

		string body = await context.ReadBodyOnceAsync();
		string jsonSource = string.IsNullOrWhiteSpace(body) ? "{}" : body;

		try {
			if (!JsonSerializer.Deserialize<JsonElement>(jsonSource).TryGetProperty("apiKey", out JsonElement apiKey) || apiKey.GetString() != Core.App.ApiKey) {
				await WriteErrorAsync(context, Usc.UnAuthorized, ls.Get("invalidAPIKey"));
				return;
			}
		}
		catch {
			await WriteErrorAsync(context, Usc.BadRequest, ls.Get("invalidJSONBody"));
			return;
		}

		await next(context);
	}

	private static bool ShouldSkip(PathString path) =>
		!Core.App.Middleware.RequireApiKey ||
		path.StartsWithSegments("/models", StringComparison.OrdinalIgnoreCase) ||
		path.StartsWithSegments("/api/Health", StringComparison.OrdinalIgnoreCase) ||
		path.StartsWithSegments("/api/Ipg/Verify", StringComparison.OrdinalIgnoreCase) ||
		path.StartsWithSegments("/api/Ipg/Gateway", StringComparison.OrdinalIgnoreCase);
	
	private static async Task WriteErrorAsync(HttpContext ctx, Usc status, string msg) {
		if (ctx.Response.HasStarted) return;
		await new UResponse(status, msg).ToResult().ExecuteAsync(ctx);
	}
}