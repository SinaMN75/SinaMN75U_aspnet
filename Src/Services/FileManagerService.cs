namespace SinaMN75U.Services;

public interface IFileManagerService {
	Task<UResponse<FileManagerListResponse?>> Browse(FileManagerBrowseParams p, CancellationToken ct);
	Task<UResponse<FileManagerEntryResponse?>> CreateFolder(FileManagerCreateFolderParams p, CancellationToken ct);
	Task<UResponse<FileManagerEntryResponse?>> Rename(FileManagerRenameParams p, CancellationToken ct);
	Task<UResponse<FileManagerEntryResponse?>> Move(FileManagerMoveParams p, CancellationToken ct);
	Task<UResponse> Delete(FileManagerDeleteParams p, CancellationToken ct);
	Task<UResponse<FileManagerEntryResponse?>> Upload(FileManagerUploadParams p, CancellationToken ct);
	UResponse<(string fullPath, string contentType)?> ResolveDownload(FileManagerDeleteParams p);
}

public class FileManagerService(
	IWebHostEnvironment env,
	ITokenService ts,
	ILocalizationService ls
) : IFileManagerService {
	private string Root => Path.GetFullPath(env.WebRootPath);

	public async Task<UResponse<FileManagerListResponse?>> Browse(FileManagerBrowseParams p, CancellationToken ct) {
		UResponse<FileManagerListResponse?>? guard = GuardAdmin<FileManagerListResponse?>(p.Token);
		if (guard != null) return guard;
		if (!TryResolve(p.Path, out string full)) return new UResponse<FileManagerListResponse?>(null, Usc.SecurityError, ls.Get("InvalidPath"));
		if (!Directory.Exists(full)) return new UResponse<FileManagerListResponse?>(null, Usc.NotFound, ls.Get("PathNotFound"));

		DirectoryInfo dir = new(full);
		List<FileManagerEntryResponse> directories = dir.GetDirectories()
			.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
			.Select(ToEntry)
			.ToList();
		List<FileManagerEntryResponse> files = dir.GetFiles()
			.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
			.Select(ToEntry)
			.ToList();

		FileManagerListResponse result = new() {
			Path = ToRelative(full),
			Directories = directories,
			Files = files,
			TotalSize = files.Sum(x => x.Size)
		};
		return await Task.FromResult(new UResponse<FileManagerListResponse?>(result));
	}

	public async Task<UResponse<FileManagerEntryResponse?>> CreateFolder(FileManagerCreateFolderParams p, CancellationToken ct) {
		UResponse<FileManagerEntryResponse?>? guard = GuardAdmin<FileManagerEntryResponse?>(p.Token);
		if (guard != null) return guard;
		if (!TryResolve(Path.Combine(p.Path, SanitizeName(p.Name)), out string full)) return new UResponse<FileManagerEntryResponse?>(null, Usc.SecurityError, ls.Get("InvalidPath"));
		if (Directory.Exists(full) || File.Exists(full)) return new UResponse<FileManagerEntryResponse?>(null, Usc.Conflict, ls.Get("PathAlreadyExists"));

		Directory.CreateDirectory(full);
		return await Task.FromResult(new UResponse<FileManagerEntryResponse?>(ToEntry(new DirectoryInfo(full)), Usc.Created));
	}

	public async Task<UResponse<FileManagerEntryResponse?>> Rename(FileManagerRenameParams p, CancellationToken ct) {
		UResponse<FileManagerEntryResponse?>? guard = GuardAdmin<FileManagerEntryResponse?>(p.Token);
		if (guard != null) return guard;
		if (!TryResolve(p.Path, out string source) || !Exists(source)) return new UResponse<FileManagerEntryResponse?>(null, Usc.NotFound, ls.Get("PathNotFound"));

		string? parent = Path.GetDirectoryName(source);
		if (parent == null) return new UResponse<FileManagerEntryResponse?>(null, Usc.SecurityError, ls.Get("InvalidPath"));
		string target = Path.Combine(parent, SanitizeName(p.NewName));
		if (!IsInsideRoot(target)) return new UResponse<FileManagerEntryResponse?>(null, Usc.SecurityError, ls.Get("InvalidPath"));
		if (Exists(target)) return new UResponse<FileManagerEntryResponse?>(null, Usc.Conflict, ls.Get("PathAlreadyExists"));

		if (Directory.Exists(source)) Directory.Move(source, target);
		else File.Move(source, target);
		return await Task.FromResult(new UResponse<FileManagerEntryResponse?>(ToEntry(target)));
	}

	public async Task<UResponse<FileManagerEntryResponse?>> Move(FileManagerMoveParams p, CancellationToken ct) {
		UResponse<FileManagerEntryResponse?>? guard = GuardAdmin<FileManagerEntryResponse?>(p.Token);
		if (guard != null) return guard;
		if (!TryResolve(p.Path, out string source) || !Exists(source)) return new UResponse<FileManagerEntryResponse?>(null, Usc.NotFound, ls.Get("PathNotFound"));
		if (!TryResolve(p.Destination, out string destDir) || !Directory.Exists(destDir)) return new UResponse<FileManagerEntryResponse?>(null, Usc.NotFound, ls.Get("PathNotFound"));

		string target = Path.Combine(destDir, Path.GetFileName(source));
		if (!IsInsideRoot(target)) return new UResponse<FileManagerEntryResponse?>(null, Usc.SecurityError, ls.Get("InvalidPath"));
		if (Exists(target)) return new UResponse<FileManagerEntryResponse?>(null, Usc.Conflict, ls.Get("PathAlreadyExists"));

		if (Directory.Exists(source)) Directory.Move(source, target);
		else File.Move(source, target);
		return await Task.FromResult(new UResponse<FileManagerEntryResponse?>(ToEntry(target)));
	}

	public async Task<UResponse> Delete(FileManagerDeleteParams p, CancellationToken ct) {
		UResponse? guard = GuardAdmin(p.Token);
		if (guard != null) return guard;
		if (!TryResolve(p.Path, out string full)) return new UResponse(Usc.SecurityError, ls.Get("InvalidPath"));
		if (full == Root) return new UResponse(Usc.Forbidden, ls.Get("InvalidPath"));

		if (Directory.Exists(full)) Directory.Delete(full, true);
		else if (File.Exists(full)) File.Delete(full);
		else return new UResponse(Usc.NotFound, ls.Get("PathNotFound"));
		return await Task.FromResult(new UResponse(Usc.Deleted));
	}

	public async Task<UResponse<FileManagerEntryResponse?>> Upload(FileManagerUploadParams p, CancellationToken ct) {
		UResponse<FileManagerEntryResponse?>? guard = GuardAdmin<FileManagerEntryResponse?>(p.Token);
		if (guard != null) return guard;
		if (!TryResolve(p.Path, out string destDir)) return new UResponse<FileManagerEntryResponse?>(null, Usc.SecurityError, ls.Get("InvalidPath"));
		Directory.CreateDirectory(destDir);

		string target = Path.Combine(destDir, SanitizeName(Path.GetFileName(p.File.FileName)));
		if (!IsInsideRoot(target)) return new UResponse<FileManagerEntryResponse?>(null, Usc.SecurityError, ls.Get("InvalidPath"));

		await using FileStream stream = new(target, FileMode.Create);
		await p.File.CopyToAsync(stream, ct);
		return new UResponse<FileManagerEntryResponse?>(ToEntry(target), Usc.Created);
	}

	public UResponse<(string fullPath, string contentType)?> ResolveDownload(FileManagerDeleteParams p) {
		UResponse<(string, string)?>? guard = GuardAdmin<(string, string)?>(p.Token);
		if (guard != null) return guard;
		if (!TryResolve(p.Path, out string full) || !File.Exists(full)) return new UResponse<(string, string)?>(null, Usc.NotFound, ls.Get("PathNotFound"));

		FileExtensionContentTypeProvider provider = new();
		if (!provider.TryGetContentType(full, out string? contentType)) contentType = "application/octet-stream";
		return new UResponse<(string, string)?>((full, contentType));
	}

	// ---- helpers ----

	// Resolves a wwwroot-relative path to an absolute one and rejects anything that escapes the root.
	private bool TryResolve(string? relative, out string fullPath) {
		string combined = Path.GetFullPath(Path.Combine(Root, (relative ?? "").Replace('\\', '/').TrimStart('/')));
		fullPath = combined;
		return IsInsideRoot(combined);
	}

	private bool IsInsideRoot(string fullPath) => fullPath == Root || fullPath.StartsWith(Root + Path.DirectorySeparatorChar, StringComparison.Ordinal);

	private static bool Exists(string fullPath) => Directory.Exists(fullPath) || File.Exists(fullPath);

	private string ToRelative(string fullPath) => Path.GetRelativePath(Root, fullPath).Replace('\\', '/') is "." ? "" : Path.GetRelativePath(Root, fullPath).Replace('\\', '/');

	// Strips any directory components so a name can never be used to traverse.
	private static string SanitizeName(string name) => Path.GetFileName(name.Trim());

	private FileManagerEntryResponse ToEntry(FileSystemInfo info) {
		bool isDir = info is DirectoryInfo;
		string relative = ToRelative(info.FullName);
		return new FileManagerEntryResponse {
			Name = info.Name,
			Path = relative,
			IsDirectory = isDir,
			Size = isDir ? 0 : ((FileInfo)info).Length,
			ModifiedAt = info.LastWriteTimeUtc,
			Extension = isDir ? null : info.Extension.TrimStart('.').ToLowerInvariant(),
			Url = isDir ? null : $"{Core.App.BaseUrl}/{relative}"
		};
	}

	private FileManagerEntryResponse ToEntry(string fullPath) => Directory.Exists(fullPath) ? ToEntry(new DirectoryInfo(fullPath)) : ToEntry(new FileInfo(fullPath));

	private UResponse? GuardAdmin(string? token) {
		JwtClaimData? userData = ts.ExtractClaims(token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (!userData.Tags.Contains(TagUser.SystemAdmin)) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));
		return null;
	}

	private UResponse<T>? GuardAdmin<T>(string? token) {
		JwtClaimData? userData = ts.ExtractClaims(token);
		if (userData == null) return new UResponse<T>(default!, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (!userData.Tags.Contains(TagUser.SystemAdmin)) return new UResponse<T>(default!, Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));
		return null;
	}
}
