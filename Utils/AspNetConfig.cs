namespace SinaMN75U.Utils;

using Microsoft.AspNetCore.SignalR;

public static class AspNetConfig {
	public static void AddUServices<T>(this WebApplicationBuilder builder) where T : DbContext {
		builder.Services.AddCors(c => c.AddPolicy("AllowOrigin", o => o.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
		builder.Services.AddUSwagger();
		builder.Services.AddHttpContextAccessor();
		builder.Services.AddHttpClient();
		builder.Services.AddMemoryCache();
		builder.Services.AddURateLimiter();
		builder.Services.AddSignalR();
		builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
		builder.Services.ConfigureHttpJsonOptions(o => {
			o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
			o.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
			o.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
			o.SerializerOptions.WriteIndented = false;
		});
		builder.Services.AddControllersWithViews(o => o.EnableEndpointRouting = false)
			.AddJsonOptions(o => {
				o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
				o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
				o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
				o.JsonSerializerOptions.WriteIndented = false;
			});

		builder.Services.AddResponseCompression(o => o.EnableForHttps = true);
		builder.Services.AddScoped<DbContext, T>();
		builder.Services.AddDbContextPool<T>(b => {
			b.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
			b.UseNpgsql(builder.Configuration.GetConnectionString("ServerPostgres"), o => {
				AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
				o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
				o.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
			});
		});

		builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
		builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")));
		builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 100_000_000);
		builder.Services.Configure<FormOptions>(x => {
			x.ValueLengthLimit = int.MaxValue;
			x.MultipartBodyLengthLimit = int.MaxValue;
			x.MultipartHeadersLengthLimit = int.MaxValue;
		});

		Server.Configure(builder.Services.BuildServiceProvider().GetService<IServiceProvider>()?.GetService<IHttpContextAccessor>());

		builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
		builder.Services.AddSingleton<IHttpClientService, HttpClientService>();
		builder.Services.AddSingleton<ILocalStorageService, MemoryCacheService>();
		builder.Services.AddScoped<ITokenService, TokenService>();
		builder.Services.AddScoped<IUserService, UserService>();
		builder.Services.AddScoped<IAuthService, AuthService>();
		builder.Services.AddScoped<ICategoryService, CategoryService>();
		builder.Services.AddScoped<ISmsNotificationService, SmsNotificationService>();
		builder.Services.AddScoped<IMediaService, MediaService>();
		builder.Services.AddScoped<IContentService, ContentService>();
		builder.Services.AddScoped<IProductService, ProductService>();
		builder.Services.AddScoped<ICommentService, CommentService>();
		builder.Services.AddScoped<IExamService, ExamService>();
	}

	public static void UseUServices(this WebApplication app) {
		app.UseCors(o => o.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
		app.UseResponseCompression();
		app.UseDeveloperExceptionPage();
		app.UseUSwagger();
		app.UseHttpsRedirection();
		app.UseRateLimiter();
		app.UseMiddleware<ApiRequestLoggingMiddleware>();
		app.MapHub<MetricsHub>("/metricsHub");

		app.MapGet("/metrics", () => new {
			Cpu = GetCpuUsage(),
			Memory = GetMemoryUsage(),
			Disk = GetDiskUsage()
		});

		static double GetCpuUsage() {
			var output = ExecuteShellCommand("top -bn1 | grep 'Cpu(s)' | awk '{print $2 + $4}'");
			return double.TryParse(output, out var usage) ? Math.Round(usage, 2) : 0;
		}

		static dynamic GetMemoryUsage() {
			var output = ExecuteShellCommand("free -m | awk 'NR==2{printf \"%.2f\", $3*100/$2 }'");
			var percentage = double.TryParse(output, out var pct) ? Math.Round(pct, 2) : 0;
			var totalOutput = ExecuteShellCommand("free -m | awk 'NR==2{print $2}'");
			var usedOutput = ExecuteShellCommand("free -m | awk 'NR==2{print $3}'");

			return new {
				Percentage = percentage,
				UsedMB = int.TryParse(usedOutput, out var used) ? used : 0,
				TotalMB = int.TryParse(totalOutput, out var total) ? total : 0
			};
		}

		static dynamic GetDiskUsage() {
			var output = ExecuteShellCommand("df -h / | awk 'NR==2{print $5}' | tr -d '%'");
			var percentage = int.TryParse(output, out var pct) ? pct : 0;
			return new { Percentage = percentage };
		}

		static string ExecuteShellCommand(string command) {
			var escapedArgs = command.Replace("\"", "\\\"");
			var process = new Process {
				StartInfo = new ProcessStartInfo {
					FileName = "/bin/bash",
					Arguments = $"-c \"{escapedArgs}\"",
					RedirectStandardOutput = true,
					UseShellExecute = false
				}
			};
			process.Start();
			return process.StandardOutput.ReadToEnd().Trim();
		}

		app.MapAuthRoutes("api/auth/");
		app.MapUserRoutes("api/user/");
		app.MapMediaRoutes("api/media/");
		app.MapContentRoutes("api/content/");
		app.MapProductRoutes("api/product/");
		app.MapCommentRoutes("api/comment/");
		app.MapCategoryRoutes("api/Category/");
		app.MapExamRoutes("api/Exam/");
	}
}

public class MetricsHub : Hub {
	public async IAsyncEnumerable<object> StreamMetrics() {
		var lastMetrics = new { Cpu = 0.0, Memory = (dynamic)new { Percentage = (int)0.0 }, Disk = (dynamic)new { Percentage = 0 } };

		static double GetCpuUsage() {
			var output = ExecuteShellCommand("top -bn1 | grep 'Cpu(s)' | awk '{print $2 + $4}'");
			return double.TryParse(output, out var usage) ? Math.Round(usage, 2) : 0;
		}

		static dynamic GetMemoryUsage() {
			var output = ExecuteShellCommand("free -m | awk 'NR==2{printf \"%.2f\", $3*100/$2 }'");
			var percentage = double.TryParse(output, out var pct) ? Math.Round(pct, 2) : 0;
			var totalOutput = ExecuteShellCommand("free -m | awk 'NR==2{print $2}'");
			var usedOutput = ExecuteShellCommand("free -m | awk 'NR==2{print $3}'");

			return new {
				Percentage = percentage,
				UsedMB = int.TryParse(usedOutput, out var used) ? used : 0,
				TotalMB = int.TryParse(totalOutput, out var total) ? total : 0
			};
		}

		static dynamic GetDiskUsage() {
			var output = ExecuteShellCommand("df -h / | awk 'NR==2{print $5}' | tr -d '%'");
			var percentage = int.TryParse(output, out var pct) ? pct : 0;
			return new { Percentage = percentage };
		}

		static string ExecuteShellCommand(string command) {
			var escapedArgs = command.Replace("\"", "\\\"");
			var process = new Process {
				StartInfo = new ProcessStartInfo {
					FileName = "/bin/bash",
					Arguments = $"-c \"{escapedArgs}\"",
					RedirectStandardOutput = true,
					UseShellExecute = false
				}
			};
			process.Start();
			return process.StandardOutput.ReadToEnd().Trim();
		}

		while (true) {
			var current = new {
				Cpu = GetCpuUsage(),
				Memory = GetMemoryUsage(),
				Disk = GetDiskUsage()
			};

			if (Math.Abs(current.Cpu - lastMetrics.Cpu) > 0.5 ||
			    Math.Abs(current.Memory.Percentage - lastMetrics.Memory.Percentage) > 0.5 ||
			    current.Disk.Percentage != lastMetrics.Disk.Percentage) {
				lastMetrics = current;
				yield return current;
			}

			await Task.Delay(1000); // Check every second
		}
	}
}