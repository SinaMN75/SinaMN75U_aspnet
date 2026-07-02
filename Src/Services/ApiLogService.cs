namespace SinaMN75U.Services;

public interface IApiLogService {
	Task<UResponse<IEnumerable<ApiLogListItemResponse>?>> Search(ApiLogSearchParams p, CancellationToken ct);
	Task<UResponse<ApiLogDetailResponse?>> ReadById(IdParams p, CancellationToken ct);
	Task<UResponse<ApiLogStatsResponse?>> ReadStats(ApiLogStatsParams p, CancellationToken ct);
}

public class ApiLogService(DbContext db, ILocalizationService ls) : IApiLogService {
	public async Task<UResponse<IEnumerable<ApiLogListItemResponse>?>> Search(ApiLogSearchParams p, CancellationToken ct) {
		IQueryable<ApiRequestLogEntity> q = db.Set<ApiRequestLogEntity>();

		if (p.FromDate.HasValue) q = q.Where(x => x.Timestamp >= p.FromDate.Value);
		if (p.ToDate.HasValue) q = q.Where(x => x.Timestamp <= p.ToDate.Value);
		if (!string.IsNullOrWhiteSpace(p.Method)) q = q.Where(x => x.Method == p.Method);
		if (!string.IsNullOrWhiteSpace(p.PathContains)) q = q.Where(x => EF.Functions.ILike(x.Path, $"%{p.PathContains}%"));
		if (p.StatusCode.HasValue) q = q.Where(x => x.StatusCode == p.StatusCode.Value);
		if (p.OnlyErrors == true) q = q.Where(x => !x.IsSuccess);
		if (p.UserId.HasValue) q = q.Where(x => x.UserId == p.UserId.Value);

		if (!string.IsNullOrWhiteSpace(p.Search)) {
			string term = $"%{p.Search}%";
			q = q.Where(x => EF.Functions.ILike(x.Path, term)
				|| (x.ExceptionType != null && EF.Functions.ILike(x.ExceptionType, term))
				|| (x.ExceptionMessage != null && EF.Functions.ILike(x.ExceptionMessage, term)));
		}

		q = p.OrderBy switch {
			TagApiLogOrderBy.TimestampAsc => q.OrderBy(x => x.Timestamp),
			TagApiLogOrderBy.DurationDesc => q.OrderByDescending(x => x.DurationMs),
			TagApiLogOrderBy.DurationAsc => q.OrderBy(x => x.DurationMs),
			_ => q.OrderByDescending(x => x.Timestamp)
		};

		IQueryable<ApiLogListItemResponse> projected = q.Select(x => new ApiLogListItemResponse {
			Id = x.Id,
			Timestamp = x.Timestamp,
			Method = x.Method,
			Path = x.Path,
			StatusCode = x.StatusCode,
			IsSuccess = x.IsSuccess,
			DurationMs = x.DurationMs,
			UserId = x.UserId,
			IpAddress = x.IpAddress,
			ExceptionType = x.ExceptionType
		});

		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<ApiLogDetailResponse?>> ReadById(IdParams p, CancellationToken ct) {
		ApiLogDetailResponse? e = await db.Set<ApiRequestLogEntity>()
			.Where(x => x.Id == p.Id)
			.Select(x => new ApiLogDetailResponse {
				Id = x.Id,
				Timestamp = x.Timestamp,
				Method = x.Method,
				Path = x.Path,
				StatusCode = x.StatusCode,
				IsSuccess = x.IsSuccess,
				DurationMs = x.DurationMs,
				UserId = x.UserId,
				IpAddress = x.IpAddress,
				RequestBody = x.RequestBody,
				ResponseBody = x.ResponseBody,
				ExceptionType = x.ExceptionType,
				ExceptionMessage = x.ExceptionMessage,
				StackTrace = x.StackTrace
			})
			.FirstOrDefaultAsync(ct);

		return e == null
			? new UResponse<ApiLogDetailResponse?>(null, Usc.NotFound, ls.Get("LogNotFound"))
			: new UResponse<ApiLogDetailResponse?>(e);
	}

	public async Task<UResponse<ApiLogStatsResponse?>> ReadStats(ApiLogStatsParams p, CancellationToken ct) {
		DateTime to = p.ToDate ?? DateTime.UtcNow;
		DateTime from = p.FromDate ?? to.AddDays(-1);
		bool byDay = p.Bucket == "day";

		IQueryable<ApiRequestLogEntity> range = db.Set<ApiRequestLogEntity>()
			.Where(x => x.Timestamp >= from && x.Timestamp <= to);

		int totalRequests = await range.CountAsync(ct);
		int totalErrors = await range.CountAsync(x => !x.IsSuccess, ct);
		double avgDuration = totalRequests == 0 ? 0 : await range.AverageAsync(x => (double)x.DurationMs, ct);
		long maxDuration = totalRequests == 0 ? 0 : await range.MaxAsync(x => x.DurationMs, ct);

		// Bucket via DateTime construction from Year/Month/Day(/Hour) instead of a provider-specific
		// date_trunc helper - the installed Npgsql EF provider (10.0.1) doesn't ship EF.Functions.DateTrunc
		// at all, whereas this pattern is a standard, version-agnostic translation supported by every
		// relational EF provider. Two separate GroupBy calls (rather than branching inside one lambda)
		// keep each expression tree simple and reliably translatable.
		IQueryable<IGrouping<DateTime, ApiRequestLogEntity>> grouped = byDay
			? range.GroupBy(x => new DateTime(x.Timestamp.Year, x.Timestamp.Month, x.Timestamp.Day))
			: range.GroupBy(x => new DateTime(x.Timestamp.Year, x.Timestamp.Month, x.Timestamp.Day, x.Timestamp.Hour, 0, 0));

		List<ApiLogTimeBucketResponse> timeSeries = await grouped
			.Select(g => new ApiLogTimeBucketResponse {
				Bucket = g.Key,
				Total = g.Count(),
				Errors = g.Count(x => !x.IsSuccess),
				AvgDurationMs = g.Average(x => (double)x.DurationMs)
			})
			.OrderBy(x => x.Bucket)
			.ToListAsync(ct);

		List<ApiLogStatusCountResponse> statusDistribution = await range
			.GroupBy(x => x.StatusCode)
			.Select(g => new ApiLogStatusCountResponse { StatusCode = g.Key, Count = g.Count() })
			.OrderByDescending(x => x.Count)
			.ToListAsync(ct);

		List<ApiLogEndpointStatResponse> topSlow = await range
			.GroupBy(x => x.Path)
			.Select(g => new ApiLogEndpointStatResponse {
				Path = g.Key,
				Count = g.Count(),
				ErrorCount = g.Count(x => !x.IsSuccess),
				AvgDurationMs = g.Average(x => (double)x.DurationMs)
			})
			.OrderByDescending(x => x.AvgDurationMs)
			.Take(p.TopEndpointsCount)
			.ToListAsync(ct);

		List<ApiLogEndpointStatResponse> topFailing = await range
			.GroupBy(x => x.Path)
			.Where(g => g.Any(x => !x.IsSuccess))
			.Select(g => new ApiLogEndpointStatResponse {
				Path = g.Key,
				Count = g.Count(),
				ErrorCount = g.Count(x => !x.IsSuccess),
				AvgDurationMs = g.Average(x => (double)x.DurationMs)
			})
			.OrderByDescending(x => x.ErrorCount)
			.Take(p.TopEndpointsCount)
			.ToListAsync(ct);

		return new UResponse<ApiLogStatsResponse?>(new ApiLogStatsResponse {
			TotalRequests = totalRequests,
			TotalErrors = totalErrors,
			ErrorRatePercent = totalRequests == 0 ? 0 : Math.Round(totalErrors / (double)totalRequests * 100, 2),
			AvgDurationMs = Math.Round(avgDuration, 1),
			MaxDurationMs = maxDuration,
			TimeSeries = timeSeries,
			StatusCodeDistribution = statusDistribution,
			TopSlowEndpoints = topSlow,
			TopFailingEndpoints = topFailing
		});
	}
}
