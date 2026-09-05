namespace SinaMN75U.Utils;

public class BillParser(ILocalizationService ls) {
	private const int MinIdLength = 6;
	private const int MaxIdLength = 13;
	private const int MinWeight = 2;
	private const int MaxWeight = 7;

	private static readonly Dictionary<string, string> CompanyNames = new() {
		{ "001", "توانیر - برق منطقه‌ای تهران" },
		{ "002", "توانیر - برق منطقه‌ای اصفهان" },
		{ "003", "توانیر - برق منطقه‌ای خراسان" },
		{ "101", "شرکت گاز استان تهران" },
		{ "102", "شرکت گاز استان اصفهان" },
		{ "201", "شرکت آب و فاضلاب تهران" },
		{ "202", "شرکت آب و فاضلاب اصفهان" },
		{ "301", "شرکت مخابرات ایران" }
	};

	private static readonly Dictionary<string, string> ServiceNames = new() {
		{ "1", "آب" },
		{ "2", "برق" },
		{ "3", "گاز" },
		{ "4", "تلفن ثابت" },
		{ "5", "تلفن همراه" },
		{ "6", "سایر" }
	};

	public BillInfoResponse Parse(string billId, string paymentId) {
		BillInfoResponse info = new() { BillId = billId.Trim(), PaymentId = paymentId.Trim() };

		if (info.BillId.Length is < MinIdLength or > MaxIdLength) {
			info.IsValid = false;
			info.Warnings.Add(ls.Get("billIdLengthMustBeBetween6And13Digits"));
			return info;
		}

		if (info.PaymentId.Length is < MinIdLength or > MaxIdLength) {
			info.IsValid = false;
			info.Warnings.Add(ls.Get("paymentIdLengthMustBeBetween6And13Digits"));
			return info;
		}

		if (!IsDigitsOnly(info.BillId)) {
			info.IsValid = false;
			info.Warnings.Add(ls.Get("billIdMustContainDigitsOnly"));
			return info;
		}

		if (!IsDigitsOnly(info.PaymentId)) {
			info.IsValid = false;
			info.Warnings.Add(ls.Get("paymentIdMustContainDigitsOnly"));
			return info;
		}

		try {
			info.CaseCode = info.BillId[..^5];
			info.CompanyCode = info.BillId[^5..^2];
			info.ServiceType = info.BillId[^2..^1];
			info.CheckDigit = info.BillId[^1..];

			string amountPart = info.PaymentId[..^5];
			info.YearDigit = int.Parse(info.PaymentId[^5..^4]);
			info.PeriodCode = int.Parse(info.PaymentId[^4..^2]);
			info.ControlDigit1 = int.Parse(info.PaymentId[^2..^1]);
			info.ControlDigit2 = int.Parse(info.PaymentId[^1..]);

			if (long.TryParse(amountPart, out long amount)) info.BillAmount = amount * 1000;

			if (CompanyNames.TryGetValue(info.CompanyCode, out string? companyName)) info.CompanyName = companyName;
			else info.Warnings.Add(ls.Get("companyCodeIsUnknown"));

			if (ServiceNames.TryGetValue(info.ServiceType, out string? serviceName)) info.ServiceName = serviceName;
			else info.Warnings.Add(ls.Get("serviceTypeCodeIsUnknown"));

			info.IsValid = ValidateCheckDigits(info);
		}
		catch (Exception) {
			info.IsValid = false;
			info.Warnings.Add(ls.Get("billInformationCouldNotBeParsed"));
		}

		return info;
	}

	private bool ValidateCheckDigits(BillInfoResponse info) {
		bool ok = true;

		info.ExpectedCheckDigit = CalculateCheckDigit(info.BillId[..^1]);
		if (info.ExpectedCheckDigit != int.Parse(info.CheckDigit!)) {
			info.Warnings.Add(ls.Get("billIdCheckDigitIsNotValid"));
			ok = false;
		}

		info.ExpectedControlDigit1 = CalculateCheckDigit(info.PaymentId[..^2]);
		if (info.ExpectedControlDigit1 != info.ControlDigit1) {
			info.Warnings.Add(ls.Get("paymentIdFirstControlDigitIsNotValid"));
			ok = false;
		}

		info.ExpectedControlDigit2 = CalculateCheckDigit(info.BillId + info.PaymentId[..^1]);
		if (info.ExpectedControlDigit2 != info.ControlDigit2) {
			info.Warnings.Add(ls.Get("paymentIdSecondControlDigitIsNotValid"));
			ok = false;
		}

		return ok;
	}

	private static int CalculateCheckDigit(string number) {
		int sum = 0;
		int weight = MinWeight;

		for (int i = number.Length - 1; i >= 0; i--) {
			sum += (number[i] - '0') * weight;
			weight = weight == MaxWeight ? MinWeight : weight + 1;
		}

		int remainder = sum % 11;
		return remainder is 0 or 1 ? 0 : 11 - remainder;
	}

	private static bool IsDigitsOnly(string value) => value.All(char.IsAsciiDigit);
}
