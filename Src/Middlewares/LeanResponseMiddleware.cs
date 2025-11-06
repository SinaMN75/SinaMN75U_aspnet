using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace SinaMN75U.Middlewares;

/// <summary>
/// Strips every unnecessary header, enables Brotli/Gzip, and adds smart cache-control for cached hits.
/// Call once in Program.cs after building the app.
/// </summary>
public static class LeanResponseExtensions {
	public static WebApplication UseLeanResponses(this WebApplication app) {
		// 1. Response compression (Brotli > Gzip)
		app.Services.GetRequiredService<IServiceProvider>(); // force registration
		app.Services.GetRequiredService<ResponseCompressionMiddleware>(); // ensure added
		app.UseResponseCompression();

		// 2. Kestrel – no Server header
		app.Lifetime.ApplicationStarted.Register(() => {
			IOptions<KestrelServerOptions>? kestrel = app.Services.GetService<IOptions<KestrelServerOptions>>();
			if (kestrel?.Value != null) kestrel.Value.AddServerHeader = false;
		});

		// 3. Global header-stripping + optional cache headers
		app.Use(async (ctx, next) => {
			await next();

			IHeaderDictionary h = ctx.Response.Headers;

			// --- Always remove ---
			h.Remove("X-Powered-By");
			h.Remove("Server");
			h.Remove("X-AspNet-Version");
			h.Remove("Via");

			// --- Optional: strip Date (29 B) ---
			h.Remove("Date");

			// --- Dev-only headers ---
			if (app.Environment.IsProduction()) {
				h.Remove("X-Cache-Hit");
				h.Remove("X-Cache-Store");
			}

			// --- Smart Cache-Control for cached JSON hits ---
			if (ctx.Response.StatusCode == 200 && ctx.Response.ContentType?.StartsWith("application/json") == true && h.ContainsKey("X-Cache-Hit")) {
				h.CacheControl = $"public, max-age=120";
				h.Expires = DateTimeOffset.UtcNow.AddSeconds(120).ToString("R");
			}
		});

		return app;
	}
}