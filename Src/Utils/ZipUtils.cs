namespace SinaMN75U.Utils;

public static class ZipUtils {
	public static async Task<byte[]> CreateZipAsync(
		Dictionary<string, string>? textFiles = null,
		Dictionary<string, string>? imageFiles = null,
		CancellationToken ct = default) {
		using MemoryStream memoryStream = new();
		await using (ZipArchive archive = new(memoryStream, ZipArchiveMode.Create, true)) {
			if (textFiles != null) {
				foreach ((string fileName, string content) in textFiles) await AddTextFileToZipAsync(archive, fileName, content, ct);
			}

			if (imageFiles != null) {
				foreach ((string fileName, string base64Content) in imageFiles) AddImageFileToZip(archive, fileName, base64Content);
			}
		}

		return memoryStream.ToArray();
	}

	private static async Task AddTextFileToZipAsync(ZipArchive archive, string fileName, string content, CancellationToken ct) {
		ZipArchiveEntry entry = archive.CreateEntry(fileName);
		await using Stream stream = await entry.OpenAsync(ct);
		await using StreamWriter writer = new(stream);
		await writer.WriteAsync(content.AsMemory(), ct);
	}

	private static async void AddImageFileToZip(ZipArchive archive, string fileName, string base64Content) {
		try {
			if (!string.IsNullOrEmpty(base64Content)) {
				ZipArchiveEntry entry = archive.CreateEntry(fileName);
				await using Stream stream = await entry.OpenAsync();
				byte[] bytes = Convert.FromBase64String(base64Content);
				await stream.WriteAsync(bytes);
			}
		}
		catch {
			// ignored
		}
	}
}