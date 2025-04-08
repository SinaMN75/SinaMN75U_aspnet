namespace SinaMN75U.Utils;

using System.ComponentModel;

public static class StringExtensions {
	public static bool IsNotNullOrEmpty([NotNullWhen(true)] this string? s) => s is { Length: > 0 };
	public static bool IsNotNullOrEmpty([NotNullWhen(true)] this Guid? s) => s != null;
	public static bool IsNotNull([NotNullWhen(true)] this string? s) => s != null;
	public static bool IsNullOrEmpty([NotNullWhen(false)] this string? s) => string.IsNullOrEmpty(s);
	public static bool IsNull([NotNullWhen(false)] this string? s) => s == null;
	public static bool MinMaxLength(this string? s, int min, int max) => s.IsNotNull() && s.Length >= min && s.Length <= max;
	public static bool IsEmail(this string email) => !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
	public static string EncodeJson<T>(this T obj) => JsonSerializer.Serialize(obj, UJsonOptions.Default);
	public static T DecodeJson<T>(this string json) => JsonSerializer.Deserialize<T>(json, UJsonOptions.Default)!;

	public static string Truncate(this string value, int maxLength, string truncationSuffix = "...") => value.IsNotNullOrEmpty() && value.Length > maxLength
		? value[..(maxLength - truncationSuffix.Length)] + truncationSuffix
		: value;

	public static string ToTitleCase(this string? str) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str?.ToLower() ?? "");

	public static string RemoveWhitespace(this string? input) => new string(input?.Where(c => !char.IsWhiteSpace(c)).ToArray() ?? []);

	public static bool IsNumeric(this string str) => double.TryParse(str, out _);

	public static string Join(this IEnumerable<string> strings, string separator) => string.Join(separator, strings);

	public static string Base64Encode(this string? plainText) => Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText ?? ""));

	public static string Base64Decode(this string? base64EncodedData) => Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedData ?? ""));

	public static string ToMd5(this string? input) {
		byte[] inputBytes = Encoding.ASCII.GetBytes(input ?? "");
		byte[] hashBytes = MD5.HashData(inputBytes);
		return Convert.ToHexStringLower(hashBytes);
	}

	public static bool Contains(this string? source, string toCheck, StringComparison comp) => source?.IndexOf(toCheck, comp) >= 0;

	public static string DefaultIfNullOrEmpty(this string value, string defaultValue) => value.IsNullOrEmpty() ? defaultValue : value;

	public static int ToInt(this string? s) {
		try {
			return int.Parse(s ?? "");
		}
		catch (Exception) {
			return 0;
		}
	}
}

public static class NumberExtensions {
	public static bool IsNotNullOrEmpty([NotNullWhen(true)] this double? s) => s != null;
	public static bool IsNotNullOrEmpty([NotNullWhen(true)] this int? s) => s != null;
	public static bool IsNotNullOrEmpty([NotNullWhen(true)] this decimal? s) => s != null;
	public static bool IsNotNullOrEmpty([NotNullWhen(true)] this float? s) => s != null;
	public static bool IsNotNullOrEmpty([NotNullWhen(true)] this long? s) => s != null;

	// New number extensions
	public static bool IsBetween(this int value, int min, int max) => value >= min && value <= max;
	public static bool IsBetween(this double value, double min, double max) => value >= min && value <= max;
	public static bool IsBetween(this decimal value, decimal min, decimal max) => value >= min && value <= max;

	public static string ToFileSizeString(this long byteCount) {
		string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
		if (byteCount == 0) return "0" + suf[0];
		long bytes = Math.Abs(byteCount);
		int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
		double num = Math.Round(bytes / Math.Pow(1024, place), 1);
		return Math.Sign(byteCount) * num + suf[place];
	}

	public static decimal PercentageOf(this int part, int whole) => whole == 0 ? 0 : (decimal)part / whole * 100;

	public static decimal PercentageOf(this decimal part, decimal whole) => whole == 0 ? 0 : part / whole * 100;

	public static bool IsEven(this int number) => number % 2 == 0;
	public static bool IsOdd(this int number) => !number.IsEven();

