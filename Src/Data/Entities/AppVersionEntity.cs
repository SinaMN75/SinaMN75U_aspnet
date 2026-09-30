namespace SinaMN75U.Data.Entities;

[Table("AppVersions")]
public sealed class AppVersionEntity : BaseEntity<TagAppVersion, AppVersionJson> {
	public required int LatestBuildNumber { get; set; }
	public required int MinBuildNumber { get; set; }
}

public sealed class AppVersionJson : BaseJson {
	public string? LatestVersionName { get; set; }
	public string? Description { get; set; }
	public List<AppVersionLink> Links { get; set; } = [];
}

public sealed class AppVersionLink {
	public string? Title { get; set; }
	public string? Url { get; set; }
	public string? IconBase64 { get; set; }
}
