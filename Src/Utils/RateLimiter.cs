namespace SinaMN75U.Utils;

public static class RateLimiter {
	public static void AddURateLimiter(this IServiceCollection services) {
		services.AddRateLimiter(o => {
			o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
				RateLimitPartition.GetFixedWindowLimiter(
					partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "global",
					factory: _ => new FixedWindowRateLimiterOptions {
						PermitLimit = 2,
						QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
						QueueLimit = 4,
						Window = TimeSpan.FromSeconds(1)
					}
				));

			o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
				RateLimitPartition.GetFixedWindowLimiter(
					partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "global",
					factory: _ => new FixedWindowRateLimiterOptions {
						PermitLimit = 200,
						QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
						QueueLimit = 0,
						Window = TimeSpan.FromMinutes(1)
					}
				));
		});
	}
}