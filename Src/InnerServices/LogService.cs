using System.Threading.Channels;
using Core = SinaMN75U.Constants.Core;

namespace SinaMN75U.InnerServices;

public interface IRequestLogger {
	void TryLog(RequestLogDto log);
}

// In-memory hand-off point between request threads and RequestLogBackgroundService.
// Bounded + DropOldest: under extreme load we'd rather silently lose the oldest buffered
// log entries than block requests or grow memory unbounded. Sized generously relative to
// the batching interval below (2s), so drops should only happen under sustained overload.
public sealed class RequestLogChannel {
	private readonly Channel<RequestLogDto> _channel = Channel.CreateBounded<RequestLogDto>(new BoundedChannelOptions(5_000) {
		FullMode = BoundedChannelFullMode.DropOldest,
		SingleReader = true,
		SingleWriter = false
	});

	public ChannelWriter<RequestLogDto> Writer => _channel.Writer;
	public ChannelReader<RequestLogDto> Reader => _channel.Reader;
}

// Fast path used by UMiddleware. Does no I/O: it only decides whether a log is wanted and,
// if so, hands it to the channel. This replaces the old implementation which read + parsed +
// rewrote an entire day's JSON file, under a single lock, on every single request.
public sealed class RequestLogger(RequestLogChannel channel) : IRequestLogger {
	public void TryLog(RequestLogDto log) {
		if (!Core.App.Middleware.Log) return;
		if (log.StatusCode is >= 200 and <= 299 && !Core.App.Middleware.LogSuccess) return;

		channel.Writer.TryWrite(log);
	}
}

// Drains the channel in batches and bulk-writes them to Postgres, off the request path entirely.
// Also owns retention: old rows are purged on a timer so the table (and its indexes) stay small
// regardless of traffic volume.
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

		// Best-effort final drain so we don't drop the last couple seconds of logs on shutdown.
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
				StatusCode = log.StatusCode,
				IsSuccess = log.StatusCode is >= 200 and <= 299,
				DurationMs = log.DurationMs,
				UserId = log.UserId,
				IpAddress = log.IpAddress,
				RequestBody = Truncate(log.DecodedRequest),
				ResponseBody = Truncate(log.Response),
				ExceptionType = log.Exception?.GetType().Name,
				ExceptionMessage = log.Exception?.Message,
				StackTrace = log.Exception?.StackTrace
			}).ToList();

			db.Set<ApiRequestLogEntity>().AddRange(entities);
			await db.SaveChangesAsync(ct);
		}
		catch {
			// A lost batch of logs is acceptable; a crashed background service is not.
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
			// Retention is best-effort; it will simply retry on the next cycle.
		}
	}

	private static string? Truncate(string? s) => string.IsNullOrEmpty(s) ? s : s.Length > MaxBodyLength ? s[..MaxBodyLength] + "...<truncated>" : s;
}

public sealed class RequestLogDto {
	public DateTime Timestamp { get; init; }
	public string Method { get; init; } = "";
	public string Path { get; init; } = "";
	public int StatusCode { get; init; }
	public long DurationMs { get; init; }
	public string RawRequest { get; init; } = "";
	public string DecodedRequest { get; init; } = "";
	public string Response { get; init; } = "";
	public Exception? Exception { get; init; }
	public Guid? UserId { get; init; }
	public string? IpAddress { get; init; }
}
