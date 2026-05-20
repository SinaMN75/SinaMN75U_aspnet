namespace SinaMN75U.Utils;

public static class ImageCompressor {
	public static byte[] CompressBase64(string base64, long maxKb = 100) {
		byte[] inputBytes = Convert.FromBase64String(base64);
		Console.WriteLine($"[Before] Size: {inputBytes.Length} bytes ({inputBytes.Length / 1024} KB)");
		using Image img = Image.Load(inputBytes);

		int quality = 90;
		byte[] outputBytes;
		
		do {
			outputBytes = EncodeJpeg(img, quality);
			quality -= 10;

			if (quality < 10)
				break;
		} while (outputBytes.Length > maxKb * 1024);

		Console.WriteLine($"[After] Size: {outputBytes.Length} bytes ({outputBytes.Length / 1024} KB)");
		return outputBytes;
	}

	private static byte[] EncodeJpeg(Image img, int quality) {
		using MemoryStream ms = new();
		img.Save(ms, new JpegEncoder { Quality = quality });
		return ms.ToArray();
	}
}
