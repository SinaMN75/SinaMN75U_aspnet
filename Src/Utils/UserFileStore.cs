namespace SinaMN75U.Utils;

public static class UserFileStore {
	private const string MediaRoot = "Media";
	private const string IdentityFolder = "identity";

	public static string? Save(
		string webRoot,
		Guid userId,
		string field,
		string? base64,
		string? oldPath,
		long? compressKb
	) {
		if (string.IsNullOrWhiteSpace(base64)) return oldPath;

		Delete(webRoot, oldPath);

		byte[] bytes = compressKb.HasValue ? ImageCompressor.CompressBase64(base64, compressKb.Value) : Convert.FromBase64String(base64);
		string ext = compressKb.HasValue ? "jpg" : "mp4";
		string relative = $"{IdentityFolder}/{userId}/{field}_{Guid.CreateVersion7()}.{ext}";

		string fullPath = Path.Combine(webRoot, MediaRoot, relative);
		string? dir = Path.GetDirectoryName(fullPath);
		if (dir != null && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
		File.WriteAllBytes(fullPath, bytes);

		return relative;
	}

	public static void Delete(string webRoot, string? relativePath) {
		if (string.IsNullOrWhiteSpace(relativePath)) return;
		string fullPath = Path.Combine(webRoot, MediaRoot, relativePath);
		if (File.Exists(fullPath)) File.Delete(fullPath);
	}

	public static void DeleteUserFolder(string webRoot, Guid userId) {
		string dir = Path.Combine(webRoot, MediaRoot, IdentityFolder, userId.ToString());
		if (Directory.Exists(dir)) Directory.Delete(dir, true);
	}

	public static string? ReadBase64(string webRoot, string? relativePath) {
		if (string.IsNullOrWhiteSpace(relativePath)) return null;
		string fullPath = Path.Combine(webRoot, MediaRoot, relativePath);
		return File.Exists(fullPath) ? Convert.ToBase64String(File.ReadAllBytes(fullPath)) : null;
	}
}