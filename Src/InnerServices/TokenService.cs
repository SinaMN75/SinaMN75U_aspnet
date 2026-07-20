namespace SinaMN75U.InnerServices;

public interface ITokenService {
	public string GenerateRefreshToken();
	public string GenerateJwt(UserEntity user);
	public JwtClaimData? ExtractClaims(string? token);
}

public class TokenService : ITokenService {
	public string GenerateRefreshToken() {
		byte[] randomNumber = new byte[64];
		using RandomNumberGenerator rng = RandomNumberGenerator.Create();
		rng.GetBytes(randomNumber);
		return Convert.ToBase64String(randomNumber);
	}

	public string GenerateJwt(UserEntity user) {
		DateTime expires = DateTime.UtcNow.AddMinutes(double.TryParse(Core.App.Jwt.Expires, out double minutes) ? minutes : 5);
		return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
			Core.App.Jwt.Issuer,
			Core.App.Jwt.Audience,
			[
				new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString()),
				new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
				new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
				new Claim(JwtRegisteredClaimNames.PhoneNumber, user.PhoneNumber ?? ""),
				new Claim(JwtRegisteredClaimNames.Name, user.FirstName ?? ""),
				new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? ""),
				new Claim(JwtRegisteredClaimNames.NameId, user.NationalCode ?? ""),
				new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
				new Claim(ClaimTypes.Expiration, expires.ToString(CultureInfo.InvariantCulture)),
				new Claim(ClaimTypes.Role, string.Join(",", user.Tags.Select(x => (int)x)))
			],
			expires: expires,
			signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Core.App.Jwt.Key)), SecurityAlgorithms.HmacSha256)
		));
	}

	public JwtClaimData? ExtractClaims(string? token) {
		if (token.IsNullOrEmpty()) return null;
		try {
			JwtSecurityTokenHandler handler = new() { MapInboundClaims = false }; 
			ClaimsPrincipal principal = handler.ValidateToken(token, new TokenValidationParameters {
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Core.App.Jwt.Key)),
				ValidateIssuer = true,
				ValidIssuer = Core.App.Jwt.Issuer,
				ValidateAudience = true,
				ValidAudience = Core.App.Jwt.Audience,
				ValidateLifetime = false,
				ClockSkew = TimeSpan.Zero
			}, out _);
			IEnumerable<Claim> claims = principal.Claims.ToList();
			string? rolesClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
			IEnumerable<TagUser> tags = [];
			if (!string.IsNullOrWhiteSpace(rolesClaim)) {
				tags = rolesClaim.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(roleStr => {
					if (int.TryParse(roleStr, out int roleInt)) return (TagUser)roleInt;
					return Enum.Parse<TagUser>(roleStr);
				});
			}

			return new JwtClaimData {
				Id = Guid.Parse(claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value ?? Guid.Empty.ToString()),
				UserName = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName)?.Value ?? "",
				Email = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value ?? "",
				PhoneNumber = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.PhoneNumber)?.Value ?? "",
				FirstName = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value ?? "",
				LastName = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.FamilyName)?.Value ?? "",
				NationalCode = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId)?.Value ?? "",
				FullName = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.GivenName)?.Value ?? "",
				Expiration = DateTime.TryParse(claims.FirstOrDefault(c => c.Type == ClaimTypes.Expiration)?.Value, out DateTime exp) ? exp : null,
				Tags = tags
			};
		}
		catch (Exception) {
			return null;
		}
	}
}