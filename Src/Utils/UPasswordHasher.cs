namespace SinaMN75U.Utils;

public static class UPasswordHasher {
	public static string Hash(string password) {
		byte[] salt = new byte[16];
		using RandomNumberGenerator rng = RandomNumberGenerator.Create();
		rng.GetBytes(salt);
		return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, 10000, 32))}";
	}

	public static bool Verify(string password, string storedHash) {
		string[] parts = storedHash.Split('.');
		if (parts.Length != 2) return false;
		return Convert.ToBase64String(KeyDerivation.Pbkdf2(password, Convert.FromBase64String(parts[0]), KeyDerivationPrf.HMACSHA256, 10000, 32)) == parts[1];
	}
}