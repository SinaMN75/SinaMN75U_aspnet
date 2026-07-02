namespace SinaMN75U.Data.Responses;

// Lightweight row for the searchable/paginated log table - no bodies/stack trace, kept cheap to list.
public sealed class ApiLogListItemResponse {
	public required Guid Id { get; set; }
	public required DateTime Timestamp { get; set; }
	public required string Method { get; set; }
	public required string Path { get; set; }
	public required int StatusCode { get; set; }
	public required bool IsSuccess { get; set; }
	public required long DurationMs { get; set; }
	public Guid? UserId { get; set; }
	public string? IpAddress { get; set; }
	public string? ExceptionType { get; set; }
}

// Full record for the "drill into one request" view.
public sealed class ApiLogDetailResponse {
	public required Guid Id { get; set; }
	public required DateTime Timestamp { get; set; }
	public required string Method { get; set; }
	public required string Path { get; set; }
	public required int StatusCode { get; set; }
	public required bool IsSuccess { get; set; }
	public required long DurationMs { get; set; }
	public Guid? UserId { get; set; }
	public string? IpAddress { get; set; }
	public string? RequestBody { get; set; }
	public string? ResponseBody { get; set; }
	public string? ExceptionType { get; set; }
	public string? ExceptionMessage { get; set; }
	public string? StackTrace { get; set; }
}

public sealed class ApiLogStatsResponse {
	public required int TotalRequests { get; set; }
	public required int TotalErrors { get; set; }
	public required double ErrorRatePercent { get; set; }
	public required double AvgDurationMs { get; set; }
	public required long MaxDurationMs { get; set; }

	public required List<ApiLogTimeBucketResponse> TimeSeries { get; set; }
	public required List<ApiLogStatusCountResponse> StatusCodeDistribution { get; set; }
	public required List<ApiLogEndpointStatResponse> TopSlowEndpoints { get; set; }
	public required List<ApiLogEndpointStatResponse> TopFailingEndpoints { get; set; }
}

public sealed class ApiLogTimeBucketResponse {
	public required DateTime Bucket { get; set; }
	public required int Total { get; set; }
	public required int Errors { get; set; }
	public required double AvgDurationMs { get; set; }
}

public sealed class ApiLogStatusCountResponse {
	public required int StatusCode { get; set; }
	public required int Count { get; set; }
}

public sealed class ApiLogEndpointStatResponse {
	public required string Path { get; set; }
	public required int Count { get; set; }
	public required int ErrorCount { get; set; }
	public required double AvgDurationMs { get; set; }
}
