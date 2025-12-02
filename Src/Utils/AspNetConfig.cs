using Microsoft.AspNetCore.ResponseCompression;

namespace SinaMN75U.Utils;

public static class AspNetConfig {
	public static void AddUServices<T>(
		this WebApplicationBuilder builder,
		SqlDatabaseType sqlDatabaseType,
		string sqlDatabaseConnectionStrings
	) where T : DbContext {
		Server.Configure(builder.Configuration);
		builder.Services.Configure<KestrelServerOptions>(o => o.AllowSynchronousIO = false);
		builder.Services.Configure<IISServerOptions>(o => o.AllowSynchronousIO = false);
		builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
		builder.Services.AddUSwagger();
		builder.Services.AddHttpContextAccessor();
		builder.Services.AddHttpClient();
		builder.Services.AddURateLimiter();
		builder.Services.ConfigureHttpJsonOptions(o => {
			o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
			o.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
			o.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
			o.SerializerOptions.WriteIndented = false;
		});
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
					b.UseSqlServer(builder.Configuration.GetConnectionString(sqlDatabaseConnectionStrings));
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(sqlDatabaseType), sqlDatabaseType, null);
			}
		});

		builder.Services.AddResponseCompression(opts => {
			opts.EnableForHttps = true;
			opts.Providers.Add<BrotliCompressionProvider>();
			opts.Providers.Add<GzipCompressionProvider>();
		});

		// builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
		builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")));
		builder.Services.Configure<FormOptions>(x => {
			x.ValueLengthLimit = int.MaxValue;
			x.MultipartBodyLengthLimit = int.MaxValue;
			x.MultipartHeadersLengthLimit = int.MaxValue;
		});
		
		builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
		builder.Services.AddSingleton<IHttpClientService, HttpClientService>();
		builder.Services.AddSingleton<ILocalStorageService, UMemoryCacheService>();
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
		builder.Services.AddScoped<IFollowService, FollowService>();
		builder.Services.AddScoped<IDashboardService, DashboardService>();
		builder.Services.AddScoped<IInvoiceService, InvoiceService>();
		builder.Services.AddScoped<IContractService, ContractService>();
		builder.Services.AddScoped<ContractService>();
	}

	public static void UseUServices(this WebApplication app) {
		app.UseLeanResponses();
		app.UseStaticFiles();
		app.UseCors();
		app.UseDeveloperExceptionPage();
		app.UseUSwagger();
		app.UseHttpsRedirection();
		app.UseRateLimiter();
		app.UseMiddleware<UMiddleware>();

		app.MapAuthRoutes(RouteTags.Auth);
		app.MapUserRoutes(RouteTags.User);
		app.MapMediaRoutes(RouteTags.Media);
		app.MapContentRoutes(RouteTags.Content);
		app.MapFollowRoutes(RouteTags.Follow);
		app.MapProductRoutes(RouteTags.Product);
		app.MapCommentRoutes(RouteTags.Comment);
		app.MapCategoryRoutes(RouteTags.Category);
		app.MapExamRoutes(RouteTags.Exam);
		app.MapDashboardRoutes(RouteTags.Dashboard);
		app.MapContractRoutes(RouteTags.Contract);
		app.MapInvoiceRoutes(RouteTags.Invoice);
	}
}