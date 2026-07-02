namespace SinaMN75U.Data.Entities;

[Table("ApiRequestLogs")]
[Microsoft.EntityFrameworkCore.Index(nameof(Timestamp), Name = "IX_ApiRequestLogs_Timestamp")]
[Microsoft.EntityFrameworkCore.Index(nameof(StatusCode), Name = "IX_ApiRequestLogs_StatusCode")]
[Microsoft.EntityFrameworkCore.Index(nameof(Path), Name = "IX_ApiRequestLogs_Path")]
[Microsoft.EntityFrameworkCore.Index(nameof(IsSuccess), nameof(Timestamp), Name = "IX_ApiRequestLogs_IsSuccess_Timestamp")]
[Microsoft.EntityFrameworkCore.Index(nameof(DurationMs), Name = "IX_ApiRequestLogs_DurationMs")]
[Microsoft.EntityFrameworkCore.Index(nameof(UserId), Name = "IX_ApiRequestLogs_UserId")]
[Microsoft.EntityFrameworkCore.Index(nameof(TraceId), Name = "IX_ApiRequestLogs_TraceId")]
public sealed class ApiRequestLogEntity {
	[Key]
	public Guid Id { get; set; } = Guid.CreateVersion7();

	[Required]
	public required DateTime Timestamp { get; set; }

	[Required, MaxLength(10)]
	public required string Method { get; set; }

	[Required, MaxLength(500)]
	public required string Path { get; set; }

	[Required]
	public required int StatusCode { get; set; }

	[Required]
	public required bool IsSuccess { get; set; }

	[Required]
	public required long DurationMs { get; set; }

	public Guid? UserId { get; set; }

	[MaxLength(200)]
	public string? UserName { get; set; }

	[MaxLength(300)]
	public string? UserEmail { get; set; }

	[MaxLength(300)]
	public string? UserRoles { get; set; }

	[MaxLength(64)]
	public string? IpAddress { get; set; }

	[MaxLength(2000)]
	public string? QueryString { get; set; }

	public string? RequestBody { get; set; }
	public string? ResponseBody { get; set; }

	public string? RequestHeaders { get; set; }
	public string? ResponseHeaders { get; set; }

	[MaxLength(500)]
	public string? UserAgent { get; set; }

	[MaxLength(100)]
	public string? TraceId { get; set; }

	[MaxLength(200)]
	public string? Host { get; set; }

	public int RequestSizeBytes { get; set; }
	public int ResponseSizeBytes { get; set; }

	[MaxLength(200)]
	public string? ExceptionType { get; set; }

	public string? ExceptionMessage { get; set; }
	public string? StackTrace { get; set; }
}