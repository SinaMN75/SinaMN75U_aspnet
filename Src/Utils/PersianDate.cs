namespace SinaMN75U.Utils;

public readonly partial struct PersianDateTime : IComparable<PersianDateTime>, IEquatable<PersianDateTime> {
	private static readonly PersianCalendar Pc = new();
	public int Year { get; }
	public int Month { get; }
	public int Day { get; }
	public int Hour { get; }
	public int Minute { get; }
	public int Second { get; }
	public int Millisecond { get; }
	public DateTimeKind Kind { get; }

	#region Constructors / Factory

	public PersianDateTime(int year, int month, int day, int hour = 0, int minute = 0, int second = 0, int millisecond = 0, DateTimeKind kind = DateTimeKind.Unspecified) {
		if (year < 1 || year > 9999) throw new ArgumentOutOfRangeException(nameof(year));
		if (month < 1 || month > 12) throw new ArgumentOutOfRangeException(nameof(month));
		int dim = DaysInMonth(year, month);
		if (day < 1 || day > dim) throw new ArgumentOutOfRangeException(nameof(day));
		if (hour < 0 || hour > 23) throw new ArgumentOutOfRangeException(nameof(hour));
		if (minute < 0 || minute > 59) throw new ArgumentOutOfRangeException(nameof(minute));
		if (second < 0 || second > 59) throw new ArgumentOutOfRangeException(nameof(second));
		if (millisecond < 0 || millisecond > 999) throw new ArgumentOutOfRangeException(nameof(millisecond));

		Year = year;
		Month = month;
		Day = day;
		Hour = hour;
		Minute = minute;
		Second = second;
		Millisecond = millisecond;
		Kind = kind;
	}

	public PersianDateTime(DateTime dateTime) {
		Year = Pc.GetYear(dateTime);
		Month = Pc.GetMonth(dateTime);
		Day = Pc.GetDayOfMonth(dateTime);
		Hour = Pc.GetHour(dateTime);
		Minute = Pc.GetMinute(dateTime);
		Second = Pc.GetSecond(dateTime);
		Millisecond = dateTime.Millisecond;
		Kind = dateTime.Kind;
	}

	#endregion

	#region Static Now / Today / UtcNow

	public static PersianDateTime Now => new(DateTime.Now);

	public static PersianDateTime UtcNow => new(DateTime.UtcNow);

	public static PersianDateTime Today => new(DateTime.Now.Date);

	public PersianDateTime DateOnly => new(Year, Month, Day, 0, 0, 0, 0, Kind);

	#endregion

	#region Conversions

	public DateTime ToDateTime() {
		return Pc.ToDateTime(Year, Month, Day, Hour, Minute, Second, Millisecond);
	}

	public DateTime ToDateTime(DateTimeKind kind) {
		DateTime dt = ToDateTime();
		if (kind == DateTimeKind.Unspecified) return DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
		if (kind == DateTimeKind.Utc) return DateTime.SpecifyKind(dt, DateTimeKind.Utc).ToUniversalTime();
		return DateTime.SpecifyKind(dt, DateTimeKind.Local).ToLocalTime();
	}

	public static PersianDateTime FromDateTime(DateTime dt) => new(dt);

	public static PersianDateTime FromUnixTimeSeconds(long unixSeconds) {
		DateTime dt = DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime;
		return new PersianDateTime(dt);
	}

	public long ToUnixTimeSeconds() {
		DateTime dt = ToDateTime();
		return new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc)).ToUnixTimeSeconds();
	}

	#endregion

	#region Arithmetic

	public PersianDateTime AddYears(int years) {
		DateTime dt = ToDateTime();
		DateTime newDt = Pc.AddYears(dt, years);
		return new PersianDateTime(newDt);
	}

	public PersianDateTime AddMonths(int months) {
		DateTime dt = ToDateTime();
		DateTime newDt = Pc.AddMonths(dt, months);
		return new PersianDateTime(newDt);
	}

	public PersianDateTime AddDays(double days) {
		DateTime dt = ToDateTime().AddDays(days);
		return new PersianDateTime(dt);
	}

	public PersianDateTime AddHours(double hours) {
		DateTime dt = ToDateTime().AddHours(hours);
		return new PersianDateTime(dt);
	}

	public PersianDateTime AddMinutes(double minutes) {
		DateTime dt = ToDateTime().AddMinutes(minutes);
		return new PersianDateTime(dt);
	}

	public PersianDateTime AddSeconds(double seconds) {
		DateTime dt = ToDateTime().AddSeconds(seconds);
		return new PersianDateTime(dt);
	}

	public PersianDateTime AddMilliseconds(double ms) {
		DateTime dt = ToDateTime().AddMilliseconds(ms);
		return new PersianDateTime(dt);
	}

	public PersianDateTime Add(TimeSpan ts) => AddMilliseconds(ts.TotalMilliseconds);

	#endregion

	#region Start/End helpers

	public PersianDateTime StartOfDay => new(Year, Month, Day, 0, 0, 0, 0, Kind);

	public PersianDateTime EndOfDay => new(Year, Month, Day, 23, 59, 59, 999, Kind);

	public PersianDateTime StartOfMonth => new(Year, Month, 1, 0, 0, 0, 0, Kind);

	public PersianDateTime EndOfMonth {
		get {
			int dim = DaysInMonth(Year, Month);
			return new PersianDateTime(Year, Month, dim, 23, 59, 59, 999, Kind);
		}
	}

	public PersianDateTime StartOfYear => new(Year, 1, 1, 0, 0, 0, 0, Kind);

	public PersianDateTime EndOfYear {
		get {
			int dim = DaysInMonth(Year, 12);
			return new PersianDateTime(Year, 12, dim, 23, 59, 59, 999, Kind);
		}
	}

	#endregion

	#region Utilities: Leap, DaysInMonth, Weekday

	public static bool IsLeapYear(int persianYear) {
		return Pc.IsLeapYear(persianYear);
	}

	public bool IsLeapYear() => IsLeapYear(Year);

	public static int DaysInMonth(int persianYear, int persianMonth) {
		if (persianMonth < 1 || persianMonth > 12) throw new ArgumentOutOfRangeException(nameof(persianMonth));

		if (persianMonth <= 6) return 31;
		if (persianMonth <= 11) return 30;
		return Pc.IsLeapYear(persianYear) ? 30 : 29;
	}

	public int DaysInMonth() => DaysInMonth(Year, Month);

	public DayOfWeek DayOfWeek => ToDateTime().DayOfWeek;

	public static string GetPersianDayName(DayOfWeek dow) {
		return dow switch {
			DayOfWeek.Sunday => "یکشنبه",
			DayOfWeek.Monday => "دوشنبه",
			DayOfWeek.Tuesday => "سه‌شنبه",
			DayOfWeek.Wednesday => "چهارشنبه",
			DayOfWeek.Thursday => "پنجشنبه",
			DayOfWeek.Friday => "جمعه",
			DayOfWeek.Saturday => "شنبه",
			_ => dow.ToString()
		};
	}

	public static string[] PersianMonthNamesFarsi { get; } = new[] {
		"فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
		"مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
	};

	public static string[] PersianMonthNamesLatin { get; } = new[] {
		"Farvardin", "Ordibehesht", "Khordad", "Tir", "Mordad", "Shahrivar",
		"Mehr", "Aban", "Azar", "Dey", "Bahman", "Esfand"
	};

	public static string GetMonthName(int month, bool farsi = true) {
		if (month is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(month));
		return farsi ? PersianMonthNamesFarsi[month - 1] : PersianMonthNamesLatin[month - 1];
	}

	#endregion

	#region Parsing / Formatting

	public static string ReplacePersianDigits(string input) {
		if (string.IsNullOrEmpty(input)) return input;
		StringBuilder sb = new(input.Length);
		foreach (char ch in input) {
			switch (ch) {
				case >= '\u06F0' and <= '\u06F9':
					sb.Append((char)('0' + (ch - '\u06F0')));
					break;

				case >= '\u0660' and <= '\u0669':
					sb.Append((char)('0' + (ch - '\u0660')));
					break;
				default:
					sb.Append(ch);
					break;
			}
		}

		return sb.ToString();
	}

	public string ToString(string format, bool usePersianDigits = false, bool monthNameFarsi = true) {
		if (string.IsNullOrEmpty(format)) format = "yyyy/MM/dd HH:mm:ss";


		StringBuilder result = new();
		for (int i = 0; i < format.Length;) {
			if (i + 4 <= format.Length && format.Substring(i, 4) == "yyyy") {
				result.Append(Year.ToString("D4"));
				i += 4;
				continue;
			}

			if (i + 4 <= format.Length && format.Substring(i, 4) == "dddd") {
				result.Append(GetPersianDayName(DayOfWeek));
				i += 4;
				continue;
			}

			if (i + 4 <= format.Length && format.Substring(i, 4) == "MMMM") {
				result.Append(GetMonthName(Month, monthNameFarsi));
				i += 4;
				continue;
			}

			if (i + 3 <= format.Length && format.Substring(i, 3) == "MMM") {
				result.Append(monthNameFarsi
					? GetMonthName(Month)[..Math.Min(3, GetMonthName(Month).Length)]
					: GetMonthName(Month, false)[..Math.Min(3, GetMonthName(Month, false).Length)]);
				i += 3;
				continue;
			}

			if (i + 2 <= format.Length && format.Substring(i, 2) == "yy") {
				result.Append((Year % 100).ToString("D2"));
				i += 2;
				continue;
			}

			if (i + 2 <= format.Length && format.Substring(i, 2) == "MM") {
				result.Append(Month.ToString("D2"));
				i += 2;
				continue;
			}

			if (i + 2 <= format.Length && format.Substring(i, 2) == "dd") {
				result.Append(Day.ToString("D2"));
				i += 2;
				continue;
			}

			if (i + 2 <= format.Length && format.Substring(i, 2) == "HH") {
				result.Append(Hour.ToString("D2"));
				i += 2;
				continue;
			}

			if (i + 2 <= format.Length && format.Substring(i, 2) == "mm") {
				result.Append(Minute.ToString("D2"));
				i += 2;
				continue;
			}

			if (i + 2 <= format.Length && format.Substring(i, 2) == "ss") {
				result.Append(Second.ToString("D2"));
				i += 2;
				continue;
			}

			if (i + 3 <= format.Length && format.Substring(i, 3) == "fff") {
				result.Append(Millisecond.ToString("D3"));
				i += 3;
				continue;
			}


			if (format[i] == 'M') {
				result.Append(Month.ToString());
				i++;
				continue;
			}

			if (format[i] == 'd') {
				result.Append(Day.ToString());
				i++;
				continue;
			}

			if (format[i] == 'H') {
				result.Append(Hour.ToString());
				i++;
				continue;
			}


			result.Append(format[i]);
			i++;
		}

		string outStr = result.ToString();
		if (usePersianDigits) outStr = ToPersianDigits(outStr);
		return outStr;
	}

	public override string ToString() {
		return ToString("yyyy/MM/dd HH:mm:ss");
	}

	public static string ToPersianDigits(string input) {
		if (string.IsNullOrEmpty(input)) return input;
		StringBuilder sb = new(input.Length);
		foreach (char ch in input) {
			if (ch >= '0' && ch <= '9')
				sb.Append((char)('\u06F0' + (ch - '0')));
			else
				sb.Append(ch);
		}

		return sb.ToString();
	}

	public static bool TryParse(string input, out PersianDateTime result, DateTimeKind kind = DateTimeKind.Unspecified) {
		result = default;
		if (string.IsNullOrWhiteSpace(input)) return false;
		input = input.Trim();
		input = ReplacePersianDigits(input);


		string datePart = input;
		string? timePart = null;
		string[] parts = input.Split(' ', 'T');
		if (parts.Length >= 1) datePart = parts[0];
		if (parts.Length >= 2) timePart = parts[1];


		string[] dateTokens = Regex.Split(datePart, @"\D+");
		if (dateTokens.Length < 3) return false;
		if (!int.TryParse(dateTokens[0], out int y)) return false;
		if (!int.TryParse(dateTokens[1], out int m)) return false;
		if (!int.TryParse(dateTokens[2], out int d)) return false;

		int hh = 0, mm = 0, ss = 0, fff = 0;
		if (!string.IsNullOrEmpty(timePart)) {
			string[] timeTokens = Regex.Split(timePart, @"\D+");
			if (timeTokens.Length >= 1 && !string.IsNullOrEmpty(timeTokens[0])) int.TryParse(timeTokens[0], out hh);
			if (timeTokens.Length >= 2 && !string.IsNullOrEmpty(timeTokens[1])) int.TryParse(timeTokens[1], out mm);
			if (timeTokens.Length >= 3 && !string.IsNullOrEmpty(timeTokens[2])) int.TryParse(timeTokens[2], out ss);
			if (timeTokens.Length >= 4 && !string.IsNullOrEmpty(timeTokens[3])) int.TryParse(timeTokens[3], out fff);
		}


		try {
			result = new PersianDateTime(y, m, d, hh, mm, ss, fff, kind);
			return true;
		}
		catch {
			return false;
		}
	}

	public static PersianDateTime Parse(string input, DateTimeKind kind = DateTimeKind.Unspecified) {
		if (TryParse(input, out PersianDateTime pd, kind)) return pd;
		throw new FormatException("Input string was not in a correct Persian date format.");
	}

	#endregion

	#region Comparison / Equality / Operators

	public int CompareTo(PersianDateTime other) {
		return ToDateTime().CompareTo(other.ToDateTime());
	}

	public bool Equals(PersianDateTime other) {
		return Year == other.Year && Month == other.Month && Day == other.Day
		       && Hour == other.Hour && Minute == other.Minute && Second == other.Second && Millisecond == other.Millisecond;
	}

	public override bool Equals(object? obj) => obj is PersianDateTime p && Equals(p);

	public override int GetHashCode() {
		return HashCode.Combine(Year, Month, Day, Hour, Minute, Second, Millisecond);
	}

	public static bool operator ==(PersianDateTime a, PersianDateTime b) => a.Equals(b);
	public static bool operator !=(PersianDateTime a, PersianDateTime b) => !a.Equals(b);
	public static bool operator <(PersianDateTime a, PersianDateTime b) => a.CompareTo(b) < 0;
	public static bool operator >(PersianDateTime a, PersianDateTime b) => a.CompareTo(b) > 0;
	public static bool operator <=(PersianDateTime a, PersianDateTime b) => a.CompareTo(b) <= 0;
	public static bool operator >=(PersianDateTime a, PersianDateTime b) => a.CompareTo(b) >= 0;

	#endregion

	#region Enumerators / Ranges

	public static IEnumerable<PersianDateTime> EnumerateDays(PersianDateTime start, PersianDateTime end, int step = 1) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(step);
        PersianDateTime current = start.StartOfDay;
		while (current <= end.StartOfDay) {
			yield return current;
			current = current.AddDays(step);
		}
	}

	public static IEnumerable<PersianDateTime> EnumerateMonths(PersianDateTime start, PersianDateTime end) {
		PersianDateTime cur = new(start.Year, start.Month, 1);
		PersianDateTime tgt = new(end.Year, end.Month, 1);
		while (cur <= tgt) {
			yield return cur;
			cur = cur.AddMonths(1);
		}
	}

	public static IEnumerable<PersianDateTime> EnumerateYears(int startYear, int endYear) {
		for (int y = startYear; y <= endYear; y++)
			yield return new PersianDateTime(y, 1, 1);
	}

	#endregion

	#region Misc helpers

	public int DayOfYear => Pc.GetDayOfYear(ToDateTime());

	public int WeekOfYear(DayOfWeek firstDayOfWeek = DayOfWeek.Saturday) {
		DateTime dt = ToDateTime();
		CultureInfo ci = CultureInfo.InvariantCulture;
		Calendar cal = ci.Calendar;
		return cal.GetWeekOfYear(dt, CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek);
	}

	public bool IsSameDate(PersianDateTime other) => Year == other.Year && Month == other.Month && Day == other.Day;

	public TimeSpan Subtract(PersianDateTime other) => ToDateTime() - other.ToDateTime();

	public string ToRelativeString(bool farsi = true) {
		DateTime now = DateTime.Now;
		TimeSpan diff = now - ToDateTime();
		if (diff.TotalSeconds < 0) {
			TimeSpan adiff = diff.Duration();

			if (adiff.TotalSeconds < 60) return farsi ? "در چند ثانیه" : "in a few seconds";
			if (adiff.TotalMinutes < 60) return farsi ? $"در {ToPersianDigits(((int)adiff.TotalMinutes).ToString())} دقیقه" : $"in {(int)adiff.TotalMinutes} minutes";
			if (adiff.TotalHours < 24) return farsi ? $"در {ToPersianDigits(((int)adiff.TotalHours).ToString())} ساعت" : $"in {(int)adiff.TotalHours} hours";
			if (adiff.TotalDays < 30) return farsi ? $"در {ToPersianDigits(((int)adiff.TotalDays).ToString())} روز" : $"in {(int)adiff.TotalDays} days";
			return farsi ? "در آینده" : "in the future";
		}
		else {
			if (diff.TotalSeconds < 60) return farsi ? "چند ثانیه پیش" : "a few seconds ago";
			if (diff.TotalMinutes < 60) return farsi ? $"{ToPersianDigits(((int)diff.TotalMinutes).ToString())} دقیقه پیش" : $"{(int)diff.TotalMinutes} minutes ago";
			if (diff.TotalHours < 24) return farsi ? $"{ToPersianDigits(((int)diff.TotalHours).ToString())} ساعت پیش" : $"{(int)diff.TotalHours} hours ago";
			if (diff.TotalDays < 30) return farsi ? $"{ToPersianDigits(((int)diff.TotalDays).ToString())} روز پیش" : $"{(int)diff.TotalDays} days ago";
			return farsi ? "مدتی پیش" : "some time ago";
		}
	}

	public string ToLongDateString(bool usePersianDigits = true) {
		string monthName = GetMonthName(Month);
		string dayName = GetPersianDayName(DayOfWeek);
		string dayStr = usePersianDigits ? ToPersianDigits(Day.ToString()) : Day.ToString();
		string yearStr = usePersianDigits ? ToPersianDigits(Year.ToString()) : Year.ToString();
		return $"{dayName} {dayStr} {monthName} {yearStr}";
	}

	#endregion

	#region Extensions / Converters as helper static methods

	public static PersianDateTime FromGregorian(DateTime dt) => new(dt);

	public static DateTime ToGregorianDateTime(int year, int month, int day, int hour = 0, int minute = 0, int second = 0, int millisecond = 0) {
		return Pc.ToDateTime(year, month, day, hour, minute, second, millisecond);
	}

	[GeneratedRegex("[\u06F0-\u06F9]", RegexOptions.Compiled)]
	private static partial Regex MyRegex();

	#endregion
}

