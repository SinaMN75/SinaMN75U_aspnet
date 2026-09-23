namespace SinaMN75U.Services;

public interface IDataSeedService {
	Task<UResponse> SeedUsers();
	Task<UResponse> SeedCategories();
	Task<UResponse> SeedContents();
	Task<UResponse<List<KeyValue>?>> SeedHotelsAndDorms(CancellationToken ct = default);
}

public class DataSeedService(DbContext db) : IDataSeedService {
	public async Task<UResponse> SeedUsers() {
		await db.Set<UserEntity>().AddRangeAsync(Core.App.Users.SystemAdmin, Core.App.Users.ITHub, Core.App.Users.AvaPlus, Core.App.Users.Mobtakeran);
		await db.SaveChangesAsync();
		return new UResponse();
	}

	public async Task<UResponse> SeedCategories() {
		List<CategoryEntity> categories = [
			new() {
				Id = Guid.CreateVersion7(),
				CreatedAt = DateTime.UtcNow,
				JsonData = new CategoryJson {
					Detail1 = "توضیحات کامل برای دسته بندی محصولات الکترونیکی",
					Detail2 = "شامل گوشی، لپ تاپ، تبلت و لوازم جانبی",
					Subtitle = "جدیدترین تکنولوژی‌ها",
					Link = "/categories/electronics",
					Location = "الکترونیک",
					Type = "product",
					Address = "فروشگاه مرکزی",
					PhoneNumber = "021-12345678"
				},
				Tags = [TagCategory.Category],
				CreatorId = Core.App.Users.SystemAdmin.Id,
				Title = "الکترونیک",
				Order = 1,
				Code = "ELEC-001"
			},

			new() {
				Id = Guid.CreateVersion7(),
				CreatedAt = DateTime.UtcNow,
				JsonData = new CategoryJson {
					Detail1 = "انواع پوشاک مردانه، زنانه و بچگانه",
					Detail2 = "برندهای معتبر داخلی و خارجی",
					Subtitle = "مد و پوشاک روز",
					Link = "/categories/clothing",
					Location = "مد و پوشاک",
					Type = "product",
					Address = "فروشگاه شماره ۲",
					PhoneNumber = "021-87654321"
				},
				Tags = [TagCategory.Category],
				CreatorId = Core.App.Users.SystemAdmin.Id,
				Title = "پوشاک",
				Order = 2,
				Code = "CLTH-002"
			},

			new() {
				Id = Guid.CreateVersion7(),
				CreatedAt = DateTime.UtcNow,
				JsonData = new CategoryJson {
					Detail1 = "کتاب‌های آموزشی، رمان و مجلات",
					Detail2 = "تخفیف ویژه برای کتاب‌های پرفروش",
					Subtitle = "فروشگاه آنلاین کتاب",
					Link = "/categories/books",
					Location = "کتاب و مجله",
					Type = "product",
					Address = "فروشگاه کتاب مرکزی",
					PhoneNumber = "021-98765432"
				},
				Tags = [TagCategory.Category],
				CreatorId = Core.App.Users.SystemAdmin.Id,
				Title = "کتاب و مجله",
				Order = 3,
				Code = "BOOK-003"
			},

			new() {
				Id = Guid.CreateVersion7(),
				CreatedAt = DateTime.UtcNow,
				JsonData = new CategoryJson {
					Detail1 = "لوازم دکوراتیو و مبلمان منزل",
					Detail2 = "طراحی داخلی مدرن و کلاسیک",
					Subtitle = "خانه زیبای شما",
					Link = "/categories/home-decor",
					Location = "خانه و آشپزخانه",
					Type = "product",
					Address = "شعبه ونک",
					PhoneNumber = "021-45678912"
				},
				Tags = [TagCategory.Category],
				CreatorId = Core.App.Users.SystemAdmin.Id,
				Title = "خانه و دکوراسیون",
				Order = 4,
				Code = "HOME-004"
			},

			new() {
				Id = Guid.CreateVersion7(),
				CreatedAt = DateTime.UtcNow,
				JsonData = new CategoryJson {
					Detail1 = "لوازم آرایشی و بهداشتی اصل",
					Detail2 = "محصولات ارگانیک و ضد حساسیت",
					Subtitle = "زیبایی و سلامت",
					Link = "/categories/beauty",
					Location = "زیبایی",
					Type = "product",
					Address = "مرکز خرید پاساژ",
					PhoneNumber = "021-74185296"
				},
				Tags = [TagCategory.Category],
				CreatorId = Core.App.Users.SystemAdmin.Id,
				Title = "آرایشی و بهداشتی",
				Order = 5,
				Code = "BEAU-005"
			},

			new() {
				Id = Guid.CreateVersion7(),
				CreatedAt = DateTime.UtcNow,
				JsonData = new CategoryJson {
					Detail1 = "لوازم ورزشی و بدنسازی",
					Detail2 = "تجهیزات حرفه‌ای و آماتور",
					Subtitle = "سلامت با ورزش",
					Link = "/categories/sports",
					Location = "ورزش و سفر",
					Type = "product",
					Address = "باشگاه ورزشی",
					PhoneNumber = "021-36985214"
				},
				Tags = [TagCategory.Category],
				CreatorId = Core.App.Users.SystemAdmin.Id,
				Title = "ورزش و تناسب اندام",
				Order = 6,
				Code = "SPRT-006"
			},

			new() {
				Id = Guid.CreateVersion7(),
				CreatedAt = DateTime.UtcNow,
				JsonData = new CategoryJson {
					Detail1 = "اسباب بازی و سرگرمی کودک",
					Detail2 = "محصولات ایمن و استاندارد",
					Subtitle = "شادی کودکان",
					Link = "/categories/toys",
					Location = "کودک و نوزاد",
					Type = "product",
					Address = "فروشگاه اسباب بازی",
					PhoneNumber = "021-75315984"
				},
				Tags = [TagCategory.Category],
				CreatorId = Core.App.Users.SystemAdmin.Id,
				Title = "اسباب بازی",
				Order = 7,
				Code = "TOYS-007"
			},

			new() {
				Id = Guid.CreateVersion7(),
				CreatedAt = DateTime.UtcNow,
				JsonData = new CategoryJson {
					Detail1 = "قطعات کامپیوتر و لپ تاپ",
					Detail2 = "گارانتی اصالت کالا",
					Subtitle = "تجهیزات کامپیوتری",
					Link = "/categories/computer",
					Location = "کامپیوتر",
					Type = "product",
					Address = "میدان حر",
					PhoneNumber = "021-95184762"
				},
				Tags = [TagCategory.Category],
				CreatorId = Core.App.Users.SystemAdmin.Id,
				Title = "قطعات کامپیوتر",
				Order = 8,
				Code = "COMP-008"
			},

			new() {
				Id = Guid.CreateVersion7(),
				CreatedAt = DateTime.UtcNow,
				JsonData = new CategoryJson {
					Detail1 = "انواع خودرو و موتورسیکلت",
					Detail2 = "فروش اقساطی خودرو",
					Subtitle = "بزرگترین بازار خودرو",
					Link = "/categories/vehicles",
					Location = "خودرو",
					Type = "service",
					Address = "نمایشگاه خودرو",
					PhoneNumber = "021-35715982"
				},
				Tags = [TagCategory.Category],
				CreatorId = Core.App.Users.SystemAdmin.Id,
				Title = "خودرو",
				Order = 9,
				Code = "CARS-009"
			},

			new() {
				Id = Guid.CreateVersion7(),
				CreatedAt = DateTime.UtcNow,
				JsonData = new CategoryJson {
					Detail1 = "خدمات آموزشی و مشاوره",
					Detail2 = "کلاس‌های آنلاین و حضوری",
					Subtitle = "یادگیری آسان",
					Link = "/categories/education",
					Location = "آموزش",
					Type = "service",
					Address = "موسسه آموزشی",
					PhoneNumber = "021-65498732"
				},
				Tags = [TagCategory.Category],
				CreatorId = Core.App.Users.SystemAdmin.Id,
				Title = "آموزش",
				Order = 10,
				Code = "EDUC-010"
			}
		];

		await db.Set<CategoryEntity>().AddRangeAsync(categories);
		await db.SaveChangesAsync();
		return new UResponse();
	}

