using Syncfusion.Licensing;

namespace SinaMN75U.Utils;

public static partial class AspNetConfig {
	public static void AddUServices<T>(this WebApplicationBuilder builder) where T : DbContext {
		builder.Services.Configure<KestrelServerOptions>(o => o.AllowSynchronousIO = false);
		builder.Services.Configure<IISServerOptions>(o => o.AllowSynchronousIO = false);
		builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
		builder.Services.AddUSwagger();
		builder.Services.AddHttpContextAccessor();
		builder.Services.AddHttpClient();
		builder.Services.AddURateLimiter();
		builder.Services.ConfigureHttpJsonOptions(o => {
			o.SerializerOptions.WriteIndented = false;
			o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
			o.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
			o.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
			o.SerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
			o.SerializerOptions.MaxDepth = 128;
		});
		builder.Services.AddScoped<DbContext, T>();
		builder.Services.AddDbContextPool<T>(b => {
			b.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
			b.UseNpgsql(Core.App.ConnectionStrings.Server, o => {
				AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
				o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
				o.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
			});
			if (builder.Environment.IsDevelopment())
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
		});

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
		SyncfusionLicenseProvider.RegisterLicense("@32392e302e303b32393bKq35AiUSRDJT5uIaFzRCrJWDo7gKUKH1Rwb6jH+WX4o=");
		builder.Services.AddScoped<IMediaService, MediaService>();
		builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
		builder.Services.AddSingleton<IHttpClientService, HttpClientService>();
		builder.Services.AddSingleton<ILocalStorageService, UMemoryCacheService>();
		builder.Services.AddScoped<ITokenService, TokenService>();
		builder.Services.AddScoped<IUserService, UserService>();
		builder.Services.AddScoped<IAuthService, AuthService>();
		builder.Services.AddScoped<ICategoryService, CategoryService>();
		builder.Services.AddScoped<ISmsNotificationService, SmsNotificationService>();
		builder.Services.AddScoped<IContentService, ContentService>();
		builder.Services.AddScoped<IProductService, ProductService>();
		builder.Services.AddScoped<ICommentService, CommentService>();
		builder.Services.AddScoped<IFollowService, FollowService>();
		builder.Services.AddScoped<IDashboardService, DashboardService>();
		builder.Services.AddScoped<IInvoiceService, InvoiceService>();
		builder.Services.AddScoped<IContractService, ContractService>();
		builder.Services.AddScoped<ITxnService, TxnService>();
		builder.Services.AddScoped<ITicketService, TicketService>();
		builder.Services.AddScoped<IVehicleService, VehicleService>();
		builder.Services.AddScoped<IParkingService, ParkingService>();
		builder.Services.AddScoped<IInquiryService, InquiryService>();
		builder.Services.AddScoped<IAddressService, AddressService>();
		builder.Services.AddScoped<IWalletService, WalletService>();
		builder.Services.AddScoped<ITerminalService, TerminalService>();
		builder.Services.AddScoped<IBankAccountService, BankAccountService>();
		builder.Services.AddScoped<IIpgService, IpgService>();
		builder.Services.AddScoped<ISimCardService, SimCardService>();
		builder.Services.AddScoped<INotificationService, NotificationService>();
		builder.Services.AddScoped<IDataSeedService, DataSeedService>();
		builder.Services.AddScoped<IAgreementService, AgreementService>();
		builder.Services.AddScoped<IVasService, VasService>();
		builder.Services.AddScoped<IChargeInternetService, ChargeInternetService>();
		builder.Services.AddScoped<IMerchantService, MerchantService>();
	}

	public static void UseUServices(this WebApplication app) {
		app.UseLeanResponses();
		app.UseStaticFiles();
		app.UseCors();
		app.UseDeveloperExceptionPage();
		app.UseUSwagger();
		app.UseHttpsRedirection();
		app.UseRateLimiter();
		if (app.Environment.IsProduction()) {
			app.UseMiddleware<UMiddleware>();
		}

		app.UseMiddleware<DbExceptionMiddleware>();

		app.MapAuthRoutes(RouteTags.Auth);
		app.MapUserRoutes(RouteTags.User);
		app.MapMediaRoutes(RouteTags.Media);
		app.MapContentRoutes(RouteTags.Content);
		app.MapFollowRoutes(RouteTags.Follow);
		app.MapProductRoutes(RouteTags.Product);
		app.MapCommentRoutes(RouteTags.Comment);
		app.MapCategoryRoutes(RouteTags.Category);
		app.MapDashboardRoutes(RouteTags.Dashboard);
		app.MapContractRoutes(RouteTags.Contract);
		app.MapInvoiceRoutes(RouteTags.Invoice);
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
		app.MapSimCardRoutes(RouteTags.SimCard);
		app.MapNotificationRoutes(RouteTags.Notification);
		app.MapDataSeedRoutes(RouteTags.DataSeeder);
		app.MapAgreementRoutes(RouteTags.Agreement);
		app.MapChargeInternetRoutes(RouteTags.ChargeInternet);
		app.MapMerchantRoutes(RouteTags.Merchant);
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