public static class PersianDateTimeExtensions {
	public static PersianDateTime ToPersian(this DateTime dt) => PersianDateTime.FromDateTime(dt);

	public static string ToPersianString(this DateTime dt, string format = "yyyy/MM/dd HH:mm:ss", bool usePersianDigits = false) {
		PersianDateTime pd = dt.ToPersian();
		return pd.ToString(format, usePersianDigits);
	}

	public static DateTime ParsePersianToDateTime(string persianDateString, DateTimeKind kind = DateTimeKind.Unspecified) {
		PersianDateTime pd = PersianDateTime.Parse(persianDateString, kind);
		return pd.ToDateTime();
	}

	public static bool TryParsePersianToDateTime(string persianDateString, out DateTime dt, DateTimeKind kind = DateTimeKind.Unspecified) {
		if (PersianDateTime.TryParse(persianDateString, out PersianDateTime pd, kind)) {
			dt = pd.ToDateTime();
			return true;
		}

		dt = default;
		return false;
	}
}

public static class PersianDate {
	private static readonly PersianCalendar Pc = new();

	public static string ToJalaliString(DateTime dt) {
		return $"{Pc.GetYear(dt):0000}/{Pc.GetMonth(dt):00}/{Pc.GetDayOfMonth(dt):00}";
	}