	public async Task<UResponse> SeedContents() {
		List<ContentEntity> defaults = [
			BuildContent(
				tag: TagContent.HomeBanner1,
				title: "بالش‌ت را بردار، جای خواب اینجاست :)",
				subTitle: "خواب اینجا، سامانه معرفی و تبلیغات خوابگاه و پانسیون.",
				description: "اینجا می‌توانید بهترین خوابگاه‌ها و پانسیون‌های نزدیک خودتان را پیدا کنید. اطلاعات تکمیلی هر اقامتگاه، امکانات، بازخورد دانشجویان و شرایط رزرو در اختیار شماست.",
				buttonText: "جستجوی خوابگاه",
				buttonLink: "/dormitories",
				order: 1
			),
			BuildContent(
				tag: TagContent.AboutUs,
				title: "تعهد ما: آرامش و استاندارد زندگی شماست",
				subTitle: "چرا خواب‌روم؟",
				description: "پیدا کردن اقامتگاهی در تهران که همزمان امن، یکپارچه، استاندارد و دارای دسترسی عالی باشد، یک انتخاب دشوار است.",
				order: 2,
				items: [
					new ContentItem { Title = "آرامش و آسایش در سطح هتل", Description = "واحدهای مستقل با خدمات کامل خانه‌داری و پشتیبانی ۲۴ ساعته.", Order = 1 },
					new ContentItem { Title = "امنیت ۳۶۰ درجه و مطمئن", Description = "حضور مسئول مقیم، دسترسی کنترل‌شده و سیستم مانیتورینگ.", Order = 2 },
					new ContentItem { Title = "هوشمندی مکانیک سوئیت", Description = "حسگرهای محیطی، قفل‌های هوشمند و امکانات رفاهی دیجیتال.", Order = 3 }
				]
			),
			BuildContent(
				tag: TagContent.Services1,
				title: "خدمات ما",
				subTitle: "هر آنچه برای یک اقامت راحت نیاز دارید",
				order: 3,
				items: [
					new ContentItem { Title = "پشتیبانی ۲۴ ساعته", Description = "همراه همیشگی شما در تمام ساعات شبانه‌روز.", Order = 1 },
					new ContentItem { Title = "اینترنت پرسرعت", Description = "دسترسی پایدار به اینترنت در تمام واحدها.", Order = 2 },
					new ContentItem { Title = "خانه‌داری منظم", Description = "نظافت و نگهداری دوره‌ای فضاها.", Order = 3 },
					new ContentItem { Title = "امنیت کامل", Description = "کنترل دسترسی و مانیتورینگ ۲۴ ساعته.", Order = 4 }
				]
			),
			BuildContent(
				tag: TagContent.Blog,
				title: "آخرین بلاگ",
				subTitle: "تازه‌ترین مطالب و راهنماها",
				link: "/blog",
				order: 4
			),
			BuildContent(
				tag: TagContent.Footer1,
				title: "خواب‌روم",
				description: "سامانه معرفی و تبلیغات خوابگاه و پانسیون.",
				instagram: "https://instagram.com/",
				telegram: "https://t.me/",
				whatsapp: "https://wa.me/",
				phone: "02100000000",
				links: [
					new ContentLink { Title = "درباره ما", Url = "/about-us" },
					new ContentLink { Title = "تماس با ما", Url = "/contact-us" },
					new ContentLink { Title = "قوانین و مقررات", Url = "/terms" },
					new ContentLink { Title = "بلاگ", Url = "/blog" }
				]
			),
			BuildContent(
				tag: TagContent.AboutUs,
				title: "درباره ما",
				subTitle: "معرفی مجموعه",
				description: "ما مجموعه‌ای هستیم که با تکیه بر تجربه و تخصص، خدماتی باکیفیت به مشتریان خود ارائه می‌دهیم.",
				detail1: "چشم‌انداز ما ارائه بهترین تجربه به کاربران است.",
				detail2: "ماموریت ما ساده‌سازی و بهبود مستمر خدمات است.",
				instagram: "https://instagram.com/",
				telegram: "https://t.me/",
				whatsapp: "https://wa.me/",
				phone: "02100000000",
				items: [
					new ContentItem { Title = "کیفیت", SubTitle = "بالاترین استانداردها", Description = "تعهد ما به کیفیت در تمام مراحل کار.", Order = 1 },
					new ContentItem { Title = "پشتیبانی", SubTitle = "همراه همیشگی شما", Description = "پشتیبانی سریع و پاسخگو در تمام ساعات.", Order = 2 }
				]
			),
			BuildContent(
				tag: TagContent.Terms,
				title: "قوانین و مقررات",
				subTitle: "شرایط استفاده از خدمات",
				description: "استفاده از خدمات این مجموعه به منزله پذیرش کامل قوانین و مقررات زیر است.",
				detail1: "کاربر موظف به رعایت کلیه قوانین جاری است.",
				detail2: "این مجموعه حق تغییر قوانین را برای خود محفوظ می‌دارد."
			),
			BuildContent(
				tag: TagContent.ContactUs,
				title: "تماس با ما",
				subTitle: "راه‌های ارتباطی",
				description: "برای ارتباط با ما می‌توانید از راه‌های زیر استفاده کنید.",
				instagram: "https://instagram.com/",
				telegram: "https://t.me/",
				whatsapp: "https://wa.me/",
				phone: "02100000000"
			),
			BuildContent(
				tag: TagContent.HomeSlider1,
				title: "اسلایدر اصلی ۱",
				subTitle: "بنر معرفی",
				description: "متن معرفی برای اولین اسلایدر صفحه اصلی."
			),
			BuildContent(
				tag: TagContent.HomeSlider2,
				title: "اسلایدر اصلی ۲",
				subTitle: "بنر پیشنهاد ویژه",
				description: "متن معرفی برای دومین اسلایدر صفحه اصلی."
			)
		];

		List<ContentEntity> toInsert = [];
		foreach (ContentEntity c in defaults) {
			TagContent tag = c.Tags.First();
			bool exists = await db.Set<ContentEntity>().AnyAsync(x => x.Tags.Contains(tag));
			if (!exists) toInsert.Add(c);
		}

		if (toInsert.Count == 0) return new UResponse();

		await db.Set<ContentEntity>().AddRangeAsync(toInsert);
		await db.SaveChangesAsync();
		return new UResponse();
	}

