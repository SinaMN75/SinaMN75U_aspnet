namespace SinaMN75U.Services;

public interface IApiLogService {
	Task<UResponse<IEnumerable<ApiLogListItemResponse>?>> Search(ApiLogSearchParams p, CancellationToken ct);
	Task<UResponse<ApiLogDetailResponse?>> ReadById(IdParams p, CancellationToken ct);
	Task<UResponse<ApiLogStatsResponse?>> ReadStats(ApiLogStatsParams p, CancellationToken ct);
	Task<byte[]> Export(ApiLogSearchParams p, CancellationToken ct);
}

public class ApiLogService(DbContext db, ILocalizationService ls) : IApiLogService {
	private static IQueryable<ApiRequestLogEntity> BuildFilteredQuery(IQueryable<ApiRequestLogEntity> q, ApiLogSearchParams p) {
		if (p.FromDate.HasValue) q = q.Where(x => x.Timestamp >= p.FromDate.Value);
		if (p.ToDate.HasValue) q = q.Where(x => x.Timestamp <= p.ToDate.Value);
		if (!string.IsNullOrWhiteSpace(p.Method)) q = q.Where(x => x.Method == p.Method);
		if (!string.IsNullOrWhiteSpace(p.PathContains)) q = q.Where(x => EF.Functions.ILike(x.Path, $"%{p.PathContains}%"));
		if (p.StatusCode.HasValue) q = q.Where(x => x.StatusCode == p.StatusCode.Value);
		if (p.OnlyErrors == true) q = q.Where(x => !x.IsSuccess);
		if (p.UserId.HasValue) q = q.Where(x => x.UserId == p.UserId.Value);
		if (p.HasException.HasValue) q = p.HasException.Value ? q.Where(x => x.ExceptionType != null) : q.Where(x => x.ExceptionType == null);
		if (p.MinDurationMs.HasValue) q = q.Where(x => x.DurationMs >= p.MinDurationMs.Value);
		if (p.MaxDurationMs.HasValue) q = q.Where(x => x.DurationMs <= p.MaxDurationMs.Value);

		if (!string.IsNullOrWhiteSpace(p.QueryContains)) {
			string term = $"%{p.QueryContains}%";
			q = q.Where(x => x.QueryString != null && EF.Functions.ILike(x.QueryString, term));
		}

		if (!string.IsNullOrWhiteSpace(p.UserAgentContains)) {
			string term = $"%{p.UserAgentContains}%";
			q = q.Where(x => x.UserAgent != null && EF.Functions.ILike(x.UserAgent, term));
		}

		if (!string.IsNullOrWhiteSpace(p.UserContains)) {
			string term = $"%{p.UserContains}%";
			q = q.Where(x => (x.UserName != null && EF.Functions.ILike(x.UserName, term))
			                 || (x.UserEmail != null && EF.Functions.ILike(x.UserEmail, term)));
		}

		if (!string.IsNullOrWhiteSpace(p.TraceId)) q = q.Where(x => x.TraceId == p.TraceId);

		if (!string.IsNullOrWhiteSpace(p.IpContains)) {
			string term = $"%{p.IpContains}%";
			q = q.Where(x => x.IpAddress != null && EF.Functions.ILike(x.IpAddress, term));
		}

		if (!string.IsNullOrWhiteSpace(p.HeaderContains)) {
			string term = $"%{p.HeaderContains}%";
			q = q.Where(x => (x.RequestHeaders != null && EF.Functions.ILike(x.RequestHeaders, term))
			                 || (x.ResponseHeaders != null && EF.Functions.ILike(x.ResponseHeaders, term)));
		}

		if (!string.IsNullOrWhiteSpace(p.Search)) {
			string term = $"%{p.Search}%";
			q = q.Where(x => EF.Functions.ILike(x.Path, term)
			                 || (x.ExceptionType != null && EF.Functions.ILike(x.ExceptionType, term))
			                 || (x.ExceptionMessage != null && EF.Functions.ILike(x.ExceptionMessage, term))
			                 || (x.QueryString != null && EF.Functions.ILike(x.QueryString, term))
			                 || (x.UserName != null && EF.Functions.ILike(x.UserName, term))
			                 || (x.UserEmail != null && EF.Functions.ILike(x.UserEmail, term)));
		}

		return q;
	}