	public static string ToJalaliLongString(DateTime dt) {
		string[] months = {
			"فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
			"مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
		};
		string[] weekdays = {
			"یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنجشنبه", "جمعه", "شنبه"
		};

		return $"{weekdays[(int)dt.DayOfWeek]} {Pc.GetDayOfMonth(dt)} {months[Pc.GetMonth(dt) - 1]} {Pc.GetYear(dt)}";
	}

	public static DateTime ToGregorian(int y, int m, int d)
		=> Pc.ToDateTime(y, m, d, 0, 0, 0, 0);

	public static DateTime Parse(string persianDate) {
		string[] parts = persianDate.Split('/', '-', '.');
		int y = int.Parse(parts[0]);
		int m = int.Parse(parts[1]);
		int d = int.Parse(parts[2]);
		return Pc.ToDateTime(y, m, d, 0, 0, 0, 0);
	}

	public static bool IsValid(string persianDate) {
		try {
			Parse(persianDate);
			return true;
		}
		catch {
			return false;
		}
	}

	public static bool IsLeapYear(int year) => Pc.IsLeapYear(year);

	public static int Year(DateTime dt) => Pc.GetYear(dt);

	public static int Month(DateTime dt) => Pc.GetMonth(dt);

	public static int Day(DateTime dt) => Pc.GetDayOfMonth(dt);

