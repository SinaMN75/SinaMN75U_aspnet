namespace SinaMN75U.Middlewares;

public sealed class ApiKeyMiddleware(RequestDelegate next, ILocalizationService ls) {
	public async Task InvokeAsync(HttpContext context) {
		if (!Core.App.Middleware.RequireApiKey) {
			await next(context);
			return;
		}

		string body = await context.ReadBodyOnceAsync();
		string jsonSource = string.IsNullOrWhiteSpace(body) ? "{}" : body;

		try {
			if (!JsonSerializer.Deserialize<JsonElement>(jsonSource).TryGetProperty("apiKey", out JsonElement apiKey) || apiKey.GetString() != Core.App.ApiKey) {
				await WriteErrorAsync(context, Usc.UnAuthorized, ls.Get("InvalidAPIKey"));
				return;
			}
		}
		catch {
			await WriteErrorAsync(context, Usc.BadRequest, ls.Get("InvalidJsonBody"));
			return;
		}

		await next(context);
	}

	private static async Task WriteErrorAsync(HttpContext ctx, Usc status, string msg) {
		if (ctx.Response.HasStarted) return;
		await new UResponse(status, msg).ToResult().ExecuteAsync(ctx);
	}
}
