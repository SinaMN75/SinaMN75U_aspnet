namespace SinaMN75U.Data.Responses;

public sealed class FileManagerEntryResponse {
	public required string Name { get; set; }
	public required string Path { get; set; }
	public required bool IsDirectory { get; set; }
	public long Size { get; set; }
	public DateTime ModifiedAt { get; set; }
	public string? Extension { get; set; }
	public string? Url { get; set; }
}

public sealed class FileManagerListResponse {
	public required string Path { get; set; }
	public required IEnumerable<FileManagerEntryResponse> Directories { get; set; }
	public required IEnumerable<FileManagerEntryResponse> Files { get; set; }
	public long TotalSize { get; set; }
}
