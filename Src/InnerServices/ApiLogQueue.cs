using System.Threading.Channels;

namespace SinaMN75U.InnerServices;

public interface IApiLogQueue {
	void Enqueue(ApiLogCreateParams p);
}

public sealed class ApiLogQueue : IApiLogQueue {
	private readonly Channel<ApiLogCreateParams> _channel = Channel.CreateBounded<ApiLogCreateParams>(
		new BoundedChannelOptions(20_000) {
			FullMode = BoundedChannelFullMode.DropWrite,
			SingleReader = true,
			SingleWriter = false
		});

	public ChannelReader<ApiLogCreateParams> Reader => _channel.Reader;

	public void Enqueue(ApiLogCreateParams p) => _channel.Writer.TryWrite(p);
}

public sealed class ApiLogBackgroundService(ApiLogQueue queue, IServiceScopeFactory scopeFactory) : BackgroundService {
	protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
		await foreach (ApiLogCreateParams first in queue.Reader.ReadAllAsync(stoppingToken)) {
			List<ApiLogCreateParams> batch = [first];
			while (batch.Count < 200 && queue.Reader.TryRead(out ApiLogCreateParams? next)) batch.Add(next);

			try {
				using IServiceScope scope = scopeFactory.CreateScope();
				IDashboardService service = scope.ServiceProvider.GetRequiredService<IDashboardService>();
				await service.CreateManyApiLogs(batch, stoppingToken);
			}
			catch {
				// logging must never crash the app
			}
		}
	}
}
