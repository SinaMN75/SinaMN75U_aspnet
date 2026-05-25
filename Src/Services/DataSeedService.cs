namespace SinaMN75U.Services;

public interface IDataSeedService {
	Task<UResponse> SeedUsers();
	Task<UResponse> SeedCategories();
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
}