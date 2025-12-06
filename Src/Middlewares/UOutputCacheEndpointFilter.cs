namespace SinaMN75U.Middlewares;

public sealed class CacheResponseFilter(ILocalStorageService cache, int minutes) : IEndpointFilter {
	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next) {
		HttpContext httpContext = context.HttpContext;
		HttpRequest request = httpContext.Request;

		string body = await ReadRequestBodyAsync(request);
		string cacheKey = BuildCacheKey(request, body);

		if (cache.Get(cacheKey) is { } cachedResponse)
			return Results.Json(JsonSerializer.Deserialize<object>(cachedResponse));

		object? result = await next(context);

		if (result is IResult originalResult && httpContext.Response.StatusCode == StatusCodes.Status200OK)
			return new ModifiedResult(originalResult, cacheKey, cache, TimeSpan.FromSeconds(minutes));

		return result;
	}

	private static async Task<string> ReadRequestBodyAsync(HttpRequest request) {
		if (!(HttpMethods.IsPost(request.Method) || HttpMethods.IsPut(request.Method)))
			return string.Empty;

		try {
			request.Body.Position = 0;
			using StreamReader reader = new(request.Body, Encoding.UTF8, leaveOpen: true);
			string body = await reader.ReadToEndAsync();
			request.Body.Position = 0;
			return body.Trim();
		}
		catch {
			return string.Empty;
		}
	}

	private static string BuildCacheKey(HttpRequest request, string body) {
		StringBuilder sb = new StringBuilder(256)
			.Append(request.Method).Append(':')
			.Append(request.Path).Append('?')
			.Append(request.QueryString);

		if (request.Headers.TryGetValue("locale", out StringValues locale))
			sb.Append('-').Append(locale.ToString());

		if (!string.IsNullOrEmpty(body))
			sb.Append('-').Append(ComputeSha256Hash(body));

		string key = sb.ToString();
		Console.WriteLine(key);
		return key;
	}

	private static string ComputeSha256Hash(string rawData) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawData)));
}

public class ModifiedResult(IResult originalResult, string key, ILocalStorageService ls, TimeSpan duration) : IResult {
	public async Task ExecuteAsync(HttpContext httpContext) {
		Stream originalBodyStream = httpContext.Response.Body;
		await using MemoryStream memoryStream = new();
		httpContext.Response.Body = memoryStream;

		await originalResult.ExecuteAsync(httpContext);

		if (httpContext.Response.StatusCode == StatusCodes.Status200OK) {
			try {
				memoryStream.Seek(0, SeekOrigin.Begin);
				string responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

				ls.Set(key, responseBody, duration);
				httpContext.Response.Headers.TryAdd("X-Cache-Store", "true");
			}
			catch {
				// ignored
			}
			finally {
				memoryStream.Seek(0, SeekOrigin.Begin);
				await memoryStream.CopyToAsync(originalBodyStream);
			}
		}
	}
}

public static class CacheFilterExtensions {
	public static TBuilder Cache<TBuilder>(this TBuilder builder, int minutes)
		where TBuilder : IEndpointConventionBuilder {
		return builder.AddEndpointFilter(async (context, next) => {
			ILocalStorageService cache = context.HttpContext.RequestServices.GetRequiredService<ILocalStorageService>();
			CacheResponseFilter filter = new(cache, minutes);
			return await filter.InvokeAsync(context, next);
		});
	}
}