	public static string AddDays(string persianDate, int days)
		=> ToJalaliString(Parse(persianDate).AddDays(days));

	public static string AddMonths(string persianDate, int months)
		=> ToJalaliString(Pc.AddMonths(Parse(persianDate), months));

	public static string AddYears(string persianDate, int years)
		=> ToJalaliString(Pc.AddYears(Parse(persianDate), years));

	public static string GetStartOfDay(string persianDate)
		=> ToJalaliString(Parse(persianDate).Date);

	public static string GetEndOfDay(string persianDate) {
		DateTime dt = Parse(persianDate);
		return ToJalaliString(new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59));
	}

	public static string GetStartOfMonth(string persianDate) {
		DateTime dt = Parse(persianDate);
		int y = Year(dt);
		int m = Month(dt);
		return $"{y:0000}/{m:00}/01";
	}

	public static string GetEndOfMonth(string persianDate) {
		DateTime dt = Parse(persianDate);
		int y = Year(dt);
		int m = Month(dt);
		int days = Pc.GetDaysInMonth(y, m);
		return $"{y:0000}/{m:00}/{days:00}";
	}

	public static string GetStartOfYear(string persianDate) {
		DateTime dt = Parse(persianDate);
		return $"{Year(dt):0000}/01/01";
	}

	public static string GetEndOfYear(string persianDate) {
		DateTime dt = Parse(persianDate);
		return $"{Year(dt):0000}/12/29";
	}

	public static bool IsBefore(string p1, string p2) => Parse(p1) < Parse(p2);

	public static bool IsAfter(string p1, string p2) => Parse(p1) > Parse(p2);

	public static bool IsSame(string p1, string p2) => Parse(p1).Date == Parse(p2).Date;
}