namespace SinaMN75U.Middlewares;

public sealed class CacheResponseFilter(ILocalStorageService cache, int minutes) : IEndpointFilter {
	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next) {
		HttpContext httpContext = context.HttpContext;
		HttpRequest request = httpContext.Request;

		string body = await ReadRequestBodyAsync(request);
		string cacheKey = BuildCacheKey(request, body);

		if (cache.Get(cacheKey) is { } cachedJson) {
			httpContext.Response.Headers.TryAdd("X-Cache-Hit", "true");
			return Results.Content(cachedJson, "application/json", Encoding.UTF8);
		}

		object? result = await next(context);

		if (result is IResult originalResult && httpContext.Response.StatusCode == StatusCodes.Status200OK) {
			return new ModifiedResult(originalResult, cacheKey, cache, TimeSpan.FromMinutes(minutes));
		}

		return result;
	}

	private static async Task<string> ReadRequestBodyAsync(HttpRequest request) {
		if (!HttpMethods.IsPost(request.Method) && !HttpMethods.IsPut(request.Method))
			return string.Empty;

		request.EnableBuffering();

		try {
			using StreamReader reader = new StreamReader(
				request.Body,
				encoding: Encoding.UTF8,
				detectEncodingFromByteOrderMarks: false,
				bufferSize: 1024,
				leaveOpen: true);

			string body = await reader.ReadToEndAsync();
			request.Body.Position = 0;
			return body.Trim();
		}
		catch {
			request.Body.Position = 0;
			return string.Empty;
		}
	}

	private static string BuildCacheKey(HttpRequest request, string body) {
		StringBuilder sb = new StringBuilder(256)
			.Append(request.Method).Append(':')
			.Append(request.Path);

		// Normalized query string (sorted params)
		string sortedQuery = string.Join("&",
			request.Query
				.OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
				.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

		if (!string.IsNullOrEmpty(sortedQuery))
			sb.Append('?').Append(sortedQuery);

		// Locale header
		if (request.Headers.TryGetValue("locale", out StringValues locale))
			sb.Append('-').Append(locale.ToString());

		// Body hash (first 1KB + length for large bodies)
		if (!string.IsNullOrEmpty(body)) {
			string hashableBody = body.Length <= 1024
				? body
				: body.Substring(0, 1024) + "$LEN:" + body.Length;

			sb.Append('-').Append(ComputeSha256Hash(hashableBody));
		}

		string key = sb.ToString();
		Console.WriteLine($"Cache Key: {key}");
		return key;
	}

	private static string ComputeSha256Hash(string rawData) =>
		Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawData)));
}

public class ModifiedResult : IResult {
	private readonly IResult _originalResult;
	private readonly string _key;
	private readonly ILocalStorageService _cache;
	private readonly TimeSpan _duration;

	public ModifiedResult(IResult originalResult, string key, ILocalStorageService cache, TimeSpan duration) {
		_originalResult = originalResult;
		_key = key;
		_cache = cache;
		_duration = duration;
	}

	public async Task ExecuteAsync(HttpContext httpContext) {
		Stream originalBodyStream = httpContext.Response.Body;

		await using MemoryStream memoryStream = new MemoryStream();
		httpContext.Response.Body = memoryStream;

		try {
			await _originalResult.ExecuteAsync(httpContext);

			if (httpContext.Response.StatusCode == StatusCodes.Status200OK) {
				memoryStream.Seek(0, SeekOrigin.Begin);
				using StreamReader reader = new StreamReader(memoryStream, Encoding.UTF8, leaveOpen: true);
				string responseBody = await reader.ReadToEndAsync();

				// Size limit: don't cache > 1MB responses
				if (responseBody.Length <= 1_048_576) {
					_cache.Set(_key, responseBody, _duration);
					httpContext.Response.Headers.TryAdd("X-Cache-Store", "true");
				}

				// Copy back to original stream
				memoryStream.Seek(0, SeekOrigin.Begin);
				await memoryStream.CopyToAsync(originalBodyStream);
			}
			else {
				// Non-200: copy directly
				memoryStream.Seek(0, SeekOrigin.Begin);
				await memoryStream.CopyToAsync(originalBodyStream);
			}
		}
		catch (Exception) {
			// On error: copy whatever we have
			memoryStream.Seek(0, SeekOrigin.Begin);
			await memoryStream.CopyToAsync(originalBodyStream);
			throw;
		}
		finally {
			httpContext.Response.Body = originalBodyStream;
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