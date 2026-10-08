namespace SinaMN75U.Services;

public partial class DataSeedService {
	private const string DemoPassword = "Demo1234";
	private const string DemoMark = "گروه هتل‌های پارسیان (نمونه)";

	public async Task<UResponse<List<KeyValue>?>> SeedDemo(BaseParams p, CancellationToken ct) {
		ILocalizationService ls = sp.GetRequiredService<ILocalizationService>();
		JwtClaimData? u = sp.GetRequiredService<ITokenService>().ExtractClaims(p.Token);
		if (u == null) return new UResponse<List<KeyValue>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (!u.IsSystemAdmin) return new UResponse<List<KeyValue>?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (await db.Set<OrganizationEntity>().AnyAsync(x => x.Title == DemoMark, ct)) return new UResponse<List<KeyValue>?>(null, Usc.Conflict, ls.Get("demoDataAlreadyExists"));

		Demo demo = new(sp.GetRequiredService<IServiceScopeFactory>());
		await demo.Run(ct);
		try {
			await SeedCategories();
			await SeedContents();
		}
		catch (Exception ex) {
			demo.Report.Add(new KeyValue { Key = "خطا: محتوای سایت", Value = ex.GetBaseException().Message });
		}

		return new UResponse<List<KeyValue>?>(demo.Report, Usc.Success, ls.Get("demoDataCreated"));
	}

	private sealed record DemoPerson(string Key, string FirstName, string LastName, bool Male, int BirthYear, decimal Wallet, string Role);

	private sealed class Demo(IServiceScopeFactory scopes) {
		public List<KeyValue> Report { get; } = [];

		private static readonly DemoPerson[] People = [
			new("o1", "مهدی", "رستمی", true, 1978, 300_000_000, "مالک گروه هتل‌های پارسیان"),
			new("o2", "لیلا", "شریفی", false, 1982, 300_000_000, "مالک خوابگاه‌های آرامش"),
			new("o3", "کاوه", "نیک‌نام", true, 1988, 100_000_000, "مالک کافه رستوران نارنج"),
			new("o4", "شیرین", "افشار", false, 1985, 50_000_000, "مالک اقامتگاه بوم‌گردی (اشتراک تمام‌شده)"),
			new("s1", "بهرام", "توکلی", true, 1990, 0, "مدیر پذیرش هتل"),
			new("s2", "نسترن", "ملکی", false, 1992, 0, "حسابدار هتل"),
			new("s3", "جواد", "رنجبر", true, 1987, 0, "انباردار هتل"),
			new("s4", "مینا", "اکبری", false, 1989, 0, "سرپرست خانه‌داری هتل"),
			new("s5", "فرزانه", "سلطانی", false, 1986, 0, "مدیر خوابگاه"),
			new("s6", "هادی", "کامرانی", true, 1991, 0, "حسابدار خوابگاه"),
			new("s7", "سمیه", "فرهادی", false, 1993, 0, "سرپرست و انباردار خوابگاه"),
			new("s8", "آرش", "بهرامی", true, 1995, 0, "انباردار و حسابدار کافه"),
			new("f1", "نیلوفر", "صادقی", false, 2003, 60_000_000, "ساکن خوابگاه دخترانه"),
			new("f2", "ریحانه", "مرادی", false, 2002, 60_000_000, "ساکن خوابگاه دخترانه"),
			new("f3", "پریسا", "عباسی", false, 2004, 60_000_000, "ساکن خوابگاه دخترانه (بدهکار)"),
			new("f4", "هانیه", "نوری", false, 2001, 60_000_000, "ساکن خوابگاه دخترانه"),
			new("f5", "مهسا", "قربانی", false, 2003, 60_000_000, "ساکن خوابگاه دخترانه"),
			new("f6", "یاسمن", "حیدری", false, 2002, 60_000_000, "ساکن خوابگاه دخترانه (بدهکار)"),
			new("f7", "الناز", "کریمی", false, 2004, 60_000_000, "ساکن خوابگاه دخترانه"),
			new("f8", "ترانه", "جمشیدی", false, 2005, 60_000_000, "ساکن خوابگاه دخترانه"),
			new("f9", "فاطمه", "ستاری", false, 2000, 60_000_000, "ساکن سابق (قرارداد تسویه‌شده)"),
			new("m1", "سینا", "امیری", true, 2002, 60_000_000, "ساکن خوابگاه پسرانه"),
			new("m2", "پارسا", "زارعی", true, 2003, 60_000_000, "ساکن خوابگاه پسرانه (بدهکار)"),
			new("m3", "محمدرضا", "یزدانی", true, 2001, 60_000_000, "ساکن خوابگاه پسرانه"),
			new("m4", "امید", "طاهری", true, 2004, 60_000_000, "ساکن خوابگاه پسرانه"),
			new("m5", "کیان", "رحیمی", true, 2003, 60_000_000, "ساکن خوابگاه پسرانه"),
			new("m6", "دانیال", "شکوهی", true, 2005, 60_000_000, "ساکن خوابگاه پسرانه"),
			new("a1", "ساناز", "پاکزاد", false, 2004, 5_000_000, "متقاضی اقامت (تأییدشده)"),
			new("a2", "آیدا", "منصوری", false, 2005, 5_000_000, "متقاضی اقامت (لیست انتظار)"),
			new("a3", "بردیا", "خلیلی", true, 2004, 5_000_000, "متقاضی اقامت (ردشده)"),
			new("a4", "نرگس", "فاضلی", false, 2003, 5_000_000, "متقاضی اقامت (در انتظار بررسی)"),
			new("g1", "حمید", "وحیدی", true, 1980, 80_000_000, "مهمان هتل (ویژه)"),
			new("g2", "فرشته", "کاظمی", false, 1987, 80_000_000, "مهمان هتل"),
			new("g3", "سعید", "نادری", true, 1975, 80_000_000, "مهمان هتل"),
			new("g4", "مژگان", "رحمانی", false, 1990, 80_000_000, "مهمان هتل"),
			new("g5", "بابک", "فرجی", true, 1984, 80_000_000, "مهمان هتل"),
			new("g6", "زینب", "هاشمی", false, 1992, 80_000_000, "مهمان هتل"),
			new("g7", "کامران", "اسدی", true, 1979, 80_000_000, "مهمان در حال اقامت"),
			new("g8", "شهرزاد", "دلاوری", false, 1988, 80_000_000, "مهمان در حال اقامت"),
			new("g9", "رامین", "سبحانی", true, 1983, 80_000_000, "مهمان با رزرو آینده"),
			new("g10", "گلاره", "مقدم", false, 1995, 80_000_000, "مهمان با رزرو آینده"),
			new("g11", "پیمان", "عزیزی", true, 1981, 80_000_000, "مهمان در لیست سیاه"),
			new("g12", "نازنین", "رسولی", false, 1993, 80_000_000, "مهمان هتل اصفهان")
		];

		private static readonly (string Key, string Title, string Description, TagModule[] Modules, decimal Monthly, int Trial, bool Featured, string[] Features)[] Plans = [
			("hotel", "هتل", "برای هتل‌ها، هتل‌آپارتمان‌ها و اقامتگاه‌ها", [TagModule.Hotel], 2_500_000, 14, false, ["رزرو آنلاین و تقویم", "قیمت فصلی و تعطیلات", "ممیزی شبانه و خانه‌داری"]),
			("dorm", "خوابگاه و پانسیون", "برای خوابگاه‌های دانشجویی و پانسیون‌ها", [TagModule.Dorm], 1_500_000, 14, false, ["قرارداد و قبض خودکار", "درخواست اقامت آنلاین", "غذا و لباسشویی"]),
			("accounting", "حسابداری", "دفتر حساب، صندوق، بانک و چک برای هر کسب‌وکار", [TagModule.Accounting], 1_000_000, 0, false, ["سند دوطرفه", "مدیریت چک", "گزارش ارزش افزوده"]),
			("inventory", "انبارداری", "انبار، کالا، خرید و اموال برای هر کسب‌وکار", [TagModule.Inventory], 900_000, 0, false, ["هشدار کمبود موجودی", "درخواست خرید و تأیید"]),
			("staff", "کارکنان", "شیفت، وظایف و درخواست‌های تعمیرات", [TagModule.Staff], 500_000, 0, false, ["شیفت و حضور", "ارجاع کار"]),
			("cafe", "کافی‌شاپ و رستوران", "انبارداری و حسابداری با هم", [TagModule.Inventory, TagModule.Accounting], 1_600_000, 0, true, ["سند خودکار خرید و مصرف", "صورت سود و زیان"]),
			("fullDorm", "خوابگاه کامل", "خوابگاه به همراه حسابداری، انبارداری و کارکنان", [TagModule.Dorm, TagModule.Accounting, TagModule.Inventory, TagModule.Staff], 3_200_000, 0, true, ["همه‌ی امکانات خوابگاه", "حسابداری و انبار"]),
			("fullHotel", "هتل کامل", "هتل به همراه حسابداری، انبارداری و کارکنان", [TagModule.Hotel, TagModule.Accounting, TagModule.Inventory, TagModule.Staff], 4_200_000, 0, false, ["همه‌ی امکانات هتل", "حسابداری و انبار"]),
			("everything", "همه‌ی سیستم‌ها", "هتل، خوابگاه، حسابداری، انبارداری و کارکنان", [TagModule.Hotel, TagModule.Dorm, TagModule.Accounting, TagModule.Inventory, TagModule.Staff], 5_500_000, 0, false, ["همه‌چیز در یک اشتراک"])
		];

		private readonly List<string> _errors = [];
		private readonly DateTime _today = DateTime.UtcNow.Date;
		private readonly Dictionary<string, Guid> _user = [];
		private readonly Dictionary<string, Guid> _plan = [];
		private readonly Dictionary<string, Guid> _org = [];
		private readonly Dictionary<string, Guid> _place = [];
		private readonly Dictionary<string, Guid> _room = [];
		private readonly Dictionary<string, Guid> _contract = [];
		private readonly List<Guid> _d1Beds = [];
		private readonly List<Guid> _d2Beds = [];

		private DateTime D(int days, int hour = 0) => _today.AddDays(days).AddHours(hour);

		private static string Phone(DemoPerson p) => $"09901{100 + Array.IndexOf(People, p):000000}";

		private static DemoPerson Person(string key) => People.First(x => x.Key == key);

		private static string Name(string key) => $"{Person(key).FirstName} {Person(key).LastName}";

		private static string NationalCode(int seed) {
			string body = (210_000_000 + seed * 104_729 % 700_000_000).ToString("D9");
			int sum = body.Select((c, i) => (c - '0') * (10 - i)).Sum() % 11;
			return body + (sum < 2 ? sum : 11 - sum);
		}

		private T? Ok<T>(UResponse<T> r, string step) {
			if (r.Result == null || r.Status is not (Usc.Success or Usc.Created)) _errors.Add($"{step}: {r.Message}");
			return r.Status is Usc.Success or Usc.Created ? r.Result : default;
		}

		private bool Done(UResponse r, string step) {
			bool ok = r.Status is Usc.Success or Usc.Created;
			if (!ok) _errors.Add($"{step}: {r.Message}");
			return ok;
		}

		private async Task Step(string name, Func<IServiceProvider, Task> body) {
			using IServiceScope scope = scopes.CreateScope();
			try {
				await body(scope.ServiceProvider);
			}
			catch (Exception ex) {
				_errors.Add($"{name}: {ex.GetBaseException().Message}");
			}
		}

		private static string Admin(IServiceProvider sp) => sp.GetRequiredService<ITokenService>().GenerateJwt(Core.App.Users.SystemAdmin);

		private async Task<string> Token(IServiceProvider sp, string key, CancellationToken ct) {
			Guid id = _user[key];
			UserEntity user = await sp.GetRequiredService<DbContext>().Set<UserEntity>().AsNoTracking().FirstAsync(x => x.Id == id, ct);
			return sp.GetRequiredService<ITokenService>().GenerateJwt(user);
		}

		private static async Task<Dictionary<string, Guid>> Accounts(IServiceProvider sp, string token, Guid organizationId, CancellationToken ct) {
			UResponse<IEnumerable<AccountResponse>?> r = await sp.GetRequiredService<IAccountingService>().ReadAccounts(new AccountReadParams { Token = token, OrganizationId = organizationId, PageSize = 200 }, ct);
			return (r.Result ?? []).ToDictionary(x => x.Code, x => x.Id);
		}

