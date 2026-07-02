namespace SinaMN75U.Utils;

public class BillParser {
	private static readonly Dictionary<string, string?> CompanyNames = new() {
		{ "001", "توانیر - برق منطقه‌ای تهران" },
		{ "002", "توانیر - برق منطقه‌ای اصفهان" },
		{ "003", "توانیر - برق منطقه‌ای خراسان" },
		{ "101", "شرکت گاز استان تهران" },
		{ "102", "شرکت گاز استان اصفهان" },
		{ "201", "شرکت آب و فاضلاب تهران" },
		{ "202", "شرکت آب و فاضلاب اصفهان" },
		{ "301", "شرکت مخابرات ایران" },
	};

	private static readonly Dictionary<string, string?> ServiceNames = new() {
		{ "1", "آب" },
		{ "2", "برق" },
		{ "3", "گاز" },
		{ "4", "تلفن ثابت" },
		{ "5", "تلفن همراه" },
		{ "6", "سایر" }
	};

	public BillInfoResponse Parse(string billId, string paymentId) {
		BillInfoResponse info = new() { BillId = billId.Trim(), PaymentId = paymentId.Trim() };

		if (info.BillId.Length != 13) {
			info.IsValid = false;
			info.Warnings.Add($"طول شناسه قبض باید 13 رقم باشد (ورودی: {info.BillId.Length} رقم)");
			return info;
		}

		if (info.PaymentId.Length != 13) {
			info.IsValid = false;
			info.Warnings.Add($"طول شناسه پرداخت باید 13 رقم باشد (ورودی: {info.PaymentId.Length} رقم)");
			return info;
		}

		if (!long.TryParse(info.BillId, out _)) {
			info.IsValid = false;
			info.Warnings.Add("شناسه قبض باید فقط شامل اعداد باشد");
			return info;
		}

		if (!long.TryParse(info.PaymentId, out _)) {
			info.IsValid = false;
			info.Warnings.Add("شناسه پرداخت باید فقط شامل اعداد باشد");
			return info;
		}

		try {
			info.CaseCode = info.BillId[..8];
			info.CompanyCode = info.BillId.Substring(8, 3);
			info.ServiceType = info.BillId.Substring(11, 1);
			info.CheckDigit = info.BillId.Substring(12, 1);

			string amountPart = info.PaymentId[..8];
			info.YearDigit = int.Parse(info.PaymentId.Substring(8, 1));
			info.PeriodCode = int.Parse(info.PaymentId.Substring(9, 2));
			info.ControlDigit1 = int.Parse(info.PaymentId.Substring(11, 1));
			info.ControlDigit2 = int.Parse(info.PaymentId.Substring(12, 1));

			if (long.TryParse(amountPart, out long amount)) info.BillAmount = amount * 1000;

			if (CompanyNames.TryGetValue(info.CompanyCode, out string? companyName)) info.CompanyName = companyName;
			else {
				info.CompanyName = $"نامشخص (کد: {info.CompanyCode})";
				info.Warnings.Add($"کد شرکت '{info.CompanyCode}' در دیکشنری یافت نشد");
			}

			if (ServiceNames.TryGetValue(info.ServiceType, out string? serviceName)) info.ServiceName = serviceName;
			else {
				info.ServiceName = $"نامشخص (کد: {info.ServiceType})";
				info.Warnings.Add($"کد خدمات '{info.ServiceType}' در دیکشنری یافت نشد");
			}

			info.IsValid = ValidateCheckDigits(info);
		}
		catch (Exception ex) {
			info.IsValid = false;
			info.Warnings.Add($"خطا در پردازش: {ex.Message}");
		}

		return info;
	}

	private bool ValidateCheckDigits(BillInfoResponse info) {
		bool ok = true;
		try {
			string billWithoutCheck = info.BillId[..12];
			int calculatedCheck = CalculateCheckDigit(billWithoutCheck);
			int actualCheck = int.Parse(info.CheckDigit!);

			if (calculatedCheck != actualCheck) {
				info.Warnings.Add($"رقم کنترلی شناسه قبض نامعتبر است (محاسبه شده: {calculatedCheck}، موجود: {actualCheck})");
				ok = false;
			}

			string paymentWithoutCheck2 = info.PaymentId[..12];
			int calculatedCheck2 = CalculateCheckDigit(paymentWithoutCheck2);

			if (calculatedCheck2 != info.ControlDigit2) {
				info.Warnings.Add($"رقم کنترلی دوم شناسه پرداخت نامعتبر است (محاسبه شده: {calculatedCheck2}، موجود: {info.ControlDigit2})");
				ok = false;
			}
		}
		catch (Exception ex) {
			info.Warnings.Add($"خطا در اعتبارسنجی: {ex.Message}");
			ok = false;
		}

		return ok;
	}

	private static int CalculateCheckDigit(string number) {
		int sum = 0;
		int weight = 2;

		for (int i = number.Length - 1; i >= 0; i--) {
			int digit = number[i] - '0';
			sum += digit * weight;
			weight = weight == 7 ? 2 : weight + 1;
		}

		int remainder = sum % 11;
		return remainder is 0 or 1 ? 0 : 11 - remainder;
	}
}