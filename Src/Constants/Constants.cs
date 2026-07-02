namespace SinaMN75U.Constants;

public static class RouteTags {
	public const string DataSeeder = "api/DataSeeder/";
	public const string Auth = "api/auth/";
	public const string User = "api/user/";
	public const string Media = "api/media/";
	public const string Content = "api/content/";
	public const string Follow = "api/follow/";
	public const string Product = "api/product/";
	public const string Comment = "api/comment/";
	public const string Category = "api/category/";
	public const string Dashboard = "api/dashboard/";
	public const string Accounting = "api/accounting/";
	public const string Ticket = "api/ticket/";
	public const string Txn = "api/txn/";
	public const string Parking = "api/parking/";
	public const string Vehicle = "api/vehicle/";
	public const string Inquiry = "api/inquiry/";
	public const string Address = "api/address/";
	public const string Wallet = "api/wallet/";
	public const string Terminal = "api/terminal/";
	public const string BankAccount = "api/bankAccount/";
	public const string Ipg = "api/Ipg/";
	public const string Sim = "api/Sim/";
	public const string Notification = "api/Notification/";
	public const string Bed = "api/Bed/";
	public const string ChargeInternet = "api/ChargeInternet/";
	public const string Merchant = "api/Merchant/";
	public const string AppSettings = "api/AppSettings/";
	public const string Process = "api/Process/";
	public const string Pn = "api/Pn/";
	public const string Hotel = "api/Hotel/";
	public const string ApiLog = "api/ApiLog/";
}

public static class UConstants {
	public static readonly Guid SystemAdminId = Guid.Parse("019d9545-4e5c-7b80-ab26-53e069c48a73");
	public static readonly Guid ITHubUserId = Guid.Parse("019d9545-4e5c-7ae4-b99f-a2105f785c5b");
	public static readonly Guid AvaPlusUserId = Guid.Parse("019d9545-4e5c-719f-838d-fb27b9321279");
	public static readonly Guid MobtakeranUserId = Guid.Parse("019d9545-4e5c-719f-838d-fb27b9321267");
	public static readonly Guid PnUserId = Guid.Parse("019d9545-4e5c-719f-838d-fb27b9321268");
}

public static class ProcessStepIds {
	public const string AdminApproval = "AdminApproval";
	public const string UserData = "userData";
	public const string UserDocument = "userDocument";
	public const string UserSelfieVideo = "userSelfieVideo";
	public const string UserESignature = "userESignature";
	public const string AwaitingVerification = "awaitingVerification";
	public const string AuthCompleted = "authCompleted";
}

public static class ProcessIds {
	public const string Kyc = "kyc";
	public const string Terminal = "terminal";
	public const string Charge = "charge";
	public const string Internet = "internet";
	public const string VehicleService = "vehicleService";
}

public static class RandomTexts {
	public static List<string> Sentences = [
		"گربه‌ی جدی، بستنی را خیلی آهسته خورد.",
		"فیلِ کوچولو، با دمپایی رفت خرید.",
		"کلاغِ خوش‌تیپ، برای خودش سوت زد.",
		"موشِ بانمک، سوارِ چمدان شد.",
		"پنگوئنِ خسته، روی یخ چُرت زد.",
		"خورشید، امروز کلاه لبه‌دار پوشید.",
		"ماه، پشتِ ابر قایم‌باشک بازی کرد.",
		"زرافه‌ی بلند، از آسانسور جا ماند.",
		"ماهیِ نارنجی، با سبد خرید آمد.",
		"سنجابِ عجول، کفشش را گم کرد.",
		"ابرِ پفکی، روی سقف نشست.",
		"لاک‌پشتِ تندرو، دیر رسید.",
		"گاوِ مهربان، با عینک مطالعه کرد.",
		"خرگوشِ مودب، آرام پرید.",
		"نهنگِ کوچولو، توی لیوان جا شد.",
		"پنگوئنِ باکلاس، رویِ یخ اسکیت کرد!",
		"فیلِ خجالتی، با عینکِ آفتابی قدم زد.",
		"کلاغِ آوازه‌خوان، فالِ حافظ گرفت.",
		"ماهیِ صورتی، با رباتِ چای ساز، دوست شد.",
		"ابرِ پشمالو، برایِ ستاره‌ها قصه گفت.",
		"خرگوشِ فضانورد، رویِ ماه، هویج کاشت.",
		"خورشیدِ شیطون، برایِ زمین چشمک زد.",
		"گربهِ فیلسوف، به جوجه تیغی مشاوره داد.",
		"موشِ خوش‌بین، با هلیکوپترِ کاغذی پرواز کرد.",
		"زرافه‌یِ هنرمند، با دمِ خودش نقاشی کشید.",
		"ستارهِ خسته، درِ پنجره را باز گذاشت.",
		"سنجابِ خیال‌باف، با بادکنک به خواب رفت.",
		"حلزونِ قهرمان، اولِ مسابقه رسید!",
		"لاک‌پشتِ شجاع، با تانکِ آب‌بازی کرد.",
		"نهنگِ گمشده، درِ کمدِ لباس پیدا شد.",
		"کرمِ ابریشم، ژاکتِ ابریشمی بافت.",
		"ببرِ درس‌خوان، زیرِ درختِ موز، مشق نوشت.",
		"پروانهِ خوش‌حال، با چترِ رنگی به پرواز درآمد.",
		"روباهِ حیله‌گر، به مورچه‌ها پیتزا داد.",
		"کُندویِ عسل، با لباسِ رقصِ باله، وارد شد."
	];
}