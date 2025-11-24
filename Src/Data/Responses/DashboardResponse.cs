namespace SinaMN75U.Data.Responses;

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
	public required int Exams { get; set; }
	public required int Media { get; set; }
	public required int Products { get; set; }
	public required int Users { get; set; }
	public required IEnumerable<UserEntity> NewUsers { get; set; }
	public required IEnumerable<CategoryEntity> NewCategories { get; set; }
	public required IEnumerable<CommentEntity> NewComments { get; set; }
	public required IEnumerable<ContentEntity> NewContents { get; set; }
	public required IEnumerable<ExamEntity> NewExams { get; set; }
	public required IEnumerable<MediaEntity> NewMedia { get; set; }
	public required IEnumerable<ProductEntity> NewProducts { get; set; }
}