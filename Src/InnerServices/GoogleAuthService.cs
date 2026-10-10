namespace SinaMN75U.InnerServices;

public sealed class GoogleUser {
	public required string Id { get; init; }
	public required string Email { get; init; }
	public required bool EmailVerified { get; init; }
	public string? FirstName { get; init; }
	public string? LastName { get; init; }
}

public interface IGoogleAuthService {
	bool IsConfigured { get; }
	Task<GoogleUser?> Validate(string idToken, CancellationToken ct);
}

/// <summary>Validates a Google id token (signature from Google's JWKS, issuer, audience = AppSettings.GoogleClientIds, expiry).</summary>
public sealed class GoogleAuthService : IGoogleAuthService {
	private static readonly ConfigurationManager<OpenIdConnectConfiguration> Discovery = new(
		"https://accounts.google.com/.well-known/openid-configuration",
		new OpenIdConnectConfigurationRetriever(),
		new HttpDocumentRetriever { RequireHttps = true }
	);

	private static readonly JwtSecurityTokenHandler Handler = new() { MapInboundClaims = false };

	public bool IsConfigured => Core.App.GoogleClientIds.Count > 0;

	public async Task<GoogleUser?> Validate(string idToken, CancellationToken ct) {
		if (!IsConfigured) return null;
		try {
			OpenIdConnectConfiguration config = await Discovery.GetConfigurationAsync(ct);
			ClaimsPrincipal principal = Handler.ValidateToken(idToken, new TokenValidationParameters {
				ValidIssuers = ["https://accounts.google.com", "accounts.google.com"],
				ValidAudiences = Core.App.GoogleClientIds,
				IssuerSigningKeys = config.SigningKeys,
				ValidateLifetime = true,
				ClockSkew = TimeSpan.FromMinutes(2)
			}, out _);

			string? id = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
			string? email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
			if (id.IsNullOrEmpty() || email.IsNullOrEmpty()) return null;

			return new GoogleUser {
				Id = id,
				Email = email.Trim().ToLowerInvariant(),
				EmailVerified = string.Equals(principal.FindFirst("email_verified")?.Value, "true", StringComparison.OrdinalIgnoreCase),
				FirstName = principal.FindFirst(JwtRegisteredClaimNames.GivenName)?.Value,
				LastName = principal.FindFirst(JwtRegisteredClaimNames.FamilyName)?.Value
			};
		}
		catch (Exception e) {
			ULog.Warning($"Google id token rejected: {e.Message}");
			return null;
		}
	}
}
