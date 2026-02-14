namespace SinaMN75U.Constants;

public record AppSettings(
	ConnectionStrings ConnectionStrings,
	Jwt Jwt,
	string BaseUrl,
	string ApiKey,
	bool MiddlewareDecryptParams,
	bool MiddlewareEncryptResponse,
	bool MiddlewareRequireApiKey,
	bool MiddlewareLog,
	bool MiddlewareLogSuccess
);

public record ConnectionStrings(
	string Server
);

public record Jwt(
	Uri Key,
	Uri Issuer,
	Uri Audience,
	string Expires
);