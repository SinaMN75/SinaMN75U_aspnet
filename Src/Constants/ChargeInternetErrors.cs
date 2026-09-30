namespace SinaMN75U.Constants;

// Turns Mobtakeran (charge / internet package provider) response codes into messages an end user can understand.
// Mobtakeran's own "message" field is written for the merchant (e.g. "your account balance is not enough", "token is invalid"),
// so it is never shown to the user directly.
public static class ChargeInternetErrors {
	private const string ServiceUnavailable = "سرویس خرید شارژ و بسته اینترنت موقتاً در دسترس نیست. لطفاً چند دقیقه دیگر دوباره تلاش کنید.";
	private const string OperatorUnavailable = "سرویس اپراتور در حال حاضر پاسخ‌گو نیست. لطفاً چند دقیقه دیگر دوباره تلاش کنید.";
	private const string TryAgain = "انجام درخواست با خطا مواجه شد. لطفاً دوباره تلاش کنید.";
	private const string InvalidProduct = "این محصول در حال حاضر قابل خرید نیست. لطفاً مورد دیگری را انتخاب کنید.";
	private const string SubscriberInactive = "سیم‌کارت این شماره فعال نیست یا امکان شارژ آن وجود ندارد.";
	private const string SubscriberNotFound = "شماره موبایل وارد شده در سامانه اپراتور یافت نشد.";
	private const string Pending = "تراکنش شما در حال پردازش است. نتیجه تا چند دقیقه دیگر مشخص می‌شود و در صورت ناموفق بودن، مبلغ به کیف پول شما بازمی‌گردد.";

	// Codes of the Mobtakeran charge system itself (the "code" field).
	private static readonly Dictionary<int, string> Mobtakeran = new() {
		{ 0, ServiceUnavailable },
		{ 3, ServiceUnavailable },
		{ 4, InvalidProduct },
		{ 6, TryAgain },
		{ 9, TryAgain },
		{ 10, ServiceUnavailable },
		{ 11, "اپراتور انتخاب‌شده با این شماره موبایل همخوانی ندارد. اگر سیم‌کارت را ترابرد کرده‌اید، اپراتور فعلی آن را انتخاب کنید." },
		{ 12, "مبلغ انتخاب‌شده برای این اپراتور معتبر نیست. لطفاً مبلغ دیگری را انتخاب کنید." },
		{ 13, "شماره موبایل وارد شده معتبر نیست." },
		{ 14, InvalidProduct },
		{ 15, TryAgain },
		{ 16, TryAgain },
		{ 17, ServiceUnavailable },
		{ 18, ServiceUnavailable },
		{ 19, TryAgain },
		{ 20, "این درخواست قابل انجام نیست. لطفاً از تکرار آن خودداری کنید." },
		{ 21, InvalidProduct },
		{ 22, "بسته اینترنت انتخاب‌شده دیگر موجود نیست. لطفاً فهرست بسته‌ها را دوباره باز کنید." },
		{ 23, TryAgain },
		{ 25, "این درخواست تکراری است." },
		{ 26, InvalidProduct },
		{ 27, OperatorUnavailable },
		{ 28, "درخواست قبلی شما هنوز در حال انجام است. لطفاً چند لحظه صبر کنید." },
		{ 30, ServiceUnavailable },
		{ 31, ServiceUnavailable },
		{ 32, ServiceUnavailable },
		{ 33, ServiceUnavailable },
		{ 34, ServiceUnavailable },
		{ 35, ServiceUnavailable },
		{ 39, ServiceUnavailable },
		{ 40, "این تراکنش قبلاً ناموفق بوده است." },
		{ 41, "این تراکنش قبلاً با موفقیت انجام شده است." },
		{ 50, ServiceUnavailable },
		{ 51, ServiceUnavailable },
		{ 55, ServiceUnavailable },
		{ 56, ServiceUnavailable },
		{ 60, "زمان انجام درخواست به پایان رسید. لطفاً دوباره تلاش کنید." },
		{ 64, OperatorUnavailable },
		{ 70, Pending },
		{ 71, Pending },
		{ 72, "تراکنش ناموفق بود." },
		{ 73, Pending },
		{ 74, "تراکنش ناموفق بود." },
		{ 75, Pending },
		{ 84, OperatorUnavailable },
		{ 100, ServiceUnavailable },
		{ 305, "این تراکنش قبلاً ناموفق بوده است." },
		{ 306, Pending },
		{ 401, OperatorUnavailable },
		{ 420, "تراکنش ناموفق بود." },
		{ 451, ServiceUnavailable },
		{ 511, OperatorUnavailable },
		{ 600, "این محصول با نوع سیم‌کارت شما (دائمی / اعتباری) سازگار نیست. لطفاً مورد دیگری را انتخاب کنید." },
		{ 611, OperatorUnavailable },
		{ 711, OperatorUnavailable },
		{ 747, Pending },
		{ 750, "تعداد درخواست‌ها زیاد است. لطفاً چند لحظه دیگر دوباره تلاش کنید." }
	};

