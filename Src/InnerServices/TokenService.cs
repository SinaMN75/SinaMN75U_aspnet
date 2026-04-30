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

	public string GenerateJwt(UserEntity user) => new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
				Core.App.Jwt.Issuer,
				Core.App.Jwt.Audience,
				[
					new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString()),
					new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
					new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
					new Claim(JwtRegisteredClaimNames.PhoneNumber, user.PhoneNumber ?? ""),
					new Claim(JwtRegisteredClaimNames.Name, user.FirstName ?? ""),
					new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? ""),
					new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
					new Claim(ClaimTypes.Expiration, DateTime.UtcNow.Add(TimeSpan.FromSeconds(60)).ToString(CultureInfo.InvariantCulture)),
					new Claim(ClaimTypes.Role, string.Join(",", user.Tags.Select(x => (int)x)))
				],
				expires: DateTime.UtcNow.AddMinutes(60),
				signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Core.App.Jwt.Key)), SecurityAlgorithms.HmacSha256)
			)
		);

	public JwtClaimData? ExtractClaims(string? token) {
		try {
			IEnumerable<Claim> claims = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims.ToList();
			return new JwtClaimData {
				Id = Guid.Parse(claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value ?? ""),
				Email = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value ?? "",
				PhoneNumber = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.PhoneNumber)?.Value ?? "",
				FirstName = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value ?? "",
				LastName = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.FamilyName)?.Value ?? "",
				FullName = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.GivenName)?.Value ?? "",
				Expiration = DateTime.Parse(claims.FirstOrDefault(c => c.Type == ClaimTypes.Expiration)?.Value ?? ""),
				Tags = (claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "").Split(',').Select(Enum.Parse<TagUser>)
			};
		}
		catch (Exception) {
			return null;
		}
	}
}