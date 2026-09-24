namespace SinaMN75U.InnerServices;

public interface ITokenService {
	public string GenerateRefreshToken();
	public DateTime RefreshTokenExpiry();
	public string GenerateJwt(UserEntity user);
	public JwtClaimData? ExtractClaims(string? token);
}

public class TokenService : ITokenService {
	private static readonly JwtSecurityTokenHandler Handler = new() { MapInboundClaims = false };
	private sealed record JwtCache(Jwt Jwt, SigningCredentials Credentials, TokenValidationParameters Validation);

	private static JwtCache Settings {
		get {
			Jwt jwt = Core.App.Jwt;
			JwtCache? cache = field;
			if (cache != null && ReferenceEquals(cache.Jwt, jwt)) return cache;

			SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(jwt.Key));
			cache = new JwtCache(jwt, new SigningCredentials(key, SecurityAlgorithms.HmacSha256), new TokenValidationParameters {
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = key,
				ValidateIssuer = true,
				ValidIssuer = jwt.Issuer,
				ValidateAudience = true,
				ValidAudience = jwt.Audience,
				ValidateLifetime = false,
				ClockSkew = TimeSpan.Zero
			});
			field = cache;
			return cache;
		}
	}

	public string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

	public DateTime RefreshTokenExpiry() => DateTime.UtcNow.AddDays(Core.App.Jwt.RefreshTokenExpiresInDays);

	public string GenerateJwt(UserEntity user) {
		(Jwt jwt, SigningCredentials credentials, _) = Settings;
		DateTime expires = DateTime.UtcNow.AddMinutes(jwt.Expires);
		return Handler.WriteToken(new JwtSecurityToken(
			jwt.Issuer,
			jwt.Audience,
			[
				new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString()),
				new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
				new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
				new Claim(JwtRegisteredClaimNames.PhoneNumber, user.PhoneNumber ?? ""),
				new Claim(JwtRegisteredClaimNames.Name, user.FirstName ?? ""),
				new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? ""),
				new Claim(JwtRegisteredClaimNames.NameId, user.NationalCode ?? ""),
				new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
				new Claim(ClaimTypes.Expiration, expires.ToString("O", CultureInfo.InvariantCulture)),
				new Claim(ClaimTypes.Role, string.Join(",", user.Tags.Select(x => (int)x)))
			],
			expires: expires,
			signingCredentials: credentials
		));
	}

	public JwtClaimData? ExtractClaims(string? token) {
		if (token.IsNullOrEmpty()) return null;
		try {
			ClaimsPrincipal principal = Handler.ValidateToken(token, Settings.Validation, out _);
			Dictionary<string, string> claims = new();
			foreach (Claim claim in principal.Claims) claims.TryAdd(claim.Type, claim.Value);
			List<TagUser> tags = [];
			if (claims.TryGetValue(ClaimTypes.Role, out string? rolesClaim) && !string.IsNullOrWhiteSpace(rolesClaim)) tags.AddRange(rolesClaim.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(roleStr => int.TryParse(roleStr, out int roleInt) ? (TagUser)roleInt : Enum.Parse<TagUser>(roleStr)));

			string firstName = Claim(JwtRegisteredClaimNames.Name);
			string lastName = Claim(JwtRegisteredClaimNames.FamilyName);
			return new JwtClaimData {
				Id = Guid.Parse(claims.GetValueOrDefault(JwtRegisteredClaimNames.Jti) ?? Guid.Empty.ToString()),
				UserName = Claim(JwtRegisteredClaimNames.UniqueName),
				Email = Claim(JwtRegisteredClaimNames.Email),
				PhoneNumber = Claim(JwtRegisteredClaimNames.PhoneNumber),
				FirstName = firstName,
				LastName = lastName,
				NationalCode = Claim(JwtRegisteredClaimNames.NameId),
				FullName = $"{firstName} {lastName}".Trim(),
				Expiration = ParseExpiration(claims.GetValueOrDefault(ClaimTypes.Expiration)),
				Tags = tags
			};

			string Claim(string type) => claims.GetValueOrDefault(type) ?? "";
		}
		catch (Exception) {
			return null;
		}
	}

	private static DateTime? ParseExpiration(string? value) {
		if (value.IsNullOrEmpty()) return null;
		if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime exp)) return exp;
		return DateTime.TryParse(value, out DateTime legacy) ? legacy : null;
	}
}