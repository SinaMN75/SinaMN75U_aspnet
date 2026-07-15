namespace SinaMN75U.Data.Params;

// All paths are relative to wwwroot and use "/" as separator. An empty path means the wwwroot itself.
public sealed class FileManagerBrowseParams : BaseParams {
	public string Path { get; set; } = "";
}

public sealed class FileManagerCreateFolderParams : BaseParams {
	public string Path { get; set; } = "";

	[UValidationRequired("NameRequired")]
	public required string Name { get; set; }
}

public sealed class FileManagerRenameParams : BaseParams {
	[UValidationRequired("PathRequired")]
	public required string Path { get; set; }

	[UValidationRequired("NameRequired")]
	public required string NewName { get; set; }
}

public sealed class FileManagerMoveParams : BaseParams {
	[UValidationRequired("PathRequired")]
	public required string Path { get; set; }

	public string Destination { get; set; } = "";
}

public sealed class FileManagerDeleteParams : BaseParams {
	[UValidationRequired("PathRequired")]
	public required string Path { get; set; }
}

public sealed class FileManagerUploadParams : BaseParams {
	public string Path { get; set; } = "";
	public IFormFile File { get; set; } = null!;
}
