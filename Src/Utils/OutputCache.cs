namespace SinaMN75U.Utils;

public static class OutputCacheSetup {
	public static void AddUOutputCache(this IServiceCollection services) {
		services.AddOutputCache(options => {
			options.AddPolicy("VaryByEverything", policy =>
				policy.Expire(TimeSpan.FromMinutes(5))
					.SetVaryByHeader("*")
					.SetVaryByQuery("*")
					.VaryByValue(BodyHashProvider));

			options.AddPolicy("VaryByBodyOnly", policy =>
				policy.Expire(TimeSpan.FromMinutes(10)).VaryByValue(BodyHashProvider));
		});
	}

	public static void UseUOutputCache(this IApplicationBuilder app) {
		app.UseOutputCache();
	}


	static KeyValuePair<string, string> BodyHashProvider(HttpContext context) {
		if (context.Request.ContentLength == 0)
			return new("body", "empty");

		using SHA256 sha256 = SHA256.Create();
		using StreamReader reader = new StreamReader(
			context.Request.Body,
			encoding: Encoding.UTF8,
			detectEncodingFromByteOrderMarks: false,
			leaveOpen: true); // Don't dispose the body stream

		string body = reader.ReadToEnd();
		context.Request.Body.Position = 0; // Reset stream position for downstream handlers

		string hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(body)));
		return new KeyValuePair<string, string>("body", hash);
	}
}