	// Operator error codes (the "ext_code" field) that mean something to the subscriber. Codes that are about the
	// merchant's own account (quota, credit, permissions, ...) are left out on purpose and fall back to a general message.
	private static readonly Dictionary<int, string> HamrahAvval = new() {
		{ -1003, "مبلغ انتخاب‌شده برای همراه اول معتبر نیست." },
		{ -1008, SubscriberNotFound },
		{ -1011, OperatorUnavailable },
		{ -1012, "امکان شارژ این شماره وجود ندارد (شماره در فهرست سیاه اپراتور است)." },
		{ -1013, SubscriberInactive },
		{ -1014, "اعتبار فعلی این سیم‌کارت بیش از سقف مجاز است و امکان شارژ بیشتر وجود ندارد." },
		{ -1015, OperatorUnavailable },
		{ -1016, OperatorUnavailable },
		{ -1017, "یک درخواست شارژ برای این شماره در حال انجام است. لطفاً چند لحظه صبر کنید." },
		{ -1021, InvalidProduct },
		{ -1023, "شارژ جوانان فقط برای مشترکین زیر ۲۵ سال قابل خرید است." },
		{ -1027, "این شماره مشمول شارژ بانوان نیست." },
		{ -1028, "این شماره مشمول شارژ وفاداری نیست." },
		{ -1031, InvalidProduct },
		{ -1033, InvalidProduct },
		{ -1034, "مبلغ بسته معتبر نیست. لطفاً فهرست بسته‌ها را دوباره باز کنید." },
		{ -1035, "شارژ مستقیم برای سیم‌کارت دائمی امکان‌پذیر نیست." },
		{ -1037, "شارژ بانوان با این مبلغ امکان‌پذیر نیست." },
		{ -1039, "به دلیل فعال بودن بسته اینترنت فعلی، در حال حاضر امکان خرید بسته جدید برای این شماره وجود ندارد." },
		{ -1040, "بسته دیگری برای این شماره رزرو شده است. لطفاً کمی بعد دوباره تلاش کنید." },
		{ -1042, SubscriberInactive },
		{ -1043, SubscriberInactive },
		{ -1044, SubscriberInactive },
		{ -1045, SubscriberInactive },
		{ -1046, "امکان فعال‌سازی این بسته برای این شماره وجود ندارد." },
		{ -1047, SubscriberInactive },
		{ -1048, SubscriberInactive },
		{ -1049, "این بسته فقط برای سیم‌کارت‌های دائمی است." },
		{ -1050, "این بسته فقط برای سیم‌کارت‌های اعتباری است." },
		{ -1052, "این بسته فقط برای مشترکین جدید است." },
		{ -1053, "امکان شارژ سیم‌کارت‌های تالیا از این درگاه وجود ندارد." },
		{ -1060, "این بسته برای سیم‌کارت‌های دانش‌آموزی (انارستان) قابل خرید نیست." },
		{ -1061, "این بسته فقط برای اعضای باشگاه مشتریان همراه اول است. برای عضویت به my.mci.ir مراجعه کنید." },
		{ -1063, InvalidProduct },
		{ -1070, "به دلیل ترابرد این شماره، امکان انجام درخواست وجود ندارد. اپراتور فعلی سیم‌کارت را انتخاب کنید." },
		{ -1072, "یک درخواست دیگر برای این شماره در حال پردازش است. لطفاً چند لحظه صبر کنید." },
		{ -1078, "مبلغ شارژ با نوع شارژ انتخاب‌شده همخوانی ندارد." },
		{ -1079, "این نوع شارژ برای مشترکین روستایی قابل خرید نیست." },
		{ -1080, TryAgain },
		{ -1087, "خرید بسته انارستان برای شماره دیگران امکان‌پذیر نیست." },
		{ -1088, "این درخواست برای این سیم‌کارت مجاز نیست." },
		{ -1100, "با توجه به نوع سیم‌کارت شما، امکان انجام این درخواست وجود ندارد." },
		{ -1102, "این سرویس ویژه مشترکین انارستان است." },
		{ -1112, "حداقل مبلغ خرید شارژ همراه اول ۵,۰۰۰ تومان است." },
		{ -1115, "هر کد ملی در هر ماه فقط یک بار می‌تواند این بسته را خریداری کند." },
		{ -1140, "فعال‌سازی این بسته همراه با بسته فعلی شما امکان‌پذیر نیست." },
		{ -1144, "این بسته فقط برای سیم‌کارت‌های نسل جدید (یوسیم) قابل استفاده است." },
		{ -1167, "در حال حاضر امکان فعال‌سازی این بسته برای شما وجود ندارد." },
		{ -25228, TryAgain },
		{ -9009, TryAgain },
		{ 7, TryAgain },
		{ 405610009, SubscriberNotFound },
		{ 14809002, SubscriberNotFound }
	};

