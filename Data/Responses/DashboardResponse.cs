namespace SinaMN75U.Data.Responses;

public record SystemMetricsResponse(
	double CpuUsage,
	double MemoryUsage,
	double DiskUsage,
	double TotalMemory,
	double FreeMemory,
	double TotalDisk,
	double FreeDisk,
	DateTime Date
);

public class DashboardResponse {
	public required int Categories { get; set; }
	public required int Comments { get; set; }
	public required int Contents { get; set; }
	public required int Exams { get; set; }
	public required int Media { get; set; }
	public required int Products { get; set; }
	public required int Users { get; set; }
	public required IEnumerable<UserResponse> NewUsers { get; set; }
	public required IEnumerable<CategoryResponse> NewCategories { get; set; }
	public required IEnumerable<CommentResponse> NewComments { get; set; }
	public required IEnumerable<ContentResponse> NewContents { get; set; }
	public required IEnumerable<ExamResponse> NewExams { get; set; }
	public required IEnumerable<MediaResponse> NewMedia { get; set; }
	public required IEnumerable<ProductEntity> NewProducts { get; set; }
}