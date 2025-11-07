using Microsoft.Extensions.Options;

namespace SinaMN75U.Middlewares;

public static class LeanResponseExtensions {
	public static WebApplication UseLeanResponses(this WebApplication app) {
		app.Lifetime.ApplicationStarted.Register(() => {
			IOptions<KestrelServerOptions>? kestrel = app.Services.GetService<IOptions<KestrelServerOptions>>();
			if (kestrel != null) kestrel.Value.AddServerHeader = false;
		});
		app.Use(async (context, next) => {
			context.Response.OnStarting(() => {
				IHeaderDictionary headers = context.Response.Headers;
				headers.Remove("X-Powered-By");
				headers.Remove("Server");
				headers.Remove("X-AspNet-Version");
				headers.Remove("Via");
				headers.Remove("date");
				headers.Remove("x-cache-store");
				headers.Remove("expires");
				headers.Remove("cache-control");
				headers.Remove("x-cache-hit");
				headers.Remove("content-length");
				return Task.CompletedTask;
			});

			await next();
		});
		app.UseResponseCompression();

		return app;
	}
}