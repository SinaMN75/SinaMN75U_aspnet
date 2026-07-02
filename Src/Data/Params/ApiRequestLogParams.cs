namespace SinaMN75U.Data.Params;

public sealed class ApiLogSearchParams : BaseParams {
	public int PageSize { get; set; } = 50;
	public int PageNumber { get; set; } = 1;

	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }

	public string? Method { get; set; }
	public string? PathContains { get; set; }
	public int? StatusCode { get; set; }

	// Convenience bucket filters for the UI (2xx/4xx/5xx tabs) instead of forcing exact StatusCode.
	public bool? OnlyErrors { get; set; }
	public Guid? UserId { get; set; }

	// Free-text search across Path / ExceptionType / ExceptionMessage.
	public string? Search { get; set; }

	public TagApiLogOrderBy OrderBy { get; set; } = TagApiLogOrderBy.TimestampDesc;
}

public sealed class ApiLogStatsParams : BaseParams {
	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }

	// "hour" or "day" - controls the time-series bucket granularity.
	public string Bucket { get; set; } = "hour";
	public int TopEndpointsCount { get; set; } = 10;
}

public enum TagApiLogOrderBy {
	TimestampDesc,
	TimestampAsc,
	DurationDesc,
	DurationAsc
}
