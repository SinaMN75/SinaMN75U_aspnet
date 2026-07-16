namespace SinaMN75U.Data.Params;

public sealed class ApiLogCreateParams {
	public required string Method { get; set; }
	public required string Path { get; set; }
	public required int StatusCode { get; set; }
	public required long DurationMs { get; set; }
	public Guid? UserId { get; set; }
	public string? UserName { get; set; }
	public string? UserEmail { get; set; }
	public string? UserRoles { get; set; }
	public string? IpAddress { get; set; }
	public string? QueryString { get; set; }
	public string? RequestBody { get; set; }
	public string? ResponseBody { get; set; }
	public string? RequestHeaders { get; set; }
	public string? ResponseHeaders { get; set; }
	public string? UserAgent { get; set; }
	public string? Host { get; set; }
	public int RequestSizeBytes { get; set; }
	public int ResponseSizeBytes { get; set; }
	public string? ExceptionType { get; set; }
	public string? ExceptionMessage { get; set; }
	public string? StackTrace { get; set; }
}

public sealed class ApiLogReadParams : BaseReadParams<TagApiLog> {
	public ApiLogSelectorArgs SelectorArgs { get; set; } = new();
	public string? PathContains { get; set; }
	public int? StatusCode { get; set; }
	public long? MinDurationMs { get; set; }
	public long? MaxDurationMs { get; set; }
	public Guid? UserId { get; set; }
	public string? IpAddress { get; set; }
	public bool? OnlyErrors { get; set; }
}

public sealed class ApiLogStatsParams : BaseParams {
	public DateTime? FromCreatedAt { get; set; }
	public DateTime? ToCreatedAt { get; set; }
	public string Bucket { get; set; } = "hour";
}
