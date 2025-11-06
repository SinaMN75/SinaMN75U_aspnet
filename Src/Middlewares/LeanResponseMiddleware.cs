using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace SinaMN75U.Middlewares;

/// <summary>
/// Strips every unnecessary header, enables Brotli/Gzip, and adds smart cache-control for cached hits.
/// Call once in Program.cs after building the app.
/// </summary>
public static class LeanResponseExtensions {
	public static WebApplication UseLeanResponses(this WebApplication app) {
		// 1. Kestrel – disable Server header (safe, runs on startup)
		app.Lifetime.ApplicationStarted.Register(() => {
			IOptions<KestrelServerOptions>? kestrel = app.Services.GetService<IOptions<KestrelServerOptions>>();
			if (kestrel != null) kestrel.Value.AddServerHeader = false;
		});

		// 2. HEADER CLEANUP: MUST RUN BEFORE compression & body write
		app.Use(async (context, next) => {
			// Hook into OnStarting: runs just before headers are sent
			context.Response.OnStarting(() => {
				IHeaderDictionary headers = context.Response.Headers;

				// --- Remove unnecessary headers ---
				headers.Remove("X-Powered-By");
				headers.Remove("Server");
				headers.Remove("X-AspNet-Version");
				headers.Remove("Via");
				headers.Remove("Date"); // Optional

				// --- Dev headers only in non-prod ---
				if (app.Environment.IsProduction()) {
					headers.Remove("X-Cache-Hit");
					headers.Remove("X-Cache-Store");
				}

				// --- Smart Cache-Control for cached JSON ---
				if (context.Response.StatusCode == 200 &&
				    context.Response.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) == true &&
				    headers.ContainsKey("X-Cache-Hit")) {
					headers.CacheControl = "public, max-age=120";
					headers.Expires = DateTimeOffset.UtcNow.AddSeconds(120).ToString("R");
				}

				return Task.CompletedTask;
			});

			await next();
		});

		// 3. Compression – runs AFTER headers are set, BEFORE body write
		app.UseResponseCompression();

		return app;
	}
}