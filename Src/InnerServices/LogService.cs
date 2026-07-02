using System.Threading.Channels;
using Core = SinaMN75U.Constants.Core;

namespace SinaMN75U.InnerServices;

public interface IRequestLogger {
	void TryLog(RequestLogDto log);
}

public sealed class RequestLogChannel {
	private readonly Channel<RequestLogDto> _channel = Channel.CreateBounded<RequestLogDto>(new BoundedChannelOptions(5_000) {
		FullMode = BoundedChannelFullMode.DropOldest,
		SingleReader = true,
		SingleWriter = false
	});

	public ChannelWriter<RequestLogDto> Writer => _channel.Writer;
	public ChannelReader<RequestLogDto> Reader => _channel.Reader;
}

public sealed class RequestLogger(RequestLogChannel channel) : IRequestLogger {
	public void TryLog(RequestLogDto log) {
		if (!Core.App.Middleware.Log) return;
		if (log.StatusCode is >= 200 and <= 299 && !Core.App.Middleware.LogSuccess) return;

		channel.Writer.TryWrite(log);
	}
}

public sealed class RequestLogBackgroundService(RequestLogChannel channel, IServiceScopeFactory scopeFactory) : BackgroundService {
	private const int BatchSize = 200;
	private const int MaxBodyLength = 20_000;
	private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(2);
	private static readonly TimeSpan RetentionCheckInterval = TimeSpan.FromHours(1);

	protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
		DateTime nextRetentionCheck = DateTime.UtcNow;
		using PeriodicTimer timer = new(FlushInterval);

		while (!stoppingToken.IsCancellationRequested) {
			List<RequestLogDto> batch = [];
			while (batch.Count < BatchSize && channel.Reader.TryRead(out RequestLogDto? item))
				batch.Add(item);

			if (batch.Count > 0)
				await FlushAsync(batch, stoppingToken);

			if (DateTime.UtcNow >= nextRetentionCheck) {
				await PurgeOldLogsAsync(stoppingToken);
				nextRetentionCheck = DateTime.UtcNow.Add(RetentionCheckInterval);
			}

			if (batch.Count == 0) {
				try {
					await timer.WaitForNextTickAsync(stoppingToken);
				}
				catch (OperationCanceledException) {
					break;
				}
			}
		}

		List<RequestLogDto> remaining = [];
		while (channel.Reader.TryRead(out RequestLogDto? item)) remaining.Add(item);
		if (remaining.Count > 0) await FlushAsync(remaining, CancellationToken.None);
	}

	private async Task FlushAsync(List<RequestLogDto> batch, CancellationToken ct) {
		try {
			using IServiceScope scope = scopeFactory.CreateScope();
			DbContext db = scope.ServiceProvider.GetRequiredService<DbContext>();

			List<ApiRequestLogEntity> entities = batch.Select(log => new ApiRequestLogEntity {
				Timestamp = log.Timestamp,
				Method = log.Method,
				Path = log.Path,
				QueryString = TruncateTo(log.QueryString, 2000),
				StatusCode = log.StatusCode,
				IsSuccess = log.StatusCode is >= 200 and <= 299,
				DurationMs = log.DurationMs,
				UserId = log.UserId,
				UserName = TruncateTo(log.UserName, 200),
				UserEmail = TruncateTo(log.UserEmail, 300),
				UserRoles = TruncateTo(log.UserRoles, 300),
				IpAddress = log.IpAddress,
				RequestBody = Truncate(log.DecodedRequest),
				ResponseBody = Truncate(log.Response),
				RequestHeaders = Truncate(log.RequestHeaders),
				ResponseHeaders = Truncate(log.ResponseHeaders),
				RequestSizeBytes = log.RequestSizeBytes,
				ResponseSizeBytes = log.ResponseSizeBytes,
				UserAgent = TruncateTo(log.UserAgent, 500),
				TraceId = TruncateTo(log.TraceId, 100),
				Host = TruncateTo(log.Host, 200),
				ExceptionType = log.Exception?.GetType().Name,
				ExceptionMessage = log.Exception?.Message,
				StackTrace = log.Exception?.StackTrace
			}).ToList();

			db.Set<ApiRequestLogEntity>().AddRange(entities);
			await db.SaveChangesAsync(ct);
		}
		catch {
			// ignored
		}
	}

	private async Task PurgeOldLogsAsync(CancellationToken ct) {
		try {
			using IServiceScope scope = scopeFactory.CreateScope();
			DbContext db = scope.ServiceProvider.GetRequiredService<DbContext>();
			DateTime cutoff = DateTime.UtcNow.AddDays(-Core.App.Middleware.LogRetentionDays);
			await db.Set<ApiRequestLogEntity>().Where(x => x.Timestamp < cutoff).ExecuteDeleteAsync(ct);
		}
		catch {
			// ignored
		}
	}

	private static string? Truncate(string? s) => string.IsNullOrEmpty(s) ? s : s.Length > MaxBodyLength ? s[..MaxBodyLength] + "...<truncated>" : s;
	
	private static string? TruncateTo(string? s, int max) => string.IsNullOrEmpty(s) || s.Length <= max ? s : s[..max];
}

public sealed class RequestLogDto {
	public DateTime Timestamp { get; init; }
	public string Method { get; init; } = "";
	public string Path { get; init; } = "";
	public string? QueryString { get; init; }
	public int StatusCode { get; init; }
	public long DurationMs { get; init; }
	public string RawRequest { get; init; } = "";
	public string DecodedRequest { get; init; } = "";
	public string Response { get; init; } = "";
	public string? RequestHeaders { get; init; }
	public string? ResponseHeaders { get; init; }
	public int RequestSizeBytes { get; init; }
	public int ResponseSizeBytes { get; init; }
	public string? UserAgent { get; init; }
	public string? TraceId { get; init; }
	public string? Host { get; init; }
	public Exception? Exception { get; init; }
	public Guid? UserId { get; init; }
	public string? UserName { get; init; }
	public string? UserEmail { get; init; }
	public string? UserRoles { get; init; }
	public string? IpAddress { get; init; }
}
