namespace SinaMN75U.Services;

public interface IDashboardService {
	Task<SystemMetricsResponse> ReadSystemMetrics();
	Task<DashboardResponse> ReadDashboardData(BaseParams p, CancellationToken ct);
	Task<UResponse<OsMetricsResponse?>> ReadOsMetrics(BaseParams p, CancellationToken ct);
	Task CreateApiLog(ApiLogCreateParams p, CancellationToken ct);
	Task CreateManyApiLogs(IReadOnlyCollection<ApiLogCreateParams> items, CancellationToken ct);
	Task<UResponse<IEnumerable<ApiLogResponse>?>> ReadApiLogs(ApiLogReadParams p, CancellationToken ct);
	Task<UResponse<ApiLogStatsResponse?>> ApiLogStats(ApiLogStatsParams p, CancellationToken ct);
}

public class DashboardService(
	DbContext db,
	ICategoryService categoryService,
	ICommentService commentService,
	IContentService contentService,
	IMediaService mediaService,
	IProductService productService,
	IUserService userService,
	ITokenService ts,
	ILocalizationService ls
) : IDashboardService {
	public async Task<SystemMetricsResponse> ReadSystemMetrics() {
		string[] linuxMemInfoKeys = ["MemTotal:", "MemAvailable:"];
		const double bytesToGb = 1024.0 * 1024 * 1024;
		double memUsage = 0, totalMem = 0, freeMem = 0, cpuUsage = 0, diskUsage = 0, totalDisk = 0, freeDisk = 0;
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
			try {
				(ulong user, ulong nice, ulong system, ulong total) firstSample = await GetLinuxCpuSample();
				await Task.Delay(1000);
				(ulong user, ulong nice, ulong system, ulong total) secondSample = await GetLinuxCpuSample();
				ulong used = secondSample.user + secondSample.nice + secondSample.system - (firstSample.user + firstSample.nice + firstSample.system);
				ulong total = secondSample.total - firstSample.total;
				cpuUsage = (double)used / total * 100;
				string[] memLines = await File.ReadAllLinesAsync("/proc/meminfo");
				double[] memInfo = memLines.Take(3)
					.Where(l => linuxMemInfoKeys.Any(l.StartsWith))
					.Select(l => double.Parse(l.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]))
					.ToArray();

				totalMem = memInfo[0] / (1024 * 1024);
				freeMem = memInfo[1] / (1024 * 1024);
				memUsage = 100 - freeMem / totalMem * 100;
			}
			catch {
				// Fallback values remain at 0
			}
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
			try {
				using Process? cpuProcess = Process.Start(new ProcessStartInfo {
					FileName = "top",
					Arguments = "-l 1 -n 0 -stats cpu",
					RedirectStandardOutput = true,
					UseShellExecute = false
				});

				if (cpuProcess != null) {
					string cpuOutput = await cpuProcess.StandardOutput.ReadToEndAsync();
					await cpuProcess.WaitForExitAsync();

					string? cpuLine = cpuOutput.Split('\n')
						.FirstOrDefault(l => l.Trim().StartsWith("CPU usage:"));

					if (cpuLine != null) {
						string percent = cpuLine.Split(':')[1].Trim().Split(' ')[0].TrimEnd('%');
						cpuUsage = double.Parse(percent);
					}
				}

				(double macTotalGb, double macFreeGb, double _, double macUsagePercent) = await GetMacMemory(CancellationToken.None);
				totalMem = macTotalGb;
				freeMem = macFreeGb;
				memUsage = macUsagePercent;
			}
			catch {
				// Fallback values remain at 0
			}
		}

		try {
			DriveInfo drive = DriveInfo.GetDrives()
				.First(d => d is { IsReady: true, Name: "/" or "C:\\" });

			totalDisk = drive.TotalSize / bytesToGb;
			freeDisk = drive.AvailableFreeSpace / bytesToGb;
			diskUsage = 100 - freeDisk / totalDisk * 100;
		}
		catch {
			// Fallback values remain at 0
		}

		return new SystemMetricsResponse(
			Math.Round(cpuUsage, 1),
			Math.Round(memUsage, 1),
			Math.Round(diskUsage, 1),
			Math.Round(totalMem, 6),
			Math.Round(freeMem, 6),
			Math.Round(totalDisk, 6),
			Math.Round(freeDisk, 6),
			DateTime.UtcNow
		);
	}

	public async Task<DashboardResponse> ReadDashboardData(BaseParams p, CancellationToken ct) {
		UResponse<IEnumerable<UserResponse>?> newUsers = await userService.Read(new UserReadParams { Token = p.Token, PageSize = 5 }, ct);
		UResponse<IEnumerable<CategoryResponse>?> newCategories = await categoryService.Read(new CategoryReadParams { Token = p.Token, PageSize = 5 }, ct);
		UResponse<IEnumerable<CommentResponse>?> newComments = await commentService.Read(new CommentReadParams { Token = p.Token, PageSize = 5 }, ct);
		UResponse<IEnumerable<ContentResponse>?> newContents = await contentService.Read(new ContentReadParams { Token = p.Token, PageSize = 5 }, ct);
		UResponse<IEnumerable<MediaResponse>?> newMedia = await mediaService.Read(new BaseReadParams<TagMedia> { Token = p.Token, PageSize = 5 }, ct);
		UResponse<IEnumerable<ProductResponse>?> newProducts = await productService.Read(new ProductReadParams { Token = p.Token, PageSize = 5 }, ct);

		return new DashboardResponse {
			Categories = await db.Set<CategoryEntity>().CountAsync(ct),
			Comments = await db.Set<CommentEntity>().CountAsync(ct),
			Contents = await db.Set<ContentEntity>().CountAsync(ct),
			Media = await db.Set<MediaEntity>().CountAsync(ct),
			Products = await db.Set<ProductEntity>().CountAsync(ct),
			Users = await db.Set<UserEntity>().CountAsync(ct),
			NewUsers = newUsers.Result ?? [],
			NewCategories = newCategories.Result ?? [],
			NewComments = newComments.Result ?? [],
			NewContents = newContents.Result ?? [],
			NewMedia = newMedia.Result ?? [],
			NewProducts = newProducts.Result ?? []
		};
	}

	private static async Task<(ulong user, ulong nice, ulong system, ulong total)> GetLinuxCpuSample() {
		string[] lines = await File.ReadAllLinesAsync("/proc/stat");
		string[] values = lines.First(l => l.StartsWith("cpu ")).Split(' ', StringSplitOptions.RemoveEmptyEntries);

		ulong user = ulong.Parse(values[1]);
		ulong nice = ulong.Parse(values[2]);
		ulong system = ulong.Parse(values[3]);
		ulong idle = ulong.Parse(values[4]);
		ulong total = user + nice + system + idle;

		return (user, nice, system, total);
	}
	
	
	
	public async Task<UResponse<OsMetricsResponse?>> ReadOsMetrics(BaseParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<OsMetricsResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<OsMetricsResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse<OsMetricsResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		const double bytesToGb = 1024.0 * 1024 * 1024;

		double cpuUsage = 0, memUsage = 0, totalMemGb = 0, freeMemGb = 0;
		double? load1 = null, load5 = null, load15 = null;

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
			try {
				(ulong user, ulong nice, ulong system, ulong total) firstSample = await GetLinuxCpuSample();
				await Task.Delay(700, ct);
				(ulong user, ulong nice, ulong system, ulong total) secondSample = await GetLinuxCpuSample();
				ulong used = secondSample.user + secondSample.nice + secondSample.system - (firstSample.user + firstSample.nice + firstSample.system);
				ulong total = secondSample.total - firstSample.total;
				cpuUsage = total == 0 ? 0 : (double)used / total * 100;

				string[] memLines = await File.ReadAllLinesAsync("/proc/meminfo", ct);
				double[] memInfo = memLines.Take(3)
					.Where(l => l.StartsWith("MemTotal:") || l.StartsWith("MemAvailable:"))
					.Select(l => double.Parse(l.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]))
					.ToArray();
				totalMemGb = memInfo[0] / (1024 * 1024);
				double freeMemGbLocal = memInfo[1] / (1024 * 1024);
				freeMemGb = freeMemGbLocal;
				memUsage = totalMemGb == 0 ? 0 : 100 - freeMemGb / totalMemGb * 100;
			}
			catch {
				/* fallback values remain at 0 */
			}

			try {
				string loadAvg = await File.ReadAllTextAsync("/proc/loadavg", ct);
				string[] parts = loadAvg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
				load1 = double.Parse(parts[0]);
				load5 = double.Parse(parts[1]);
				load15 = double.Parse(parts[2]);
			}
			catch {
				/* not available */
			}
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
			try {
				using Process? cpuProcess = Process.Start(new ProcessStartInfo { FileName = "top", Arguments = "-l 1 -n 0 -stats cpu", RedirectStandardOutput = true, UseShellExecute = false });
				if (cpuProcess != null) {
					string cpuOutput = await cpuProcess.StandardOutput.ReadToEndAsync(ct);
					await cpuProcess.WaitForExitAsync(ct);
					string? cpuLine = cpuOutput.Split('\n').FirstOrDefault(l => l.Trim().StartsWith("CPU usage:"));
					if (cpuLine != null) {
						string percent = cpuLine.Split(':')[1].Trim().Split(' ')[0].TrimEnd('%');
						cpuUsage = double.Parse(percent);
					}
				}

				(double macTotalGb, double macFreeGb, double _, double macUsagePercent) = await GetMacMemory(ct);
				totalMemGb = macTotalGb;
				freeMemGb = macFreeGb;
				memUsage = macUsagePercent;
			}
			catch {
				/* fallback values remain at 0 */
			}

			try {
				using Process? loadProcess = Process.Start(new ProcessStartInfo { FileName = "sysctl", Arguments = "-n vm.loadavg", RedirectStandardOutput = true, UseShellExecute = false });
				if (loadProcess != null) {
					string output = await loadProcess.StandardOutput.ReadToEndAsync(ct);
					await loadProcess.WaitForExitAsync(ct);
					string[] parts = output.Trim().Trim('{', '}').Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (parts.Length >= 3) {
						load1 = double.Parse(parts[0]);
						load5 = double.Parse(parts[1]);
						load15 = double.Parse(parts[2]);
					}
				}
			}
			catch {
				/* not available */
			}
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
			try {
				GetSystemTimes(out Win32FileTime firstIdle, out Win32FileTime firstKernel, out Win32FileTime firstUser);
				await Task.Delay(700, ct);
				GetSystemTimes(out Win32FileTime secondIdle, out Win32FileTime secondKernel, out Win32FileTime secondUser);

				ulong idleDelta = ToUlong(secondIdle) - ToUlong(firstIdle);
				ulong kernelDelta = ToUlong(secondKernel) - ToUlong(firstKernel);
				ulong userDelta = ToUlong(secondUser) - ToUlong(firstUser);
				ulong total = kernelDelta + userDelta;
				cpuUsage = total == 0 ? 0 : 100.0 * (1 - idleDelta / (double)total);

				MemoryStatusEx status = new() { dwLength = (uint)Marshal.SizeOf<MemoryStatusEx>() };
				if (GlobalMemoryStatusEx(ref status)) {
					totalMemGb = status.ullTotalPhys / bytesToGb;
					freeMemGb = status.ullAvailPhys / bytesToGb;
					memUsage = totalMemGb == 0 ? 0 : 100 - freeMemGb / totalMemGb * 100;
				}
			}
			catch {
				/* fallback values remain at 0 */
			}
		}
		
		(double diskTotalGb, double diskUsedGb, double diskFreeGb, double diskUsagePercent) = await GetDisk(ct);

		DateTime processStartedAt = DateTime.UtcNow;
		double processUptimeSeconds = 0;
		try {
			Process current = Process.GetCurrentProcess();
			processStartedAt = current.StartTime.ToUniversalTime();
			processUptimeSeconds = (DateTime.UtcNow - processStartedAt).TotalSeconds;
		}
		catch {
			/* leave process uptime at defaults */
		}

		return new UResponse<OsMetricsResponse?>(new OsMetricsResponse {
			GeneratedAt = DateTime.UtcNow,
			OsName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "macOS" : "Unknown",
			OsDescription = RuntimeInformation.OSDescription,
			OsArchitecture = RuntimeInformation.OSArchitecture.ToString(),
			ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
			FrameworkDescription = RuntimeInformation.FrameworkDescription,
			MachineName = SafeGet(() => Environment.MachineName) ?? "",
			Is64BitOperatingSystem = Environment.Is64BitOperatingSystem,
			Is64BitProcess = Environment.Is64BitProcess,
			ProcessorCount = Environment.ProcessorCount,
			SystemUptimeSeconds = Math.Round(Environment.TickCount64 / 1000.0, 1),
			ProcessUptimeSeconds = Math.Round(processUptimeSeconds, 1),
			ProcessStartedAt = processStartedAt,
			CpuUsagePercent = Math.Round(cpuUsage, 1),
			LoadAverage1Min = load1,
			LoadAverage5Min = load5,
			LoadAverage15Min = load15,
			MemoryTotalGb = Math.Round(totalMemGb, 2),
			MemoryUsedGb = Math.Round(totalMemGb - freeMemGb, 2),
			MemoryFreeGb = Math.Round(freeMemGb, 2),
			MemoryUsagePercent = Math.Round(memUsage, 1),
			DiskTotalGb = diskTotalGb,
			DiskUsedGb = diskUsedGb,
			DiskFreeGb = diskFreeGb,
			DiskUsagePercent = diskUsagePercent
		});
	}
	
	public async Task CreateApiLog(ApiLogCreateParams p, CancellationToken ct) {
		ApiLogEntity? entity = MapToApiLogEntity(p);
		if (entity == null) return;

		db.Set<ApiLogEntity>().Add(entity);
		await db.SaveChangesAsync(ct);
	}

	public async Task CreateManyApiLogs(IReadOnlyCollection<ApiLogCreateParams> items, CancellationToken ct) {
		List<ApiLogEntity> entities = [];
		entities.AddRange(items.Select(MapToApiLogEntity).OfType<ApiLogEntity>());

		if (entities.Count == 0) return;

		await db.Set<ApiLogEntity>().AddRangeAsync(entities, ct);
		await db.SaveChangesAsync(ct);
	}

	private static ApiLogEntity? MapToApiLogEntity(ApiLogCreateParams p) {
		if (!Core.App.Middleware.Log) return null;

		if (p.StatusCode is >= 200 and <= 299 && !Core.App.Middleware.LogSuccess) return null;
		if (p.Path.StartsWith("/" + RouteTags.Log, StringComparison.OrdinalIgnoreCase)) return null;
		if (p.Path.StartsWith("/" + RouteTags.Dashboard, StringComparison.OrdinalIgnoreCase)) return null;

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
				UserFirstName = Truncate(p.UserFirstName, 200),
				UserLastName = Truncate(p.UserLastName, 200),
				UserPhoneNumber = Truncate(p.UserPhoneNumber, 50),
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

	public async Task<UResponse<IEnumerable<ApiLogResponse>?>> ReadApiLogs(ApiLogReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<ApiLogResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<ApiLogResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsSystemAdmin) return new UResponse<IEnumerable<ApiLogResponse>?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		IQueryable<ApiLogEntity> q = FilterApiLogs(db.Set<ApiLogEntity>(), p).ApplyReadParams(p);
		IQueryable<ApiLogResponse> projected = q.Select(Projections.ApiLogSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<ApiLogStatsResponse?>> ApiLogStats(ApiLogStatsParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ApiLogStatsResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<ApiLogStatsResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsSystemAdmin) return new UResponse<ApiLogStatsResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		DateTime to = p.ToCreatedAt ?? DateTime.UtcNow;
		DateTime from = p.FromCreatedAt ?? to.AddDays(-1);
		bool byDay = p.Bucket == "day";

		IQueryable<ApiLogEntity> range = db.Set<ApiLogEntity>().Where(x => x.CreatedAt >= from && x.CreatedAt <= to);
		
		List<StatRow> rows = await range.Select(x => new StatRow { CreatedAt = x.CreatedAt, StatusCode = x.StatusCode, DurationMs = x.DurationMs, Path = x.Path }).ToListAsync(ct);

		int total = rows.Count;
		int errorCount = rows.Count(x => x.StatusCode >= 400);
		int successCount = total - errorCount;
		double avgDuration = total == 0 ? 0 : rows.Average(x => (double)x.DurationMs);

		List<long> durations = rows.Select(x => x.DurationMs).Order().ToList();
		double p50 = Percentile(durations, 0.50);
		double p95 = Percentile(durations, 0.95);
		double p99 = Percentile(durations, 0.99);

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

	private static IQueryable<ApiLogEntity> FilterApiLogs(IQueryable<ApiLogEntity> q, ApiLogReadParams p) {
		if (!string.IsNullOrWhiteSpace(p.PathContains)) q = q.Where(x => EF.Functions.ILike(x.Path, $"%{p.PathContains}%"));
		if (p.StatusCode.HasValue) q = q.Where(x => x.StatusCode == p.StatusCode.Value);
		if (p.MinDurationMs.HasValue) q = q.Where(x => x.DurationMs >= p.MinDurationMs.Value);
		if (p.MaxDurationMs.HasValue) q = q.Where(x => x.DurationMs <= p.MaxDurationMs.Value);
		if (p.UserId.HasValue) q = q.Where(x => x.UserId == p.UserId.Value);
		if (!string.IsNullOrWhiteSpace(p.IpAddress)) q = q.Where(x => x.IpAddress == p.IpAddress);
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

	private static string? SafeGet(Func<string> getter) {
		try {
			return getter();
		}
		catch {
			return null;
		}
	}

	private static async Task<string> RunProcess(string fileName, string args, CancellationToken ct) {
		using Process? p = Process.Start(new ProcessStartInfo { FileName = fileName, Arguments = args, RedirectStandardOutput = true, UseShellExecute = false });
		if (p == null) return "";
		string output = await p.StandardOutput.ReadToEndAsync(ct);
		await p.WaitForExitAsync(ct);
		return output;
	}

	private static async Task<(double totalGb, double freeGb, double usedGb, double usagePercent)> GetMacMemory(CancellationToken ct) {
		const double bytesToGb = 1024.0 * 1024 * 1024;
		double totalBytes = 0;
		try {
			totalBytes = double.Parse((await RunProcess("sysctl", "-n hw.memsize", ct)).Trim());
		}
		catch {
			/* fallback below */
		}

		double pageSize = 4096, active = 0, wired = 0, compressor = 0, free = 0, inactive = 0, speculative = 0;
		try {
			string[] lines = (await RunProcess("vm_stat", "", ct)).Split('\n');
			int marker = lines[0].IndexOf("page size of ", StringComparison.Ordinal);
			if (marker >= 0) {
				string digits = new(lines[0][(marker + 13)..].TakeWhile(char.IsDigit).ToArray());
				if (double.TryParse(digits, out double ps) && ps > 0) pageSize = ps;
			}

			foreach (string line in lines) {
				int colon = line.IndexOf(':');
				if (colon < 0) continue;
				if (!double.TryParse(line[(colon + 1)..].Trim().TrimEnd('.'), out double val)) continue;
				switch (line[..colon].Trim()) {
					case "Pages active": active = val; break;
					case "Pages wired down": wired = val; break;
					case "Pages occupied by compressor": compressor = val; break;
					case "Pages free": free = val; break;
					case "Pages inactive": inactive = val; break;
					case "Pages speculative": speculative = val; break;
				}
			}
		}
		catch {
			/* fallback below */
		}

		if (totalBytes <= 0) totalBytes = (active + wired + compressor + free + inactive + speculative) * pageSize;
		double usedBytes = Math.Min((active + wired + compressor) * pageSize, totalBytes);
		double freeBytes = totalBytes - usedBytes;
		return (
			totalBytes / bytesToGb,
			freeBytes / bytesToGb,
			usedBytes / bytesToGb,
			totalBytes == 0 ? 0 : usedBytes / totalBytes * 100
		);
	}
	
	private static async Task<(double totalGb, double usedGb, double freeGb, double usagePercent)> GetDisk(CancellationToken ct) {
		const double bytesToGb = 1024.0 * 1024 * 1024;
		double totalBytes = 0, freeBytes = 0;

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
			try {
				string rootName = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
				DriveInfo[] ready = DriveInfo.GetDrives().Where(d => d.IsReady).ToArray();
				DriveInfo? root = ready.FirstOrDefault(d => string.Equals(d.Name, rootName, StringComparison.OrdinalIgnoreCase))
					?? ready.FirstOrDefault(d => d.Name == "C:\\")
					?? ready.MaxBy(d => d.TotalSize);
				if (root != null) {
					totalBytes = root.TotalSize;
					freeBytes = root.AvailableFreeSpace;
				}
			}
			catch {
				/* leave zeros */
			}
		}
		else {
			try {
				string[] lines = (await RunProcess("df", "-kP /", ct)).Split('\n', StringSplitOptions.RemoveEmptyEntries);
				string[] cols = lines[^1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
				totalBytes = double.Parse(cols[1]) * 1024;
				freeBytes = double.Parse(cols[3]) * 1024;
			}
			catch {
				try {
					DriveInfo root = DriveInfo.GetDrives().First(d => d is { IsReady: true, Name: "/" });
					totalBytes = root.TotalSize;
					freeBytes = root.AvailableFreeSpace;
				}
				catch {
					/* leave zeros */
				}
			}
		}

		double totalGb = totalBytes / bytesToGb;
		double freeGb = freeBytes / bytesToGb;
		double usedGb = totalGb - freeGb;
		return (
			Math.Round(totalGb, 2),
			Math.Round(usedGb, 2),
			Math.Round(freeGb, 2),
			totalGb == 0 ? 0 : Math.Round(usedGb / totalGb * 100, 1)
		);
	}

	private static ulong ToUlong(Win32FileTime ft) => ((ulong)ft.dwHighDateTime << 32) | ft.dwLowDateTime;

	[StructLayout(LayoutKind.Sequential)]
	private struct Win32FileTime {
		public uint dwLowDateTime;
		public uint dwHighDateTime;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct MemoryStatusEx {
		public uint dwLength;
		public uint dwMemoryLoad;
		public ulong ullTotalPhys;
		public ulong ullAvailPhys;
		public ulong ullTotalPageFile;
		public ulong ullAvailPageFile;
		public ulong ullTotalVirtual;
		public ulong ullAvailVirtual;
		public ulong ullAvailExtendedVirtual;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx lpBuffer);

	[DllImport("kernel32.dll")]
	private static extern bool GetSystemTimes(out Win32FileTime lpIdleTime, out Win32FileTime lpKernelTime, out Win32FileTime lpUserTime);
}