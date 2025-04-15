namespace SinaMN75U.Utils;

public static class AspNetConfig {
	public static void AddUServices<T>(this WebApplicationBuilder builder) where T : DbContext {
		builder.Services.AddCors(c => c.AddPolicy("AllowOrigin", o => o.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
		builder.Services.AddUSwagger();
		builder.Services.AddUOutputCache();
		builder.Services.AddHttpContextAccessor();
		builder.Services.AddHttpClient();
		builder.Services.AddMemoryCache();
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
		builder.Services.AddRateLimiter(o => o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(c =>
				RateLimitPartition.GetFixedWindowLimiter(
					c.Request.Headers.Host.ToString(),
					_ => new FixedWindowRateLimiterOptions {
						AutoReplenishment = true,
						PermitLimit = 100,
						Window = TimeSpan.FromMinutes(1)
					}
				)
			)
		);
		
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
	}

	public static void UseUServices(this WebApplication app) {
		app.UseCors(o => o.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
		app.UseResponseCompression();
		app.UseDeveloperExceptionPage();
		app.MapOpenApi();
		app.MapScalarApiReference();
		app.UseUSwagger();
		app.UseHttpsRedirection();
		app.UseRateLimiter();
		app.UseMiddleware<ApiRequestLoggingMiddleware>();
		app.UseOutputCache();

		app.MapAuthRoutes("Auth");
		app.MapUserRoutes("User");
		app.MapMediaRoutes("Media");
		app.MapContentRoutes("Content");
		app.MapProductRoutes("Product");
		app.MapCommentRoutes("Comment");
		app.MapCategoryRoutes("Category");
	}
}