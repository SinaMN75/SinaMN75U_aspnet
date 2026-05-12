namespace SinaMN75U.Utils;

using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

public static class ImageReducer {
	// Entry for byte[]
	public static byte[] ReduceImage(byte[] inputBytes, long maxBytes) {
		if (inputBytes == null || inputBytes.Length == 0)
			throw new ArgumentException("Input is empty");

		IImageFormat format = Image.DetectFormat(inputBytes);
		using Image image = Image.Load(inputBytes);

		return ReduceImageInternal(image, format, maxBytes);
	}

	// Entry for Base64
	public static byte[] ReduceImageFromBase64(string base64, long maxBytes) {
		byte[] bytes = Convert.FromBase64String(base64);
		return ReduceImage(bytes, maxBytes);
	}

	private static byte[] ReduceImageInternal(Image image, IImageFormat format, long maxBytes) {
		// 1) Try only quality reduction (no resize)
		byte[]? result = TryEncodeWithQuality(image, format, maxBytes);
		if (result != null) return result;

		// 2) If still too large, downscale gradually and retry
		int width = image.Width;
		int height = image.Height;

		for (int scaleStep = 90; scaleStep >= 40; scaleStep -= 10) {
			int newW = width * scaleStep / 100;
			int newH = height * scaleStep / 100;

			using Image cloned = image.Clone(ctx => ctx.Resize(newW, newH));
			result = TryEncodeWithQuality(cloned, format, maxBytes);

			if (result != null)
				return result;
		}

		// 3) Final fallback (best effort)
		return EncodeWithQuality(image, format, 50);
	}

	private static byte[]? TryEncodeWithQuality(Image image, IImageFormat format, long maxBytes) {
		for (int quality = 95; quality >= 50; quality -= 5) {
			byte[] bytes = EncodeWithQuality(image, format, quality);
			if (bytes.Length <= maxBytes)
				return bytes;
		}

		return null;
	}

	private static byte[] EncodeWithQuality(Image image, IImageFormat format, int quality) {
		using MemoryStream ms = new();

		if (format.Name.Equals("JPEG", StringComparison.OrdinalIgnoreCase)) {
			JpegEncoder encoder = new() { Quality = quality };
			image.Save(ms, encoder);
		}
		else if (format.Name.Equals("PNG", StringComparison.OrdinalIgnoreCase)) {
			// PNG is lossless; quality doesn't apply
			PngEncoder encoder = new();
			image.Save(ms, encoder);
		}
		else {
			// Fallback to WebP (smaller with good quality)
			WebpEncoder encoder = new() { Quality = quality };
			image.Save(ms, encoder);
		}

		return ms.ToArray();
	}
}