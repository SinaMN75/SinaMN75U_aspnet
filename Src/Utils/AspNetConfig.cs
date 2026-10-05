namespace SinaMN75U.Utils;

public static partial class AspNetConfig {
	public static void AddUServices<T>(this WebApplicationBuilder builder) where T : DbContext {
		builder.Services.Configure<KestrelServerOptions>(o => o.AllowSynchronousIO = false);
		builder.Services.Configure<IISServerOptions>(o => o.AllowSynchronousIO = false);
		builder.Services.AddCors(options => options.AddDefaultPolicy(policy => {
			// if (builder.IsDevOrTest()) 
				policy.AllowAnyOrigin();
			// else policy.WithOrigins(Core.App.Cors.AllowedOrigins);
			policy.AllowAnyMethod().AllowAnyHeader();
		}));
		if (builder.IsDevOrTest()) builder.Services.AddUSwagger();
		builder.Services.AddHttpContextAccessor();
		builder.Services.AddHttpClient<IHttpClientService, HttpClientService>().ConfigurePrimaryHttpMessageHandler(() => {
			HttpClientHandler handler = new();
			if (builder.IsDevOrTest()) handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
			return handler;
		});
		builder.Services.AddURateLimiter();
		builder.Services.AddMemoryCache();
		builder.Services.ConfigureHttpJsonOptions(o => {
			o.SerializerOptions.WriteIndented = false;
			o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
			o.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
			o.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
			o.SerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
			o.SerializerOptions.MaxDepth = 128;
			o.SerializerOptions.Converters.Add(new UDateTimeConverter());
		});
		builder.Services.AddDbContextPool<T>(b => {
			b.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
			b.UseNpgsql(Core.App.ConnectionStrings.Server, o => {
				AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
				o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
				o.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
			});
			if (builder.IsDevOrTest())
				b.LogTo(x => {
						if (x.Contains("Executed DbCommand")) {
							Match timeMatch = MyRegex3().Match(x);
							Match queryMatch = MyRegex4().Match(x);

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
		});
		builder.Services.AddScoped<DbContext>(x => x.GetRequiredService<T>());

		builder.Services.AddResponseCompression(o => {
			o.EnableForHttps = true;
			o.Providers.Add<BrotliCompressionProvider>();
			o.Providers.Add<GzipCompressionProvider>();
		});

		builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")));
		builder.Services.Configure<FormOptions>(x => {
			x.ValueLengthLimit = int.MaxValue;
			x.MultipartBodyLengthLimit = int.MaxValue;
			x.MultipartHeadersLengthLimit = int.MaxValue;
		});
		builder.Services.AddScoped<IMediaService, MediaService>();
		builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
		builder.Services.AddSingleton<ILocalStorageService, UMemoryCacheService>();
		builder.Services.AddSingleton<ApiLogQueue>();
		builder.Services.AddSingleton<IApiLogQueue>(x => x.GetRequiredService<ApiLogQueue>());
		builder.Services.AddHostedService<ApiLogBackgroundService>();
		builder.Services.AddSingleton<ITokenService, TokenService>();
		builder.Services.AddScoped<IUserService, UserService>();
		builder.Services.AddScoped<IAuthService, AuthService>();
		builder.Services.AddScoped<ICategoryService, CategoryService>();
		builder.Services.AddScoped<IContentService, ContentService>();
		builder.Services.AddScoped<IProductService, ProductService>();
		builder.Services.AddScoped<ICommentService, CommentService>();
		builder.Services.AddScoped<IFollowService, FollowService>();
		builder.Services.AddScoped<IDashboardService, DashboardService>();
		builder.Services.AddScoped<IAccountingService, AccountingService>();
		builder.Services.AddScoped<ITxnService, TxnService>();
		builder.Services.AddScoped<ITicketService, TicketService>();
		builder.Services.AddScoped<IVehicleService, VehicleService>();
		builder.Services.AddScoped<IParkingService, ParkingService>();
		builder.Services.AddScoped<IAddressService, AddressService>();
		builder.Services.AddScoped<IWalletService, WalletService>();
		builder.Services.AddScoped<ITerminalService, TerminalService>();
		builder.Services.AddScoped<IBankAccountService, BankAccountService>();
		builder.Services.AddScoped<IIpgProvider, PnIpgProvider>();
		builder.Services.AddScoped<IIpgService, IpgService>();
		builder.Services.AddScoped<ISimCardService, SimCardService>();
		builder.Services.AddScoped<INotificationService, NotificationService>();
		builder.Services.AddScoped<IDataSeedService, DataSeedService>();
		builder.Services.AddScoped<IVasService, VasService>();
		builder.Services.AddScoped<IMoadiService, MoadiService>();
		builder.Services.AddScoped<IPnService, PnService>();
		builder.Services.AddScoped<IProcessService, ProcessService>();
		builder.Services.AddScoped<IHotelService, HotelService>();
		builder.Services.AddScoped<IBlogService, BlogService>();
		builder.Services.AddScoped<IFileManagerService, FileManagerService>();
		builder.Services.AddScoped<IDbAdminService, DbAdminService>();
		builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
		builder.Services.AddScoped<IEmailService, EmailService>();
		builder.Services.AddScoped<ISportService, SportService>();
		builder.Services.AddScoped<IVenueService, VenueService>();
		builder.Services.AddScoped<ISocialService, SocialService>();
		builder.Services.AddScoped<IChatService, ChatService>();
		builder.Services.AddSignalR();
		builder.Services.AddSingleton<IRealtimeService, RealtimeService>();
		builder.Services.AddHostedService<SportReminderService>();

		if (Core.App.Test) {
			builder.Services.AddScoped<IInquiryService, InquiryServiceFake>();
			builder.Services.AddScoped<ISmsNotificationService, SmsNotificationServiceFake>();
			builder.Services.AddScoped<IChargeInternetService, ChargeInternetServiceFake>();
			builder.Services.AddScoped<IGoldService, GoldServiceFake>();
		}
		else {
			builder.Services.AddScoped<IInquiryService, InquiryService>();
			builder.Services.AddScoped<ISmsNotificationService, SmsNotificationService>();
			builder.Services.AddScoped<IChargeInternetService, ChargeInternetService>();
			builder.Services.AddHostedService<ChargeInternetRecoveryService>();
			builder.Services.AddScoped<IGoldService, GoldService>();
		}
	}

	public static void UseUServices(this WebApplication app) {
		app.MigrateDatabase();
		app.SeedDefaultUsers();
		app.SeedDefaultAppVersions();
		app.UseCors();
		app.UseStaticFiles();
		app.UseHttpsRedirection();
		app.UseRateLimiter();
		app.UseMiddleware<TimezoneMiddleware>();
		app.UseMiddleware<ApiKeyMiddleware>();
		app.UseMiddleware<ApiLogMiddleware>();
		app.UseMiddleware<ExceptionMiddleware>();
		app.UseMiddleware<DbExceptionMiddleware>();
		if (app.IsDevOrTest()) {
			app.UseDeveloperExceptionPage();
			app.UseUSwagger();
			app.MapUModelsPage();
		}

		app.MapAuthRoutes(RouteTags.Auth);
		app.MapUserRoutes(RouteTags.User);
		app.MapMediaRoutes(RouteTags.Media);
		app.MapContentRoutes(RouteTags.Content);
		app.MapFollowRoutes(RouteTags.Follow);
		app.MapProductRoutes(RouteTags.Product);
		app.MapCommentRoutes(RouteTags.Comment);
		app.MapCategoryRoutes(RouteTags.Category);
		app.MapDashboardRoutes(RouteTags.Dashboard);
		app.MapAccountingRoutes(RouteTags.Accounting);
		app.MapTicketRoutes(RouteTags.Ticket);
		app.MapTxnRoutes(RouteTags.Txn);
		app.MapParkingRoutes(RouteTags.Parking);
		app.MapVehicleRoutes(RouteTags.Vehicle);
		app.MapInquiryRoutes(RouteTags.Inquiry);
		app.MapAddressRoutes(RouteTags.Address);
		app.MapWalletRoutes(RouteTags.Wallet);
		app.MapTerminalRoutes(RouteTags.Terminal);
		app.MapBankAccountRoutes(RouteTags.BankAccount);
		app.MapIpgRoutes(RouteTags.Ipg);
		app.MapSimCardRoutes(RouteTags.Sim);
		app.MapNotificationRoutes(RouteTags.Notification);
		app.MapDataSeedRoutes(RouteTags.DataSeeder);
		app.MapChargeInternetRoutes(RouteTags.ChargeInternet);
		app.MapMoadiRoutes(RouteTags.Moadi);
		app.MapAppSettingsRoutes(RouteTags.AppSettings);
		app.MapProcessRoutes(RouteTags.Process);
		app.MapPnRoutes(RouteTags.Pn);
		app.MapHotelRoutes(RouteTags.Hotel);
		app.MapBlogRoutes(RouteTags.Blog);
		app.MapFileManagerRoutes(RouteTags.FileManager);
		app.MapDbAdminRoutes(RouteTags.DbAdmin);
		app.MapHealthRoutes(RouteTags.Health);
		app.MapLogRoutes(RouteTags.Log);
		app.MapGoldRoutes(RouteTags.Gold);
		app.MapSportRoutes(RouteTags.Sport);
		app.MapVenueRoutes(RouteTags.Venue);
		app.MapSocialRoutes(RouteTags.Social);
		app.MapChatRoutes(RouteTags.Chat);
		app.MapHub<UHub>("/hubs/u");
		ULog.Info("App Started in " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
	}
	
	private static void MigrateDatabase(this WebApplication app) {
		using IServiceScope scope = app.Services.CreateScope();
		DbContext db = scope.ServiceProvider.GetRequiredService<DbContext>();
		try {
			List<string> pending = db.Database.GetPendingMigrations().ToList();
			if (pending.Count == 0) {
				ULog.Info("Database migration: no update required.");
				return;
			}

			ULog.Info($"Database migration: {pending.Count} pending migration(s) found: {string.Join(", ", pending)}");
			db.Database.Migrate();
			ULog.Success($"Database migration: completed successfully. Applied: {string.Join(", ", pending)}");
		}
		catch (Exception ex) {
			ULog.Error(ex, "Database migration failed");
		}
	}

	private static void SeedDefaultUsers(this WebApplication app) {
		using IServiceScope scope = app.Services.CreateScope();
		try {
			scope.ServiceProvider.GetRequiredService<IDataSeedService>().SeedUsers().GetAwaiter().GetResult();
		}
		catch (Exception ex) {
			ULog.Error(ex, "Seed users failed");
		}
	}

	private static void SeedDefaultAppVersions(this WebApplication app) {
		using IServiceScope scope = app.Services.CreateScope();
		try {
			scope.ServiceProvider.GetRequiredService<IDataSeedService>().SeedAppVersions().GetAwaiter().GetResult();
		}
		catch (Exception ex) {
			ULog.Error(ex, "Seed app versions failed");
		}
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

	[GeneratedRegex("SELECT.*", RegexOptions.Singleline)]
	private static partial Regex MyRegex4();
}