namespace SinaMN75U.Services;

public interface IDashboardService {
	Task<SystemMetricsResponse> ReadSystemMetrics();
	Task<DashboardResponse> ReadDashboardData(CancellationToken ct);
}

public class DashboardService(
	DbContext db,
	ICategoryService categoryService,
	ICommentService commentService,
	IContentService contentService,
	IExamService examService,
	IMediaService mediaService,
	IProductService productService,
	IUserService userService
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

				// Memory metrics
				using Process? memProcess = Process.Start(new ProcessStartInfo {
					FileName = "vm_stat",
					RedirectStandardOutput = true,
					UseShellExecute = false
				});

				if (memProcess != null) {
					string memOutput = await memProcess.StandardOutput.ReadToEndAsync();
					await memProcess.WaitForExitAsync();

					string[] lines = memOutput.Split('\n');
					const double pageSize = 4096.0;
					double free = double.Parse(lines[1].Split(':')[1].Trim().Split(' ')[0]) * pageSize;
					double active = double.Parse(lines[2].Split(':')[1].Trim().Split(' ')[0]) * pageSize;
					double inactive = double.Parse(lines[3].Split(':')[1].Trim().Split(' ')[0]) * pageSize;
					double wired = double.Parse(lines[4].Split(':')[1].Trim().Split(' ')[0]) * pageSize;

					double used = (active + inactive + wired) / bytesToGb;
					totalMem = used + free / bytesToGb;
					freeMem = free / bytesToGb;
					memUsage = used / totalMem * 100;
				}
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

	public async Task<DashboardResponse> ReadDashboardData(CancellationToken ct) {
		UResponse<IEnumerable<UserEntity>?> newUsers = await userService.Read(new UserReadParams { PageSize = 5 }, ct);
		UResponse<IEnumerable<CategoryResponse>?> newCategories = await categoryService.Read(new CategoryReadParams { PageSize = 5 }, ct);
		UResponse<IEnumerable<CommentEntity>?> newComments = await commentService.Read(new CommentReadParams { PageSize = 5 }, ct);
		UResponse<IEnumerable<ContentEntity>?> newContents = await contentService.Read(new ContentReadParams { PageSize = 5 }, ct);
		UResponse<IEnumerable<ExamEntity>?> newExams = await examService.Read(new ExamReadParams { PageSize = 5 }, ct);
		UResponse<IEnumerable<MediaEntity>?> newMedia = await mediaService.Read(new BaseReadParams<TagMedia> { PageSize = 5 }, ct);
		UResponse<IEnumerable<ProductEntity>?> newProducts = await productService.Read(new ProductReadParams { PageSize = 5 }, ct);

		return new DashboardResponse {
			Categories = await db.Set<CategoryEntity>().CountAsync(ct),
			Comments = await db.Set<CommentEntity>().CountAsync(ct),
			Contents = await db.Set<ContentEntity>().CountAsync(ct),
			Exams = await db.Set<ExamEntity>().CountAsync(ct),
			Media = await db.Set<MediaEntity>().CountAsync(ct),
			Products = await db.Set<ProductEntity>().CountAsync(ct),
			Users = await db.Set<UserEntity>().CountAsync(ct),
			NewUsers = newUsers.Result ?? [],
			NewCategories = newCategories.Result ?? [],
			NewComments = newComments.Result ?? [],
			NewContents = newContents.Result ?? [],
			NewExams = newExams.Result ?? [],
			NewMedia = newMedia.Result ?? [],
			NewProducts = newProducts.Result ?? []
		};
	}

	private static async Task<(ulong user, ulong nice, ulong system, ulong total)> GetLinuxCpuSample() {
		string[] lines = await File.ReadAllLinesAsync("/proc/stat");
		string[] values = lines.First(l => l.StartsWith("cpu "))
			.Split(' ', StringSplitOptions.RemoveEmptyEntries);

		ulong user = ulong.Parse(values[1]);
		ulong nice = ulong.Parse(values[2]);
		ulong system = ulong.Parse(values[3]);
		ulong idle = ulong.Parse(values[4]);
		ulong total = user + nice + system + idle;

		return (user, nice, system, total);
	}
}