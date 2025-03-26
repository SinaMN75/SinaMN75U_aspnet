namespace SinaMN75U.Utils;

public static class OutputCache {
	public static void AddUOutputCache(this IServiceCollection services) => services.AddOutputCache(x => x.AddPolicy("default", y => {
		y.Cache();
		y.SetVaryByHeader("*");
		y.SetVaryByQuery("*");
		y.Expire(TimeSpan.FromHours(1));
		y.AddPolicy<CustomCachePolicy>().VaryByValue(context => {
				context.Request.EnableBuffering();
				using StreamReader reader = new(context.Request.Body, leaveOpen: true);
				Task<string> body = reader.ReadToEndAsync();
				context.Request.Body.Position = 0;
				KeyValuePair<string, string> keyVal = new("requestBody", body.Result);
				return keyVal;
			}
		);
	}));

	internal class CustomCachePolicy : IOutputCachePolicy {
		public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation) {
			context.AllowCacheLookup = true;
			context.AllowCacheStorage = true;
			context.AllowLocking = true;
			context.EnableOutputCaching = true;
			context.ResponseExpirationTimeSpan = TimeSpan.FromHours(6);
			return ValueTask.CompletedTask;
		}

		public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation) => ValueTask.CompletedTask;

		public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation) => ValueTask.CompletedTask;
	}
}