namespace SinaMN75U.Utils;

public static partial class AspNetConfig {
	public static void AddUServices<T>(this WebApplicationBuilder builder, string sqlDatabaseConnectionStrings) where T : DbContext {
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
			b.UseNpgsql(builder.Configuration.GetConnectionString(sqlDatabaseConnectionStrings), o => {
				AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
				o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
				o.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
			});
			if (builder.Environment.IsDevelopment()) {
				b.LogTo(message => {
						if (message.Contains("Executed DbCommand")) {
							Match timeMatch = MyRegex3().Match(message);
							Match queryMatch = MyRegex4().Match(message);

							if (timeMatch.Success && queryMatch.Success) {
								string cleanSql = CleanAndFormatSql(queryMatch.Value);
								Console.WriteLine($"{timeMatch.Groups[1].Value}ms:");
								Console.WriteLine(cleanSql);
								Console.WriteLine();
							}
						}
					},
					[DbLoggerCategory.Database.Command.Name],
					LogLevel.Information);
			}
		});

		builder.Services.AddResponseCompression(opts => {
			opts.EnableForHttps = true;
			opts.Providers.Add<BrotliCompressionProvider>();
			opts.Providers.Add<GzipCompressionProvider>();
		});

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
		builder.Services.AddScoped<IChatBotService, ChatBotService>();
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

	private static string CleanAndFormatSql(string sql) {
		sql = MyRegex().Replace(sql, "");
		sql = MyRegex1().Replace(sql, " ").Trim();
		string formatted = sql
			.Replace("SELECT ", "SELECT\n    ")
			.Replace(" FROM ", "\nFROM ")
			.Replace(" WHERE ", "\nWHERE ")
			.Replace(" ORDER BY ", "\nORDER BY ")
			.Replace(" LIMIT ", "\nLIMIT ")
			.Replace(" OFFSET ", "\nOFFSET ")
			.Replace(" INNER JOIN ", "\nINNER JOIN ")
			.Replace(" LEFT JOIN ", "\nLEFT JOIN ")
			.Replace(" ON ", "\n    ON ")
			.Replace(" AS ", " AS ")
			.Replace("),", "),\n    ")
			.Replace(") AS ", ")\n    AS ");
		formatted = MyRegex2().Replace(formatted, " ");

		return formatted;
	}

	[GeneratedRegex(@"\[Parameters=.*?\]")]
	private static partial Regex MyRegex();

	[GeneratedRegex(@"\s+")]
	private static partial Regex MyRegex1();

	[GeneratedRegex(@"\s+")]
	private static partial Regex MyRegex2();

	[GeneratedRegex(@"\((\d+)ms\)")]
	private static partial Regex MyRegex3();

	[GeneratedRegex(@"SELECT.*", RegexOptions.Singleline)]
	private static partial Regex MyRegex4();
}