		public async Task Run(CancellationToken ct) {
			await Step("کاربران", sp => Users(sp, ct));
			await Step("پلن‌ها", sp => SubscriptionPlans(sp, ct));
			await Step("مجموعه‌ها", sp => Organizations(sp, ct));
			await Step("هتل‌ها", sp => Hotels(sp, ct));
			await Step("رزروهای هتل", sp => Reservations(sp, ct));
			await Step("خوابگاه‌ها", sp => Dorms(sp, ct));
			await Step("قراردادهای خوابگاه", sp => Contracts(sp, ct));
			await Step("عملیات روزانه‌ی خوابگاه", sp => DormOperations(sp, ct));
			await Step("انبارداری", sp => Inventory(sp, ct));
			await Step("حسابداری", sp => Accounting(sp, ct));
			await Step("کارکنان و مشتریان", sp => Staff(sp, ct));
			await Step("کارهای دوره‌ای", async sp => {
				await sp.GetRequiredService<IHotelService>().PostDueInvoices(ct);
				await sp.GetRequiredService<IDormService>().ProcessDueInvoices(ct);
				await sp.GetRequiredService<IAccountingService>().RemindChecks(ct);
			});
			await Step("پایان اشتراک اقامتگاه", sp => ExpireLodge(sp, ct));
			await Step("گزارش", sp => Summary(sp, ct));
		}

		private async Task Users(IServiceProvider sp, CancellationToken ct) {
			DbContext db = sp.GetRequiredService<DbContext>();
			List<string> phones = People.Select(Phone).ToList();
			Dictionary<string, Guid> existing = await db.Set<UserEntity>().Where(x => phones.Contains(x.UserName)).ToDictionaryAsync(x => x.UserName, x => x.Id, ct);
			DateTime now = DateTime.UtcNow;
			string password = UPasswordHasher.Hash(DemoPassword);
			for (int i = 0; i < People.Length; i++) {
				DemoPerson p = People[i];
				string phone = Phone(p);
				if (existing.TryGetValue(phone, out Guid found)) {
					_user[p.Key] = found;
					continue;
				}

				Guid id = Guid.CreateVersion7();
				_user[p.Key] = id;
				await db.Set<UserEntity>().AddAsync(new UserEntity {
					Id = id,
					CreatorId = Core.App.Users.SystemAdmin.Id,
					CreatedAt = now.AddDays(-200 + i),
					Tags = [p.Male ? TagUser.Male : TagUser.Female, TagUser.Verified],
					UserName = phone,
					Password = password,
					RefreshToken = "",
					PhoneNumber = phone,
					NationalCode = NationalCode(i + 1),
					FirstName = p.FirstName,
					LastName = p.LastName,
					Birthdate = new DateTime(p.BirthYear, 1 + i % 12, 1 + i % 27, 0, 0, 0, DateTimeKind.Utc),
					JsonData = new UserJson(),
					Wallets = [new WalletEntity { Id = id, CreatorId = id, CreatedAt = now, JsonData = new WalletJson(), Tags = [TagWallet.Primary], Balance = p.Wallet }]
				}, ct);
			}

			await db.SaveChangesAsync(ct);
		}

		private async Task SubscriptionPlans(IServiceProvider sp, CancellationToken ct) {
			IOrganizationService os = sp.GetRequiredService<IOrganizationService>();
			List<SubscriptionPlanEntity> plans = await sp.GetRequiredService<DbContext>().Set<SubscriptionPlanEntity>().ToListAsync(ct);
			string admin = Admin(sp);
			int order = 0;
			foreach ((string key, string title, string description, TagModule[] modules, decimal monthly, int trial, bool featured, string[] features) in Plans) {
				order++;
				SubscriptionPlanEntity? same = plans.FirstOrDefault(x => x.Tags.Contains(TagSubscriptionPlan.Active) && x.JsonData.Modules.Order().SequenceEqual(modules.Order()) && x.JsonData.Prices.Any(y => y.Months == 12));
				if (same != null) {
					_plan[key] = same.Id;
					continue;
				}

				Guid? id = Ok(await os.CreatePlan(new SubscriptionPlanCreateParams {
					Token = admin,
					Title = title,
					Detail1 = description,
					Modules = modules.ToList(),
					Prices = [new PlanPrice { Months = 1, Price = monthly }, new PlanPrice { Months = 12, Price = monthly * 10 }],
					Features = features.ToList(),
					TrialDays = trial,
					Order = order,
					Tags = featured ? [TagSubscriptionPlan.Active, TagSubscriptionPlan.Featured] : [TagSubscriptionPlan.Active]
				}, ct), $"پلن {title}");
				if (id != null) _plan[key] = id.Value;
			}
		}

		private async Task<Guid> Buy(IServiceProvider sp, string owner, string title, string plan, int months, CancellationToken ct) {
			IOrganizationService os = sp.GetRequiredService<IOrganizationService>();
			string token = await Token(sp, owner, ct);
			bool trial = months == 0 && await sp.GetRequiredService<DbContext>().Set<SubscriptionPlanEntity>().AnyAsync(x => x.Id == _plan[plan] && x.JsonData.TrialDays > 0, ct);
			SubscriptionBuyResponse? r = Ok(await os.BuySubscription(new SubscriptionBuyParams {
				Token = token, PlanId = _plan[plan], Months = trial ? 0 : Math.Max(1, months), Trial = trial, Title = title, Password = DemoPassword, FromWallet = true
			}, ct), $"خرید اشتراک {title}");
			return r?.OrganizationId ?? Guid.Empty;
		}

		private async Task Organizations(IServiceProvider sp, CancellationToken ct) {
			IOrganizationService os = sp.GetRequiredService<IOrganizationService>();
			_org["hotel"] = await Buy(sp, "o1", DemoMark, "fullHotel", 12, ct);
			_org["dorm"] = await Buy(sp, "o2", "خوابگاه‌های دانشجویی آرامش (نمونه)", "fullDorm", 12, ct);
			_org["cafe"] = await Buy(sp, "o3", "کافه رستوران نارنج (نمونه)", "cafe", 1, ct);
			_org["lodge"] = await Buy(sp, "o4", "اقامتگاه بوم‌گردی کویر مصر (نمونه)", "hotel", 0, ct);

			string admin = Admin(sp);
			(string Org, string Owner, string Address, string Phone, string NationalId, string EconomicCode, decimal Vat, decimal Commission, string About)[] info = [
				("hotel", "o1", "تهران، خیابان ولیعصر، بالاتر از میدان ونک، پلاک ۲۴۵", "02188776655", "14009876543", "411122233344", 10, 5, "گروه هتل‌های پارسیان با دو هتل در تهران و اصفهان"),
				("dorm", "o2", "تهران، خیابان انقلاب، خیابان قدس، پلاک ۱۸", "02166403322", "14005544332", "411199887766", 0, 3, "خوابگاه‌های دانشجویی دخترانه و پسرانه در تهران و مشهد"),
				("cafe", "o3", "شیراز، بلوار چمران، نبش کوچه‌ی ۱۲", "07136281100", "14003322110", "411155566677", 10, 0, "کافه و رستوران با منوی صبحانه، ناهار و قهوه‌ی تخصصی"),
				("lodge", "o4", "اصفهان، شهرستان خور و بیابانک، روستای مصر", "03146223344", "", "", 0, 0, "اقامتگاه بوم‌گردی در دل کویر")
			];
			foreach ((string org, string owner, string address, string phone, string nationalId, string economicCode, decimal vat, decimal commission, string about) in info) {
				if (_org[org] == Guid.Empty) continue;
				Done(await os.UpdateOrganization(new OrganizationUpdateParams {
					Token = await Token(sp, owner, ct), Id = _org[org], Address = address, PhoneNumber = phone, NationalId = nationalId, EconomicCode = economicCode, VatPercent = vat,
					TaxServiceId = vat > 0 ? "2720000114542" : null, Detail1 = about
				}, ct), $"اطلاعات مجموعه {org}");
				if (commission > 0) Done(await os.UpdateOrganization(new OrganizationUpdateParams { Token = admin, Id = _org[org], CommissionPercent = commission }, ct), $"کمیسیون {org}");
			}

			(string Org, string Owner, string Staff, TagUser[] Permissions)[] members = [
				("hotel", "o1", "s1", [TagUser.PermissionManageHotels, TagUser.PermissionManageReservations, TagUser.PermissionDeleteReservations, TagUser.PermissionManageInvoices, TagUser.PermissionPayInvoices, TagUser.PermissionManageUsers, TagUser.PermissionViewDashboard]),
				("hotel", "o1", "s2", [TagUser.PermissionManageAccounting, TagUser.PermissionViewAccounting, TagUser.PermissionPayInvoices, TagUser.PermissionViewDashboard]),
				("hotel", "o1", "s3", [TagUser.PermissionManageInventory]),
				("hotel", "o1", "s4", [TagUser.PermissionManageHotels, TagUser.PermissionManageStaff]),
				("dorm", "o2", "s5", [TagUser.PermissionManageDorms, TagUser.PermissionManageContracts, TagUser.PermissionManageInvoices, TagUser.PermissionPayInvoices, TagUser.PermissionManageUsers, TagUser.PermissionViewDashboard]),
				("dorm", "o2", "s6", [TagUser.PermissionManageAccounting, TagUser.PermissionViewAccounting, TagUser.PermissionPayInvoices]),
				("dorm", "o2", "s7", [TagUser.PermissionManageDorms, TagUser.PermissionManageInventory, TagUser.PermissionManageStaff]),
				("cafe", "o3", "s8", [TagUser.PermissionManageInventory, TagUser.PermissionManageAccounting, TagUser.PermissionViewAccounting])
			];
			foreach ((string org, string owner, string staff, TagUser[] permissions) in members) {
				if (_org[org] == Guid.Empty) continue;
				Done(await os.SetOrganizationMember(new OrganizationMemberParams {
					Token = await Token(sp, owner, ct), OrganizationId = _org[org], UserId = _user[staff], Permissions = permissions.ToList(), Password = DemoPassword
				}, ct), $"کارمند {Name(staff)}");
			}
		}

		private async Task<Guid> Room(IHotelService hs, string token, string key, Guid hotel, string title, TagRoom[] tags, int capacity, decimal price, int quantity, string number, string bed, double size, int floor, CancellationToken ct, decimal? extraGuest = null) {
			Guid? id = Ok(await hs.CreateHotelRoom(new HotelRoomCreateParams {
				Token = token, HotelId = hotel, Title = title, Tags = tags.ToList(), Capacity = capacity, PricePerNight = price, Quantity = quantity, RoomNumber = number, IsAvailable = true,
				BedType = bed, SizeSquareMeters = size, Floor = floor, ExtraGuestCapacity = extraGuest == null ? null : 1, ExtraGuestPrice = extraGuest,
				Description = $"{title} با {capacity} تخت، {size} متر مربع، طبقه‌ی {floor}"
			}, ct), $"اتاق {title}");
			_room[key] = id ?? Guid.Empty;
			return id ?? Guid.Empty;
		}