	public static int RoundToNearest(this int number, int roundTo) => (int)Math.Round(number / (double)roundTo) * roundTo;

	public static string ToOrdinal(this int num) {
		if (num <= 0) return num.ToString();

		switch (num % 100) {
			case 11:
			case 12:
			case 13:
				return num + "th";
		}

		return (num % 10) switch {
			1 => num + "st",
			2 => num + "nd",
			3 => num + "rd",
			_ => num + "th"
		};
	}
}

public static class DateTimeExtensions {
	public static bool IsWeekend(this DateTime date) => date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;

	public static bool IsWeekday(this DateTime date) => !date.IsWeekend();

	public static DateTime StartOfDay(this DateTime date) => new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);

	public static DateTime EndOfDay(this DateTime date) => new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);

	public static int GetAge(this DateTime birthDate, DateTime? referenceDate = null) {
		DateTime refDate = referenceDate ?? DateTime.Today;
		int age = refDate.Year - birthDate.Year;
		if (birthDate.Date > refDate.AddYears(-age)) age--;
		return age;
	}

	public static bool IsBetween(this DateTime date, DateTime startDate, DateTime endDate) => date >= startDate && date <= endDate;

	public static string ToRelativeTime(this DateTime date) {
		const int second = 1;
		const int minute = 60 * second;
		const int hour = 60 * minute;
		const int day = 24 * hour;
		const int month = 30 * day;

		TimeSpan ts = new TimeSpan(DateTime.UtcNow.Ticks - date.Ticks);
		double delta = Math.Abs(ts.TotalSeconds);

		switch (delta) {
			case < 1 * minute:
				return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";
			case < 2 * minute:
				return "a minute ago";
			case < 45 * minute:
				return ts.Minutes + " minutes ago";
			case < 90 * minute:
				return "an hour ago";
			case < 24 * hour:
				return ts.Hours + " hours ago";
			case < 48 * hour:
				return "yesterday";
			case < 30 * day:
				return ts.Days + " days ago";
			case < 12 * month: {
				int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
				return months <= 1 ? "one month ago" : months + " months ago";
			}
			default: {
				int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
				return years <= 1 ? "one year ago" : years + " years ago";
			}
		}
	}

	public static DateTime ToTimeZone(this DateTime date, string timeZoneId) {
		TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
		return TimeZoneInfo.ConvertTimeFromUtc(date, timeZone);
	}
}

public static class EnumExtensions {
	public static IEnumerable<IdTitleParams> GetValues<T>() where T : Enum => Enum.GetValues(typeof(T)).Cast<int>()
		.Select(item => new IdTitleParams {
			Title = Enum.GetName(typeof(T), item),
			Id = item
		}).ToList();

	// New enum extensions
	public static string GetDescription(this Enum value) {
		FieldInfo? field = value.GetType().GetField(value.ToString());
		DescriptionAttribute? attribute = field?.GetCustomAttribute<DescriptionAttribute>();
		return attribute?.Description ?? value.ToString();
	}

	public static T Next<T>(this T src) where T : Enum {
		T[] arr = (T[])Enum.GetValues(src.GetType());
		int j = Array.IndexOf(arr, src) + 1;
		return arr.Length == j ? arr[0] : arr[j];
	}

	public static T Previous<T>(this T src) where T : Enum {
		T[] arr = (T[])Enum.GetValues(src.GetType());
		int j = Array.IndexOf(arr, src) - 1;
		return j < 0 ? arr[^1] : arr[j];
	}

	public static bool HasFlag(this Enum? variable, Enum? value) {
		if (variable == null) return false;
		ArgumentNullException.ThrowIfNull(value);

		if (!Enum.IsDefined(variable.GetType(), value)) {
			throw new ArgumentException($"Enumeration type mismatch. The flag is of type '{value.GetType()}', was expecting '{variable.GetType()}'.");
		}

		ulong num = Convert.ToUInt64(value);
		return (Convert.ToUInt64(variable) & num) == num;
	}
}

public static class UtilitiesStatusCodesExtension {
	public static int Value(this USC statusCode) => (int)statusCode;
}

