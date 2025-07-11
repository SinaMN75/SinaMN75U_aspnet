namespace SinaMN75U.Middlewares;

public class UMiddleware : IMiddleware {
	private readonly bool _decryptParams;
	private readonly bool _requireApiKey;
	private readonly bool _encryptResponse;
	private readonly string? _apiKey;

	public UMiddleware(IConfiguration config) {
		_decryptParams = bool.TryParse(config["DecryptParams"], out bool d) && d;
		_requireApiKey = bool.TryParse(config["RequireApiKey"], out bool r) && r;
		_encryptResponse = bool.TryParse(config["EncryptResponse"], out bool e) && e;
		_apiKey = config["ApiKey"];
	}

	public async Task InvokeAsync(HttpContext context, RequestDelegate next) {
		if (!HttpMethods.IsPost(context.Request.Method)) {
			await next(context);
			return;
		}

		string? requestBody = null;

		// Decrypt
		if (_decryptParams && context.Request.ContentLength > 0) {
			using StreamReader reader = new(context.Request.Body, Encoding.UTF8);
			string base64Body = await reader.ReadToEndAsync();

			try {
				string decoded = base64Body.DecodeBase64();
				requestBody = decoded;

				MemoryStream newBody = new(Encoding.UTF8.GetBytes(decoded));
				context.Request.Body = newBody;
				context.Request.ContentLength = newBody.Length;
				newBody.Seek(0, SeekOrigin.Begin);
			}
			catch (FormatException) {
				context.Response.StatusCode = 400;
				await context.Response.WriteAsync("Invalid Base64 input.");
				return;
			}
		}

		// API key validation
		if (_requireApiKey) {
			if (!context.Request.ContentType?.Contains("application/json") ?? true) {
				context.Response.StatusCode = 400;
				await context.Response.WriteAsync("Invalid content type.");
				return;
			}

			context.Request.EnableBuffering();
			if (requestBody == null) {
				using StreamReader reader = new(context.Request.Body, Encoding.UTF8);
				requestBody = await reader.ReadToEndAsync();
				context.Request.Body.Position = 0;
			}

			try {
				JsonElement json = JsonSerializer.Deserialize<JsonElement>(requestBody);
				if (!json.TryGetProperty("apiKey", out JsonElement apiKeyProp) || apiKeyProp.GetString() != _apiKey) {
					context.Response.StatusCode = 401;
					await context.Response.WriteAsync("Invalid API key.");
					return;
				}

				if (json.TryGetProperty("token", out JsonElement tokenProp))
					context.Items["JwtToken"] = tokenProp.GetString();
			}
			catch {
				context.Response.StatusCode = 400;
				await context.Response.WriteAsync("Malformed JSON.");
				return;
			}
		}

		// Encrypt response
		if (_encryptResponse) {
			Stream originalBody = context.Response.Body;
			await using MemoryStream tempBody = new();
			context.Response.Body = tempBody;

			await next(context);

			if (context.Response.StatusCode == 200 && tempBody.Length > 0) {
				tempBody.Seek(0, SeekOrigin.Begin);
				string body = await new StreamReader(tempBody).ReadToEndAsync();
				string encrypted = body.EncodeBase64();

				tempBody.SetLength(0);
				await tempBody.WriteAsync(Encoding.UTF8.GetBytes(encrypted));
			}

			tempBody.Seek(0, SeekOrigin.Begin);
			await tempBody.CopyToAsync(originalBody);
			context.Response.Body = originalBody;
		}
		else {
			await next(context);
		}
	}
}