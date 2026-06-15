namespace SinaMN75U.Utils;

public static class UExtensions {
	public static decimal ToDecimal([NotNullWhen(true)] this string s) => decimal.Parse(s);
	public static bool IsNotNullOrEmpty([NotNullWhen(true)] this string? s) => s is { Length: > 0 };
	public static bool IsNotNullOrEmpty([NotNullWhen(true)] this Guid? s) => s != null;
	public static bool IsNotNullOrEmpty<T>([NotNullWhen(true)] this IEnumerable<T>? list) => list != null && list.Any();
	public static bool IsNotNullOrZero([NotNullWhen(true)] this int? s) => s != null && s != 0;
	public static bool IsNotNullOrZero([NotNullWhen(true)] this decimal? s) => s != null && s != 0;
	public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? list) => list == null || !list.Any();
	public static bool IsNull([NotNullWhen(false)] this string? s) => s == null;
	public static bool IsNotNull([NotNullWhen(true)] this object? s) => s != null;
	public static bool IsGuid(this string s) => Guid.TryParse(s, out Guid _);
	public static string ToJson<T>(this T obj) => JsonSerializer.Serialize(obj, Core.Default);
	public static int ToInt(this string s) => int.Parse(s);
	public static int ToInt(this decimal s) => (int)s;
	public static string ToIntString(this decimal s) => ((int)s).ToString();
	public static string? ToBase64(this byte[]? s) => s == null ? null : Convert.ToBase64String(s);
	public static byte[]? FromBase64(this string? s) => s == null ? null : Convert.FromBase64String(s);
	public static T FromJson<T>(this string json) => JsonSerializer.Deserialize<T>(json, Core.Default)!;

	public static T Random<T>(this List<T> list) => list[new Random().Next(list.Count)];

	public static IEnumerable<IdTitleParams> GetValues<T>() where T : Enum => Enum.GetValues(typeof(T)).Cast<int>().Select(item => new IdTitleParams { Title = Enum.GetName(typeof(T), item), Id = item }).ToList();

	public static bool ContainsAny<T>(this IEnumerable<T>? source, params T[]? values) {
		if (source == null || values == null || values.Length == 0) return false;

		HashSet<T> set = new(source);
		return values.Any(set.Contains);
	}

	public static void AddRangeIfNotExist<T>(this ICollection<T>? collection, IEnumerable<T>? items) {
		if (collection == null || items == null) return;
		foreach (T item in items)
			if (!collection.Contains(item))
				collection.Add(item);
	}

	public static void AddRangeIfNotExist<T>(this ICollection<T>? collection, ICollection<T>? items) {
		if (collection == null || items == null) return;
		foreach (T item in items)
			if (!collection.Contains(item))
				collection.Add(item);
	}

	public static void RemoveRangeIfExist<T>(this ICollection<T>? collection, ICollection<T>? items) {
		if (collection == null || items == null) return;
		foreach (T item in items) collection.Remove(item);
	}

	public static IEnumerable<T> RemoveAll<T>(this IEnumerable<T>? enumerable, Func<T, bool> predicate) => enumerable?.Where(item => !predicate(item)) ?? [];

	public static bool ContainsSafe<T>(this IEnumerable<T>? enumerable, T item) => enumerable != null && enumerable.Contains(item);

	public static IEnumerable<T> AddSafe<T>(this IEnumerable<T>? enumerable, T item) {
		List<T> list = enumerable?.ToList() ?? [];
		list.Add(item);
		return list;
	}

	public static string? GetStringOrNull(this JsonElement element, string propertyName) {
		if (element.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind == JsonValueKind.String) return value.GetString();
		return null;
	}

	public static decimal? GetDecimalOrNull(this JsonElement element, string propertyName) {
		if (element.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind == JsonValueKind.String) return value.GetDecimal();
		return null;
	}

	public static bool? GetBoolOrNull(this JsonElement element, string propertyName) {
		if (element.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind is JsonValueKind.False or JsonValueKind.True) return value.GetBoolean();
		return null;
	}

	public static int? GetIntOrNull(this JsonElement element, string propertyName) {
		if (element.TryGetProperty(propertyName, out JsonElement value))
			if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int intValue))
				return intValue;
		return null;
	}
}