namespace SinaMN75U.Utils;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Represents a Persian (Shamsi / Jalali) date and time. Internally uses System.Globalization.PersianCalendar
/// for conversions to/from Gregorian <see cref="DateTime"/>.
/// This struct is immutable.
/// </summary>
public readonly partial struct PersianDateTime : IComparable<PersianDateTime>, IEquatable<PersianDateTime> {
	private static readonly PersianCalendar Pc = new();

	/// <summary>Persian (Shamsi) year</summary>
	public int Year { get; }

	/// <summary>Persian month (1..12)</summary>
	public int Month { get; }

	/// <summary>Persian day (1..31)</summary>
	public int Day { get; }

	/// <summary>Hour (0..23)</summary>
	public int Hour { get; }

	/// <summary>Minute (0..59)</summary>
	public int Minute { get; }

	/// <summary>Second (0..59)</summary>
	public int Second { get; }

	/// <summary>Millisecond (0..999)</summary>
	public int Millisecond { get; }

	/// <summary>Underlying DateTimeKind when converted to DateTime.</summary>
	public DateTimeKind Kind { get; }

	#region Constructors / Factory

	/// <summary>Create a PersianDateTime from components. Values validated.</summary>
	public PersianDateTime(int year, int month, int day, int hour = 0, int minute = 0, int second = 0, int millisecond = 0, DateTimeKind kind = DateTimeKind.Unspecified) {
		// Validate ranges using PersianCalendar
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

	/// <summary>Construct from a Gregorian DateTime (converts to Persian calendar fields). Keeps DateTimeKind.</summary>
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

	/// <summary>Current local time as PersianDateTime.</summary>
	public static PersianDateTime Now => new(DateTime.Now);

	/// <summary>Current UTC time as PersianDateTime.</summary>
	public static PersianDateTime UtcNow => new(DateTime.UtcNow);

	/// <summary>Today (date-only) in local time (time components zeroed).</summary>
	public static PersianDateTime Today => new(DateTime.Now.Date);

	/// <summary>Date-only representing the given Gregorian DateTime's date.</summary>
	public PersianDateTime DateOnly => new(Year, Month, Day, 0, 0, 0, 0, Kind);

	#endregion

	#region Conversions

	/// <summary>Convert to Gregorian DateTime (with Kind preserved).</summary>
	public DateTime ToDateTime() {
		// PersianCalendar ToDateTime takes year/month/day/hour/minute/second/millisecond
		return Pc.ToDateTime(Year, Month, Day, Hour, Minute, Second, Millisecond);
		// Note: PersianCalendar returns a DateTime with Kind = Unspecified. We preserve semantic Kind separately when needed.
	}

	/// <summary>Convert to DateTime with requested DateTimeKind (Local/UTC/Unspecified).</summary>
	public DateTime ToDateTime(DateTimeKind kind) {
		DateTime dt = ToDateTime();
		if (kind == DateTimeKind.Unspecified) return DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
		if (kind == DateTimeKind.Utc) return DateTime.SpecifyKind(dt, DateTimeKind.Utc).ToUniversalTime();
		return DateTime.SpecifyKind(dt, DateTimeKind.Local).ToLocalTime();
	}

	/// <summary>Construct from Gregorian DateTime (alias).</summary>
	public static PersianDateTime FromDateTime(DateTime dt) => new(dt);

	/// <summary>Create from Unix seconds (UTC assumed).</summary>
	public static PersianDateTime FromUnixTimeSeconds(long unixSeconds) {
		DateTime dt = DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime;
		return new PersianDateTime(dt);
	}

	/// <summary>To Unix epoch seconds (UTC)</summary>
	public long ToUnixTimeSeconds() {
		DateTime dt = ToDateTime();
		return new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc)).ToUnixTimeSeconds();
	}

	#endregion

	#region Arithmetic

	public PersianDateTime AddYears(int years) {
		// Use PersianCalendar AddYears on equivalent DateTime
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

	/// <summary>Start of current day (00:00:00.000)</summary>
	public PersianDateTime StartOfDay => new(Year, Month, Day, 0, 0, 0, 0, Kind);

	/// <summary>End of current day (23:59:59.999)</summary>
	public PersianDateTime EndOfDay => new(Year, Month, Day, 23, 59, 59, 999, Kind);

	/// <summary>First day of current month (time preserved or zeroed?) returns midnight.</summary>
	public PersianDateTime StartOfMonth => new(Year, Month, 1, 0, 0, 0, 0, Kind);

	/// <summary>Last day of current month (time set to end of day).</summary>
	public PersianDateTime EndOfMonth {
		get {
			int dim = DaysInMonth(Year, Month);
			return new PersianDateTime(Year, Month, dim, 23, 59, 59, 999, Kind);
		}
	}

	/// <summary>First day of the Persian year (Farvardin 1)</summary>
	public PersianDateTime StartOfYear => new(Year, 1, 1, 0, 0, 0, 0, Kind);

	/// <summary>Last day of Persian year</summary>
	public PersianDateTime EndOfYear {
		get {
			int dim = DaysInMonth(Year, 12);
			return new PersianDateTime(Year, 12, dim, 23, 59, 59, 999, Kind);
		}
	}

	#endregion

	#region Utilities: Leap, DaysInMonth, Weekday

	/// <summary>Return true if the specified Persian year is a leap year.</summary>
	public static bool IsLeapYear(int persianYear) {
		return Pc.IsLeapYear(persianYear);
	}

	/// <summary>Return true if this PersianDateTime's year is leap.</summary>
	public bool IsLeapYear() => IsLeapYear(Year);

	/// <summary>Days in a given Persian month/year.</summary>
	public static int DaysInMonth(int persianYear, int persianMonth) {
		if (persianMonth < 1 || persianMonth > 12) throw new ArgumentOutOfRangeException(nameof(persianMonth));
		// Farvardin..Shahrivar (1..6) => 31 days, Mehr..Esfand (7..11) => 30 days, Esfand => 29 or 30 depending on leap.
		if (persianMonth <= 6) return 31;
		if (persianMonth <= 11) return 30;
		return Pc.IsLeapYear(persianYear) ? 30 : 29;
	}

	/// <summary>Days in this instance's month.</summary>
	public int DaysInMonth() => DaysInMonth(Year, Month);

	/// <summary>Get weekday as System.DayOfWeek for underlying Gregorian mapping.</summary>
	public DayOfWeek DayOfWeek => ToDateTime().DayOfWeek;

	/// <summary>Persian weekday names (short/long). Sunday = Yekshanbeh? (Localization: returns Persian strings commonly used)</summary>
	public static string GetPersianDayName(DayOfWeek dow) {
		// Persian week names: "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنجشنبه", "جمعه", "شنبه"
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

	/// <summary>Persian month names (Farsi)</summary>
	public static string[] PersianMonthNamesFarsi { get; } = new[] {
		"فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
		"مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
	};

	/// <summary>Persian month names (transliterated)</summary>
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

	/// <summary>Replace Persian digits (۰..۹) with ASCII digits (0..9).</summary>
	public static string ReplacePersianDigits(string input) {
		if (string.IsNullOrEmpty(input)) return input;
		StringBuilder sb = new(input.Length);
		foreach (char ch in input) {
			switch (ch) {
				case >= '\u06F0' and <= '\u06F9':
					sb.Append((char)('0' + (ch - '\u06F0')));
					break;
				// Arabic-Indic digits
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

	/// <summary>Format tokens supported:
	/// yyyy, yy, MMMM (Persian month name, Farsi), MMM (Latin short), MM, M, dd, d,
	/// dddd (Persian weekday name), HH, H, mm, ss, fff, PersianDigits:true/false via an optional parameter.
	/// Example: pd.ToString("yyyy/MM/dd HH:mm:ss", usePersianDigits:false)
	/// </summary>
	public string ToString(string format, bool usePersianDigits = false, bool monthNameFarsi = true) {
		if (string.IsNullOrEmpty(format)) format = "yyyy/MM/dd HH:mm:ss";

		// Replace tokens carefully to avoid double-replacing; build result progressively.
		// Safer to scan and build token by token:
		StringBuilder result = new();
		for (int i = 0; i < format.Length;) {
			// Match longest tokens first
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

			// single-letter tokens M, d, H
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

			// fallback: copy char
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

	/// <summary>Convert ASCII digits to Persian digits in a string.</summary>
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

	/// <summary>Try parse from a variety of formats like yyyy/MM/dd, yyyy-MM-dd HH:mm:ss, accepts Persian digits</summary>
	public static bool TryParse(string input, out PersianDateTime result, DateTimeKind kind = DateTimeKind.Unspecified) {
		result = default;
		if (string.IsNullOrWhiteSpace(input)) return false;
		input = input.Trim();
		input = ReplacePersianDigits(input);

		// Try a variety of common patterns: date only, date + time
		// Accept separators '/', '-', '.', ' '
		string datePart = input;
		string? timePart = null;
		string[] parts = input.Split(' ', 'T');
		if (parts.Length >= 1) datePart = parts[0];
		if (parts.Length >= 2) timePart = parts[1];

		// Now split datePart by non-digit
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

		// Validate month/day
		try {
			result = new PersianDateTime(y, m, d, hh, mm, ss, fff, kind);
			return true;
		}
		catch {
			return false;
		}
	}

	/// <summary>Parse or throw.</summary>
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
		// mix fields
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

	/// <summary>Enumerate dates from start to end (inclusive) by day step.</summary>
	public static IEnumerable<PersianDateTime> EnumerateDays(PersianDateTime start, PersianDateTime end, int step = 1) {
		if (step <= 0) throw new ArgumentOutOfRangeException(nameof(step));
		PersianDateTime current = start.StartOfDay;
		PersianDateTime target = end.StartOfDay;
		while (current <= target) {
			yield return current;
			current = current.AddDays(step);
		}
	}

	/// <summary>Enumerate months from start (inclusive) to end (inclusive)</summary>
	public static IEnumerable<PersianDateTime> EnumerateMonths(PersianDateTime start, PersianDateTime end) {
		PersianDateTime cur = new(start.Year, start.Month, 1);
		PersianDateTime tgt = new(end.Year, end.Month, 1);
		while (cur <= tgt) {
			yield return cur;
			cur = cur.AddMonths(1);
		}
	}

	/// <summary>Enumerate Persian years from start to end inclusive (year-step 1).</summary>
	public static IEnumerable<PersianDateTime> EnumerateYears(int startYear, int endYear) {
		for (int y = startYear; y <= endYear; y++)
			yield return new PersianDateTime(y, 1, 1);
	}

	#endregion

	#region Misc helpers

	/// <summary>Return day of year in Persian calendar (1..365/366)</summary>
	public int DayOfYear => Pc.GetDayOfYear(ToDateTime());

	/// <summary>Return week of year (ISO-like) based on Gregorian mapping (useful for interoperability)</summary>
	public int WeekOfYear(DayOfWeek firstDayOfWeek = DayOfWeek.Saturday) {
		DateTime dt = ToDateTime();
		CultureInfo ci = CultureInfo.InvariantCulture;
		Calendar cal = ci.Calendar;
		return cal.GetWeekOfYear(dt, CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek);
	}

	/// <summary>Return whether two PersianDateTime values are on same Persian calendar day.</summary>
	public bool IsSameDate(PersianDateTime other) => Year == other.Year && Month == other.Month && Day == other.Day;

	/// <summary>Return difference as TimeSpan (Gregorian-backed difference).</summary>
	public TimeSpan Subtract(PersianDateTime other) => ToDateTime() - other.ToDateTime();

	/// <summary>Return friendly relative string in Persian (e.g., "۳ روز پیش") or in English fallback.</summary>
	public string ToRelativeString(bool farsi = true) {
		DateTime now = DateTime.Now;
		TimeSpan diff = now - ToDateTime();
		if (diff.TotalSeconds < 0) {
			TimeSpan adiff = diff.Duration();
			// future
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

	/// <summary>Return localized Persian date string with month name (Farsi) optionally using Persian digits.</summary>
	public string ToLongDateString(bool usePersianDigits = true) {
		string monthName = GetMonthName(Month);
		string dayName = GetPersianDayName(DayOfWeek);
		string dayStr = usePersianDigits ? ToPersianDigits(Day.ToString()) : Day.ToString();
		string yearStr = usePersianDigits ? ToPersianDigits(Year.ToString()) : Year.ToString();
		return $"{dayName} {dayStr} {monthName} {yearStr}";
	}

	#endregion

	#region Extensions / Converters as helper static methods

	/// <summary>Convert a Gregorian DateTime to PersianDateTime with optional preservation of Kind.</summary>
	public static PersianDateTime FromGregorian(DateTime dt) => new(dt);

	/// <summary>Convert from Persian date components to Gregorian DateTime</summary>
	public static DateTime ToGregorianDateTime(int year, int month, int day, int hour = 0, int minute = 0, int second = 0, int millisecond = 0) {
		return Pc.ToDateTime(year, month, day, hour, minute, second, millisecond);
	}

	[GeneratedRegex("[\u06F0-\u06F9]", RegexOptions.Compiled)]
	private static partial Regex MyRegex();

	#endregion
}

/// <summary>Extension methods for System.DateTime providing convenient Persian/Jalali helpers.</summary>
public static class PersianDateTimeExtensions {
	/// <summary>Convert DateTime to PersianDateTime.</summary>
	public static PersianDateTime ToPersian(this DateTime dt) => PersianDateTime.FromDateTime(dt);

	/// <summary>Return the Persian formatted date string.</summary>
	public static string ToPersianString(this DateTime dt, string format = "yyyy/MM/dd HH:mm:ss", bool usePersianDigits = false) {
		PersianDateTime pd = dt.ToPersian();
		return pd.ToString(format, usePersianDigits);
	}

	/// <summary>Parse a Persian date string to DateTime (Gregorian) - throws on failure.</summary>
	public static DateTime ParsePersianToDateTime(string persianDateString, DateTimeKind kind = DateTimeKind.Unspecified) {
		PersianDateTime pd = PersianDateTime.Parse(persianDateString, kind);
		return pd.ToDateTime();
	}

	/// <summary>Try parse to DateTime (Gregorian)</summary>
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
	private static readonly PersianCalendar Pc = new PersianCalendar();

	// =====================================================
	//   CONVERSIONS
	// =====================================================

	/// <summary>
	/// Convert DateTime → Persian formatted yyyy/MM/dd
	/// </summary>
	public static string ToJalaliString(DateTime dt) {
		return $"{Pc.GetYear(dt):0000}/{Pc.GetMonth(dt):00}/{Pc.GetDayOfMonth(dt):00}";
	}

	/// <summary>
	/// Convert DateTime → long date: weekday day month year
	/// Example: "پنجشنبه 10 آبان 1403"
	/// </summary>
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

	/// <summary>
	/// Convert Persian y/m/d → DateTime
	/// </summary>
	public static DateTime ToGregorian(int y, int m, int d)
		=> Pc.ToDateTime(y, m, d, 0, 0, 0, 0);

	/// <summary>
	/// Parse Persian yyyy/MM/dd → Gregorian DateTime
	/// </summary>
	public static DateTime Parse(string persianDate) {
		string[] parts = persianDate.Split('/', '-', '.');
		int y = int.Parse(parts[0]);
		int m = int.Parse(parts[1]);
		int d = int.Parse(parts[2]);
		return Pc.ToDateTime(y, m, d, 0, 0, 0, 0);
	}

	// =====================================================
	//   VALIDATION
	// =====================================================

	/// <summary>
	/// Check validity of Persian date
	/// </summary>
	public static bool IsValid(string persianDate) {
		try {
			Parse(persianDate);
			return true;
		}
		catch {
			return false;
		}
	}

	/// <summary>
	/// Check if Jalali year is leap
	/// </summary>
	public static bool IsLeapYear(int year) => Pc.IsLeapYear(year);

	// =====================================================
	//   EXTRACTORS
	// =====================================================

	/// <summary> Extract Persian Year </summary>
	public static int Year(DateTime dt) => Pc.GetYear(dt);

	/// <summary> Extract Persian Month </summary>
	public static int Month(DateTime dt) => Pc.GetMonth(dt);

	/// <summary> Extract Persian Day </summary>
	public static int Day(DateTime dt) => Pc.GetDayOfMonth(dt);

	// =====================================================
	//   MANIPULATION
	// =====================================================

	public static string AddDays(string persianDate, int days)
		=> ToJalaliString(Parse(persianDate).AddDays(days));

	public static string AddMonths(string persianDate, int months)
		=> ToJalaliString(Pc.AddMonths(Parse(persianDate), months));

	public static string AddYears(string persianDate, int years)
		=> ToJalaliString(Pc.AddYears(Parse(persianDate), years));


	// =====================================================
	//   START / END
	// =====================================================

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
		return $"{Year(dt):0000}/12/29"; // leap handled below
	}

	// =====================================================
	//   COMPARISON
	// =====================================================

	public static bool IsBefore(string p1, string p2)
		=> Parse(p1) < Parse(p2);

	public static bool IsAfter(string p1, string p2)
		=> Parse(p1) > Parse(p2);

	public static bool IsSame(string p1, string p2)
		=> Parse(p1).Date == Parse(p2).Date;
}