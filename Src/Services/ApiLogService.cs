namespace SinaMN75U.Services;

public interface IApiLogService {
	Task Create(ApiLogCreateParams p, CancellationToken ct);
	Task CreateMany(IReadOnlyCollection<ApiLogCreateParams> items, CancellationToken ct);
	Task<UResponse<IEnumerable<ApiLogResponse>?>> Read(ApiLogReadParams p, CancellationToken ct);
	Task<UResponse<ApiLogStatsResponse?>> Stats(ApiLogStatsParams p, CancellationToken ct);
}

public class ApiLogService(DbContext db) : IApiLogService {
	public async Task Create(ApiLogCreateParams p, CancellationToken ct) {
		ApiLogEntity? entity = MapToEntity(p);
		if (entity == null) return;

		db.Set<ApiLogEntity>().Add(entity);
		await db.SaveChangesAsync(ct);
	}

	public async Task CreateMany(IReadOnlyCollection<ApiLogCreateParams> items, CancellationToken ct) {
		List<ApiLogEntity> entities = [];
		entities.AddRange(items.Select(MapToEntity).OfType<ApiLogEntity>());

		if (entities.Count == 0) return;

		await db.Set<ApiLogEntity>().AddRangeAsync(entities, ct);
		await db.SaveChangesAsync(ct);
	}

	private static ApiLogEntity? MapToEntity(ApiLogCreateParams p) {
		if (!Core.App.Middleware.Log) return null;

		if (p.StatusCode is >= 200 and <= 299 && !Core.App.Middleware.LogSuccess) return null;
		if (p.Path.Contains("log", StringComparison.CurrentCultureIgnoreCase)) return null;
		if (p.Path.Contains("dashboard", StringComparison.CurrentCultureIgnoreCase)) return null;

		List<TagApiLog> tags = [
			p.Method.ToUpperInvariant() switch {
				"GET" => TagApiLog.Get,
				"POST" => TagApiLog.Post,
				"PUT" => TagApiLog.Put,
				"PATCH" => TagApiLog.Patch,
				"DELETE" => TagApiLog.Delete,
				_ => TagApiLog.Other
			},

			p.StatusCode switch {
				>= 200 and <= 299 => TagApiLog.Success,
				>= 400 and <= 499 => TagApiLog.ClientError,
				_ => TagApiLog.ServerError
			}
		];

		if (p.ExceptionType.IsNotNullOrEmpty()) tags.Add(TagApiLog.HasException);

		return new ApiLogEntity {
			Path = Truncate(p.Path, 500) ?? "",
			StatusCode = p.StatusCode,
			DurationMs = p.DurationMs,
			UserId = p.UserId,
			IpAddress = Truncate(p.IpAddress, 64),
			TraceId = Truncate(p.TraceId, 100),
			Tags = tags,
			JsonData = new ApiLogJson {
				Method = p.Method,
				QueryString = TruncateBody(p.QueryString),
				RequestBody = TruncateBody(p.RequestBody),
				ResponseBody = TruncateBody(p.ResponseBody),
				RequestHeaders = TruncateBody(p.RequestHeaders),
				ResponseHeaders = TruncateBody(p.ResponseHeaders),
				UserAgent = Truncate(p.UserAgent, 500),
				Host = Truncate(p.Host, 200),
				UserName = Truncate(p.UserName, 200),
				UserEmail = Truncate(p.UserEmail, 300),
				UserRoles = Truncate(p.UserRoles, 300),
				ExceptionType = p.ExceptionType,
				ExceptionMessage = p.ExceptionMessage,
				StackTrace = p.StackTrace,
				RequestSizeBytes = p.RequestSizeBytes,
				ResponseSizeBytes = p.ResponseSizeBytes
			},
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = UConstants.SystemAdminId
		};
	}

