namespace SinaMN75U.Data.Entities;

// Structured, queryable replacement for the old per-day JSON log files.
// Written in batches by RequestLogBackgroundService (see InnerServices/LogService.cs) so the
// request thread never does file/DB I/O inline - it only pushes onto an in-memory channel.
[Table("ApiRequestLogs")]
[Microsoft.EntityFrameworkCore.Index(nameof(Timestamp), Name = "IX_ApiRequestLogs_Timestamp")]
[Microsoft.EntityFrameworkCore.Index(nameof(StatusCode), Name = "IX_ApiRequestLogs_StatusCode")]
[Microsoft.EntityFrameworkCore.Index(nameof(Path), Name = "IX_ApiRequestLogs_Path")]
[Microsoft.EntityFrameworkCore.Index(nameof(IsSuccess), nameof(Timestamp), Name = "IX_ApiRequestLogs_IsSuccess_Timestamp")]
public sealed class ApiRequestLogEntity {
	// Guid.CreateVersion7 is time-ordered, so the PK index stays naturally sorted/insert-friendly
	// (avoids the random-Guid B-tree fragmentation you'd get from Guid.NewGuid on a hot-write table).
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

	[MaxLength(64)]
	public string? IpAddress { get; set; }

	public string? RequestBody { get; set; }
	public string? ResponseBody { get; set; }

	[MaxLength(200)]
	public string? ExceptionType { get; set; }

	public string? ExceptionMessage { get; set; }
	public string? StackTrace { get; set; }
}
