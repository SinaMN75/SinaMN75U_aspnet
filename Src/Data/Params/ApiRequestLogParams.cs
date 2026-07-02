namespace SinaMN75U.Data.Params;

public sealed class ApiLogSearchParams : BaseParams {
	public int PageSize { get; set; } = 50;
	public int PageNumber { get; set; } = 1;
	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }
	public string? Method { get; set; }
	public string? PathContains { get; set; }
	public int? StatusCode { get; set; }
	public bool? OnlyErrors { get; set; }
	public Guid? UserId { get; set; }
	public string? Search { get; set; }
	public string? QueryContains { get; set; }
	public string? UserAgentContains { get; set; }
	public string? UserContains { get; set; }
	public string? TraceId { get; set; }
	public string? IpContains { get; set; }
	public string? HeaderContains { get; set; }
	public long? MinDurationMs { get; set; }
	public long? MaxDurationMs { get; set; }
	public bool? HasException { get; set; }
	public TagApiLogOrderBy OrderBy { get; set; } = TagApiLogOrderBy.TimestampDesc;
}

public sealed class ApiLogStatsParams : BaseParams {
	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }
	public string Bucket { get; set; } = "hour";
	public int TopEndpointsCount { get; set; } = 10;
}

public enum TagApiLogOrderBy {
	TimestampDesc,
	TimestampAsc,
	DurationDesc,
	DurationAsc
}
