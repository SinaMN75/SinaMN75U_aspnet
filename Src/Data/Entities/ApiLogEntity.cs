namespace SinaMN75U.Data.Entities;

[Table("ApiLogs")]
[Microsoft.EntityFrameworkCore.Index(nameof(Path), Name = "IX_ApiLogs_Path")]
[Microsoft.EntityFrameworkCore.Index(nameof(StatusCode), Name = "IX_ApiLogs_StatusCode")]
[Microsoft.EntityFrameworkCore.Index(nameof(DurationMs), Name = "IX_ApiLogs_DurationMs")]
[Microsoft.EntityFrameworkCore.Index(nameof(UserId), Name = "IX_ApiLogs_UserId")]
[Microsoft.EntityFrameworkCore.Index(nameof(TraceId), Name = "IX_ApiLogs_TraceId")]
public sealed class ApiLogEntity : BaseEntity<TagApiLog, ApiLogJson> {
	[Required, MaxLength(500)]
	public required string Path { get; set; }

	[Required]
	public required int StatusCode { get; set; }

	[Required]
	public required long DurationMs { get; set; }

	public Guid? UserId { get; set; }

	[MaxLength(64)]
	public string? IpAddress { get; set; }

	[MaxLength(100)]
	public string? TraceId { get; set; }
}

public sealed class ApiLogJson : BaseJson {
	public string Method { get; set; } = "";
	public string? QueryString { get; set; }
	public string? RequestBody { get; set; }
	public string? ResponseBody { get; set; }
	public string? RequestHeaders { get; set; }
	public string? ResponseHeaders { get; set; }
	public string? UserAgent { get; set; }
	public string? Host { get; set; }
	public string? UserName { get; set; }
	public string? UserEmail { get; set; }
	public string? UserRoles { get; set; }
	public string? ExceptionType { get; set; }
	public string? ExceptionMessage { get; set; }
	public string? StackTrace { get; set; }
	public int RequestSizeBytes { get; set; }
	public int ResponseSizeBytes { get; set; }
}
