namespace SinaMN75U.Utils;

public static class Encryption {
	public static string EncodeBase64(string plainText) => Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));

	public static string DecodeBase64(string base64EncodedData) => Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedData));
}