	private static readonly Dictionary<int, string> IranCell = new() {
		{ 2, TryAgain },
		{ 21, "مبلغ انتخاب‌شده کمتر از حد مجاز است." },
		{ 22, "مبلغ انتخاب‌شده بیشتر از حد مجاز است." },
		{ 23, "این شماره ایرانسل غیرفعال است یا ثبت نشده است." },
		{ 25, "این تراکنش قبلاً پردازش شده است." },
		{ 33, SubscriberNotFound },
		{ 35, SubscriberInactive },
		{ 37, "امکان شارژ این سری از شماره‌ها وجود ندارد." },
		{ 65, "شماره موبایل وارد شده معتبر نیست." },
		{ 66, "مبلغ انتخاب‌شده معتبر نیست." },
		{ 70, "مبلغ انتخاب‌شده کمتر از حد مجاز است." },
		{ 71, "مبلغ انتخاب‌شده بیشتر از حد مجاز است." },
		{ 76, SubscriberInactive },
		{ 77, SubscriberInactive },
		{ 82, SubscriberInactive },
		{ 89, OperatorUnavailable },
		{ 100, OperatorUnavailable },
		{ 101, OperatorUnavailable },
		{ 102, InvalidProduct },
		{ 104, "مبلغ انتخاب‌شده برای ایرانسل مجاز نیست." },
		{ 105, InvalidProduct },
		{ 107, OperatorUnavailable },
		{ 119, OperatorUnavailable },
		{ 122, "این شماره اعتباری است و امکان پرداخت قبض برای آن وجود ندارد." },
		{ 126, "سقف خرید روزانه این شماره تکمیل شده است." },
		{ 127, "سقف خرید هفتگی این شماره تکمیل شده است." },
		{ 128, "سقف خرید ماهانه این شماره تکمیل شده است." },
		{ 129, "امکان شارژ این شماره وجود ندارد (شماره در فهرست سیاه اپراتور است)." },
		{ 131, InvalidProduct },
		{ 133, "این تراکنش تکراری است." },
		{ 150, OperatorUnavailable },
		{ 160, OperatorUnavailable },
		{ 187, "فعال‌سازی بسته با خطا مواجه شد." },
		{ 408, OperatorUnavailable },
		{ 2041, "این سری از شماره‌ها توسط ایرانسل پشتیبانی نمی‌شود." },
		{ 20006, OperatorUnavailable },
		{ 20008, OperatorUnavailable }
	};