	public async Task<UResponse<IEnumerable<ApiLogListItemResponse>?>> Search(ApiLogSearchParams p, CancellationToken ct) {
		IQueryable<ApiRequestLogEntity> q = BuildFilteredQuery(db.Set<ApiRequestLogEntity>(), p);

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
			QueryString = x.QueryString,
			StatusCode = x.StatusCode,
			IsSuccess = x.IsSuccess,
			DurationMs = x.DurationMs,
			UserId = x.UserId,
			UserName = x.UserName,
			IpAddress = x.IpAddress,
			UserAgent = x.UserAgent,
			TraceId = x.TraceId,
			Host = x.Host,
			RequestSizeBytes = x.RequestSizeBytes,
			ResponseSizeBytes = x.ResponseSizeBytes,
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
				QueryString = x.QueryString,
				StatusCode = x.StatusCode,
				IsSuccess = x.IsSuccess,
				DurationMs = x.DurationMs,
				UserId = x.UserId,
				UserName = x.UserName,
				UserEmail = x.UserEmail,
				UserRoles = x.UserRoles,
				IpAddress = x.IpAddress,
				UserAgent = x.UserAgent,
				TraceId = x.TraceId,
				Host = x.Host,
				RequestSizeBytes = x.RequestSizeBytes,
				ResponseSizeBytes = x.ResponseSizeBytes,
				RequestBody = x.RequestBody,
				ResponseBody = x.ResponseBody,
				RequestHeaders = x.RequestHeaders,
				ResponseHeaders = x.ResponseHeaders,
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
		
		List<long> sortedDurations = totalRequests == 0
			? []
			: await range.OrderBy(x => x.DurationMs).Select(x => x.DurationMs).ToListAsync(ct);
		double p50 = Percentile(sortedDurations, 0.50);
		double p95 = Percentile(sortedDurations, 0.95);
		double p99 = Percentile(sortedDurations, 0.99);
		
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

		List<ApiLogListItemResponse> slowest = await range
			.OrderByDescending(x => x.DurationMs)
			.Take(15)
			.Select(x => new ApiLogListItemResponse {
				Id = x.Id,
				Timestamp = x.Timestamp,
				Method = x.Method,
				Path = x.Path,
				QueryString = x.QueryString,
				StatusCode = x.StatusCode,
				IsSuccess = x.IsSuccess,
				DurationMs = x.DurationMs,
				UserId = x.UserId,
				UserName = x.UserName,
				IpAddress = x.IpAddress,
				UserAgent = x.UserAgent,
				TraceId = x.TraceId,
				Host = x.Host,
				RequestSizeBytes = x.RequestSizeBytes,
				ResponseSizeBytes = x.ResponseSizeBytes,
				ExceptionType = x.ExceptionType
			})
			.ToListAsync(ct);

		return new UResponse<ApiLogStatsResponse?>(new ApiLogStatsResponse {
			TotalRequests = totalRequests,
			TotalErrors = totalErrors,
			ErrorRatePercent = totalRequests == 0 ? 0 : Math.Round(totalErrors / (double)totalRequests * 100, 2),
			AvgDurationMs = Math.Round(avgDuration, 1),
			MaxDurationMs = maxDuration,
			P50DurationMs = p50,
			P95DurationMs = p95,
			P99DurationMs = p99,
			TimeSeries = timeSeries,
			StatusCodeDistribution = statusDistribution,
			TopSlowEndpoints = topSlow,
			TopFailingEndpoints = topFailing,
			SlowestRequests = slowest
		});
	}

	private static double Percentile(List<long> sortedValues, double p) {
		if (sortedValues.Count == 0) return 0;
		int rank = (int)Math.Ceiling(p * sortedValues.Count) - 1;
		rank = Math.Clamp(rank, 0, sortedValues.Count - 1);
		return sortedValues[rank];
	}

	public async Task<byte[]> Export(ApiLogSearchParams p, CancellationToken ct) {
		IQueryable<ApiRequestLogEntity> q = BuildFilteredQuery(db.Set<ApiRequestLogEntity>(), p)
			.OrderByDescending(x => x.Timestamp)
			.Take(10_000); // hard cap - this is a diagnostic export, not a data pipeline

		List<ApiRequestLogEntity> rows = await q.ToListAsync(ct);

		StringBuilder sb = new();
		sb.AppendLine(string.Join(",", "Timestamp", "Method", "Path", "QueryString", "StatusCode", "DurationMs",
			"UserId", "UserName", "UserEmail", "IpAddress", "UserAgent", "TraceId", "Host", "ExceptionType", "ExceptionMessage"));

		foreach (ApiRequestLogEntity r in rows) {
			sb.AppendLine(string.Join(",",
				CsvEscape(r.Timestamp.ToString("o")),
				CsvEscape(r.Method),
				CsvEscape(r.Path),
				CsvEscape(r.QueryString),
				CsvEscape(r.StatusCode.ToString()),
				CsvEscape(r.DurationMs.ToString()),
				CsvEscape(r.UserId?.ToString()),
				CsvEscape(r.UserName),
				CsvEscape(r.UserEmail),
				CsvEscape(r.IpAddress),
				CsvEscape(r.UserAgent),
				CsvEscape(r.TraceId),
				CsvEscape(r.Host),
				CsvEscape(r.ExceptionType),
				CsvEscape(r.ExceptionMessage)));
		}

		return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
	}

	private static string CsvEscape(string? value) {
		if (string.IsNullOrEmpty(value)) return "";
		bool needsQuoting = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
		string escaped = value.Replace("\"", "\"\"");
		return needsQuoting ? $"\"{escaped}\"" : escaped;
	}
}