namespace SinaMN75U.Data.Responses;

// ===================== OS / Server Metrics (rich, cross-platform) =====================

public sealed class OsMetricsResponse {
	public DateTime GeneratedAt { get; set; }

	// Identity
	public string OsName { get; set; } = "";
	public string OsDescription { get; set; } = "";
	public string OsArchitecture { get; set; } = "";
	public string ProcessArchitecture { get; set; } = "";
	public string FrameworkDescription { get; set; } = "";
	public string MachineName { get; set; } = "";
	public bool Is64BitOperatingSystem { get; set; }
	public bool Is64BitProcess { get; set; }
	public int ProcessorCount { get; set; }

	// Uptime
	public double SystemUptimeSeconds { get; set; }
	public double ProcessUptimeSeconds { get; set; }
	public DateTime ProcessStartedAt { get; set; }

	// CPU
	public double CpuUsagePercent { get; set; }
	public double? LoadAverage1Min { get; set; }
	public double? LoadAverage5Min { get; set; }
	public double? LoadAverage15Min { get; set; }

	// Memory (GB)
	public double MemoryTotalGb { get; set; }
	public double MemoryUsedGb { get; set; }
	public double MemoryFreeGb { get; set; }
	public double MemoryUsagePercent { get; set; }

	// Disk (GB)
	public double DiskTotalGb { get; set; }
	public double DiskUsedGb { get; set; }
	public double DiskFreeGb { get; set; }
	public double DiskUsagePercent { get; set; }
}

public sealed record SystemMetricsResponse(
	double CpuUsage,
	double MemoryUsage,
	double DiskUsage,
	double TotalMemory,
	double FreeMemory,
	double TotalDisk,
	double FreeDisk,
	DateTime Date
);

public sealed class DashboardResponse {
	public required int Categories { get; set; }
	public required int Comments { get; set; }
	public required int Contents { get; set; }
	public required int Media { get; set; }
	public required int Products { get; set; }
	public required int Users { get; set; }
	public required IEnumerable<UserResponse> NewUsers { get; set; }
	public required IEnumerable<CategoryResponse> NewCategories { get; set; }
	public required IEnumerable<CommentResponse> NewComments { get; set; }
	public required IEnumerable<ContentResponse> NewContents { get; set; }
	public required IEnumerable<MediaResponse> NewMedia { get; set; }
	public required IEnumerable<ProductResponse> NewProducts { get; set; }
}

// ===================== Shared small DTOs =====================

public sealed class RecentUserItem {
	public Guid Id { get; set; }
	public string DisplayName { get; set; } = "";
	public string? UserName { get; set; }
	public string? PhoneNumber { get; set; }
	public DateTime CreatedAt { get; set; }
}