	private static readonly Dictionary<int, string> Rightel = new() {
		{ 1, OperatorUnavailable },
		{ 1150, "شماره موبایل وارد شده معتبر نیست." },
		{ 1780, "شماره وارد شده در رایتل وجود ندارد یا غیرفعال است." },
		{ 2020, "درخواست قبلی این شماره در حال پردازش است. لطفاً کمی بعد دوباره تلاش کنید." },
		{ 2050, "به دلیل خرید اخیر، در حال حاضر امکان خرید مجدد برای این شماره وجود ندارد. لطفاً کمی بعد دوباره تلاش کنید." },
		{ 2052, "سقف خرید روزانه این شماره تکمیل شده است." },
		{ 2071, "مبلغ انتخاب‌شده برای رایتل مجاز نیست." },
		{ 2130, "در شارژ شورانگیز فقط مبالغ ۲۰۰۰۰، ۵۰۰۰۰، ۱۰۰۰۰۰، ۲۰۰۰۰۰، ۵۰۰۰۰۰ و ۱۰۰۰۰۰۰ ریال قابل خرید است." },
		{ 5100, OperatorUnavailable },
		{ 5110, "سیم‌کارت این شماره سلب امتیاز شده است." },
		{ 5120, "این شماره اعتباری نیست و امکان شارژ مستقیم آن وجود ندارد." },
		{ 5130, "سیم‌کارت این شماره مشکل دارد. لطفاً با مرکز تماس رایتل تماس بگیرید." },
		{ 616, OperatorUnavailable },
		{ 8512, "مبلغ انتخاب‌شده بیشتر از حد مجاز است." },
		{ 9007, OperatorUnavailable },
		{ 9967, OperatorUnavailable },
		{ 13000, TryAgain },
		{ 17001, "این درخواست برای این شماره مجاز نیست (برای مثال نوع بسته با نوع سیم‌کارت دائمی / اعتباری همخوانی ندارد)." },
		{ 17005, "نوع بسته (دائمی / اعتباری) با سیم‌کارت شما همخوانی ندارد." },
		{ 17008, "این شماره قطع دوطرفه است." },
		{ 17014, OperatorUnavailable },
		{ 17017, "شماره موبایل وارد شده معتبر نیست." },
		{ 19308, InvalidProduct },
		{ 19309, "این شماره موبایل در رایتل وجود ندارد." },
		{ 44011, SubscriberInactive },
		{ 44047, InvalidProduct }
	};

	private static readonly Dictionary<int, string> Shatel = new() {
		{ 105, SubscriberNotFound },
		{ 110, "این شماره مسدود است." },
		{ 111, "امکان شارژ این شماره وجود ندارد (شماره در فهرست سیاه اپراتور است)." },
		{ 112, InvalidProduct },
		{ 113, "این شماره به اپراتور دیگری ترابرد شده است." },
		{ 115, InvalidProduct },
		{ 117, "مبلغ انتخاب‌شده معتبر نیست." },
		{ 119, "این مورد قبلاً پرداخت شده است." },
		{ 122, "سقف خرید برای این شماره تکمیل شده است." }
	};

	// Builds the message to show the user. The operator error (ext_code) is more specific, so it wins when it is known;
	// otherwise the Mobtakeran code decides. operatorId is the operator we called, used when message_source is missing.
	public static string Message(int? code, string? extCode, string? messageSource, string? operatorId = null) {
		if (int.TryParse(extCode, out int ext) && ext != 0) {
			Dictionary<int, string>? operatorErrors = Source(messageSource, operatorId) switch {
				1 => HamrahAvval,
				2 => IranCell,
				3 => Rightel,
				5 => Shatel,
				_ => null
			};
			if (operatorErrors != null && operatorErrors.TryGetValue(ext, out string? operatorMessage)) return operatorMessage;
		}

		if (code.HasValue && Mobtakeran.TryGetValue(code.Value, out string? message)) return message;
		return TryAgain;
	}

	// message_source is documented as "the issuer of the error". It may come as the numeric code of the table in the
	// document (0 Mobtakeran, 1 MCI, 2 Irancell, 3 Rightel, 5 Shatel) or as a name, so both are accepted.
	private static int? Source(string? messageSource, string? operatorId) {
		string s = (messageSource ?? "").Trim().ToLowerInvariant();
		if (int.TryParse(s, out int number)) return number == 0 ? null : number;
		if (s.Contains("mci") || s.Contains("hamrah") || s.Contains("همراه")) return 1;
		if (s.Contains("mtn") || s.Contains("irancell") || s.Contains("ایرانسل")) return 2;
		if (s.Contains("rightel") || s.Contains("رایتل")) return 3;
		if (s.Contains("shatel") || s.Contains("شاتل")) return 5;
		if (s.Contains("mobtakeran") || s.Contains("مبتکران")) return null;
		return int.TryParse(operatorId, out int op) ? op : null;
	}
}