	// =====================================================================================================
	// Demo data for the whole hotel & dorm system, created by ONE call: POST api/DataSeeder/HotelsAndDorms
	//   - guest / resident users            - hotels with rooms, reservations, invoices (paid, unpaid, refunded) and reviews
	//   - dorms with rooms and beds         - dorm contracts (active, expired, late) with deposit + monthly rent invoices, and reviews
	// Every text/detail field (highlights, amenities, nearby places, FAQs, policies...) is filled so all screens look complete.
	// Photos cannot be seeded (files must be uploaded); upload them from the admin panel ("Details & photos").
	// It is safe to call twice: when the demo data already exists nothing is created.
	// Requires DataSeeder/Users to have been run (it uses the system admin as creator).
	// =====================================================================================================
	public async Task<UResponse<List<KeyValue>?>> SeedHotelsAndDorms(CancellationToken ct = default) {
		Guid adminId = Core.App.Users.SystemAdmin.Id;
		DateTime now = DateTime.UtcNow;
		DateTime today = now.Date;

		if (!await db.Set<UserEntity>().AnyAsync(x => x.Id == adminId, ct)) return new UResponse<List<KeyValue>?>(null, Usc.BadRequest, "Run DataSeeder/Users first.");
		if (await db.Set<HotelEntity>().AnyAsync(x => x.Title == "هتل سنتی عباسی اصفهان", ct)) return new UResponse<List<KeyValue>?>(null, Usc.Conflict, "Hotel and dorm demo data already exists.");

		// ---------------------------------------------------------------- users (guests and dorm residents)
		string[] firstNames = ["علی", "مریم", "رضا", "زهرا", "امیر", "سارا", "حسین", "نگار", "محمد", "فاطمه", "پویا", "الهام"];
		string[] lastNames = ["احمدی", "رضایی", "کریمی", "موسوی", "حسینی", "صادقی", "نجفی", "قاسمی", "جعفری", "محمدی", "کاظمی", "یوسفی"];
		HashSet<string> takenNames = (await db.Set<UserEntity>().Select(x => x.UserName).ToListAsync(ct)).ToHashSet();
		List<UserEntity> users = [];
		for (int i = 0; i < firstNames.Length; i++) {
			string userName = $"demo{i + 1:00}";
			if (takenNames.Contains(userName)) continue;
			users.Add(new UserEntity {
				Id = Guid.CreateVersion7(),
				CreatedAt = now.AddDays(-90 + i),
				CreatorId = adminId,
				Tags = [TagUser.Unspecified, i % 2 == 0 ? TagUser.Male : TagUser.Female, TagUser.Verified],
				UserName = userName,
				Password = UPasswordHasher.Hash("Demo1234"),
				RefreshToken = "",
				PhoneNumber = $"0912000{i + 1:0000}",
				Email = $"{userName}@example.com",
				FirstName = firstNames[i],
				LastName = lastNames[i],
				JsonData = new UserJson()
			});
		}

		// Users that already exist from an earlier partial run are looked up so the demo data can still reference them.
		List<Guid> userIds = users.Select(x => x.Id).ToList();
		if (userIds.Count < firstNames.Length)
			userIds.AddRange(await db.Set<UserEntity>().Where(x => x.UserName.StartsWith("demo")).Select(x => x.Id).ToListAsync(ct));
		if (userIds.Count == 0) userIds.Add(adminId);
		Guid UserAt(int i) => userIds[i % userIds.Count];

		List<HotelEntity> hotels = [];
		List<HotelRoomEntity> rooms = [];
		List<HotelReservationEntity> reservations = [];
		List<HotelInvoiceEntity> hotelInvoices = [];
		List<CommentEntity> comments = [];
		List<DormEntity> dorms = [];
		List<DormRoomEntity> dormRooms = [];
		List<DormBedEntity> beds = [];
		List<DormBedContractEntity> contracts = [];
		List<DormBedInvoiceEntity> dormInvoices = [];

		CommentEntity Review(Guid? hotelId, Guid? dormId, int userIndex, decimal score, string text, TagComment tag, int daysAgo) => new() {
			Id = Guid.CreateVersion7(),
			CreatedAt = now.AddDays(-daysAgo),
			CreatorId = UserAt(userIndex),
			UserId = UserAt(userIndex),
			HotelId = hotelId,
			DormId = dormId,
			Score = score,
			Description = text,
			Tags = [tag],
			JsonData = new CommentJson()
		};

		// ---------------------------------------------------------------- hotels
		// Type, policies, amenities and meal plans are tags; the Json only keeps texts and numbers.
		(string Title, string City, int Stars, string Address, string Phone, List<TagHotel> Tags, HotelJson Json, (string Title, int Capacity, decimal Price, int Quantity, string Bed, double Size, int Floor, List<TagRoom> Tags)[] Rooms)[] hotelSeeds = [
			("هتل سنتی عباسی اصفهان", "104005", 4, "اصفهان، خیابان چهارباغ عباسی، کوچه ملک", "03132200100",
				[
					TagHotel.Traditional, TagHotel.Active, TagHotel.Featured, TagHotel.Approved,
					TagHotel.ChildrenAllowed, TagHotel.ExtraBedAvailable, TagHotel.PriceIncludesTax,
					TagHotel.Wifi, TagHotel.Parking, TagHotel.Reception24, TagHotel.LuggageStorage, TagHotel.Cafe, TagHotel.Garden, TagHotel.Cctv,
					TagHotel.Breakfast, TagHotel.HalfBoard
				],
				new HotelJson {
					Highlights = ["حیاط مرکزی با حوض و چهار باغچه", "ده دقیقه پیاده تا میدان نقش جهان", "صبحانه سنتی هر روز", "بنای مرمت‌شده‌ی دوره قاجار"],
					Website = "https://khabroom.com", Whatsapp = "989120000001", Instagram = "khabroom", Telegram = "khabroom",
					HowToGetThere = "از خیابان چهارباغ عباسی وارد کوچه‌ی ملک شوید؛ هتل سمت راست، روبه‌روی نانوایی سنتی است.",
					Nearby = [
						new PlaceNearby { Title = "میدان نقش جهان", DistanceMeters = 800, Minutes = 10 }, new PlaceNearby { Title = "سی‌وسه‌پل", DistanceMeters = 1200, Minutes = 15 }, new PlaceNearby { Title = "بازار قیصریه", DistanceMeters = 900, Minutes = 11 },
						new PlaceNearby { Title = "ایستگاه مترو", DistanceMeters = 1500, Minutes = 5 }
					],
					Faqs = [new PlaceFaq { Question = "آیا پارکینگ دارید؟", Answer = "بله، پارکینگ اختصاصی و رایگان برای مهمانان وجود دارد." }, new PlaceFaq { Question = "امکان تحویل زودتر اتاق هست؟", Answer = "با هماهنگی قبلی و بسته به ظرفیت، ورود از ساعت ۱۲ ممکن است." }, new PlaceFaq { Question = "صبحانه شامل چه چیزهایی است؟", Answer = "نان سنگک، پنیر، تخم‌مرغ محلی، مربا، عسل و چای سماوری." }],
					Description = "خانه‌ی قاجاری مرمت‌شده با حیاط مرکزی، حوض و چهار باغچه؛ ده دقیقه پیاده تا میدان نقش جهان. اتاق‌ها دور حیاط چیده شده‌اند و شب‌ها سکوت کامل است.",
					Policies = "ورود از ساعت ۱۴ و خروج تا ساعت ۱۲ ظهر. پرداخت بیعانه هنگام رزرو الزامی است. کودکان زیر ۶ سال با والدین رایگان اقامت می‌کنند.",
					CheckInTime = "14:00", CheckOutTime = "12:00",
					Rules = ["استعمال دخانیات در اتاق‌ها ممنوع است", "ورود حیوان خانگی ممنوع است", "سکوت پس از ساعت ۲۳ رعایت شود"],
					Latitude = 32.6607, Longitude = 51.6693, CancellationFreeHours = 48, CancellationPenaltyNights = 1
				},
				[
					("اتاق دو تخته سنتی", 2, 4_200_000m, 12, "دو تخت", 24, 1, [TagRoom.Double, TagRoom.BreakfastIncluded, TagRoom.CourtyardView, TagRoom.Tv, TagRoom.Minibar, TagRoom.AirConditioning, TagRoom.Wardrobe, TagRoom.PrivateBathroom]),
					("اتاق سه تخته", 3, 5_400_000m, 8, "سه تخت", 32, 1, [TagRoom.Triple, TagRoom.BreakfastIncluded, TagRoom.CourtyardView, TagRoom.Tv, TagRoom.Minibar, TagRoom.AirConditioning, TagRoom.Kettle, TagRoom.PrivateBathroom]),
					("سوئیت خانوادگی", 4, 7_800_000m, 4, "یک دو نفره و دو تک", 48, 2, [TagRoom.Family, TagRoom.BreakfastIncluded, TagRoom.GardenView, TagRoom.Tv, TagRoom.Minibar, TagRoom.AirConditioning, TagRoom.Fridge, TagRoom.Kettle, TagRoom.Balcony, TagRoom.Bathtub])
				]),

			("هتل پارسیان ولیعصر", "108012", 5, "تهران، بلوار ولیعصر، بالاتر از پارک ملت", "02122000200",
				[
					TagHotel.Hotel, TagHotel.Active, TagHotel.Featured, TagHotel.Approved,
					TagHotel.ChildrenAllowed, TagHotel.ExtraBedAvailable,
					TagHotel.Wifi, TagHotel.Parking, TagHotel.Elevator, TagHotel.Reception24, TagHotel.LuggageStorage, TagHotel.Laundry, TagHotel.AirportShuttle, TagHotel.Restaurant, TagHotel.Cafe,
					TagHotel.RoomService, TagHotel.Pool, TagHotel.Gym, TagHotel.Sauna, TagHotel.Spa, TagHotel.MeetingRoom, TagHotel.Wheelchair, TagHotel.Cctv,
					TagHotel.RoomOnly, TagHotel.Breakfast, TagHotel.HalfBoard, TagHotel.FullBoard
				],
				new HotelJson {
					Highlights = ["استخر و سونای اختصاصی مهمانان", "ترانسفر رایگان فرودگاه امام", "سالن همایش تا ۳۰۰ نفر", "ده دقیقه تا مترو ولیعصر"],
					Website = "https://example.com/parsian", Whatsapp = "989120000002", Instagram = "parsian_valiasr",
					HowToGetThere = "ورودی اصلی از بلوار ولیعصر است؛ پارکینگ طبقات منفی از خیابان فرعی شرقی.",
					Nearby = [
						new PlaceNearby { Title = "مترو ولیعصر", DistanceMeters = 600, Minutes = 8 }, new PlaceNearby { Title = "پارک ملت", DistanceMeters = 400, Minutes = 5 }, new PlaceNearby { Title = "بیمارستان آرش", DistanceMeters = 1800, Minutes = 6 },
						new PlaceNearby { Title = "فرودگاه امام خمینی", DistanceMeters = 48000, Minutes = 50 }
					],
					Faqs = [new PlaceFaq { Question = "آیا سالن همایش دارید؟", Answer = "بله، سه سالن با ظرفیت ۵۰ تا ۳۰۰ نفر، با تجهیزات کامل صوتی و تصویری." }, new PlaceFaq { Question = "ساعت کار استخر چیست؟", Answer = "هر روز از ۷ صبح تا ۲۲ شب، ویژه‌ی مهمانان هتل." }],
					Description = "هتل پنج‌ستاره‌ی مدرن در قلب تهران با استخر سرپوشیده، اسپا و سالن‌های همایش؛ مناسب سفرهای کاری و خانوادگی.",
					Policies = "ورود از ساعت ۱۴ و خروج تا ۱۲. کارت ملی یا گذرنامه هنگام ورود الزامی است. کودکان زیر ۱۲ سال با استفاده از تخت موجود رایگان هستند.",
					CheckInTime = "14:00", CheckOutTime = "12:00",
					Rules = ["ساعت سکوت ۲۳ تا ۷ صبح", "میهمان‌پذیری در لابی تا ساعت ۲۲ مجاز است"],
					Latitude = 35.7580, Longitude = 51.4090, CancellationFreeHours = 24, CancellationPenaltyNights = 1
				},
				[
					("اتاق استاندارد", 2, 6_500_000m, 30, "دو تخت", 28, 5, [TagRoom.Double, TagRoom.BreakfastIncluded, TagRoom.CityView, TagRoom.Tv, TagRoom.Minibar, TagRoom.SafeBox, TagRoom.AirConditioning, TagRoom.HairDryer, TagRoom.PrivateBathroom]),
					("اتاق دلوکس", 3, 8_900_000m, 20, "کینگ + کاناپه", 38, 10, [TagRoom.Deluxe, TagRoom.BreakfastIncluded, TagRoom.CityView, TagRoom.Tv, TagRoom.Minibar, TagRoom.SafeBox, TagRoom.AirConditioning, TagRoom.Desk, TagRoom.Kettle, TagRoom.Bathtub]),
					("سوئیت رویال", 4, 16_500_000m, 6, "کینگ", 75, 17, [TagRoom.Suite, TagRoom.MountainView, TagRoom.Tv, TagRoom.Minibar, TagRoom.SafeBox, TagRoom.AirConditioning, TagRoom.Desk, TagRoom.Kettle, TagRoom.Bathtub, TagRoom.Balcony, TagRoom.Fridge])
				]),

			("مهمان‌پذیر باغ‌نو شیراز", "117044", 3, "شیراز، خیابان لطفعلی‌خان زند، کوچه‌ی باغ‌نو", "07132300300",
				[
					TagHotel.Guesthouse, TagHotel.Active, TagHotel.Approved,
					TagHotel.PetsAllowed, TagHotel.ChildrenAllowed, TagHotel.PriceIncludesTax,
					TagHotel.Wifi, TagHotel.Parking, TagHotel.Garden,
					TagHotel.Breakfast
				],
				new HotelJson {
					Highlights = ["باغچه‌ی نارنج و سایه‌بان", "صبحانه‌ی خانگی", "پنج دقیقه تا ارگ کریم‌خان"],
					Whatsapp = "989120000003", Instagram = "baghnow_shiraz",
					HowToGetThere = "از میدان شهدا به سمت لطفعلی‌خان زند؛ کوچه‌ی باغ‌نو دومین کوچه‌ی سمت چپ.",
					Nearby = [new PlaceNearby { Title = "ارگ کریم‌خان", DistanceMeters = 450, Minutes = 6 }, new PlaceNearby { Title = "بازار وکیل", DistanceMeters = 600, Minutes = 8 }],
					Faqs = [new PlaceFaq { Question = "آیا حیوان خانگی مجاز است؟", Answer = "بله، سگ و گربه‌ی کوچک با هماهنگی قبلی." }],
					Description = "اقامتگاه صمیمی و خانوادگی در بافت تاریخی شیراز، با باغچه‌ی نارنج و صبحانه‌ی خانگی؛ پنج دقیقه تا ارگ کریم‌خان.",
					Policies = "ورود از ساعت ۱۳ و خروج تا ۱۱:۳۰.",
					CheckInTime = "13:00", CheckOutTime = "11:30",
					Rules = ["ورود پس از ساعت ۲۳ با هماهنگی"],
					Latitude = 29.6100, Longitude = 52.5420, CancellationFreeHours = 24, CancellationPenaltyNights = 1
				},
				[
					("اتاق دو نفره", 2, 2_800_000m, 5, "دو تخت", 20, 1, [TagRoom.Double, TagRoom.BreakfastIncluded, TagRoom.GardenView, TagRoom.AirConditioning, TagRoom.Tv, TagRoom.PrivateBathroom]),
					("اتاق خانوادگی", 4, 4_100_000m, 3, "چهار تخت", 34, 1, [TagRoom.Family, TagRoom.BreakfastIncluded, TagRoom.CourtyardView, TagRoom.AirConditioning, TagRoom.Tv, TagRoom.Fridge, TagRoom.PrivateBathroom])
				]),

			("هتل‌آپارتمان زائر مشهد", "111062", 4, "مشهد، خیابان امام رضا، نبش امام رضا ۲۱", "05132400400",
				[
					TagHotel.Apartment, TagHotel.Active, TagHotel.PendingApproval,
					TagHotel.ChildrenAllowed, TagHotel.ExtraBedAvailable, TagHotel.PriceIncludesTax,
					TagHotel.Wifi, TagHotel.Elevator, TagHotel.Parking, TagHotel.Reception24, TagHotel.LuggageStorage, TagHotel.PrayerRoom, TagHotel.Laundry,
					TagHotel.RoomOnly, TagHotel.Breakfast
				],
				new HotelJson {
					Highlights = ["آشپزخانه‌ی کامل در هر واحد", "پنج دقیقه پیاده تا حرم", "نمازخانه و انبار چمدان"],
					Whatsapp = "989120000004",
					HowToGetThere = "ورودی از خیابان امام رضا ۲۱؛ ایستگاه مترو حرم رضوی ۳ دقیقه پیاده.",
					Nearby = [new PlaceNearby { Title = "حرم مطهر امام رضا (ع)", DistanceMeters = 400, Minutes = 5 }, new PlaceNearby { Title = "مترو شهدا", DistanceMeters = 250, Minutes = 3 }, new PlaceNearby { Title = "مرکز خرید رضوی", DistanceMeters = 700, Minutes = 9 }],
					Faqs = [new PlaceFaq { Question = "آیا لوازم آشپزخانه در واحدها موجود است؟", Answer = "بله، ظروف، اجاق و یخچال کامل است." }],
					Description = "واحدهای مبله با آشپزخانه‌ی کامل، ۵ دقیقه پیاده تا حرم مطهر؛ مناسب اقامت‌های چندشبه‌ی خانوادگی.",
					Policies = "حداقل اقامت ۲ شب. ورود ۱۴ و خروج ۱۲.",
					CheckInTime = "14:00", CheckOutTime = "12:00",
					Rules = ["حداقل اقامت دو شب", "تعداد مهمان بیش از ظرفیت واحد مجاز نیست"],
					Latitude = 36.2880, Longitude = 59.6170, CancellationFreeHours = 72, CancellationPenaltyNights = 1
				},
				[
					("واحد یک‌خوابه", 3, 3_600_000m, 10, "یک دو نفره + مبل تخت‌شو", 45, 3, [TagRoom.Triple, TagRoom.CityView, TagRoom.Kitchenette, TagRoom.Fridge, TagRoom.Tv, TagRoom.AirConditioning, TagRoom.PrivateBathroom]),
					("واحد دوخوابه", 5, 5_200_000m, 8, "دو دو نفره + مبل تخت‌شو", 70, 5, [TagRoom.Family, TagRoom.NonRefundable, TagRoom.CityView, TagRoom.Kitchenette, TagRoom.Fridge, TagRoom.Tv, TagRoom.AirConditioning, TagRoom.Balcony, TagRoom.PrivateBathroom])
				])
		];

		int hotelIndex = 0;
		foreach (var h in hotelSeeds) {
			Guid hotelId = Guid.CreateVersion7();
			h.Json.Detail1 = "";
			hotels.Add(new HotelEntity {
				Id = hotelId, CreatedAt = now.AddDays(-80), CreatorId = adminId, Tags = h.Tags, JsonData = h.Json,
				Title = h.Title, CityCode = h.City, Stars = h.Stars, Address = h.Address, PhoneNumber = h.Phone, Email = $"info{hotelIndex + 1}@example.com"
			});

			List<HotelRoomEntity> hotelRooms = [];
			foreach (var r in h.Rooms) {
				HotelRoomEntity room = new() {
					Id = Guid.CreateVersion7(), CreatedAt = now.AddDays(-79), CreatorId = adminId, HotelId = hotelId,
					Tags = [TagRoom.Available, .. r.Tags],
					Title = r.Title, Capacity = r.Capacity, PricePerNight = r.Price, Quantity = r.Quantity, IsAvailable = true, RoomNumber = $"{100 * (hotelRooms.Count + 1)}",
					JsonData = new HotelRoomJson {
						Description = $"{r.Title} با امکانات کامل و پاکیزگی روزانه.", BedType = r.Bed, SizeSquareMeters = r.Size, Floor = r.Floor,
						ExtraGuestCapacity = 1, ExtraGuestPrice = 10000
					}
				};
				hotelRooms.Add(room);
			}

			rooms.AddRange(hotelRooms);

			// reservations: one of every status, with an invoice that matches it
			(TagHotelReservation Status, int StartOffset, int Nights)[] plan = [
				(TagHotelReservation.CheckedOut, -20, 3), (TagHotelReservation.CheckedIn, -1, 3), (TagHotelReservation.Confirmed, 5, 2),
				(TagHotelReservation.Pending, 12, 2), (TagHotelReservation.Cancelled, 3, 2), (TagHotelReservation.CheckedOut, -45, 4)
			];
			for (int i = 0; i < plan.Length; i++) {
				HotelRoomEntity room = hotelRooms[i % hotelRooms.Count];
				Guid userId = UserAt(hotelIndex * 3 + i);
				DateTime checkIn = today.AddDays(plan[i].StartOffset);
				DateTime checkOut = checkIn.AddDays(plan[i].Nights);
				decimal total = room.PricePerNight * plan[i].Nights;
				bool cancelled = plan[i].Status == TagHotelReservation.Cancelled;
				Guid reservationId = Guid.CreateVersion7();
				reservations.Add(new HotelReservationEntity {
					Id = reservationId, CreatedAt = checkIn.AddDays(-10), CreatorId = userId, Tags = [plan[i].Status],
					CheckInDate = checkIn, CheckOutDate = checkOut, GuestCount = Math.Min(room.Capacity, 2 + i % 2), TotalPrice = total,
					UserId = userId, RoomId = room.Id, HotelId = hotelId,
					JsonData = new HotelReservationJson {
						GuestName = $"{firstNames[(hotelIndex * 3 + i) % firstNames.Length]} {lastNames[(hotelIndex * 3 + i) % lastNames.Length]}",
						GuestPhone = $"0912000{(hotelIndex * 3 + i) % 12 + 1:0000}", Notes = i % 2 == 0 ? "لطفاً اتاق طبقه‌ی بالا باشد." : null,
						NightCount = plan[i].Nights, ReservationCode = $"KH{hotelIndex}{i}{Random.Shared.Next(1000, 9999)}",
						Guests = [new ReservationGuestJson { FullName = $"{firstNames[(hotelIndex * 3 + i) % firstNames.Length]} {lastNames[(hotelIndex * 3 + i) % lastNames.Length]}", PhoneNumber = $"0912000{(hotelIndex * 3 + i) % 12 + 1:0000}" }],
						CancelledAt = cancelled ? checkIn.AddDays(-4) : null, CancelReason = cancelled ? "تغییر برنامه‌ی سفر" : null,
						CancellationPenalty = cancelled ? room.PricePerNight : null, RefundAmount = cancelled ? total - room.PricePerNight : null
					}
				});
				bool paid = plan[i].Status is TagHotelReservation.CheckedOut or TagHotelReservation.CheckedIn or TagHotelReservation.Confirmed;
				hotelInvoices.Add(new HotelInvoiceEntity {
					Id = Guid.CreateVersion7(), CreatedAt = checkIn.AddDays(-10), CreatorId = userId,
					Tags = cancelled ? [TagHotelInvoice.Refunded, TagHotelInvoice.Full] : paid ? [TagHotelInvoice.Paid, i % 2 == 0 ? TagHotelInvoice.PaidOnline : TagHotelInvoice.PaidManual, TagHotelInvoice.Full] : [TagHotelInvoice.NotPaid, TagHotelInvoice.Full],
					DebtAmount = total, CreditorAmount = cancelled ? total - room.PricePerNight : 0, PaidAmount = paid || cancelled ? total : 0, PenaltyAmount = cancelled ? room.PricePerNight : 0,
					ReservationId = reservationId, DueDate = checkIn, JsonData = new HotelInvoiceJson { PenaltyPrecentEveryDate = 0 }
				});
			}

			string[] good = ["اقامتی بسیار دلنشین؛ پرسنل مهربان و اتاق‌ها تمیز بودند.", "موقعیت عالی و صبحانه‌ی خوشمزه. حتماً دوباره می‌آیم.", "قیمت مناسب نسبت به امکانات. تحویل اتاق سریع بود.", "برای سفر خانوادگی گزینه‌ی خوبی است؛ فضای آرام و امن."];
			for (int i = 0; i < good.Length; i++) comments.Add(Review(hotelId, null, hotelIndex * 4 + i, i == 2 ? 4m : i == 3 ? 4.5m : 5m, good[i], TagComment.Released, 5 + i * 9));
			comments.Add(Review(hotelId, null, hotelIndex + 6, 3m, "نظر در صف بررسی: سرویس بهداشتی می‌توانست بهتر باشد.", TagComment.InQueue, 2));
			hotelIndex++;
		}

		// ---------------------------------------------------------------- dorms
		// Residents, amenities, meals and what the rent includes are tags; the Json only keeps texts and numbers.
		(string Title, string City, string Address, string Phone, List<TagDorm> Tags, DormJson Json, (string Title, int Beds, decimal Rent, decimal Deposit, double Size, int Floor, List<TagDormRoom> Tags)[] Rooms)[] dormSeeds = [
			("خوابگاه دخترانه‌ی نگین", "108012", "تهران، امیرآباد شمالی، خیابان چهارم", "02166001000",
				[
					TagDorm.Girls, TagDorm.Active, TagDorm.Featured, TagDorm.Approved,
					TagDorm.Bachelor, TagDorm.Master, TagDorm.Phd,
					TagDorm.Wifi, TagDorm.SharedKitchen, TagDorm.StudyRoom, TagDorm.Laundry, TagDorm.Supervisor, TagDorm.Cctv, TagDorm.SecurityGuard, TagDorm.Lockers, TagDorm.Lounge,
					TagDorm.Breakfast, TagDorm.Dinner,
					TagDorm.InternetIncluded, TagDorm.UtilitiesIncluded, TagDorm.CleaningIncluded
				],
				new DormJson {
					Highlights = ["هفت دقیقه پیاده تا دانشگاه تهران", "اتاق مطالعه‌ی شبانه‌روزی", "اینترنت فیبر ۱۰۰ مگابیت"],
					Website = "https://example.com/negin", Whatsapp = "989120000011", Instagram = "negin_dorm", CurfewTime = "23:00", MinimumStayMonths = 6,
					Policies = "ودیعه هنگام عقد قرارداد و اجاره‌ی ماهانه تا پنجم هر ماه پرداخت می‌شود. ودیعه پس از تسویه و تحویل اتاق حداکثر ظرف ۷ روز کاری مسترد می‌شود. فسخ زودهنگام با معرفی جایگزین بدون جریمه است.",
					UniversityWalkMinutes = 7, HowToGetThere = "از ایستگاه مترو دانشگاه تهران با تاکسی یا ۱۰ دقیقه پیاده تا خیابان چهارم امیرآباد.",
					Nearby = [new PlaceNearby { Title = "دانشگاه تهران", DistanceMeters = 450, Minutes = 7 }, new PlaceNearby { Title = "مترو دانشگاه تهران", DistanceMeters = 900, Minutes = 12 }, new PlaceNearby { Title = "داروخانه‌ی شبانه‌روزی", DistanceMeters = 200, Minutes = 3 }],
					Faqs = [new PlaceFaq { Question = "ساعت آخرین ورود چه زمانی است؟", Answer = "ساعت ۲۳؛ برای ورود دیرتر باید از پیش با سرپرست هماهنگ کنید." }, new PlaceFaq { Question = "آیا امکان آشپزی وجود دارد؟", Answer = "بله، در هر طبقه یک آشپزخانه‌ی مشترک با یخچال جداگانه برای هر اتاق هست." }],
					Description = "هفت دقیقه پیاده تا درِ اصلی دانشگاه تهران؛ اتاق‌های مبله‌ی دو و چهارنفره، اتاق مطالعه‌ی شبانه‌روزی و سرپرست مقیم.",
					NearbyUniversity = "دانشگاه تهران", VisitingHours = "۱۶ تا ۲۰",
					Rules = ["رعایت سکوت از ساعت ۲۲", "استعمال دخانیات ممنوع", "ورود مهمان تنها در لابی"], RequiredDocuments = ["کارت ملی", "گواهی اشتغال به تحصیل", "دو قطعه عکس ۳×۴"],
					Latitude = 35.7300, Longitude = 51.3900
				},
				[
					("اتاق دو نفره‌ی A", 2, 3_200_000m, 15_000_000m, 18, 2, [TagDormRoom.Double, TagDormRoom.Furnished, TagDormRoom.PrivateBathroom, TagDormRoom.Desk, TagDormRoom.Wardrobe, TagDormRoom.AirConditioning]),
					("اتاق چهارنفره‌ی B", 4, 2_100_000m, 10_000_000m, 28, 3, [TagDormRoom.Dorm, TagDormRoom.Furnished, TagDormRoom.Desk, TagDormRoom.Wardrobe, TagDormRoom.Heating]),
					("اتاق سه نفره‌ی C", 3, 2_600_000m, 12_000_000m, 22, 4, [TagDormRoom.Dorm, TagDormRoom.Furnished, TagDormRoom.PrivateBathroom, TagDormRoom.Desk, TagDormRoom.AirConditioning, TagDormRoom.Balcony])
				]),

			("خوابگاه پسرانه‌ی آرمان", "108012", "تهران، انقلاب، خیابان فخر رازی", "02166002000",
				[
					TagDorm.Boys, TagDorm.Active, TagDorm.Approved,
					TagDorm.Bachelor, TagDorm.Master,
					TagDorm.Wifi, TagDorm.SharedKitchen, TagDorm.BikeParking, TagDorm.Laundry, TagDorm.Cctv, TagDorm.Lockers, TagDorm.Lounge,
					TagDorm.Dinner,
					TagDorm.InternetIncluded, TagDorm.UtilitiesIncluded
				],
				new DormJson {
					Highlights = ["نزدیک مترو انقلاب", "آشپزخانه‌ی مرکزی", "پارکینگ دوچرخه و موتور"], Whatsapp = "989120000012", MinimumStayMonths = 4,
					Policies = "اجاره‌ی ماهانه. ودیعه معادل دو ماه اجاره است. ورود مهمان ممنوع.",
					UniversityWalkMinutes = 15, HowToGetThere = "خروجی ۳ مترو انقلاب، ۱۰ دقیقه پیاده به سمت خیابان فخر رازی.",
					Nearby = [new PlaceNearby { Title = "مترو انقلاب", DistanceMeters = 800, Minutes = 10 }, new PlaceNearby { Title = "کتابفروشی‌های انقلاب", DistanceMeters = 300, Minutes = 4 }],
					Faqs = [new PlaceFaq { Question = "قرارداد ترمی است یا ماهانه؟", Answer = "هر دو ممکن است؛ حداقل مدت ۴ ماه." }],
					Description = "ساختمان بازسازی‌شده‌ی چهارطبقه با آشپزخانه‌ی مرکزی؛ ۱۰ دقیقه تا مترو انقلاب.", NearbyUniversity = "دانشگاه تهران", VisitingHours = "۱۷ تا ۲۰",
					Rules = ["ورود تا ساعت ۲۴", "ورود مهمان ممنوع"], RequiredDocuments = ["کارت ملی", "گواهی اشتغال به تحصیل"], Latitude = 35.7010, Longitude = 51.3950
				},
				[
					("اتاق دو نفره", 2, 2_800_000m, 12_000_000m, 16, 1, [TagDormRoom.Double, TagDormRoom.Furnished, TagDormRoom.Desk, TagDormRoom.Heating]),
					("اتاق چهارنفره", 4, 1_800_000m, 8_000_000m, 26, 2, [TagDormRoom.Dorm, TagDormRoom.Desk, TagDormRoom.Wardrobe])
				]),

			("خوابگاه دخترانه‌ی نسیم شیراز", "117044", "شیراز، بلوار ارم، خیابان دانشجو", "07132500500",
				[
					TagDorm.Girls, TagDorm.Active, TagDorm.Approved,
					TagDorm.Bachelor, TagDorm.Master,
					TagDorm.Wifi, TagDorm.Shuttle, TagDorm.SelfService, TagDorm.Garden, TagDorm.Supervisor, TagDorm.Cctv, TagDorm.SecurityGuard, TagDorm.SharedKitchen, TagDorm.StudyRoom,
					TagDorm.Lunch, TagDorm.Dinner,
					TagDorm.InternetIncluded, TagDorm.UtilitiesIncluded, TagDorm.CleaningIncluded
				],
				new DormJson {
					Highlights = ["سرویس رفت‌وآمد رایگان", "سلف‌سرویس ناهار و شام", "حیاط و فضای سبز"], Instagram = "nasim_dorm", CurfewTime = "22:30", MinimumStayMonths = 6,
					Policies = "ودیعه + اجاره‌ی ماهانه. ودیعه پس از تسویه مسترد می‌شود. فسخ پیش از پایان ترم با معرفی جایگزین. ورود خانواده در ساعات ۱۶ تا ۱۹.",
					UniversityWalkMinutes = 20, HowToGetThere = "ایستگاه اتوبوس دانشگاه شیراز مقابل درب ورودی است.",
					Nearby = [new PlaceNearby { Title = "دانشگاه شیراز", DistanceMeters = 1500, Minutes = 20 }, new PlaceNearby { Title = "ایستگاه اتوبوس", DistanceMeters = 50, Minutes = 1 }],
					Faqs = [new PlaceFaq { Question = "سرویس رفت‌وآمد چند بار در روز است؟", Answer = "دو بار: صبح و عصر؛ رایگان برای ساکنین." }],
					Description = "ویژه‌ی خواهران با ورودی مستقل، حیاط، سلف‌سرویس و سرویس رفت‌وآمد رایگان تا دانشگاه.", NearbyUniversity = "دانشگاه شیراز", VisitingHours = "۱۶ تا ۱۹",
					Rules = ["رعایت پوشش و شئونات", "ورود تا ساعت ۲۲:۳۰"], RequiredDocuments = ["کارت ملی", "گواهی اشتغال به تحصیل", "معرفی‌نامه‌ی دانشگاه"], Latitude = 29.6400, Longitude = 52.5250
				},
				[
					("اتاق سه نفره", 3, 1_900_000m, 10_000_000m, 22, 1, [TagDormRoom.Dorm, TagDormRoom.Furnished, TagDormRoom.PrivateBathroom, TagDormRoom.Desk, TagDormRoom.Wardrobe]),
					("اتاق دو نفره", 2, 2_400_000m, 10_000_000m, 16, 2, [TagDormRoom.Double, TagDormRoom.Furnished, TagDormRoom.PrivateBathroom, TagDormRoom.Desk, TagDormRoom.AirConditioning])
				]),

			("خوابگاه پسرانه‌ی دانا", "101013", "تبریز، خیابان دانشگاه، کوچه‌ی سوم", "04133600600",
				[
					TagDorm.Boys, TagDorm.Active, TagDorm.PendingApproval,
					TagDorm.Bachelor,
					TagDorm.Wifi, TagDorm.SharedKitchen, TagDorm.BikeParking, TagDorm.Laundry, TagDorm.Lockers,
					TagDorm.InternetIncluded, TagDorm.UtilitiesIncluded
				],
				new DormJson {
					Highlights = ["هزینه‌ی شارژ و اینترنت در اجاره", "آشپزخانه‌ی بزرگ مرکزی"], MinimumStayMonths = 3,
					Policies = "اجاره‌ی ماهانه. ودیعه‌ی ۸ میلیون تومان.", UniversityWalkMinutes = 12,
					Nearby = [new PlaceNearby { Title = "دانشگاه تبریز", DistanceMeters = 900, Minutes = 12 }],
					Description = "ارزان‌ترین گزینه‌ی ما با آشپزخانه‌ی بزرگ و پارکینگ دوچرخه؛ شارژ و اینترنت در اجاره لحاظ شده است.", NearbyUniversity = "دانشگاه تبریز", VisitingHours = "۱۷ تا ۲۰",
					Rules = ["سکوت پس از ۲۳"], RequiredDocuments = ["کارت ملی"], Latitude = 38.0800, Longitude = 46.3200
				},
				[
					("اتاق چهارنفره", 4, 1_500_000m, 8_000_000m, 26, 1, [TagDormRoom.Dorm, TagDormRoom.Desk]),
					("اتاق دو نفره", 2, 2_000_000m, 8_000_000m, 15, 2, [TagDormRoom.Double, TagDormRoom.Desk, TagDormRoom.Heating])
				]),

			// Built without the Active tag to show the "hidden" state: the site and the app do not list it.
			("خوابگاه دخترانه‌ی آرام کرج", "105009", "کرج، گوهردشت، فاز ۳", "02634700700",
				[TagDorm.Girls, TagDorm.Inactive, TagDorm.Bachelor, TagDorm.Wifi, TagDorm.Garden, TagDorm.SharedKitchen],
				new DormJson { Highlights = ["سی تخت", "حیاط بزرگ"], Description = "کوچک‌ترین مجموعه‌ی ما؛ سی تخت، حیاط و سکوت.", NearbyUniversity = "دانشگاه آزاد کرج", Latitude = 35.83, Longitude = 50.93 },
				[("اتاق دو نفره", 2, 1_600_000m, 7_000_000m, 18, 1, [TagDormRoom.Double, TagDormRoom.Desk])])
		];

		int dormIndex = 0;
		int residentIndex = 0;
		foreach (var d in dormSeeds) {
			Guid dormId = Guid.CreateVersion7();
			dorms.Add(new DormEntity {
				Id = dormId, CreatedAt = now.AddDays(-85), CreatorId = adminId, Tags = d.Tags, JsonData = d.Json,
				Title = d.Title, CityCode = d.City, Address = d.Address, PhoneNumber = d.Phone
			});

			int roomNumber = 0;
			foreach (var r in d.Rooms) {
				roomNumber++;
				Guid roomId = Guid.CreateVersion7();
				dormRooms.Add(new DormRoomEntity {
					Id = roomId, CreatedAt = now.AddDays(-84), CreatorId = adminId, DormId = dormId, Title = r.Title, Capacity = r.Beds,
					Tags = r.Tags,
					JsonData = new DormRoomJson { Description = $"{r.Title}؛ با میز مطالعه و کمد شخصی برای هر نفر.", Floor = r.Floor, SizeSquareMeters = r.Size }
				});

				for (int b = 0; b < r.Beds; b++) {
					Guid bedId = Guid.CreateVersion7();
					beds.Add(new DormBedEntity {
						Id = bedId, CreatedAt = now.AddDays(-83), CreatorId = adminId, RoomId = roomId, Title = $"{(char)('A' + roomNumber - 1)}{b + 1}", Deposit = r.Deposit, MonthlyRent = r.Rent,
						Tags = r.Beds > 2
							? [TagDormBed.Single, b % 2 == 0 ? TagDormBed.BunkBottom : TagDormBed.BunkTop, TagDormBed.Locker, TagDormBed.ReadingLamp, TagDormBed.PowerOutlet]
							: [TagDormBed.Single, TagDormBed.Desk, TagDormBed.Locker, TagDormBed.PrivacyCurtain, TagDormBed.PowerOutlet],
						JsonData = new DormBedJson { Description = "تخت با تشک طبی." }
					});

					// pattern per bed: 0 = active contract, 1 = expired contract, 2 = free (no contract); inactive dorms have none
					int pattern = (b + roomNumber + dormIndex) % 3;
					if (d.Tags.Contains(TagDorm.Inactive) || pattern == 2) continue;

					bool active = pattern == 0;
					DateTime start = active ? today.AddMonths(-2).AddDays(-b) : today.AddMonths(-10);
					DateTime end = active ? today.AddMonths(4) : today.AddMonths(-4);
					Guid userId = UserAt(6 + residentIndex++);
					Guid contractId = Guid.CreateVersion7();
					contracts.Add(new DormBedContractEntity {
						Id = contractId, CreatedAt = start.AddDays(-5), CreatorId = adminId, Tags = [TagDormBedContract.Monthly],
						StartDate = start, EndDate = end, Deposit = r.Deposit, Rent = r.Rent, UserId = userId, BedId = bedId, JsonData = new DormBedContractJson()
					});
					dormInvoices.Add(new DormBedInvoiceEntity {
						Id = Guid.CreateVersion7(), CreatedAt = start.AddDays(-5), CreatorId = adminId, Tags = [TagDormBedInvoice.Deposit, TagDormBedInvoice.Paid, TagDormBedInvoice.PaidOnline],
						DebtAmount = r.Deposit, CreditorAmount = 0, PaidAmount = r.Deposit, PenaltyAmount = 0, ContractId = contractId, DueDate = start, JsonData = new DormBedInvoiceJson { PenaltyPrecentEveryDate = 1 }
					});
					bool lateResident = active && b == 0; // the first active resident of every room is late with the last rent
					for (int m = 0; start.AddMonths(m) < end; m++) {
						DateTime due = start.AddMonths(m);
						bool inPast = due < today;
						bool late = lateResident && inPast && due.AddMonths(1) >= today;
						bool paid = inPast && !late;
						decimal penalty = late ? 10000 : 0;
						dormInvoices.Add(new DormBedInvoiceEntity {
							Id = Guid.CreateVersion7(), CreatedAt = due.AddDays(-7), CreatorId = adminId,
							Tags = paid ? [TagDormBedInvoice.Rent, TagDormBedInvoice.Paid, m % 2 == 0 ? TagDormBedInvoice.PaidOnline : TagDormBedInvoice.PaidManual] : [TagDormBedInvoice.Rent, TagDormBedInvoice.NotPaid],
							DebtAmount = r.Rent, CreditorAmount = 0, PaidAmount = paid ? r.Rent : 0, PenaltyAmount = penalty, ContractId = contractId, DueDate = due,
							JsonData = new DormBedInvoiceJson { PenaltyPrecentEveryDate = 1 }
						});
					}
				}
			}

			if (!d.Tags.Contains(TagDorm.Inactive)) {
				string[] texts = ["نزدیک دانشگاه و امن؛ سرپرست خوبی دارد.", "اینترنت پایدار و اتاق مطالعه‌ی عالی، قیمت منطقی.", "تمیز و آرام است؛ فقط آشپزخانه در ساعات اوج شلوغ می‌شود."];
				for (int i = 0; i < texts.Length; i++) comments.Add(Review(null, dormId, dormIndex * 3 + i + 2, i == 2 ? 4m : 5m - i * 0.5m, texts[i], TagComment.Released, 8 + i * 12));
			}

			dormIndex++;
		}

		// ---------------------------------------------------------------- save everything in one transaction
		await db.Set<UserEntity>().AddRangeAsync(users, ct);
		await db.Set<HotelEntity>().AddRangeAsync(hotels, ct);
		await db.Set<HotelRoomEntity>().AddRangeAsync(rooms, ct);
		await db.Set<HotelReservationEntity>().AddRangeAsync(reservations, ct);
		await db.Set<HotelInvoiceEntity>().AddRangeAsync(hotelInvoices, ct);
		await db.Set<DormEntity>().AddRangeAsync(dorms, ct);
		await db.Set<DormRoomEntity>().AddRangeAsync(dormRooms, ct);
		await db.Set<DormBedEntity>().AddRangeAsync(beds, ct);
		await db.Set<DormBedContractEntity>().AddRangeAsync(contracts, ct);
		await db.Set<DormBedInvoiceEntity>().AddRangeAsync(dormInvoices, ct);
		await db.Set<CommentEntity>().AddRangeAsync(comments, ct);
		await db.SaveChangesAsync(ct);

		return new UResponse<List<KeyValue>?>([
			new KeyValue { Key = "users", Value = users.Count.ToString() },
			new KeyValue { Key = "hotels", Value = hotels.Count.ToString() },
			new KeyValue { Key = "hotelRooms", Value = rooms.Count.ToString() },
			new KeyValue { Key = "hotelReservations", Value = reservations.Count.ToString() },
			new KeyValue { Key = "hotelInvoices", Value = hotelInvoices.Count.ToString() },
			new KeyValue { Key = "dorms", Value = dorms.Count.ToString() },
			new KeyValue { Key = "dormRooms", Value = dormRooms.Count.ToString() },
			new KeyValue { Key = "dormBeds", Value = beds.Count.ToString() },
			new KeyValue { Key = "dormContracts", Value = contracts.Count.ToString() },
			new KeyValue { Key = "dormInvoices", Value = dormInvoices.Count.ToString() },
			new KeyValue { Key = "reviews", Value = comments.Count.ToString() },
			new KeyValue { Key = "demoUsersPassword", Value = "Demo1234 (usernames demo01 ... demo12)" }
		], Usc.Created);
	}

	private static ContentEntity BuildContent(
		TagContent tag,
		string title,
		string? subTitle = null,
		string? description = null,
		string detail1 = "",
		string detail2 = "",
		string? buttonText = null,
		string? buttonLink = null,
		string? link = null,
		int? order = null,
		string? instagram = null,
		string? telegram = null,
		string? whatsapp = null,
		string? phone = null,
		List<ContentItem>? items = null,
		List<ContentLink>? links = null
	) => new() {
		Id = Guid.CreateVersion7(),
		CreatedAt = DateTime.UtcNow,
		CreatorId = Core.App.Users.SystemAdmin.Id,
		Tags = [tag],
		JsonData = new ContentJson {
			Title = title,
			SubTitle = subTitle,
			Description = description,
			Detail1 = detail1,
			Detail2 = detail2,
			ButtonText = buttonText,
			ButtonLink = buttonLink,
			Link = link,
			Order = order,
			Instagram = instagram,
			Telegram = telegram,
			Whatsapp = whatsapp,
			Phone = phone,
			Items = items ?? [],
			Links = links ?? []
		}
	};
}