		private async Task Hotels(IServiceProvider sp, CancellationToken ct) {
			IHotelService hs = sp.GetRequiredService<IHotelService>();
			string owner = await Token(sp, "o1", ct);
			string admin = Admin(sp);
			_place["h1"] = Ok(await hs.CreateHotel(new HotelCreateParams {
				Token = owner, OrganizationId = _org["hotel"], Title = "هتل پارسیان آزادی (نمونه)", CityCode = "108012", Stars = 5,
				Address = "تهران، بزرگراه چمران، نبش خیابان یمن", PhoneNumber = "02122068011", Email = "azadi@example.com",
				Description = "هتل پنج‌ستاره با چشم‌انداز البرز، استخر سرپوشیده، سالن همایش و رستوران ایرانی و بین‌المللی.",
				Policies = "پذیرش از ساعت ۱۴ و تخلیه تا ساعت ۱۲. لغو رایگان تا ۴۸ ساعت قبل از ورود. همراه داشتن کارت ملی یا گذرنامه الزامی است.",
				CheckInTime = "14:00", CheckOutTime = "12:00",
				Highlights = ["چشم‌انداز کوه‌های البرز", "استخر و سونا", "صبحانه‌ی بوفه"], Rules = ["استعمال دخانیات در اتاق‌ها ممنوع است", "ورود حیوان خانگی ممنوع است"],
				HowToGetThere = "۱۰ دقیقه پیاده تا ایستگاه مترو شهید چمران", Nearby = [new PlaceNearby { Title = "برج میلاد", DistanceMeters = 4500, Minutes = 12 }, new PlaceNearby { Title = "پارک ملت", DistanceMeters = 2000, Minutes = 6 }],
				Faqs = [new PlaceFaq { Question = "آیا پارکینگ رایگان است؟", Answer = "بله، برای مهمانان رایگان است." }],
				Latitude = 35.7796, Longitude = 51.3900, CancellationFreeHours = 48, CancellationPenaltyNights = 1,
				Tags = [TagHotel.Hotel, TagHotel.Active, TagHotel.Featured, TagHotel.Approved, TagHotel.ChildrenAllowed, TagHotel.ExtraBedAvailable, TagHotel.PriceIncludesTax,
					TagHotel.Wifi, TagHotel.Parking, TagHotel.Elevator, TagHotel.Reception24, TagHotel.Restaurant, TagHotel.Cafe, TagHotel.RoomService, TagHotel.Laundry, TagHotel.Pool,
					TagHotel.Gym, TagHotel.Sauna, TagHotel.MeetingRoom, TagHotel.PrayerRoom, TagHotel.Cctv, TagHotel.Breakfast]
			}, ct), "هتل آزادی") ?? Guid.Empty;
			_place["h2"] = Ok(await hs.CreateHotel(new HotelCreateParams {
				Token = owner, OrganizationId = _org["hotel"], Title = "هتل سنتی کوثر اصفهان (نمونه)", CityCode = "104005", Stars = 4,
				Address = "اصفهان، خیابان چهارباغ عباسی، کوچه‌ی شاهزاده‌ها", PhoneNumber = "03132225566",
				Description = "خانه‌ای قاجاری با حیاط و حوض، پنج دقیقه تا میدان نقش جهان.", Policies = "پذیرش از ساعت ۱۴ و تخلیه تا ساعت ۱۲.",
				CheckInTime = "14:00", CheckOutTime = "12:00", Highlights = ["حیاط سنتی با حوض", "نزدیک میدان نقش جهان"],
				Nearby = [new PlaceNearby { Title = "میدان نقش جهان", DistanceMeters = 600, Minutes = 8 }], Latitude = 32.6546, Longitude = 51.6680, CancellationFreeHours = 24, CancellationPenaltyNights = 1,
				Tags = [TagHotel.Traditional, TagHotel.Active, TagHotel.Approved, TagHotel.ChildrenAllowed, TagHotel.Wifi, TagHotel.Reception24, TagHotel.Cafe, TagHotel.Garden, TagHotel.LuggageStorage, TagHotel.Breakfast]
			}, ct), "هتل کوثر") ?? Guid.Empty;
			if (_place["h1"] == Guid.Empty) return;

			TagRoom[] comfort = [TagRoom.Available, TagRoom.PrivateBathroom, TagRoom.AirConditioning, TagRoom.Heating, TagRoom.Tv, TagRoom.Fridge, TagRoom.Kettle, TagRoom.HairDryer];
			await Room(hs, owner, "single", _place["h1"], "اتاق یک‌تخته‌ی استاندارد", [TagRoom.Single, TagRoom.CityView, TagRoom.Desk, ..comfort], 1, 2_800_000, 6, "101", "یک‌نفره", 18, 1, ct);
			await Room(hs, owner, "double", _place["h1"], "اتاق دوتخته‌ی دبل", [TagRoom.Double, TagRoom.MountainView, TagRoom.Minibar, ..comfort], 2, 4_200_000, 10, "201", "دونفره", 26, 2, ct);
			await Room(hs, owner, "suite", _place["h1"], "سوئیت جونیور", [TagRoom.Suite, TagRoom.MountainView, TagRoom.Bathtub, TagRoom.Minibar, TagRoom.SafeBox, TagRoom.Balcony, ..comfort], 3, 7_800_000, 3, "301", "کینگ‌سایز", 48, 3, ct);
			await Room(hs, owner, "family", _place["h1"], "اتاق خانوادگی", [TagRoom.Family, TagRoom.CityView, TagRoom.Wardrobe, ..comfort], 4, 6_500_000, 4, "401", "یک دونفره و دو یک‌نفره", 40, 4, ct, 900_000);
			if (_place["h2"] != Guid.Empty) {
				await Room(hs, owner, "isfDouble", _place["h2"], "اتاق دوتخته‌ی سنتی", [TagRoom.Double, TagRoom.CourtyardView, ..comfort], 2, 3_600_000, 6, "11", "دونفره", 22, 1, ct);
				await Room(hs, owner, "isfTriple", _place["h2"], "اتاق سه‌تخته", [TagRoom.Triple, TagRoom.CourtyardView, ..comfort], 3, 4_800_000, 3, "21", "سه یک‌نفره", 28, 1, ct);
				await Room(hs, owner, "isfSuite", _place["h2"], "سوئیت شاه‌نشین", [TagRoom.Suite, TagRoom.CourtyardView, TagRoom.Kitchenette, ..comfort], 4, 7_200_000, 2, "31", "دونفره و دو تخت سنتی", 55, 2, ct);
			}

			foreach (string hotel in new[] { "h1", "h2" }.Where(x => _place[x] != Guid.Empty))
				Done(await hs.UpdateHotel(new HotelUpdateParams { Token = admin, Id = _place[hotel], AddAdminUserIds = [_user["s1"], _user["s4"]] }, ct), "دسترسی کارکنان به هتل");

			int y = _today >= new DateTime(_today.Year, 3, 20) ? _today.Year + 1 : _today.Year;
			int summer = _today >= new DateTime(_today.Year, 9, 22) ? _today.Year + 1 : _today.Year;
			(Guid Hotel, Guid? Room, DateTime From, DateTime To, decimal? Price, decimal? Percent, List<int> Weekdays, int? MinNights, TagHotelRate Tag, string Title)[] rates = [
				(_place["h1"], null, new DateTime(y, 3, 20), new DateTime(y, 4, 2), null, 40, [], 2, TagHotelRate.Holiday, "تعطیلات نوروز"),
				(_place["h1"], null, D(-30), D(365), null, 15, [4, 5], null, TagHotelRate.Seasonal, "آخر هفته"),
				(_place["h1"], _room["double"], new DateTime(summer, 6, 22), new DateTime(summer, 9, 22), 5_000_000, null, [], null, TagHotelRate.Seasonal, "تابستان"),
				(_place["h1"], _room["suite"], D(20), D(22), null, null, [], null, TagHotelRate.Closed, "بازسازی سوئیت‌ها")
			];
			if (_place["h2"] != Guid.Empty) rates = [..rates, (_place["h2"], null, new DateTime(y, 3, 20), new DateTime(y, 4, 2), null, 50, [], 3, TagHotelRate.Holiday, "تعطیلات نوروز")];
			foreach ((Guid hotel, Guid? room, DateTime from, DateTime to, decimal? price, decimal? percent, List<int> weekdays, int? minNights, TagHotelRate tag, string title) in rates)
				Ok(await hs.CreateHotelRate(new HotelRateCreateParams {
					Token = owner, HotelId = hotel, RoomId = room, StartDate = from, EndDate = to, Price = price, Percent = percent, Weekdays = weekdays, MinNights = minNights, Tags = [tag], Detail1 = title
				}, ct), $"نرخ {title}");

			foreach ((string room, string number, TagHousekeeping status, string? note) in new[] {
				         ("single", "101", TagHousekeeping.Clean, (string?)null), ("single", "105", TagHousekeeping.Inspected, null), ("double", "204", TagHousekeeping.Dirty, null),
				         ("double", "209", TagHousekeeping.OutOfOrder, "خرابی سیستم سرمایش، منتظر تعمیرکار"), ("family", "403", TagHousekeeping.Clean, null)
			         })
				Done(await hs.SetHotelRoomHousekeeping(new HotelHousekeepingParams { Token = owner, RoomId = _room[room], Number = number, Status = status, Note = note }, ct), $"خانه‌داری {number}");

			string lodgeOwner = await Token(sp, "o4", ct);
			if (_org["lodge"] != Guid.Empty) {
				_place["lodge"] = Ok(await hs.CreateHotel(new HotelCreateParams {
					Token = lodgeOwner, OrganizationId = _org["lodge"], Title = "اقامتگاه بوم‌گردی خانه‌ی خشتی (نمونه)", CityCode = "104005", Stars = 0,
					Address = "اصفهان، خور و بیابانک، روستای مصر", Description = "خانه‌ی خشتی در حاشیه‌ی کویر، شب‌های پرستاره و شترسواری.",
					Tags = [TagHotel.Guesthouse, TagHotel.Active, TagHotel.Approved, TagHotel.Parking, TagHotel.Garden, TagHotel.HalfBoard]
				}, ct), "اقامتگاه بوم‌گردی") ?? Guid.Empty;
				if (_place["lodge"] != Guid.Empty)
					await Room(hs, lodgeOwner, "lodgeRoom", _place["lodge"], "اتاق سنتی", [TagRoom.Family, TagRoom.CourtyardView, TagRoom.Heating], 4, 2_200_000, 5, "1", "رختخواب سنتی", 20, 1, ct);
			}
		}

		private async Task<Guid> Reserve(IServiceProvider sp, string token, string room, string guest, int from, int to, string? number, TagHotelReservation status, CancellationToken ct, int guests = 1, List<ReservationGuestParams>? list = null) {
			IHotelService hs = sp.GetRequiredService<IHotelService>();
			Guid? id = Ok(await hs.CreateHotelReservation(new HotelReservationCreateParams {
				Token = token, RoomId = _room[room], UserId = _user[guest], CheckInDate = D(from, 10), CheckOutDate = D(to, 8), GuestCount = guests, Guests = list,
				GuestName = Name(guest), GuestPhone = Phone(Person(guest)), RoomNumber = number, Tags = [status == TagHotelReservation.Pending ? TagHotelReservation.Pending : TagHotelReservation.Confirmed],
				Notes = guests > 2 ? "درخواست تخت اضافه برای کودک" : null
			}, ct), $"رزرو {Name(guest)}");
			if (id == null) return Guid.Empty;
			IdParams p = new() { Token = token, Id = id.Value };
			if (status is TagHotelReservation.CheckedIn or TagHotelReservation.CheckedOut) Done(await hs.CheckInHotelReservation(p, ct), "پذیرش");
			if (status == TagHotelReservation.CheckedOut) Done(await hs.CheckOutHotelReservation(p, ct), "تسویه");
			if (status == TagHotelReservation.Cancelled) Done(await hs.CancelHotelReservation(p, ct), "لغو رزرو");
			return id.Value;
		}

		private static Task<Guid> InvoiceOf(IServiceProvider sp, Guid reservation, CancellationToken ct) =>
			sp.GetRequiredService<DbContext>().Set<HotelInvoiceEntity>().Where(x => x.ReservationId == reservation && x.Tags.Contains(TagHotelInvoice.NotPaid)).OrderBy(x => x.CreatedAt).Select(x => x.Id).FirstOrDefaultAsync(ct);

		private async Task PayHotel(IServiceProvider sp, Guid reservation, string guest, CancellationToken ct) {
			Guid invoice = await InvoiceOf(sp, reservation, ct);
			if (invoice != Guid.Empty) Done(await sp.GetRequiredService<IHotelService>().PayHotelInvoiceInternal(new HotelInvoicePayParams { InvoiceId = invoice, UserId = _user[guest] }, ct), $"پرداخت آنلاین {Name(guest)}");
		}

		private async Task ReceiveHotel(IServiceProvider sp, string token, Guid reservation, Guid account, CancellationToken ct, decimal? amount = null) {
			Guid invoice = await InvoiceOf(sp, reservation, ct);
			if (invoice != Guid.Empty) Done(await sp.GetRequiredService<IHotelService>().ReceiveHotelInvoice(new InvoiceReceiveParams { Token = token, Id = invoice, AccountId = account, Amount = amount }, ct), "دریافت حضوری هتل");
		}

