namespace SinaMN75U.Middlewares;

using Microsoft.Extensions.Primitives;
using System.Buffers;

public sealed class CustomRequestResponseFilter(ILocalStorageService cache, int minutes) : IEndpointFilter {
	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next) {
		HttpContext httpContext = context.HttpContext;
		HttpRequest request = httpContext.Request;
		string cacheKey = BuildCacheKey(request);

		if (cache.Get(cacheKey) is { } cachedResponse) return Results.Json(JsonSerializer.Deserialize<object>(cachedResponse));

		object? result = await next(context);

		if (result is IResult originalResult && httpContext.Response.StatusCode == StatusCodes.Status200OK) {
			return new ModifiedResult(originalResult, cacheKey, cache, TimeSpan.FromMinutes(minutes));
		}

		return result;
	}

	private static string BuildCacheKey(HttpRequest request) {
		StringBuilder sb = new StringBuilder(256)
			.Append(request.Scheme).Append("://")
			.Append(request.Host)
			.Append(request.Path)
			.Append(request.QueryString);
		foreach (KeyValuePair<string, StringValues> header in request.Headers.OrderBy(h => h.Key)) {
			sb.Append(header.Key).Append('=').Append(header.Value);
		}

		if (!(request.ContentLength > 0) || !request.Body.CanRead) return sb.ToString();
		try {
			long originalPosition = request.Body.Position;
			request.Body.Position = 0;
			byte[] buffer = ArrayPool<byte>.Shared.Rent((int)request.ContentLength);
			try {
				int bytesRead = request.Body.Read(buffer, 0, (int)request.ContentLength);
				sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
			}
			finally {
				ArrayPool<byte>.Shared.Return(buffer);
				request.Body.Position = originalPosition;
			}
		}
		catch {
			// ignored
		}

		Console.WriteLine(sb.ToString());
		return sb.ToString();
	}
}

public sealed class ModifiedResult(
	IResult originalResult,
	string cacheKey,
	ILocalStorageService cache,
	TimeSpan cacheDuration)
	: IResult {
	public async Task ExecuteAsync(HttpContext httpContext) {
		Stream originalBodyStream = httpContext.Response.Body;
		await using MemoryStream memoryStream = new(1024); // Pre-allocate buffer
		httpContext.Response.Body = memoryStream;

		await originalResult.ExecuteAsync(httpContext);

		if (httpContext.Response.StatusCode == StatusCodes.Status200OK) {
			try {
				memoryStream.Seek(0, SeekOrigin.Begin);
				using StreamReader reader = new StreamReader(memoryStream, Encoding.UTF8, false, 1024, leaveOpen: true);
				string responseBody = await reader.ReadToEndAsync();

				cache.Set(cacheKey, responseBody, cacheDuration);
				httpContext.Response.Headers["X-Cache-Store"] = "true";
			}
			catch {
				// ignored
			}
		}

		memoryStream.Seek(0, SeekOrigin.Begin);
		await memoryStream.CopyToAsync(originalBodyStream);
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