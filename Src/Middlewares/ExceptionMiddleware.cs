using BadHttpRequestException = Microsoft.AspNetCore.Http.BadHttpRequestException;

namespace SinaMN75U.Middlewares;

public sealed class ExceptionMiddleware(RequestDelegate next, ILocalizationService ls, IWebHostEnvironment env) {
	public async Task InvokeAsync(HttpContext context) {
		try {
			await next(context);
		}
		catch (BadHttpRequestException ex) when (ex.Message.Contains("Failed to read parameter")) {
			context.Items["ApiLogException"] = ex;
			await WriteErrorAsync(context, Usc.BadRequest, ls.Get("InvalidRequestFormat"));
		}
		catch (Exception ex) {
			context.Items["ApiLogException"] = ex;
			if (!context.Response.HasStarted) await WriteErrorAsync(context, Usc.InternalServerError, env.IsDevelopment() ? $"{ex.GetType().Name}: {ex.Message}" : ls.Get("InternalServerError"));
		}
	}

	private static async Task WriteErrorAsync(HttpContext ctx, Usc status, string msg) {
		if (ctx.Response.HasStarted) return;
		await new UResponse(status, msg).ToResult().ExecuteAsync(ctx);
	}
}