		private async Task Reservations(IServiceProvider sp, CancellationToken ct) {
			if (_place.GetValueOrDefault("h1") == Guid.Empty) return;
			IHotelService hs = sp.GetRequiredService<IHotelService>();
			string owner = await Token(sp, "o1", ct);
			Dictionary<string, Guid> accounts = await Accounts(sp, owner, _org["hotel"], ct);
			Guid cash = accounts.GetValueOrDefault("1101");
			Guid bank = accounts.GetValueOrDefault("1102");

			Guid r1 = await Reserve(sp, owner, "double", "g1", -40, -37, "201", TagHotelReservation.CheckedOut, ct, 2);
			await PayHotel(sp, r1, "g1", ct);
			Guid r2 = await Reserve(sp, owner, "single", "g2", -30, -28, "102", TagHotelReservation.CheckedOut, ct);
			await ReceiveHotel(sp, owner, r2, cash, ct);
			Guid r3 = await Reserve(sp, owner, "family", "g3", -21, -18, "401", TagHotelReservation.CheckedOut, ct, 4);
			await PayHotel(sp, r3, "g3", ct);
			Ok(await hs.CreateHotelInvoice(new HotelInvoiceCreateParams { Token = owner, ReservationId = r3, DebtAmount = 1_350_000, DueDate = D(-19), Tags = [TagHotelInvoice.NotPaid, TagHotelInvoice.Extra], Detail1 = "شام رستوران و سرویس لباسشویی" }, ct), "هزینه‌ی اضافه");
			await PayHotel(sp, r3, "g3", ct);
			Guid r4 = await Reserve(sp, owner, "double", "g4", -14, -12, "203", TagHotelReservation.CheckedOut, ct, 2);
			await ReceiveHotel(sp, owner, r4, bank, ct);
			Guid r5 = await Reserve(sp, owner, "suite", "g1", -9, -7, "301", TagHotelReservation.CheckedOut, ct, 2);
			await PayHotel(sp, r5, "g1", ct);
			Guid r6 = await Reserve(sp, owner, "double", "g5", -5, -3, "205", TagHotelReservation.CheckedOut, ct, 2);
			await ReceiveHotel(sp, owner, r6, cash, ct, 4_000_000);

			Guid r7 = await Reserve(sp, owner, "double", "g7", -1, 2, "206", TagHotelReservation.CheckedIn, ct, 2, [
				new ReservationGuestParams { FullName = Name("g7"), NationalCode = NationalCode(38), Gender = "مرد" },
				new ReservationGuestParams { FullName = "John Smith", Nationality = "بریتانیا", PassportNumber = "533812947", BirthDate = new DateTime(1979, 5, 12, 0, 0, 0, DateTimeKind.Utc), Gender = "مرد" }
			]);
			await PayHotel(sp, r7, "g7", ct);
			Done(await hs.ChangeHotelReservationRoom(new HotelReservationChangeRoomParams { Token = owner, Id = r7, RoomId = _room["suite"], KeepPrice = true, RoomNumber = "302" }, ct), "تغییر اتاق (ارتقای رایگان)");
			Guid r8 = await Reserve(sp, owner, "family", "g8", -2, 1, "402", TagHotelReservation.CheckedIn, ct, 3);
			await PayHotel(sp, r8, "g8", ct);
			Ok(await hs.CreateHotelInvoice(new HotelInvoiceCreateParams { Token = owner, ReservationId = r8, DebtAmount = 680_000, DueDate = D(0), Tags = [TagHotelInvoice.NotPaid, TagHotelInvoice.Extra], Detail1 = "مینی‌بار و سرویس اتاق" }, ct), "هزینه‌ی اضافه‌ی پرداخت‌نشده");
			Guid r9 = await Reserve(sp, owner, "single", "g2", 0, 3, "103", TagHotelReservation.CheckedIn, ct);
			Done(await hs.ExtendHotelReservation(new HotelReservationExtendParams { Token = owner, Id = r9, CheckOutDate = D(4, 8) }, ct), "تمدید اقامت");

			Guid r10 = await Reserve(sp, owner, "double", "g9", 5, 8, null, TagHotelReservation.Confirmed, ct, 2);
			await PayHotel(sp, r10, "g9", ct);
			await Reserve(sp, owner, "suite", "g10", 10, 12, null, TagHotelReservation.Confirmed, ct, 2);
			await Reserve(sp, owner, "single", "g4", 3, 4, null, TagHotelReservation.Pending, ct);
			await Reserve(sp, owner, "double", "g6", 15, 17, null, TagHotelReservation.Cancelled, ct, 2);
			await Reserve(sp, owner, "single", "g11", -1, 1, null, TagHotelReservation.Confirmed, ct);
			Ok(await hs.CreateHotelReservationGroup(new HotelReservationGroupParams {
				Token = owner, UserId = _user["g3"], CheckInDate = D(25, 10), CheckOutDate = D(27, 8), GroupName = "تور دانشجویی دانشگاه شریف", GuestPhone = Phone(Person("g3")),
				Rooms = [new HotelGroupRoomParams { RoomId = _room["double"], Count = 3, GuestCount = 2 }], Notes = "۶ نفر، صبحانه زودتر از ساعت ۷"
			}, ct), "رزرو گروهی");
			Done(await hs.CloseHotelNightAudit(new HotelNightAuditParams { Token = owner, HotelId = _place["h1"], Date = D(-1) }, ct), "بستن روز (ممیزی شبانه)");

			if (_place.GetValueOrDefault("h2") != Guid.Empty) {
				Guid i1 = await Reserve(sp, owner, "isfDouble", "g12", -12, -9, "12", TagHotelReservation.CheckedOut, ct, 2);
				await PayHotel(sp, i1, "g12", ct);
				Guid i2 = await Reserve(sp, owner, "isfSuite", "g5", -1, 3, "31", TagHotelReservation.CheckedIn, ct, 4);
				await ReceiveHotel(sp, owner, i2, cash, ct);
				await Reserve(sp, owner, "isfTriple", "g6", 30, 33, null, TagHotelReservation.Confirmed, ct, 3);
			}
		}

		private async Task<Guid> DormRoom(IDormService ds, string token, Guid dorm, string title, int beds, decimal rent, decimal deposit, int floor, double size, List<Guid> into, CancellationToken ct) {
			Guid? room = Ok(await ds.CreateDormRoom(new DormRoomCreateParams {
				Token = token, DormId = dorm, Title = title, Capacity = beds, Floor = floor, SizeSquareMeters = size, Description = $"{beds} تخته، طبقه‌ی {floor}",
				Tags = [beds == 1 ? TagDormRoom.Single : beds == 2 ? TagDormRoom.Double : TagDormRoom.Dorm, TagDormRoom.Furnished, TagDormRoom.Heating, TagDormRoom.Wardrobe, TagDormRoom.Desk]
			}, ct), $"اتاق {title}");
			if (room == null) return Guid.Empty;
			string[] letters = ["A", "B", "C", "D"];
			for (int i = 0; i < beds; i++) {
				Guid? bed = Ok(await ds.CreateDormBed(new DormBedCreateParams {
					Token = token, RoomId = room.Value, Title = letters[i], MonthlyRent = rent, Deposit = deposit,
					Tags = [beds > 2 ? (i % 2 == 0 ? TagDormBed.BunkBottom : TagDormBed.BunkTop) : TagDormBed.Single, TagDormBed.Desk, TagDormBed.Locker, TagDormBed.ReadingLamp]
				}, ct), $"تخت {title}{letters[i]}");
				if (bed != null) into.Add(bed.Value);
			}

			return room.Value;
		}

		private async Task Dorms(IServiceProvider sp, CancellationToken ct) {
			if (_org.GetValueOrDefault("dorm") == Guid.Empty) return;
			IDormService ds = sp.GetRequiredService<IDormService>();
			string owner = await Token(sp, "o2", ct);
			_place["d1"] = Ok(await ds.CreateDorm(new DormCreateParams {
				Token = owner, OrganizationId = _org["dorm"], Title = "خوابگاه دخترانه آرامش (نمونه)", CityCode = "108012",
				Address = "تهران، خیابان انقلاب، خیابان قدس، کوچه‌ی بزرگمهر، پلاک ۱۸", PhoneNumber = "02166403322",
				Description = "خوابگاه دخترانه‌ی مجهز، ۱۰ دقیقه پیاده تا دانشگاه تهران، با سرپرست مقیم و دوربین مداربسته.",
				Policies = "ورود آقایان ممنوع است. ملاقات فقط در سالن انتظار. ساعت ورود شبانه حداکثر ۲۲:۳۰.",
				Highlights = ["۱۰ دقیقه تا دانشگاه تهران", "اینترنت پرسرعت رایگان", "اتاق مطالعه‌ی ۲۴ ساعته"], Rules = ["رعایت سکوت از ساعت ۲۳", "پخت‌وپز فقط در آشپزخانه‌ی مشترک"],
				RequiredDocuments = ["تصویر کارت ملی", "گواهی اشتغال به تحصیل", "رضایت‌نامه‌ی ولی"], NearbyUniversity = "دانشگاه تهران", UniversityWalkMinutes = 10,
				VisitingHours = "۱۶ تا ۱۹", CurfewTime = "22:30", MinimumStayMonths = 6, Latitude = 35.7030, Longitude = 51.3950,
				LaundryMachines = ["ماشین ۱", "ماشین ۲"], LaundrySlotMinutes = 90,
				Tags = [TagDorm.Girls, TagDorm.Active, TagDorm.Featured, TagDorm.Approved, TagDorm.Bachelor, TagDorm.Master, TagDorm.Wifi, TagDorm.Elevator, TagDorm.SharedKitchen, TagDorm.Laundry,
					TagDorm.StudyRoom, TagDorm.PrayerRoom, TagDorm.Lounge, TagDorm.Lockers, TagDorm.Cctv, TagDorm.SecurityGuard, TagDorm.Supervisor, TagDorm.Breakfast, TagDorm.Dinner,
					TagDorm.InternetIncluded, TagDorm.UtilitiesIncluded]
			}, ct), "خوابگاه دخترانه") ?? Guid.Empty;
			_place["d2"] = Ok(await ds.CreateDorm(new DormCreateParams {
				Token = owner, OrganizationId = _org["dorm"], Title = "خوابگاه پسرانه آرامش مشهد (نمونه)", CityCode = "111062",
				Address = "مشهد، بلوار وکیل‌آباد، وکیل‌آباد ۱۵", PhoneNumber = "05138801122", Description = "خوابگاه پسرانه نزدیک دانشگاه فردوسی با سلف‌سرویس و پارکینگ دوچرخه.",
				NearbyUniversity = "دانشگاه فردوسی مشهد", UniversityWalkMinutes = 15, CurfewTime = "23:30", MinimumStayMonths = 3, LaundryMachines = ["ماشین ۱"], LaundrySlotMinutes = 60,
				Tags = [TagDorm.Boys, TagDorm.Active, TagDorm.Approved, TagDorm.Bachelor, TagDorm.Wifi, TagDorm.SelfService, TagDorm.Laundry, TagDorm.BikeParking, TagDorm.Gym, TagDorm.Lunch, TagDorm.InternetIncluded]
			}, ct), "خوابگاه پسرانه") ?? Guid.Empty;
			if (_place["d1"] == Guid.Empty) return;

			_room["d1.101"] = await DormRoom(ds, owner, _place["d1"], "اتاق ۱۰۱", 2, 6_500_000, 15_000_000, 1, 18, _d1Beds, ct);
			_room["d1.102"] = await DormRoom(ds, owner, _place["d1"], "اتاق ۱۰۲", 3, 5_200_000, 12_000_000, 1, 22, _d1Beds, ct);
			_room["d1.201"] = await DormRoom(ds, owner, _place["d1"], "اتاق ۲۰۱", 4, 4_300_000, 10_000_000, 2, 26, _d1Beds, ct);
			_room["d1.202"] = await DormRoom(ds, owner, _place["d1"], "اتاق ۲۰۲", 2, 6_500_000, 15_000_000, 2, 18, _d1Beds, ct);
			_room["d1.301"] = await DormRoom(ds, owner, _place["d1"], "اتاق ۳۰۱", 3, 5_200_000, 12_000_000, 3, 22, _d1Beds, ct);
			if (_place["d2"] != Guid.Empty) {
				_room["d2.1"] = await DormRoom(ds, owner, _place["d2"], "اتاق ۱", 3, 3_400_000, 8_000_000, 1, 20, _d2Beds, ct);
				_room["d2.2"] = await DormRoom(ds, owner, _place["d2"], "اتاق ۲", 2, 4_100_000, 9_000_000, 1, 16, _d2Beds, ct);
				_room["d2.3"] = await DormRoom(ds, owner, _place["d2"], "اتاق ۳", 4, 2_900_000, 7_000_000, 2, 24, _d2Beds, ct);
			}

			string admin = Admin(sp);
			foreach (string dorm in new[] { "d1", "d2" }.Where(x => _place[x] != Guid.Empty))
				Done(await ds.UpdateDorm(new DormUpdateParams { Token = admin, Id = _place[dorm], AddAdminUserIds = [_user["s5"], _user["s7"]] }, ct), "دسترسی کارکنان به خوابگاه");
		}

		private static async Task<List<(Guid Id, DateTime DueDate, bool Deposit)>> DormInvoices(IServiceProvider sp, Guid contract, CancellationToken ct) {
			var list = await sp.GetRequiredService<DbContext>().Set<DormBedInvoiceEntity>()
				.Where(x => x.ContractId == contract && x.Tags.Contains(TagDormBedInvoice.NotPaid))
				.OrderBy(x => x.DueDate)
				.Select(x => new { x.Id, x.DueDate, Deposit = x.Tags.Contains(TagDormBedInvoice.Deposit) })
				.ToListAsync(ct);
			return list.Select(x => (x.Id, x.DueDate, x.Deposit)).ToList();
		}

