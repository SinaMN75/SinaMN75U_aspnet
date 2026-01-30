namespace SinaMN75U.Utils;

public static class RateLimiter {
	public static void AddURateLimiter(this IServiceCollection services) {
		services.AddRateLimiter(o => {
			o.AddConcurrencyLimiter("global-concurrency", opts => {
				opts.PermitLimit = 50; // simultaneous requests
			});

			o.AddFixedWindowLimiter("per-ip-minute", opts => {
				opts.PermitLimit = 200;
				opts.Window = TimeSpan.FromMinutes(1);
				opts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
				opts.QueueLimit = 10;
			});

			o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
				RateLimitPartition.GetFixedWindowLimiter(
					partitionKey: ctx.GetRealIp() ?? "unknown",
					factory: _ => new FixedWindowRateLimiterOptions {
						PermitLimit = 200,
						Window = TimeSpan.FromMinutes(1),
						QueueLimit = 10
					}));
		});
	}

	private static string? GetRealIp(this HttpContext ctx) => ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim() ?? ctx.Connection.RemoteIpAddress?.ToString();
}