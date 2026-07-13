namespace SinaMN75U.Middlewares;

public sealed class RefreshTokenMiddleware(RequestDelegate next, ILocalizationService ls, ITokenService ts) {
	public async Task InvokeAsync(HttpContext context) {
		if (!Core.App.Middleware.RequireRefreshToken) {
			await next(context);
			return;
		}

		string body = await context.ReadBodyOnceAsync();
		string jsonSource = string.IsNullOrWhiteSpace(body) ? "{}" : body;

		try {
			JsonElement json = JsonSerializer.Deserialize<JsonElement>(jsonSource);
			JwtClaimData? userData = ts.ExtractClaims(json.GetStringOrNull("token"));
			if (userData is { IsExpired: true }) {
				await WriteErrorAsync(context, Usc.ExpiredToken, ls.Get("TokenExpired"));
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
