using Microsoft.Extensions.Options;

namespace SinaMN75U.Middlewares;

public static class LeanResponseExtensions {
	public static void UseLeanResponses(this WebApplication app) {
		app.Use(async (context, next) => {
			context.Response.OnStarting(() => {
				IHeaderDictionary headers = context.Response.Headers;
				headers.Remove("X-Powered-By");
				headers.Remove("Server"); 
				headers.Remove("X-AspNet-Version");
				return Task.CompletedTask;
			});
			await next();
		});
    
		app.UseResponseCompression();
		app.UseStaticFiles(new StaticFileOptions {
			OnPrepareResponse = ctx => ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000")
		});
	}
}