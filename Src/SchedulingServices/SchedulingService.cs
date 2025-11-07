using Microsoft.Extensions.Hosting;

namespace SinaMN75U.SchedulingServices;

public class SimpleSchedulerService : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		Console.WriteLine("Scheduler started...");

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await RunScheduledTask();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
			}

			// Wait 10 seconds before running again
			await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
		}
	}

	private Task RunScheduledTask()
	{
		Console.WriteLine($"Task executed at: {DateTime.Now}");
		return Task.CompletedTask;
	}
}