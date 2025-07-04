namespace SinaMN75U.Utils;

using System.IO.Compression;

public static class AspNetConfig {
	public static void AddUServices<T>(
		this WebApplicationBuilder builder,
		SqlDatabaseType sqlDatabaseType,
		string sqlDatabaseConnectionStrings
	) where T : DbContext {
		builder.Services.AddCors(c => c.AddPolicy("AllowOrigin", o => o.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
		builder.Services.AddUSwagger();
		builder.Services.AddHttpContextAccessor();
		builder.Services.AddHttpClient();
		builder.Services.AddURateLimiter();
		builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
		builder.Services.ConfigureHttpJsonOptions(o => {
			o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
			o.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
			o.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
			o.SerializerOptions.WriteIndented = false;
		});
		builder.Services.AddResponseCompression(o => {
			o.EnableForHttps = true;
			o.Providers.Add<GzipCompressionProvider>();
			o.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/json", "text/plain"]);
		});

		builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
		builder.Services.AddScoped<DbContext, T>();
		builder.Services.AddDbContextPool<T>(b => {
			b.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
			switch (sqlDatabaseType) {
				case SqlDatabaseType.Postgres:
					b.UseNpgsql(builder.Configuration.GetConnectionString(sqlDatabaseConnectionStrings), o => {
						AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
						o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
						o.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
					});
					break;
				case SqlDatabaseType.SqlServer:
					b.UseSqlServer(builder.Configuration.GetConnectionString(sqlDatabaseConnectionStrings), o => { });
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(sqlDatabaseType), sqlDatabaseType, null);
			}
		});

		builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
		builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")));
		builder.Services.Configure<FormOptions>(x => {
			x.ValueLengthLimit = int.MaxValue;
			x.MultipartBodyLengthLimit = int.MaxValue;
			x.MultipartHeadersLengthLimit = int.MaxValue;
		});

		Server.Configure(builder.Services.BuildServiceProvider().GetService<IServiceProvider>()?.GetService<IHttpContextAccessor>());

		builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
		builder.Services.AddSingleton<IHttpClientService, HttpClientService>();
		builder.Services.AddSingleton<ILocalStorageService, StaticCacheService>();
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
		builder.Services.AddScoped<IDashboardService, DashboardService>();
	}

	public static void UseUServices(this WebApplication app) {
		app.UseCors(o => o.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
		app.UseResponseCompression();
		app.UseDeveloperExceptionPage();
		app.UseUSwagger();
		app.UseHttpsRedirection();
		app.UseRateLimiter();

		app.MapAuthRoutes("api/auth/");
		app.MapUserRoutes("api/user/");
		app.MapMediaRoutes("api/media/");
		app.MapContentRoutes("api/content/");
		app.MapProductRoutes("api/product/");
		app.MapCommentRoutes("api/comment/");
		app.MapCategoryRoutes("api/category/");
		app.MapExamRoutes("api/exam/");
		app.MapDashboardRoutes("api/dashboard/");
	}
}