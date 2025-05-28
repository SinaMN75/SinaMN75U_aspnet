namespace SinaMN75U.Middlewares;

public class CustomRequestResponseFilter(ILocalStorageService cache, int minutes) : IEndpointFilter {
	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext c, EndpointFilterDelegate n) {
		HttpContext httpContext = c.HttpContext;
		HttpRequest request = httpContext.Request;
		string url = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
		Dictionary<string, string> headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
		string? body = null;

		if (request is { ContentLength: > 0, Body.CanRead: true }) {
			try {
				long originalPosition = request.Body.Position;
				request.Body.Position = 0;
				using StreamReader reader = new(request.Body, Encoding.UTF8, leaveOpen: true);
				body = await reader.ReadToEndAsync();
				request.Body.Position = originalPosition;
			}
			catch (Exception) {
				// ignored
			}
		}

		string cacheKey = GenerateCacheKey(url, headers, body);

		Console.WriteLine(cacheKey);

		string? cachedResponse = cache.Get(cacheKey);
		if (cachedResponse != null) {
			httpContext.Response.Headers.TryAdd("X-Cache-Hit", "true");
			return Results.Json(
				JsonSerializer.Deserialize<object>(cachedResponse),
				statusCode: StatusCodes.Status200OK);
		}

		object? result = await n(c);

		if (result is IResult originalResult && httpContext.Response.StatusCode == StatusCodes.Status200OK) {
			return new ModifiedResult(originalResult, cacheKey, cache, TimeSpan.FromMinutes(minutes));
		}

		return result;
	}

	private static string GenerateCacheKey(string url, Dictionary<string, string> headers, string? body) => $"{url}{string.Join(";", headers.OrderBy(h => h.Key).Select(h => $"{h.Key}={h.Value}"))}{body ?? ""}";
}

public class ModifiedResult(
	IResult originalResult,
	string cacheKey,
	ILocalStorageService cache,
	TimeSpan cacheDuration)
	: IResult {
	public async Task ExecuteAsync(HttpContext httpContext) {
		Stream originalBodyStream = httpContext.Response.Body;
		await using MemoryStream memoryStream = new();
		httpContext.Response.Body = memoryStream;

		await originalResult.ExecuteAsync(httpContext);

		if (httpContext.Response.StatusCode == StatusCodes.Status200OK) {
			try {
				memoryStream.Seek(0, SeekOrigin.Begin);
				string responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

				cache.Set(cacheKey, responseBody, cacheDuration);
				httpContext.Response.Headers.TryAdd("X-Cache-Store", "true");
			}
			catch (Exception) {
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
			CustomRequestResponseFilter filter = new(cache, minutes);
			return await filter.InvokeAsync(context, next);
		});
	}
}