	public async Task<UResponse<IEnumerable<ApiLogResponse>?>> Read(ApiLogReadParams p, CancellationToken ct) {
		IQueryable<ApiLogEntity> q = Filter(db.Set<ApiLogEntity>(), p).ApplyReadParams(p);
		IQueryable<ApiLogResponse> projected = q.Select(Projections.ApiLogSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<ApiLogStatsResponse?>> Stats(ApiLogStatsParams p, CancellationToken ct) {
		DateTime to = p.ToCreatedAt ?? DateTime.UtcNow;
		DateTime from = p.FromCreatedAt ?? to.AddDays(-1);
		bool byDay = p.Bucket == "day";

		IQueryable<ApiLogEntity> range = db.Set<ApiLogEntity>().Where(x => x.CreatedAt >= from && x.CreatedAt <= to);

		int total = await range.CountAsync(ct);
		int errorCount = await range.CountAsync(x => x.StatusCode >= 400, ct);
		int successCount = total - errorCount;
		double avgDuration = total == 0 ? 0 : await range.AverageAsync(x => (double)x.DurationMs, ct);

		List<long> durations = total == 0 ? [] : await range.OrderBy(x => x.DurationMs).Select(x => x.DurationMs).ToListAsync(ct);
		double p50 = Percentile(durations, 0.50);
		double p95 = Percentile(durations, 0.95);
		double p99 = Percentile(durations, 0.99);

		List<StatRow> rows = await range.Select(x => new StatRow { CreatedAt = x.CreatedAt, StatusCode = x.StatusCode, DurationMs = x.DurationMs, Path = x.Path }).ToListAsync(ct);

		List<ApiLogBucketResponse> timeline = rows
			.GroupBy(x => byDay ? new DateTime(x.CreatedAt.Year, x.CreatedAt.Month, x.CreatedAt.Day) : new DateTime(x.CreatedAt.Year, x.CreatedAt.Month, x.CreatedAt.Day, x.CreatedAt.Hour, 0, 0))
			.OrderBy(g => g.Key)
			.Select(g => new ApiLogBucketResponse {
				Time = g.Key,
				Count = g.Count(),
				ErrorCount = g.Count(x => x.StatusCode >= 400),
				AverageDurationMs = g.Average(x => (double)x.DurationMs)
			})
			.ToList();

		List<ApiLogEndpointResponse> slowestEndpoints = rows
			.GroupBy(x => x.Path)
			.Select(g => new ApiLogEndpointResponse { Path = g.Key, Count = g.Count(), AverageDurationMs = g.Average(x => (double)x.DurationMs) })
			.OrderByDescending(x => x.AverageDurationMs)
			.Take(10)
			.ToList();

		List<ApiLogEndpointResponse> failingEndpoints = rows
			.Where(x => x.StatusCode >= 400)
			.GroupBy(x => x.Path)
			.Select(g => new ApiLogEndpointResponse { Path = g.Key, Count = g.Count(), AverageDurationMs = g.Average(x => (double)x.DurationMs) })
			.OrderByDescending(x => x.Count)
			.Take(10)
			.ToList();

		List<ApiLogResponse> slowestRequests = await range
			.OrderByDescending(x => x.DurationMs)
			.Take(15)
			.Select(Projections.ApiLogSelector(new ApiLogSelectorArgs()))
			.ToListAsync(ct);

		return new UResponse<ApiLogStatsResponse?>(new ApiLogStatsResponse {
			TotalCount = total,
			SuccessCount = successCount,
			ErrorCount = errorCount,
			AverageDurationMs = Math.Round(avgDuration, 1),
			P50DurationMs = p50,
			P95DurationMs = p95,
			P99DurationMs = p99,
			Timeline = timeline,
			SlowestEndpoints = slowestEndpoints,
			FailingEndpoints = failingEndpoints,
			SlowestRequests = slowestRequests
		});
	}

	private static IQueryable<ApiLogEntity> Filter(IQueryable<ApiLogEntity> q, ApiLogReadParams p) {
		if (!string.IsNullOrWhiteSpace(p.PathContains)) q = q.Where(x => EF.Functions.ILike(x.Path, $"%{p.PathContains}%"));
		if (p.StatusCode.HasValue) q = q.Where(x => x.StatusCode == p.StatusCode.Value);
		if (p.MinDurationMs.HasValue) q = q.Where(x => x.DurationMs >= p.MinDurationMs.Value);
		if (p.MaxDurationMs.HasValue) q = q.Where(x => x.DurationMs <= p.MaxDurationMs.Value);
		if (p.UserId.HasValue) q = q.Where(x => x.UserId == p.UserId.Value);
		if (!string.IsNullOrWhiteSpace(p.IpAddress)) q = q.Where(x => x.IpAddress == p.IpAddress);
		if (!string.IsNullOrWhiteSpace(p.TraceId)) q = q.Where(x => x.TraceId == p.TraceId);
		if (p.OnlyErrors == true) q = q.Where(x => x.StatusCode >= 400);
		return q;
	}

	private static double Percentile(List<long> sortedValues, double p) {
		if (sortedValues.Count == 0) return 0;
		int rank = (int)Math.Ceiling(p * sortedValues.Count) - 1;
		rank = Math.Clamp(rank, 0, sortedValues.Count - 1);
		return sortedValues[rank];
	}

	private static string? Truncate(string? s, int max) => string.IsNullOrEmpty(s) || s.Length <= max ? s : s[..max];

	private static string? TruncateBody(string? s) => string.IsNullOrEmpty(s) || s.Length <= 20_000 ? s : s[..20_000] + "...<truncated>";

	private sealed class StatRow {
		public DateTime CreatedAt { get; init; }
		public int StatusCode { get; init; }
		public long DurationMs { get; init; }
		public string Path { get; init; } = "";
	}
}