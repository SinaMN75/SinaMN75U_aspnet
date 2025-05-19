namespace SinaMN75U.Routes;

public static class MetricsRoutes {
	public static void MapMetricsRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder route = app.MapGroup(tag).WithTags(tag);

		route.MapGet("cpu", GetCpuMetric)
			.Produces<UResponse<CpuMetricResponse>>();

		route.MapGet("memory", GetMemoryMetric)
			.Produces<UResponse<MemoryMetricResponse>>();

		route.MapGet("disk", GetDiskMetric)
			.Produces<UResponse<DiskMetricResponse>>();

		route.MapGet("all", GetAllMetrics)
			.Produces<UResponse<AllMetricsResponse>>();
	}

	private static CpuMetricResponse GetCpuMetric() {
		try {
			string output = ExecuteShellCommand("top -bn1 | grep 'Cpu(s)' | awk '{print $2 + $4}'");
			return new CpuMetricResponse {
				UsagePercent = Math.Round(double.Parse(output), 2),
				Timestamp = DateTime.UtcNow
			};
		}
		catch {
			return new CpuMetricResponse { Error = "Failed to get CPU metrics" };
		}
	}

	private static MemoryMetricResponse GetMemoryMetric() {
		try {
			string percentage = ExecuteShellCommand("free -m | awk 'NR==2{printf \"%.2f\", $3*100/$2 }'");
			string total = ExecuteShellCommand("free -m | awk 'NR==2{print $2}'");
			string used = ExecuteShellCommand("free -m | awk 'NR==2{print $3}'");

			return new MemoryMetricResponse {
				UsagePercent = Math.Round(double.Parse(percentage), 2),
				TotalMb = int.Parse(total),
				UsedMb = int.Parse(used),
				Timestamp = DateTime.UtcNow
			};
		}
		catch {
			return new MemoryMetricResponse { Error = "Failed to get memory metrics" };
		}
	}

	private static DiskMetricResponse GetDiskMetric() {
		try {
			string output = ExecuteShellCommand("df -h / | awk 'NR==2{print $5}' | tr -d '%'");
			return new DiskMetricResponse {
				UsagePercent = int.Parse(output),
				Timestamp = DateTime.UtcNow
			};
		}
		catch {
			return new DiskMetricResponse { Error = "Failed to get disk metrics" };
		}
	}

	private static AllMetricsResponse GetAllMetrics() {
		return new AllMetricsResponse {
			Cpu = GetCpuMetric(),
			Memory = GetMemoryMetric(),
			Disk = GetDiskMetric(),
			Timestamp = DateTime.UtcNow
		};
	}

	private static string ExecuteShellCommand(string command) {
		Process process = new Process {
			StartInfo = new ProcessStartInfo {
				FileName = "/bin/bash",
				Arguments = $"-c \"{command}\"",
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true
			}
		};
		process.Start();
		return process.StandardOutput.ReadToEnd().Trim();
	}
}

// Response models
public record CpuMetricResponse {
	public double? UsagePercent { get; init; }
	public DateTime Timestamp { get; init; }
	public string? Error { get; init; }
}

public record MemoryMetricResponse {
	public double? UsagePercent { get; init; }
	public int? TotalMb { get; init; }
	public int? UsedMb { get; init; }
	public DateTime Timestamp { get; init; }
	public string? Error { get; init; }
}

public record DiskMetricResponse {
	public int? UsagePercent { get; init; }
	public DateTime Timestamp { get; init; }
	public string? Error { get; init; }
}

public record AllMetricsResponse {
	public CpuMetricResponse Cpu { get; init; }
	public MemoryMetricResponse Memory { get; init; }
	public DiskMetricResponse Disk { get; init; }
	public DateTime Timestamp { get; init; }
}