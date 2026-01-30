namespace SinaMN75U.Middlewares;

public sealed class CacheResponseFilter(ILocalStorageService cache, int minutes) : IEndpointFilter {
	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next) {
		HttpContext httpContext = context.HttpContext;
		HttpRequest request = httpContext.Request;

		// Read and cache the body
		string body = await ReadRequestBodyAsync(request);
		string cacheKey = $"{request.Path}{body}";

		if (cache.Get(cacheKey) is { } cachedJson) 
			return Results.Content(cachedJson, "application/json", Encoding.UTF8);

		// Restore the body for the actual endpoint
		await RestoreRequestBodyAsync(request, body);

		object? result = await next(context);

		if (result is IResult originalResult && httpContext.Response.StatusCode == StatusCodes.Status200OK) {
			return new ModifiedResult(originalResult, cacheKey, cache, TimeSpan.FromMinutes(minutes));
		}

		return result;
	}

	private static async Task<string> ReadRequestBodyAsync(HttpRequest request) {
		// Enable buffering if not already enabled
		if (!request.Body.CanSeek) {
			request.EnableBuffering();
		}

		try {
			request.Body.Position = 0;
			using StreamReader reader = new(request.Body, Encoding.UTF8, leaveOpen: true);
			string body = await reader.ReadToEndAsync();
			return body.Trim();
		}
		catch (Exception) {
			return string.Empty;
		}
	}

	private static async Task RestoreRequestBodyAsync(HttpRequest request, string body) {
		if (string.IsNullOrEmpty(body)) return;

		byte[] bytes = Encoding.UTF8.GetBytes(body);

		// Reuse the existing stream if possible, rather than creating new ones
		if (request.Body is MemoryStream memoryStream) {
			memoryStream.SetLength(0); // Clear existing content
			await memoryStream.WriteAsync(bytes);
			memoryStream.Position = 0;
		}
		else {
			request.Body = new MemoryStream(bytes);
		}

		request.ContentLength = bytes.Length;
		request.Headers.ContentLength = bytes.Length;
	}
}

public class ModifiedResult(IResult originalResult, string key, ILocalStorageService cache, TimeSpan duration) : IResult {
	public async Task ExecuteAsync(HttpContext httpContext) {
		Stream originalBodyStream = httpContext.Response.Body;

		await using MemoryStream memoryStream = new();
		httpContext.Response.Body = memoryStream;

		try {
			await originalResult.ExecuteAsync(httpContext);

			// Only cache successful responses
			if (httpContext.Response.StatusCode == StatusCodes.Status200OK) {
				memoryStream.Seek(0, SeekOrigin.Begin);
				using StreamReader reader = new(memoryStream, Encoding.UTF8, leaveOpen: true);
				string responseBody = await reader.ReadToEndAsync();
				cache.Set(key, responseBody, duration);

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
		where TBuilder : IEndpointConventionBuilder =>
		builder.AddEndpointFilter(async (context, next) =>
			await new CacheResponseFilter(context.HttpContext.RequestServices.GetRequiredService<ILocalStorageService>(), minutes).InvokeAsync(context, next));
}