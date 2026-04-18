namespace SinaMN75U.Utils;

public static class UExtensions {
	public static bool IsNotNullOrEmpty([NotNullWhen(true)] this string? s) => s is { Length: > 0 };
	public static bool IsNotNullOrEmpty<T>([NotNullWhen(true)] this IEnumerable<T>? list) => list != null && list.Any();
	public static bool IsNotNullOrZero([NotNullWhen(true)] this int? s) => s != null && s != 0;
	public static bool IsNotNullOrZero([NotNullWhen(true)] this decimal? s) => s != null && s != 0;
	public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? list) => list == null || !list.Any();
	public static bool IsNull([NotNullWhen(false)] this string? s) => s == null;
	public static bool IsNotNull([NotNullWhen(true)] this object? s) => s != null;
	public static bool IsGuid(this string s) => Guid.TryParse(s, out Guid _);
	public static string ToJson<T>(this T obj) => JsonSerializer.Serialize(obj, Core.Default);
	public static int ToInt(this string s) => int.Parse(s);
	public static T FromJson<T>(this string json) => JsonSerializer.Deserialize<T>(json, Core.Default)!;

	public static IEnumerable<IdTitleParams> GetValues<T>() where T : Enum =>
		Enum.GetValues(typeof(T)).Cast<int>().Select(item => new IdTitleParams { Title = Enum.GetName(typeof(T), item), Id = item }).ToList();

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

	public static void RemoveRangeIfExist<T>(this IEnumerable<T>? enumerable, IEnumerable<T> itemsToRemove) => enumerable.RemoveAll(item => new HashSet<T>(itemsToRemove).Contains(item));

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

	public static int? GetIntOrNull(this JsonElement element, string propertyName) {
		if (element.TryGetProperty(propertyName, out JsonElement value))
			if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int intValue))
				return intValue;
		return null;
	}
}