public static class EnumerableExtensions {
	public static bool IsNotNullOrEmpty<T>([NotNullWhen(true)] this IEnumerable<T>? list) => list != null && list.Any();
	public static bool IsNotNull<T>([NotNullWhen(true)] this IEnumerable<T>? list) => list != null;
	public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? list) => list == null || !list.Any();
	public static bool ContainsAny<T>(this IEnumerable<T> source, IEnumerable<T> target) => source.Any(target.Contains);

	// New enumerable extensions
	public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action) {
		foreach (T item in source) {
			action(item);
			yield return item;
		}
	}

	public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector) => source.GroupBy(keySelector).Select(x => x.First());

	public static bool None<T>(this IEnumerable<T> source) => !source.Any();

	public static bool None<T>(this IEnumerable<T> source, Func<T, bool> predicate) => !source.Any(predicate);

	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : class => source.Where(item => item != null)!;

	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : struct => source.Where(item => item.HasValue).Select(item => item!.Value);

	public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source) => source.SelectMany(x => x);

	public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source) where TKey : notnull => source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

	public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source) => source.OrderBy(_ => Guid.NewGuid());

	public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int count) {
		IEnumerable<T> enumerable = source.ToList();
		return enumerable.Skip(Math.Max(0, enumerable.Count() - count));
	}

	public static bool IsEquivalentTo<T>(this IEnumerable<T> first, IEnumerable<T> second) => first.Count() == second.Count() && !first.Except(second).Any();

	public static string Join<T>(this IEnumerable<T> source, string separator) => string.Join(separator, source);

	public static IEnumerable<T> Paginate<T>(this IEnumerable<T> source, int page, int pageSize) => source.Skip((page - 1) * pageSize).Take(pageSize);

	public static double Median<T>(this IEnumerable<T> source, Func<T, double> selector) {
		double[] sorted = source.Select(selector).OrderBy(x => x).ToArray();
		int count = sorted.Length;
		if (count == 0) throw new InvalidOperationException("Empty collection");

		int midpoint = count / 2;
		return count % 2 == 0
			? (sorted[midpoint - 1] + sorted[midpoint]) / 2.0
			: sorted[midpoint];
	}
}

public static class ObjectExtensions {
	public static bool IsNull<T>(this T? obj) where T : class => obj == null;
	public static bool IsNotNull<T>(this T? obj) where T : class => obj != null;

	public static T DeepClone<T>(this T obj) {
		if (obj == null) return default!;
		string json = JsonSerializer.Serialize(obj, UJsonOptions.Default);
		return JsonSerializer.Deserialize<T>(json, UJsonOptions.Default)!;
	}

	public static string ToQueryString(this object? obj) {
		if (obj == null) return "";

		IEnumerable<string> properties = obj.GetType().GetProperties()
			.Where(x => x.GetValue(obj, null) != null)
			.Select(x => $"{x.Name}={Uri.EscapeDataString(x.GetValue(obj, null)!.ToString()!)}");

		return string.Join("&", properties);
	}

	public static bool In<T>(this T source, params T[] list) => list.Contains(source);

	public static T? GetPropertyValue<T>(this object obj, string propertyName) {
		PropertyInfo? prop = obj.GetType().GetProperty(propertyName);
		return prop != null ? (T?)prop.GetValue(obj, null) : default;
	}

	public static bool HasProperty(this object obj, string propertyName) => obj.GetType().GetProperty(propertyName) != null;

	public static bool IsDefault<T>(this T value) where T : struct => value.Equals(default(T));
}

public static class TaskExtensions {
	public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout) {
		Task delayTask = Task.Delay(timeout);
		Task completedTask = await Task.WhenAny(task, delayTask);
		if (completedTask == delayTask) throw new TimeoutException();
		return await task;
	}

	public static async Task<TResult> Then<T, TResult>(this Task<T> task, Func<T, TResult> mapper) => mapper(await task);

	public static async Task<T> Catch<T>(this Task<T> task, Func<Exception, T> handler) {
		try {
			return await task;
		}
		catch (Exception ex) {
			return handler(ex);
		}
	}

	public static async void FireAndForget(this Task task, Action<Exception>? errorHandler = null) {
		try {
			await task;
		}
		catch (Exception ex) {
			errorHandler?.Invoke(ex);
		}
	}
}