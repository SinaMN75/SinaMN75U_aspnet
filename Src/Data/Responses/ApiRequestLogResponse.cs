namespace SinaMN75U.Data.Responses;

public sealed class ApiLogListItemResponse {
	public required Guid Id { get; set; }
	public required DateTime Timestamp { get; set; }
	public required string Method { get; set; }
	public required string Path { get; set; }
	public string? QueryString { get; set; }
	public required int StatusCode { get; set; }
	public required bool IsSuccess { get; set; }
	public required long DurationMs { get; set; }
	public Guid? UserId { get; set; }
	public string? UserName { get; set; }
	public string? IpAddress { get; set; }
	public string? UserAgent { get; set; }
	public string? TraceId { get; set; }
	public string? Host { get; set; }
	public int RequestSizeBytes { get; set; }
	public int ResponseSizeBytes { get; set; }
	public string? ExceptionType { get; set; }
}

public sealed class ApiLogDetailResponse {
	public required Guid Id { get; set; }
	public required DateTime Timestamp { get; set; }
	public required string Method { get; set; }
	public required string Path { get; set; }
	public string? QueryString { get; set; }
	public required int StatusCode { get; set; }
	public required bool IsSuccess { get; set; }
	public required long DurationMs { get; set; }
	public Guid? UserId { get; set; }
	public string? UserName { get; set; }
	public string? UserEmail { get; set; }
	public string? UserRoles { get; set; }
	public string? IpAddress { get; set; }
	public string? UserAgent { get; set; }
	public string? TraceId { get; set; }
	public string? Host { get; set; }
	public int RequestSizeBytes { get; set; }
	public int ResponseSizeBytes { get; set; }
	public string? RequestBody { get; set; }
	public string? ResponseBody { get; set; }
	public string? RequestHeaders { get; set; }
	public string? ResponseHeaders { get; set; }
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
	public required double P50DurationMs { get; set; }
	public required double P95DurationMs { get; set; }
	public required double P99DurationMs { get; set; }
	public required List<ApiLogTimeBucketResponse> TimeSeries { get; set; }
	public required List<ApiLogStatusCountResponse> StatusCodeDistribution { get; set; }
	public required List<ApiLogEndpointStatResponse> TopSlowEndpoints { get; set; }
	public required List<ApiLogEndpointStatResponse> TopFailingEndpoints { get; set; }
	public required List<ApiLogListItemResponse> SlowestRequests { get; set; }
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