		private async Task Contracts(IServiceProvider sp, CancellationToken ct) {
			if (_d1Beds.Count == 0) return;
			IDormService ds = sp.GetRequiredService<IDormService>();
			string owner = await Token(sp, "o2", ct);
			Dictionary<string, Guid> accounts = await Accounts(sp, owner, _org["dorm"], ct);
			Guid cash = accounts.GetValueOrDefault("1101");

			(string Resident, Guid Bed, int Start, bool Late)[] residents = [
				("f9", _d1Beds[^1], -260, false),
				("f1", _d1Beds[0], -150, false), ("f2", _d1Beds[1], -120, false), ("f3", _d1Beds[2], -95, true), ("f4", _d1Beds[3], -70, false),
				("f5", _d1Beds[5], -45, false), ("f6", _d1Beds[6], -30, true), ("f7", _d1Beds[9], -12, false), ("f8", _d1Beds[10], -3, false)
			];
			if (_d2Beds.Count >= 6)
				residents = [..residents, ("m1", _d2Beds[0], -100, false), ("m2", _d2Beds[1], -80, true), ("m3", _d2Beds[3], -60, false), ("m4", _d2Beds[5], -40, false), ("m5", _d2Beds[6], -20, false), ("m6", _d2Beds[7], -5, false)];

			int n = 0;
			foreach ((string resident, Guid bed, int start, bool late) in residents) {
				n++;
				DemoPerson p = Person(resident);
				Guid? contract = Ok(await ds.CreateDormBedContract(new DormBedContractCreateParams {
					Token = owner, UserId = _user[resident], BedId = bed, StartDate = D(start), EndDate = D(start).AddMonths(12), PenaltyPrecentEveryDate = 1, Tags = [TagDormBedContract.Monthly],
					GuardianName = $"{(n % 2 == 0 ? "محمد" : "علی")} {p.LastName}", GuardianPhone = $"0913{3000000 + n * 1717:0000000}",
					EmergencyName = $"{(n % 2 == 0 ? "مریم" : "زهرا")} {p.LastName}", EmergencyPhone = $"0915{4000000 + n * 2323:0000000}", EmergencyRelation = n % 2 == 0 ? "مادر" : "خواهر",
					Detail1 = "قرارداد اجاره‌ی تخت به مدت یک سال"
				}, ct), $"قرارداد {Name(resident)}");
				if (contract == null) continue;
				_contract[resident] = contract.Value;

				List<(Guid Id, DateTime DueDate, bool Deposit)> invoices = await DormInvoices(sp, contract.Value, ct);
				List<(Guid Id, DateTime DueDate, bool Deposit)> due = invoices.Where(x => x.Deposit || x.DueDate <= DateTime.UtcNow).ToList();
				if (late && due.Count > 2) due = due.Take(due.Count - 1).ToList();
				int i = 0;
				foreach ((Guid id, DateTime _, bool _) in due) {
					i++;
					if (resident == "f2" && i == 3)
						Done(await ds.ReceiveDormBedInvoice(new InvoiceReceiveParams {
							Token = owner, Id = id, Check = new InvoiceReceiveCheckParams { Number = "476120", DueDate = D(20), Bank = "بانک ملت", SayadId = "1405123456789012", Drawer = $"محمد {p.LastName}" }
						}, ct), "دریافت قبض با چک");
					else if ((n + i) % 3 == 0) Done(await ds.ReceiveDormBedInvoice(new InvoiceReceiveParams { Token = owner, Id = id, AccountId = cash }, ct), "دریافت نقدی قبض");
					else Done(await ds.PayDormBedInvoice(new DormBedInvoicePayParams { InvoiceId = id, UserId = _user[resident] }, ct), $"پرداخت آنلاین قبض {Name(resident)}");
				}
			}

			if (_contract.TryGetValue("f1", out Guid f1))
				Done(await ds.SetDormBedContractChecklist(new DormBedContractChecklistParams {
					Token = owner, ContractId = f1, CheckOut = false, Items = [
						new HandoverItemParams { Title = "تخت و تشک", Ok = true, PhotoUrls = [] },
						new HandoverItemParams { Title = "کمد و کلید", Ok = true, PhotoUrls = [] },
						new HandoverItemParams { Title = "پرده و چراغ مطالعه", Ok = false, Note = "پرده لک دارد", PhotoUrls = [] }
					]
				}, ct), "چک‌لیست تحویل");
			if (_contract.TryGetValue("f9", out Guid f9))
				Done(await ds.SettleDormBedContract(new DormBedContractSettleParams { Token = owner, Id = f9, EndDate = D(-8), Deductions = 1_200_000, DeductionReason = "شکستگی قفل کمد و تعویض پرده" }, ct), "تسویه‌ی قرارداد ساکن سابق");
		}

		private async Task DormOperations(IServiceProvider sp, CancellationToken ct) {
			if (_place.GetValueOrDefault("d1") == Guid.Empty) return;
			IDormService ds = sp.GetRequiredService<IDormService>();
			DbContext db = sp.GetRequiredService<DbContext>();
			string owner = await Token(sp, "o2", ct);
			Guid d1 = _place["d1"];

			List<Guid> taken = await db.Set<DormBedContractEntity>().Where(x => x.EndDate >= DateTime.UtcNow).Select(x => x.BedId).ToListAsync(ct);
			Guid? freeBed = _d1Beds.Where(x => !taken.Contains(x)).Cast<Guid?>().FirstOrDefault();
			(string Applicant, Guid Dorm, TagDormApplication? Review, string? Note)[] applications = [
				("a1", d1, TagDormApplication.Approved, "مدارک کامل است، تخت تخصیص داده شد"),
				("a2", d1, TagDormApplication.Waitlisted, "فعلاً تخت خالی در اتاق دوتخته نداریم"),
				("a3", _place.GetValueOrDefault("d2", d1), TagDormApplication.Rejected, "گواهی اشتغال به تحصیل ارسال نشده است"),
				("a4", d1, null, null)
			];
			foreach ((string applicant, Guid dorm, TagDormApplication? review, string? note) in applications) {
				Guid? id = Ok(await ds.CreateDormApplication(new DormApplicationCreateParams {
					Token = await Token(sp, applicant, ct), DormId = dorm, DesiredStartDate = D(20), DesiredEndDate = D(20).AddMonths(6),
					Detail1 = "دانشجوی ورودی جدید، به دنبال تخت در اتاق دو یا سه‌تخته",
					Documents = [new DormApplicationDocumentParams { Title = "تصویر کارت ملی" }, new DormApplicationDocumentParams { Title = "گواهی اشتغال به تحصیل" }]
				}, ct), $"درخواست اقامت {Name(applicant)}");
				if (id == null || review == null) continue;
				Done(await ds.ReviewDormApplication(new DormApplicationReviewParams {
					Token = owner, Id = id.Value, Status = review.Value, ReviewNote = note, BedId = review == TagDormApplication.Approved ? freeBed : null,
					DocumentApprovals = review == TagDormApplication.Rejected ? [true, false] : [true, true]
				}, ct), "بررسی درخواست اقامت");
			}

			(TagDormRecord[] Tags, string Title, string? Body, string? User, int Day, int? EndDay, decimal? Penalty, string? Visitor, string? Relation, string? Room)[] records = [
				([TagDormRecord.Announcement], "قطعی آب، پنج‌شنبه ساعت ۱۰ تا ۱۴", "به دلیل تعمیر لوله‌کشی ساختمان، آب در این ساعت‌ها قطع است.", null, 0, null, null, null, null, null),
				([TagDormRecord.Announcement], "جلسه‌ی ماهانه‌ی ساکنان", "جمعه ساعت ۱۸ در سالن طبقه‌ی همکف.", null, -3, null, null, null, null, null),
				([TagDormRecord.Violation], "ورود دیرتر از ساعت مقرر", "ورود ساعت ۰۰:۱۵", "f2", -6, null, 300_000, null, null, null),
				([TagDormRecord.Warning], "رعایت نکردن سکوت شبانه", "اخطار کتبی اول", "f6", -4, null, null, null, null, null),
				([TagDormRecord.Inspection], "بازرسی دوره‌ای ماهانه", "وضعیت اتاق مرتب و بدون مشکل", null, -2, null, null, null, null, "d1.201"),
				([TagDormRecord.CheckIn], "ورود", null, "f1", 0, null, null, null, null, null),
				([TagDormRecord.CheckOut], "خروج", null, "f4", 0, null, null, null, null, null),
				([TagDormRecord.Visitor, TagDormRecord.Approved], "ملاقات خانواده", null, "f1", -2, null, null, "خانم صادقی", "مادر", null),
				([TagDormRecord.NightLeave, TagDormRecord.Approved], "مرخصی شبانه", "سفر به شهرستان", "f5", -10, -8, null, null, null, null)
			];
			foreach ((TagDormRecord[] tags, string title, string? body, string? user, int day, int? endDay, decimal? penalty, string? visitor, string? relation, string? room) in records)
				Ok(await ds.CreateDormRecord(new DormRecordCreateParams {
					Token = owner, DormId = d1, Tags = tags.ToList(), Title = title, Body = body, UserId = user == null ? null : _user[user], Date = D(day, 8), EndDate = endDay == null ? null : D(endDay.Value, 18),
					Penalty = penalty, VisitorName = visitor, Relation = relation, RoomId = room == null ? null : _room[room],
					Items = tags.Contains(TagDormRecord.Inspection) ? [new HandoverItemParams { Title = "نظافت", Ok = true, PhotoUrls = [] }, new HandoverItemParams { Title = "وسایل برقی غیرمجاز", Ok = true, PhotoUrls = [] }] : null
				}, ct), $"گزارش {title}");

			Ok(await ds.CreateDormRecord(new DormRecordCreateParams { Token = await Token(sp, "f4", ct), DormId = d1, Tags = [TagDormRecord.NightLeave], Title = "مرخصی شبانه", Body = "عروسی خواهرم در کرج", Date = D(2, 18), EndDate = D(3, 18) }, ct), "درخواست مرخصی ساکن");
			Ok(await ds.CreateDormRecord(new DormRecordCreateParams { Token = await Token(sp, "f7", ct), DormId = d1, Tags = [TagDormRecord.Visitor], Title = "ملاقات", VisitorName = "پدرم، آقای کریمی", Relation = "پدر", Date = D(1, 17) }, ct), "درخواست ملاقات ساکن");

			string[] lunches = ["چلوخورش قیمه", "زرشک‌پلو با مرغ", "عدس‌پلو", "چلوکباب کوبیده", "ماکارونی", "خورش قورمه‌سبزی", "سبزی‌پلو با ماهی"];
			string[] dinners = ["سوپ جو و نان", "کوکو سبزی", "الویه", "عدسی", "کتلت", "خوراک لوبیا", "املت"];
			List<Guid> meals = [];
			for (int i = 0; i < 7; i++) {
				Guid? lunch = Ok(await ds.CreateDormMeal(new DormMealCreateParams { Token = owner, DormId = d1, Title = lunches[i], Date = D(i, 9), Price = 180_000, Capacity = 40, Tags = [TagDormMeal.Lunch] }, ct), "غذای ناهار");
				Guid? dinner = Ok(await ds.CreateDormMeal(new DormMealCreateParams { Token = owner, DormId = d1, Title = dinners[i], Date = D(i, 16), Price = 120_000, Capacity = 40, Tags = [TagDormMeal.Dinner] }, ct), "غذای شام");
				if (lunch != null) meals.Add(lunch.Value);
				if (dinner != null) meals.Add(dinner.Value);
			}

			foreach ((string resident, int[] picks) in new[] { ("f1", new[] { 2, 3, 5 }), ("f2", new[] { 2, 4 }), ("f5", new[] { 3, 6, 7 }), ("f7", new[] { 2 }) }) {
				string token = await Token(sp, resident, ct);
				foreach (int i in picks.Where(x => x < meals.Count))
					Ok(await ds.CreateDormBooking(new DormBookingCreateParams { Token = token, DormId = d1, MealId = meals[i], Tags = [TagDormBooking.Meal] }, ct), "رزرو غذا");
			}

			foreach ((string resident, int hour, string machine) in new[] { ("f1", 8, "ماشین ۱"), ("f3", 10, "ماشین ۱"), ("f5", 8, "ماشین ۲"), ("f8", 14, "ماشین ۲") })
				Ok(await ds.CreateDormBooking(new DormBookingCreateParams {
					Token = await Token(sp, resident, ct), DormId = d1, StartAt = D(1, hour), EndAt = D(1, hour).AddMinutes(90), Resource = machine, Tags = [TagDormBooking.Laundry]
				}, ct), "نوبت لباسشویی");
		}

