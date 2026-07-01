namespace SinaMN75U.Utils;

// کلاس اصلی برای ذخیره اطلاعات استخراج شده
public class BillInfoResponse {
	public string BillId { get; set; } // شناسه قبض کامل
	public string PaymentId { get; set; } // شناسه پرداخت کامل

	// اطلاعات استخراج شده از شناسه قبض
	public string CaseCode { get; set; } // کد پرونده (تا 8 رقم)
	public string CompanyCode { get; set; } // کد شرکت (3 رقم)
	public string ServiceType { get; set; } // کد نوع خدمت (1 رقم)
	public string CheckDigit { get; set; } // رقم کنترلی (1 رقم)

	// اطلاعات استخراج شده از شناسه پرداخت
	public long BillAmount { get; set; } // مبلغ قبض (ریال)
	public int YearDigit { get; set; } // یکان سال
	public int PeriodCode { get; set; } // کد دوره (2 رقم)
	public int ControlDigit1 { get; set; } // رقم کنترلی اول
	public int ControlDigit2 { get; set; } // رقم کنترلی دوم

	// اطلاعات تکمیلی
	public string CompanyName { get; set; } // نام شرکت
	public string ServiceName { get; set; } // نام خدمات

	public override string ToString() {
		return $@"
========== اطلاعات قبض ==========
شناسه قبض: {BillId}
شناسه پرداخت: {PaymentId}

--- اطلاعات شناسه قبض ---
کد پرونده: {CaseCode}
کد شرکت: {CompanyCode} ({CompanyName})
نوع خدمت: {ServiceType} ({ServiceName})
رقم کنترلی: {CheckDigit}

--- اطلاعات شناسه پرداخت ---
مبلغ قبض: {BillAmount:N0} ریال
یکان سال: {YearDigit}
کد دوره: {PeriodCode}
رقم کنترلی اول: {ControlDigit1}
رقم کنترلی دوم: {ControlDigit2}
==================================
";
	}
}

// کلاس اصلی برای پردازش و استخراج اطلاعات
public class BillParser {
	// دیکشنری کد شرکت‌ها (فقط چند نمونه)
	private static readonly Dictionary<string, string> CompanyNames = new Dictionary<string, string> {
		{ "001", "توانیر - برق منطقه‌ای تهران" },
		{ "002", "توانیر - برق منطقه‌ای اصفهان" },
		{ "003", "توانیر - برق منطقه‌ای خراسان" },
		{ "101", "شرکت گاز استان تهران" },
		{ "102", "شرکت گاز استان اصفهان" },
		{ "201", "شرکت آب و فاضلاب تهران" },
		{ "202", "شرکت آب و فاضلاب اصفهان" },
		{ "301", "شرکت مخابرات ایران" },
		// شرکت‌های دیگر را می‌توانید اضافه کنید
	};

	// دیکشنری نوع خدمات
	private static readonly Dictionary<string, string> ServiceNames = new Dictionary<string, string> {
		{ "1", "آب" },
		{ "2", "برق" },
		{ "3", "گاز" },
		{ "4", "تلفن ثابت" },
		{ "5", "تلفن همراه" },
		{ "6", "سایر" }
	};

	/// <summary>
	/// استخراج اطلاعات از شناسه قبض و شناسه پرداخت
	/// </summary>
	public BillInfoResponse Parse(string billId, string paymentId) {
		// حذف فاصله‌ها و کاراکترهای اضافی
		billId = billId.Trim();
		paymentId = paymentId.Trim();

		// بررسی طول شناسه‌ها
		if (billId.Length != 13)
			throw new ArgumentException($"طول شناسه قبض باید 13 رقم باشد (ورودی: {billId.Length} رقم)");

		if (paymentId.Length != 13)
			throw new ArgumentException($"طول شناسه پرداخت باید 13 رقم باشد (ورودی: {paymentId.Length} رقم)");

		// بررسی عددی بودن
		if (!long.TryParse(billId, out _))
			throw new ArgumentException("شناسه قبض باید فقط شامل اعداد باشد");

		if (!long.TryParse(paymentId, out _))
			throw new ArgumentException("شناسه پرداخت باید فقط شامل اعداد باشد");

		var info = new BillInfoResponse {
			BillId = billId,
			PaymentId = paymentId
		};

		// ---- پردازش شناسه قبض ----
		// ساختار: [کد پرونده: 8 رقم][کد شرکت: 3 رقم][نوع خدمت: 1 رقم][رقم کنترلی: 1 رقم]
		info.CaseCode = billId.Substring(0, 8);
		info.CompanyCode = billId.Substring(8, 3);
		info.ServiceType = billId.Substring(11, 1);
		info.CheckDigit = billId.Substring(12, 1);

		// ---- پردازش شناسه پرداخت ----
		// ساختار: [مبلغ: 8 رقم][سال: 1 رقم][دوره: 2 رقم][کنترلی1: 1 رقم][کنترلی2: 1 رقم]
		string amountPart = paymentId.Substring(0, 8);
		info.YearDigit = int.Parse(paymentId.Substring(8, 1));
		info.PeriodCode = int.Parse(paymentId.Substring(9, 2));
		info.ControlDigit1 = int.Parse(paymentId.Substring(11, 1));
		info.ControlDigit2 = int.Parse(paymentId.Substring(12, 1));

		// تبدیل مبلغ (سه صفر آخر حذف شده)
		if (long.TryParse(amountPart, out long amount)) {
			info.BillAmount = amount * 1000; // اضافه کردن سه صفر
		}

		// ---- استخراج نام شرکت و خدمات ----
		CompanyNames.TryGetValue(info.CompanyCode, out string companyName);
		info.CompanyName = companyName ?? "نامشخص";

		ServiceNames.TryGetValue(info.ServiceType, out string serviceName);
		info.ServiceName = serviceName ?? "نامشخص";

		// اعتبارسنجی رقم کنترلی (اختیاری)
		ValidateCheckDigits(info);

		return info;
	}

