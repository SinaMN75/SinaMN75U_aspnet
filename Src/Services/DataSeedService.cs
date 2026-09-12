namespace SinaMN75U.Services;

public interface IDataSeedService {
	Task<UResponse> SeedUsers();
	Task<UResponse> SeedCategories();
	Task<UResponse> SeedContents();
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
		// One default row per TagContent. Idempotent: a tag already present in the
		// Contents table is skipped, so this endpoint is safe to call repeatedly.
		List<ContentEntity> defaults = [
			BuildContent(
				tag: TagContent.HeroBanner,
				title: "بالش‌ت را بردار، جای خواب اینجاست :)",
				subTitle: "خواب اینجا، سامانه معرفی و تبلیغات خوابگاه و پانسیون.",
				description: "اینجا می‌توانید بهترین خوابگاه‌ها و پانسیون‌های نزدیک خودتان را پیدا کنید. اطلاعات تکمیلی هر اقامتگاه، امکانات، بازخورد دانشجویان و شرایط رزرو در اختیار شماست.",
				buttonText: "جستجوی خوابگاه",
				buttonLink: "/dormitories",
				order: 1
			),
			BuildContent(
				tag: TagContent.AboutSection,
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
				tag: TagContent.ServicesCarousel,
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
				tag: TagContent.LatestBlogPosts,
				title: "آخرین بلاگ",
				subTitle: "تازه‌ترین مطالب و راهنماها",
				link: "/blog",
				order: 4
			),
			BuildContent(
				tag: TagContent.Footer,
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