		private async Task<Dictionary<string, Guid>> Items(IInventoryService inv, string token, Guid org, (string Title, string Unit, string Code, decimal Min, TagInventoryItem Tag)[] items, CancellationToken ct) {
			Dictionary<string, Guid> result = [];
			foreach ((string title, string unit, string code, decimal min, TagInventoryItem tag) in items) {
				Guid? id = Ok(await inv.CreateItem(new InventoryItemCreateParams { Token = token, OrganizationId = org, Title = title, Unit = unit, Code = code, MinStock = min, Tags = [tag] }, ct), $"کالا {title}");
				if (id != null) result[title] = id.Value;
			}

			return result;
		}

		private async Task<Guid?> Purchase(IInventoryService inv, string token, Guid org, Guid warehouse, Guid? supplier, Dictionary<string, Guid> items, (string Item, decimal Quantity, decimal Price)[] lines, int day, string note, int stage, Guid? account, CancellationToken ct) {
			List<PurchaseLineParams> list = lines.Where(x => items.ContainsKey(x.Item)).Select(x => new PurchaseLineParams { ItemId = items[x.Item], Quantity = x.Quantity, UnitPrice = x.Price }).ToList();
			decimal total = list.Sum(x => x.Quantity * x.UnitPrice);
			Guid? id = Ok(await inv.CreatePurchase(new PurchaseCreateParams {
				Token = token, OrganizationId = org, WarehouseId = warehouse, SupplierId = supplier, Date = D(day), Detail1 = note, Lines = list, Vat = Math.Round(total * 0.1m)
			}, ct), $"درخواست خرید {note}");
			if (id == null || stage == 0) return id;
			Done(await inv.ReviewPurchase(new PurchaseReviewParams { Token = token, Id = id.Value, Approve = true, Note = "تأیید شد" }, ct), "تأیید خرید");
			if (stage > 1) Done(await inv.ReceivePurchase(new PurchaseReceiveParams { Token = token, Id = id.Value, AccountId = account, Date = D(day + 1) }, ct), "دریافت کالا");
			return id;
		}

		private async Task Move(IInventoryService inv, string token, Guid org, Guid item, Guid warehouse, decimal quantity, TagStockMovement tag, int day, string note, CancellationToken ct, Guid? target = null) =>
			Ok(await inv.CreateMovement(new StockMovementCreateParams {
				Token = token, OrganizationId = org, ItemId = item, WarehouseId = warehouse, TargetWarehouseId = target, Quantity = quantity, Date = D(day), Tags = [tag], Detail1 = note
			}, ct), $"حرکت انبار {note}");

		private async Task Inventory(IServiceProvider sp, CancellationToken ct) {
			IInventoryService inv = sp.GetRequiredService<IInventoryService>();

			if (_org.GetValueOrDefault("hotel") != Guid.Empty && _place.GetValueOrDefault("h1") != Guid.Empty) {
				Guid org = _org["hotel"];
				string token = await Token(sp, "o1", ct);
				Dictionary<string, Guid> accounts = await Accounts(sp, token, org, ct);
				Guid? main = Ok(await inv.CreateWarehouse(new WarehouseCreateParams { Token = token, OrganizationId = org, Title = "انبار مرکزی هتل آزادی", PlaceId = _place["h1"], KeeperId = _user["s3"], Address = "زیرزمین، ضلع شمالی", Tags = [TagWarehouse.Main] }, ct), "انبار مرکزی هتل");
				Guid? floor = Ok(await inv.CreateWarehouse(new WarehouseCreateParams { Token = token, OrganizationId = org, Title = "انبار خانه‌داری", PlaceId = _place["h1"], KeeperId = _user["s4"], Tags = [TagWarehouse.Branch] }, ct), "انبار خانه‌داری");
				Dictionary<string, Guid> items = await Items(inv, token, org, [
					("حوله‌ی حمام", "عدد", "HK-001", 60, TagInventoryItem.Consumable), ("ملحفه‌ی دونفره", "عدد", "HK-002", 40, TagInventoryItem.Consumable),
					("شامپو یک‌بارمصرف", "عدد", "HK-003", 200, TagInventoryItem.Cleaning), ("صابون یک‌بارمصرف", "عدد", "HK-004", 200, TagInventoryItem.Cleaning),
					("دستمال کاغذی", "بسته", "HK-005", 50, TagInventoryItem.Consumable), ("آب معدنی ۵۰۰ سی‌سی", "بطری", "FB-001", 150, TagInventoryItem.Food),
					("چای کیسه‌ای", "بسته", "FB-002", 20, TagInventoryItem.Food), ("مایع شوینده‌ی سطوح", "لیتر", "CL-001", 30, TagInventoryItem.Cleaning)
				], ct);
				Guid? sepid = Ok(await inv.CreateSupplier(new SupplierCreateParams { Token = token, OrganizationId = org, Title = "پخش بهداشتی سپید", PhoneNumber = "02133445566", ContactName = "آقای حسینی", Address = "تهران، بازار", NationalId = "10101234567", Iban = "IR120170000000123456789012", Tags = [TagSupplier.Active] }, ct), "تأمین‌کننده");
				Guid? textile = Ok(await inv.CreateSupplier(new SupplierCreateParams { Token = token, OrganizationId = org, Title = "نساجی ملحفه‌ی نو", PhoneNumber = "03133221100", ContactName = "خانم یزدانی", Tags = [TagSupplier.Active] }, ct), "تأمین‌کننده");
				Guid? food = Ok(await inv.CreateSupplier(new SupplierCreateParams { Token = token, OrganizationId = org, Title = "پخش مواد غذایی آوا", PhoneNumber = "02144556677", Tags = [TagSupplier.Active] }, ct), "تأمین‌کننده");
				if (main != null && floor != null) {
					await Purchase(inv, token, org, main.Value, textile, items, [("حوله‌ی حمام", 150, 180_000), ("ملحفه‌ی دونفره", 90, 450_000)], -60, "حوله و ملحفه‌ی فصل جدید", 2, accounts.GetValueOrDefault("1102"), ct);
					await Purchase(inv, token, org, main.Value, sepid, items, [("شامپو یک‌بارمصرف", 800, 9_000), ("صابون یک‌بارمصرف", 800, 6_000), ("دستمال کاغذی", 120, 35_000), ("مایع شوینده‌ی سطوح", 60, 85_000)], -30, "اقلام بهداشتی ماهانه", 2, accounts.GetValueOrDefault("1101"), ct);
					await Purchase(inv, token, org, main.Value, food, items, [("آب معدنی ۵۰۰ سی‌سی", 480, 12_000), ("چای کیسه‌ای", 60, 95_000)], -2, "آب و چای اتاق‌ها", 1, null, ct);
					await Purchase(inv, token, org, main.Value, sepid, items, [("دستمال کاغذی", 200, 36_000)], 0, "دستمال کاغذی اضافه", 0, null, ct);
					await Move(inv, token, org, items["حوله‌ی حمام"], main.Value, 50, TagStockMovement.TransferOut, -25, "انتقال به خانه‌داری", ct, floor.Value);
					await Move(inv, token, org, items["شامپو یک‌بارمصرف"], main.Value, 640, TagStockMovement.Out, -5, "مصرف اتاق‌ها", ct);
					await Move(inv, token, org, items["صابون یک‌بارمصرف"], main.Value, 520, TagStockMovement.Out, -5, "مصرف اتاق‌ها", ct);
					await Move(inv, token, org, items["مایع شوینده‌ی سطوح"], main.Value, 38, TagStockMovement.Out, -3, "نظافت روزانه", ct);
				}

				(string Title, string Code, string Location, string Room, TagAsset Kind, decimal Price, TagAsset Status)[] assets = [
					("تلویزیون ال‌جی ۴۳ اینچ", "H1-TV-201", "اتاق ۲۰۱", "double", TagAsset.Electronic, 18_500_000, TagAsset.InUse),
					("تلویزیون ال‌جی ۴۳ اینچ", "H1-TV-202", "اتاق ۲۰۲", "double", TagAsset.Electronic, 18_500_000, TagAsset.InUse),
					("یخچال کوچک ۹۰ لیتری", "H1-FR-301", "سوئیت ۳۰۱", "suite", TagAsset.Appliance, 12_000_000, TagAsset.InUse),
					("کولر گازی ۱۸ هزار", "H1-AC-209", "اتاق ۲۰۹", "double", TagAsset.Appliance, 32_000_000, TagAsset.InRepair),
					("مبلمان لابی", "H1-FU-001", "لابی", "", TagAsset.Furniture, 95_000_000, TagAsset.InUse)
				];
				foreach ((string title, string code, string location, string room, TagAsset kind, decimal price, TagAsset status) in assets)
					Ok(await inv.CreateAsset(new AssetCreateParams {
						Token = token, OrganizationId = org, Title = title, Code = code, PlaceId = _place["h1"], Location = location, RoomId = room == "" ? null : _room[room],
						SerialNumber = $"SN{code.Replace("-", "")}", PurchaseDate = D(-420), Price = price, Tags = [kind, status]
					}, ct), $"اموال {code}");
			}

			if (_org.GetValueOrDefault("dorm") != Guid.Empty && _place.GetValueOrDefault("d1") != Guid.Empty) {
				Guid org = _org["dorm"];
				string token = await Token(sp, "o2", ct);
				Dictionary<string, Guid> accounts = await Accounts(sp, token, org, ct);
				Guid? store = Ok(await inv.CreateWarehouse(new WarehouseCreateParams { Token = token, OrganizationId = org, Title = "انبار خوابگاه دخترانه", PlaceId = _place["d1"], KeeperId = _user["s7"], Tags = [TagWarehouse.Main] }, ct), "انبار خوابگاه");
				Guid? kitchen = Ok(await inv.CreateWarehouse(new WarehouseCreateParams { Token = token, OrganizationId = org, Title = "انبار آشپزخانه", PlaceId = _place["d1"], Tags = [TagWarehouse.Kitchen] }, ct), "انبار آشپزخانه");
				Dictionary<string, Guid> items = await Items(inv, token, org, [
					("پتو", "عدد", "DM-001", 10, TagInventoryItem.Equipment), ("بالش", "عدد", "DM-002", 10, TagInventoryItem.Equipment), ("لامپ ال‌ای‌دی", "عدد", "DM-003", 15, TagInventoryItem.Consumable),
					("پودر ماشین لباسشویی", "کیلوگرم", "DM-004", 10, TagInventoryItem.Cleaning), ("برنج ایرانی", "کیلوگرم", "KT-001", 50, TagInventoryItem.Food), ("روغن مایع", "لیتر", "KT-002", 20, TagInventoryItem.Food)
				], ct);
				Guid? supplier = Ok(await inv.CreateSupplier(new SupplierCreateParams { Token = token, OrganizationId = org, Title = "فروشگاه لوازم خانگی امید", PhoneNumber = "02166778899", Tags = [TagSupplier.Active] }, ct), "تأمین‌کننده");
				Guid? grocery = Ok(await inv.CreateSupplier(new SupplierCreateParams { Token = token, OrganizationId = org, Title = "عمده‌فروشی برنج و حبوبات نصر", PhoneNumber = "02155443322", Tags = [TagSupplier.Active] }, ct), "تأمین‌کننده");
				if (store != null && kitchen != null) {
					await Purchase(inv, token, org, store.Value, supplier, items, [("پتو", 30, 1_200_000), ("بالش", 30, 450_000), ("لامپ ال‌ای‌دی", 60, 95_000), ("پودر ماشین لباسشویی", 40, 160_000)], -90, "تجهیز ترم جدید", 2, accounts.GetValueOrDefault("1102"), ct);
					await Purchase(inv, token, org, kitchen.Value, grocery, items, [("برنج ایرانی", 200, 1_100_000), ("روغن مایع", 60, 210_000)], -20, "مواد غذایی ماهانه", 2, accounts.GetValueOrDefault("1101"), ct);
					await Move(inv, token, org, items["برنج ایرانی"], kitchen.Value, 165, TagStockMovement.Out, -2, "مصرف آشپزخانه", ct);
					await Move(inv, token, org, items["لامپ ال‌ای‌دی"], store.Value, 18, TagStockMovement.Out, -7, "تعویض لامپ اتاق‌ها", ct);
				}

				Ok(await inv.CreateAsset(new AssetCreateParams { Token = token, OrganizationId = org, Title = "ماشین لباسشویی ال‌جی ۹ کیلویی", Code = "D1-WM-01", PlaceId = _place["d1"], Location = "رختشوی‌خانه (ماشین ۱)", PurchaseDate = D(-500), Price = 38_000_000, Tags = [TagAsset.Appliance, TagAsset.InUse] }, ct), "اموال ماشین لباسشویی");
				Ok(await inv.CreateAsset(new AssetCreateParams { Token = token, OrganizationId = org, Title = "ماشین لباسشویی ال‌جی ۹ کیلویی", Code = "D1-WM-02", PlaceId = _place["d1"], Location = "رختشوی‌خانه (ماشین ۲)", PurchaseDate = D(-500), Price = 38_000_000, Tags = [TagAsset.Appliance, TagAsset.InUse] }, ct), "اموال ماشین لباسشویی");
				Ok(await inv.CreateAsset(new AssetCreateParams { Token = token, OrganizationId = org, Title = "یخچال ساید آشپزخانه", Code = "D1-FR-01", PlaceId = _place["d1"], Location = "آشپزخانه‌ی مشترک", PurchaseDate = D(-300), Price = 85_000_000, Tags = [TagAsset.Appliance, TagAsset.InUse] }, ct), "اموال یخچال");
				Ok(await inv.CreateAsset(new AssetCreateParams { Token = token, OrganizationId = org, Title = "تخت دوطبقه‌ی فلزی", Code = "D1-BD-201", PlaceId = _place["d1"], Location = "اتاق ۲۰۱", RoomId = _room["d1.201"], PurchaseDate = D(-700), Price = 14_000_000, Tags = [TagAsset.Furniture, TagAsset.InUse] }, ct), "اموال تخت");
			}

			if (_org.GetValueOrDefault("cafe") != Guid.Empty) {
				Guid org = _org["cafe"];
				string token = await Token(sp, "o3", ct);
				Dictionary<string, Guid> accounts = await Accounts(sp, token, org, ct);
				Guid? store = Ok(await inv.CreateWarehouse(new WarehouseCreateParams { Token = token, OrganizationId = org, Title = "انبار خشک کافه", KeeperId = _user["s8"], Address = "پشت آشپزخانه", Tags = [TagWarehouse.Main] }, ct), "انبار کافه");
				Guid? fridge = Ok(await inv.CreateWarehouse(new WarehouseCreateParams { Token = token, OrganizationId = org, Title = "یخچال و سردخانه", Tags = [TagWarehouse.Kitchen] }, ct), "سردخانه‌ی کافه");
				Dictionary<string, Guid> items = await Items(inv, token, org, [
					("دانه‌ی قهوه‌ی اسپرسو (۷۰ عربیکا)", "کیلوگرم", "CF-001", 5, TagInventoryItem.Food), ("شیر پرچرب", "لیتر", "CF-002", 20, TagInventoryItem.Food),
					("شکر", "کیلوگرم", "CF-003", 5, TagInventoryItem.Food), ("لیوان کاغذی ۸ اونس", "عدد", "CF-004", 500, TagInventoryItem.Consumable),
					("سیروپ کارامل", "بطری", "CF-005", 3, TagInventoryItem.Food), ("کیک شکلاتی", "برش", "CF-006", 10, TagInventoryItem.Food),
					("چای سیاه ممتاز", "کیلوگرم", "CF-007", 2, TagInventoryItem.Food), ("مرغ فیله", "کیلوگرم", "CF-008", 8, TagInventoryItem.Food)
				], ct);
				Guid? roastery = Ok(await inv.CreateSupplier(new SupplierCreateParams { Token = token, OrganizationId = org, Title = "رُست قهوه‌ی آرمان", PhoneNumber = "07136112233", ContactName = "آقای فرهمند", Iban = "IR450120000000987654321001", Tags = [TagSupplier.Active] }, ct), "تأمین‌کننده");
				Guid? dairy = Ok(await inv.CreateSupplier(new SupplierCreateParams { Token = token, OrganizationId = org, Title = "پخش لبنیات فارس", PhoneNumber = "07137223344", Tags = [TagSupplier.Active] }, ct), "تأمین‌کننده");
				Guid? packaging = Ok(await inv.CreateSupplier(new SupplierCreateParams { Token = token, OrganizationId = org, Title = "بسته‌بندی سبز", PhoneNumber = "07138334455", Tags = [TagSupplier.Active] }, ct), "تأمین‌کننده");
				if (store != null && fridge != null) {
					await Purchase(inv, token, org, store.Value, roastery, items, [("دانه‌ی قهوه‌ی اسپرسو (۷۰ عربیکا)", 20, 4_200_000), ("چای سیاه ممتاز", 5, 1_800_000)], -25, "قهوه و چای ماهانه", 2, accounts.GetValueOrDefault("1102"), ct);
					await Purchase(inv, token, org, fridge.Value, dairy, items, [("شیر پرچرب", 120, 48_000), ("کیک شکلاتی", 60, 150_000), ("مرغ فیله", 30, 420_000)], -6, "لبنیات و مواد تازه", 2, accounts.GetValueOrDefault("1101"), ct);
					await Purchase(inv, token, org, store.Value, packaging, items, [("لیوان کاغذی ۸ اونس", 2000, 3_200), ("شکر", 20, 75_000), ("سیروپ کارامل", 10, 650_000)], -15, "بسته‌بندی و افزودنی", 2, accounts.GetValueOrDefault("1101"), ct);
					await Purchase(inv, token, org, store.Value, roastery, items, [("دانه‌ی قهوه‌ی اسپرسو (۷۰ عربیکا)", 15, 4_300_000)], 0, "سفارش قهوه‌ی هفته‌ی آینده", 0, null, ct);
					await Move(inv, token, org, items["دانه‌ی قهوه‌ی اسپرسو (۷۰ عربیکا)"], store.Value, 17, TagStockMovement.Out, -1, "مصرف بار قهوه", ct);
					await Move(inv, token, org, items["شیر پرچرب"], fridge.Value, 108, TagStockMovement.Out, -1, "مصرف بار قهوه", ct);
					await Move(inv, token, org, items["لیوان کاغذی ۸ اونس"], store.Value, 1650, TagStockMovement.Out, -1, "سفارش‌های بیرون‌بر", ct);
					await Move(inv, token, org, items["کیک شکلاتی"], fridge.Value, 54, TagStockMovement.Out, -1, "فروش کیک", ct);
					await Move(inv, token, org, items["مرغ فیله"], fridge.Value, 12, TagStockMovement.Out, -1, "منوی ناهار", ct);
				}

				Ok(await inv.CreateAsset(new AssetCreateParams { Token = token, OrganizationId = org, Title = "دستگاه اسپرسوساز دو گروپ", Code = "CF-ES-01", Location = "بار قهوه", SerialNumber = "LM2G-88231", PurchaseDate = D(-365), Price = 650_000_000, Tags = [TagAsset.Appliance, TagAsset.InUse] }, ct), "اموال اسپرسوساز");
				Ok(await inv.CreateAsset(new AssetCreateParams { Token = token, OrganizationId = org, Title = "آسیاب قهوه‌ی الکترونیکی", Code = "CF-GR-01", Location = "بار قهوه", PurchaseDate = D(-365), Price = 120_000_000, Tags = [TagAsset.Appliance, TagAsset.InUse] }, ct), "اموال آسیاب");
				Ok(await inv.CreateAsset(new AssetCreateParams { Token = token, OrganizationId = org, Title = "یخچال ویترینی کیک", Code = "CF-FR-01", Location = "سالن", PurchaseDate = D(-200), Price = 95_000_000, Tags = [TagAsset.Appliance, TagAsset.InRepair] }, ct), "اموال یخچال ویترینی");
				Ok(await inv.CreateAsset(new AssetCreateParams { Token = token, OrganizationId = org, Title = "میز و صندلی چوبی (۱۲ دست)", Code = "CF-FU-01", Location = "سالن", PurchaseDate = D(-400), Price = 180_000_000, Tags = [TagAsset.Furniture, TagAsset.InUse] }, ct), "اموال میز و صندلی");
			}
		}

