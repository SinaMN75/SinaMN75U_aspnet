namespace SinaMN75U.Middlewares;

using Microsoft.Extensions.Options;

public class CacheOptions {
	public int Minutes { get; set; } = 1;
	public int MaxBodySizeToCache { get; set; } = 1024 * 1024;
}

public class CustomRequestResponseFilter(ILocalStorageService cache, IOptions<CacheOptions> options) : IEndpointFilter {
	private readonly CacheOptions _options = options.Value;

	public async ValueTask<object?> InvokeAsync(
		EndpointFilterInvocationContext context,
		EndpointFilterDelegate next) {
		HttpContext httpContext = context.HttpContext;
		HttpRequest request = httpContext.Request;

		string url = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";

		Dictionary<string, string> headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());

		string? body = null;
		if (request.ContentLength > 0 &&
		    request.ContentLength <= _options.MaxBodySizeToCache &&
		    request.Body.CanRead) {
			try {
				long originalPosition = request.Body.Position;
				request.Body.Position = 0;
				using StreamReader reader = new (request.Body, Encoding.UTF8, leaveOpen: true);
				body = await reader.ReadToEndAsync();
				request.Body.Position = originalPosition;
			}
			catch (Exception) {
				// ignored
			}
		}

		string cacheKey = GenerateCacheKey(url, headers, body);

		string? cachedResponse = cache.Get(cacheKey);
		if (cachedResponse != null) {
			httpContext.Response.Headers.TryAdd("X-Cache-Hit", "true");
			return Results.Json(
				JsonSerializer.Deserialize<object>(cachedResponse),
				statusCode: StatusCodes.Status200OK);
		}

		object? result = await next(context);

		if (result is IResult originalResult && httpContext.Response.StatusCode == StatusCodes.Status200OK) {
			return new ModifiedResult(originalResult, cacheKey, cache, TimeSpan.FromMinutes(_options.Minutes));
		}

		return result;
	}

	private static string GenerateCacheKey(string url, Dictionary<string, string> headers, string? body) {
		using MemoryStream stream = new();
		using Utf8JsonWriter writer = new(stream);

		writer.WriteStartObject();
		writer.WriteString("url", url);

		writer.WriteStartObject("headers");
		foreach (KeyValuePair<string, string> header in headers) {
			writer.WriteString(header.Key, header.Value);
		}

		writer.WriteEndObject();

		if (!string.IsNullOrEmpty(body)) {
			writer.WriteString("body", body);
		}

		writer.WriteEndObject();

		writer.Flush();
		return Convert.ToBase64String(stream.ToArray());
	}
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
	public static TBuilder Cache<TBuilder>(
		this TBuilder builder,
		Action<CacheOptions>? configure = null)
		where TBuilder : IEndpointConventionBuilder {
		CacheOptions options = new();
		configure?.Invoke(options);

		return builder.AddEndpointFilter(async (context, next) => {
			ILocalStorageService cache = context.HttpContext.RequestServices.GetRequiredService<ILocalStorageService>();
			CustomRequestResponseFilter filter = new(cache, Options.Create(options));
			return await filter.InvokeAsync(context, next);
		});
	}
}