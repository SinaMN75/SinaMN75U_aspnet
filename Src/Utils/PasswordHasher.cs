namespace SinaMN75U.Utils;

public static class PasswordHasher {
	public static string Hash(string password) {
		byte[] salt = new byte[16];
		using RandomNumberGenerator rng = RandomNumberGenerator.Create();
		rng.GetBytes(salt);

		string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
				password,
				salt,
				KeyDerivationPrf.HMACSHA256,
				10000,
				32
			)
		);

		return $"{Convert.ToBase64String(salt)}.{hashed}";
	}

	public static bool Verify(string password, string storedHash) {
		string[] parts = storedHash.Split('.');
		if (parts.Length != 2) return false;

		byte[] salt = Convert.FromBase64String(parts[0]);
		string hashedInput = Convert.ToBase64String(KeyDerivation.Pbkdf2(
				password,
				salt,
				KeyDerivationPrf.HMACSHA256,
				10000,
				32
			)
		);

		return hashedInput == parts[1];
	}
}