		private async Task Voucher(IAccountingService acc, string token, Guid org, Dictionary<string, Guid> accounts, TagVoucher tag, int day, string description, string debit, string credit, decimal amount, CancellationToken ct) {
			if (!accounts.ContainsKey(debit) || !accounts.ContainsKey(credit)) return;
			Ok(await acc.CreateVoucher(new VoucherCreateParams {
				Token = token, OrganizationId = org, Date = D(day), Tags = [tag], Detail1 = description,
				Lines = [new VoucherLineParams { AccountId = accounts[debit], Debit = amount, Description = description }, new VoucherLineParams { AccountId = accounts[credit], Credit = amount, Description = description }]
			}, ct), $"سند {description}");
		}

		private async Task Accounting(IServiceProvider sp, CancellationToken ct) {
			IAccountingService acc = sp.GetRequiredService<IAccountingService>();
			(string Org, string Owner, decimal Capital, decimal Salary, decimal Utilities, decimal Rent)[] books = [
				("hotel", "o1", 2_000_000_000, 380_000_000, 65_000_000, 0),
				("dorm", "o2", 800_000_000, 140_000_000, 28_000_000, 0),
				("cafe", "o3", 400_000_000, 75_000_000, 12_000_000, 55_000_000)
			];
			foreach ((string org, string owner, decimal capital, decimal salary, decimal utilities, decimal rent) in books) {
				if (_org.GetValueOrDefault(org) == Guid.Empty) continue;
				string token = await Token(sp, owner, ct);
				Dictionary<string, Guid> accounts = await Accounts(sp, token, _org[org], ct);
				await Voucher(acc, token, _org[org], accounts, TagVoucher.Opening, -180, "سرمایه‌ی اولیه", "1102", "3101", capital, ct);
				await Voucher(acc, token, _org[org], accounts, TagVoucher.Transfer, -175, "شارژ صندوق از بانک", "1101", "1102", capital / 20, ct);
				for (int m = 3; m >= 1; m--) {
					await Voucher(acc, token, _org[org], accounts, TagVoucher.Expense, -30 * m + 2, $"حقوق کارکنان ({m} ماه پیش)", "5201", "1102", salary, ct);
					await Voucher(acc, token, _org[org], accounts, TagVoucher.Expense, -30 * m + 5, $"قبض آب، برق و گاز ({m} ماه پیش)", "5202", "1102", utilities + m * 1_500_000, ct);
					if (rent > 0) await Voucher(acc, token, _org[org], accounts, TagVoucher.Expense, -30 * m + 1, $"اجاره‌ی مغازه ({m} ماه پیش)", "5206", "1102", rent, ct);
				}

				await Voucher(acc, token, _org[org], accounts, TagVoucher.Expense, -12, "تعمیر آبگرمکن و لوله‌کشی", "5203", "1101", org == "cafe" ? 6_500_000 : 18_000_000, ct);
				await Voucher(acc, token, _org[org], accounts, TagVoucher.Expense, -8, "خرید مواد نظافت از بازار", "5205", "1101", 4_200_000, ct);

				Guid? cleared = Ok(await acc.CreateCheck(new CheckCreateParams {
					Token = token, OrganizationId = _org[org], Amount = org == "hotel" ? 45_000_000 : 18_000_000, DueDate = D(-6), Number = "551203", Bank = "بانک ملی", SayadId = "1405998877665544",
					Drawer = org == "hotel" ? "شرکت سفرهای آسمان آبی" : "آقای رحمانی", AccountId = accounts.GetValueOrDefault("1201"), Tags = [TagCheck.Received, TagCheck.Pending], Detail1 = "پیش‌پرداخت"
				}, ct), "چک دریافتی وصول‌شده");
				if (cleared != null) Done(await acc.SetCheckStatus(new CheckStatusParams { Token = token, Id = cleared.Value, Status = TagCheck.Cleared, AccountId = accounts.GetValueOrDefault("1102"), Date = D(-5) }, ct), "وصول چک");
				Guid? bounced = Ok(await acc.CreateCheck(new CheckCreateParams {
					Token = token, OrganizationId = _org[org], Amount = 12_500_000, DueDate = D(-15), Number = "330918", Bank = "بانک صادرات", Drawer = "آقای پیمان عزیزی", PersonId = _user["g11"],
					AccountId = accounts.GetValueOrDefault("1201"), Tags = [TagCheck.Received, TagCheck.Pending], Detail1 = "بابت بدهی"
				}, ct), "چک برگشتی");
				if (bounced != null) Done(await acc.SetCheckStatus(new CheckStatusParams { Token = token, Id = bounced.Value, Status = TagCheck.Bounced, Date = D(-14) }, ct), "برگشت چک");
				Ok(await acc.CreateCheck(new CheckCreateParams {
					Token = token, OrganizationId = _org[org], Amount = org == "cafe" ? 35_000_000 : 60_000_000, DueDate = D(12), Number = "100245", Bank = "بانک ملت",
					Drawer = "خودمان", AccountId = accounts.GetValueOrDefault("2103"), Tags = [TagCheck.Issued, TagCheck.Pending], Detail1 = "پرداخت به تأمین‌کننده"
				}, ct), "چک پرداختی");
				Ok(await acc.CreateCheck(new CheckCreateParams {
					Token = token, OrganizationId = _org[org], Amount = 30_000_000, DueDate = D(25), Number = "887102", Bank = "بانک تجارت", Drawer = "شرکت پخش نوین",
					AccountId = accounts.GetValueOrDefault("1201"), Tags = [TagCheck.Received, TagCheck.Pending], Detail1 = "چک مدت‌دار"
				}, ct), "چک دریافتی سررسید آینده");
			}

			if (_org.GetValueOrDefault("dorm") != Guid.Empty && _contract.TryGetValue("f4", out Guid f4)) {
				string token = await Token(sp, "o2", ct);
				Ok(await acc.CreateCheck(new CheckCreateParams {
					Token = token, OrganizationId = _org["dorm"], Amount = 50_000_000, DueDate = D(300), Number = "662190", Bank = "بانک سپه", Drawer = $"محمد {Person("f4").LastName}",
					PersonId = _user["f4"], ContractId = f4, PlaceId = _place.GetValueOrDefault("d1"), Tags = [TagCheck.Guarantee, TagCheck.Pending], Detail1 = "چک ضمانت قرارداد"
				}, ct), "چک ضمانت");
			}

			if (_org.GetValueOrDefault("hotel") != Guid.Empty) {
				decimal balance = await sp.GetRequiredService<DbContext>().Set<WalletEntity>().Where(x => x.CreatorId == _org["hotel"]).Select(x => x.Balance).FirstOrDefaultAsync(ct);
				if (balance > 2_000_000)
					Done(await acc.RequestOrganizationSettlement(new OrganizationSettlementRequestParams {
						Token = await Token(sp, "o1", ct), OrganizationId = _org["hotel"], Amount = Math.Floor(balance / 2_000_000) * 1_000_000, Iban = "IR820540102680020817909002"
					}, ct), "درخواست تسویه با مجموعه");
			}
		}

