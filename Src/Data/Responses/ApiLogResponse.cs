namespace SinaMN75U.Data.Responses;

public sealed class ApiLogResponse : BaseResponse<TagApiLog, ApiLogJson> {
	public required string Path { get; set; }
	public required int StatusCode { get; set; }
	public required long DurationMs { get; set; }
	public Guid? UserId { get; set; }
	public string? IpAddress { get; set; }
}

public sealed class ApiLogBucketResponse {
	public required DateTime Time { get; set; }
	public required int Count { get; set; }
	public required int ErrorCount { get; set; }
	public required double AverageDurationMs { get; set; }
}

public sealed class ApiLogEndpointResponse {
	public required string Path { get; set; }
	public required int Count { get; set; }
	public required double AverageDurationMs { get; set; }
}

public sealed class ApiLogStatsResponse {
	public required int TotalCount { get; set; }
	public required int SuccessCount { get; set; }
	public required int ErrorCount { get; set; }
	public required double AverageDurationMs { get; set; }
	public required double P50DurationMs { get; set; }
	public required double P95DurationMs { get; set; }
	public required double P99DurationMs { get; set; }
	public required IEnumerable<ApiLogBucketResponse> Timeline { get; set; }
	public required IEnumerable<ApiLogEndpointResponse> SlowestEndpoints { get; set; }
	public required IEnumerable<ApiLogEndpointResponse> FailingEndpoints { get; set; }
	public required IEnumerable<ApiLogResponse> SlowestRequests { get; set; }
}
