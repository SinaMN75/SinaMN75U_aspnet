// using SixLabors.ImageSharp;
// using SixLabors.ImageSharp.Processing;
// using SixLabors.ImageSharp.Formats.Jpeg;
//
// namespace SinaMN75U.Utils;
//
// public static class ImageCompressor
// {
// 	public static string CompressBase64(string base64, long maxBytes = 1_000_000)
// 	{
// 		byte[] inputBytes = Convert.FromBase64String(base64);
//
// 		using var img = Image.Load(inputBytes);
//
// 		int quality = 90;
// 		byte[] outputBytes;
//
// 		do
// 		{
// 			outputBytes = EncodeJpeg(img, quality);
// 			quality -= 10;
//
// 			if (quality < 10)
// 				break;
//
// 		} while (outputBytes.Length > maxBytes);
//
// 		return Convert.ToBase64String(outputBytes);
// 	}
//
// 	private static byte[] EncodeJpeg(Image img, int quality)
// 	{
// 		using var ms = new MemoryStream();
//
// 		var encoder = new JpegEncoder
// 		{
// 			Quality = quality
// 		};
//
// 		img.Save(ms, encoder);
//
// 		return ms.ToArray();
// 	}
// }

// string compressed = ImageCompressor.CompressBase64(inputBase64, 1_000_000);
// Console.WriteLine("Final size: " + (compressed.Length * 3 / 4));