		private async Task Staff(IServiceProvider sp, CancellationToken ct) {
			IOrganizationService os = sp.GetRequiredService<IOrganizationService>();
			(string Org, string Owner, string Staff, string? Place, TagStaffShift Kind, int Start, int Hours)[] shifts = [
				("hotel", "o1", "s1", "h1", TagStaffShift.Morning, 7, 8), ("hotel", "o1", "s4", "h1", TagStaffShift.Evening, 15, 8),
				("dorm", "o2", "s5", "d1", TagStaffShift.Morning, 8, 8), ("dorm", "o2", "s7", "d1", TagStaffShift.Night, 22, 9)
			];
			foreach ((string org, string owner, string staff, string? place, TagStaffShift kind, int start, int hours) in shifts) {
				if (_org.GetValueOrDefault(org) == Guid.Empty) continue;
				string token = await Token(sp, owner, ct);
				for (int day = -2; day <= 4; day++) {
					Guid? shift = Ok(await os.CreateShift(new StaffShiftCreateParams {
						Token = token, OrganizationId = _org[org], UserId = _user[staff], StartAt = D(day, start), EndAt = D(day, start + hours), PlaceId = place == null ? null : _place.GetValueOrDefault(place),
						Tags = [kind, day < 0 ? (day == -2 && staff == "s4" ? TagStaffShift.Absent : TagStaffShift.Present) : TagStaffShift.Planned]
					}, ct), $"شیفت {Name(staff)}");
					if (shift != null && day == 0 && kind != TagStaffShift.Night) Done(await os.ClockShift(new IdParams { Token = await Token(sp, staff, ct), Id = shift.Value }, ct), "ثبت ورود شیفت");
				}
			}

			(string Org, string Owner, string Title, string? Place, string? Assignee, TagStaffTask[] Tags, int Due, string? Description, string? Location)[] tasks = [
				("hotel", "o1", "تعمیر کولر اتاق ۲۰۹", "h1", "s4", [TagStaffTask.Maintenance, TagStaffTask.InProgress, TagStaffTask.Urgent], 1, "کمپرسور صدا می‌دهد، تعمیرکار در راه است", "اتاق ۲۰۹"),
				("hotel", "o1", "نظافت عمیق سوئیت‌ها قبل از بازسازی", "h1", "s4", [TagStaffTask.Cleaning, TagStaffTask.Open, TagStaffTask.Normal], 18, null, "طبقه‌ی سوم"),
				("hotel", "o1", "سرویس دوره‌ای آسانسور", "h1", null, [TagStaffTask.Maintenance, TagStaffTask.Done, TagStaffTask.Normal], -4, "انجام شد توسط شرکت آسانسور البرز", null),
				("hotel", "o1", "سفارش حوله‌ی جدید", "h1", "s3", [TagStaffTask.Task, TagStaffTask.Open, TagStaffTask.Low], 10, null, null),
				("dorm", "o2", "سم‌پاشی دوره‌ای ساختمان", "d1", "s7", [TagStaffTask.Task, TagStaffTask.Open, TagStaffTask.Normal], 7, "هماهنگی با ساکنان برای تخلیه‌ی آشپزخانه", null),
				("dorm", "o2", "تعویض قفل کمد اتاق ۲۰۲", "d1", "s7", [TagStaffTask.Maintenance, TagStaffTask.Done, TagStaffTask.Normal], -7, null, "اتاق ۲۰۲")
			];
			foreach ((string org, string owner, string title, string? place, string? assignee, TagStaffTask[] tags, int due, string? description, string? location) in tasks) {
				if (_org.GetValueOrDefault(org) == Guid.Empty) continue;
				Ok(await os.CreateTask(new StaffTaskCreateParams {
					Token = await Token(sp, owner, ct), OrganizationId = _org[org], Title = title, PlaceId = place == null ? null : _place.GetValueOrDefault(place),
					AssigneeId = assignee == null ? null : _user[assignee], DueDate = D(due, 12), Description = description, Location = location, Tags = tags.ToList()
				}, ct), $"وظیفه {title}");
			}

			if (_place.GetValueOrDefault("d1") != Guid.Empty)
				Ok(await os.CreateTask(new StaffTaskCreateParams {
					Token = await Token(sp, "f6", ct), PlaceId = _place["d1"], Title = "چکه کردن شیر آب دستشویی", Description = "از دیشب شیر آب دستشویی اتاق ۲۰۱ چکه می‌کند", Tags = [TagStaffTask.Maintenance, TagStaffTask.Normal]
				}, ct), "درخواست تعمیر ساکن");
			if (_place.GetValueOrDefault("h1") != Guid.Empty)
				Ok(await os.CreateTask(new StaffTaskCreateParams {
					Token = await Token(sp, "g8", ct), PlaceId = _place["h1"], Title = "تلویزیون اتاق روشن نمی‌شود", Description = "اتاق ۴۰۲", Tags = [TagStaffTask.Maintenance, TagStaffTask.Urgent]
				}, ct), "درخواست تعمیر مهمان");

			(string Org, string Owner, string User, TagOrganizationCustomer Tag, string Note)[] customers = [
				("hotel", "o1", "g1", TagOrganizationCustomer.Vip, "مهمان همیشگی، اتاق رو به کوه ترجیح می‌دهد"),
				("hotel", "o1", "g3", TagOrganizationCustomer.Vip, "مدیر تورهای دانشجویی، تخفیف گروهی دارد"),
				("hotel", "o1", "g11", TagOrganizationCustomer.Blacklisted, "چک برگشتی و خسارت به اتاق در اقامت قبلی"),
				("dorm", "o2", "f1", TagOrganizationCustomer.Vip, "ساکن قدیمی و مسئول طبقه"),
				("dorm", "o2", "a3", TagOrganizationCustomer.Blacklisted, "ارائه‌ی مدرک جعلی")
			];
			foreach ((string org, string owner, string user, TagOrganizationCustomer tag, string note) in customers) {
				if (_org.GetValueOrDefault(org) == Guid.Empty) continue;
				Done(await os.SetCustomer(new OrganizationCustomerSetParams { Token = await Token(sp, owner, ct), OrganizationId = _org[org], UserId = _user[user], Tags = [tag], Note = note }, ct), $"مشتری {Name(user)}");
			}
		}

		private async Task ExpireLodge(IServiceProvider sp, CancellationToken ct) {
			if (_org.GetValueOrDefault("lodge") == Guid.Empty) return;
			DbContext db = sp.GetRequiredService<DbContext>();
			OrganizationEntity org = await db.Set<OrganizationEntity>().AsTracking().FirstAsync(x => x.Id == _org["lodge"], ct);
			foreach (OrganizationSubscription s in org.JsonData.Subscriptions.Where(x => x.Status == TagSubscription.Active)) {
				s.StartsAt = D(-30);
				s.ExpiresAt = D(-16);
			}

			org.JsonData.Subscriptions = org.JsonData.Subscriptions.ToList();
			await db.SaveChangesAsync(ct);
		}

		private async Task Summary(IServiceProvider sp, CancellationToken ct) {
			DbContext db = sp.GetRequiredService<DbContext>();
			List<Guid> orgs = _org.Values.Where(x => x != Guid.Empty).ToList();
			List<Guid> contracts = _contract.Values.ToList();
			Report.Add(new KeyValue { Key = "رمز عبور همه‌ی حساب‌های نمونه", Value = DemoPassword });
			Report.Add(new KeyValue { Key = "نام کاربری", Value = "همان شماره‌ی موبایل (برای پنل، سایت و اپ)" });
			foreach (DemoPerson p in People.Where(x => x.Key.StartsWith('o') || x.Key.StartsWith('s')))
				Report.Add(new KeyValue { Key = $"{p.Role} — {p.FirstName} {p.LastName}", Value = Phone(p) });
			foreach (string key in new[] { "f1", "f3", "f9", "m1", "a2", "a4", "g7", "g9", "g11" })
				Report.Add(new KeyValue { Key = $"{Person(key).Role} — {Name(key)}", Value = Phone(Person(key)) });

			Report.Add(new KeyValue { Key = "مجموعه‌ها", Value = orgs.Count.ToString() });
			Report.Add(new KeyValue { Key = "هتل‌ها و اقامتگاه‌ها", Value = (await db.Set<HotelEntity>().CountAsync(x => x.OrganizationId != null && orgs.Contains(x.OrganizationId.Value), ct)).ToString() });
			Report.Add(new KeyValue { Key = "رزروهای هتل", Value = (await db.Set<HotelReservationEntity>().CountAsync(x => x.Hotel.OrganizationId != null && orgs.Contains(x.Hotel.OrganizationId.Value), ct)).ToString() });
			Report.Add(new KeyValue { Key = "خوابگاه‌ها", Value = (await db.Set<DormEntity>().CountAsync(x => x.OrganizationId != null && orgs.Contains(x.OrganizationId.Value), ct)).ToString() });
			Report.Add(new KeyValue { Key = "قراردادهای خوابگاه", Value = _contract.Count.ToString() });
			Report.Add(new KeyValue { Key = "قبض‌های خوابگاه", Value = (await db.Set<DormBedInvoiceEntity>().CountAsync(x => x.ContractId != null && contracts.Contains(x.ContractId.Value), ct)).ToString() });
			Report.Add(new KeyValue { Key = "سندهای حسابداری", Value = (await db.Set<VoucherEntity>().CountAsync(x => orgs.Contains(x.OrganizationId), ct)).ToString() });
			Report.Add(new KeyValue { Key = "کالاهای انبار", Value = (await db.Set<InventoryItemEntity>().CountAsync(x => orgs.Contains(x.OrganizationId), ct)).ToString() });
			Report.Add(new KeyValue { Key = "وظایف کارکنان", Value = (await db.Set<StaffTaskEntity>().CountAsync(x => orgs.Contains(x.OrganizationId), ct)).ToString() });
			Report.Add(new KeyValue { Key = "تعداد خطاها", Value = _errors.Count.ToString() });
			Report.AddRange(_errors.Select((x, i) => new KeyValue { Key = $"خطا {i + 1}", Value = x }));
		}
	}
}