	/// <summary>
	/// اعتبارسنجی رقم‌های کنترلی (اختیاری)
	/// </summary>
	private void ValidateCheckDigits(BillInfoResponse infoResponse) {
		// محاسبه رقم کنترلی شناسه قبض (الگوریتم ساده Luhn)
		string billWithoutCheck = infoResponse.BillId.Substring(0, 12);
		int calculatedCheck = CalculateLuhnCheckDigit(billWithoutCheck);
		int actualCheck = int.Parse(infoResponse.CheckDigit);

		if (calculatedCheck != actualCheck) {
			// این فقط یک هشدار است، قبض همچنان پردازش می‌شود
			Console.WriteLine($"هشدار: رقم کنترلی شناسه قبض نامعتبر است (محاسبه شده: {calculatedCheck}، موجود: {actualCheck})");
		}

		// اعتبارسنجی رقم کنترلی دوم شناسه پرداخت
		string paymentWithoutCheck2 = infoResponse.PaymentId.Substring(0, 12);
		int calculatedCheck2 = CalculateLuhnCheckDigit(paymentWithoutCheck2);

		if (calculatedCheck2 != infoResponse.ControlDigit2) {
			Console.WriteLine($"هشدار: رقم کنترلی دوم شناسه پرداخت نامعتبر است (محاسبه شده: {calculatedCheck2}، موجود: {infoResponse.ControlDigit2})");
		}
	}

	/// <summary>
	/// محاسبه رقم کنترلی به روش Luhn
	/// </summary>
	private int CalculateLuhnCheckDigit(string number) {
		int sum = 0;
		bool alternate = true;

		for (int i = number.Length - 1; i >= 0; i--) {
			int digit = int.Parse(number[i].ToString());
			if (alternate) {
				digit *= 2;
				if (digit > 9)
					digit = digit - 9;
			}

			sum += digit;
			alternate = !alternate;
		}

		return (10 - (sum % 10)) % 10;
	}
}

// مثال استفاده
class Program {
	static void Main(string[] args) {
		try {
			// نمونه شناسه قبض و پرداخت (مقادیر واقعی را جایگزین کنید)
			string billId = "1234567800123"; // 8 رقم پرونده + 3 رقم شرکت + 1 رقم خدمت + 1 رقم کنترلی
			string paymentId = "0012345678912"; // 8 رقم مبلغ + 1 رقم سال + 2 رقم دوره + 2 رقم کنترلی

			var parser = new BillParser();
			BillInfoResponse infoResponse = parser.Parse(billId, paymentId);

			// نمایش اطلاعات
			Console.WriteLine(infoResponse.ToString());

			// دسترسی به اطلاعات به صورت جداگانه
			Console.WriteLine($"\nمبلغ قابل پرداخت: {infoResponse.BillAmount:N0} ریال");
			Console.WriteLine($"شرکت صادرکننده: {infoResponse.CompanyName}");
			Console.WriteLine($"نوع خدمت: {infoResponse.ServiceName}");
			Console.WriteLine($"کد دوره: {infoResponse.PeriodCode}");
			Console.WriteLine($"یکان سال: {infoResponse.YearDigit}");

			// استفاده از اطلاعات برای پرداخت
			// SendToPaymentGateway(info.BillId, info.PaymentId, info.BillAmount);
		}
		catch (Exception ex) {
			Console.WriteLine($"خطا: {ex.Message}");
		}
	}
}