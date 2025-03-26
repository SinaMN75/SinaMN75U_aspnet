namespace SinaMN75U.Utils;

public static class StringExtension {
	public static bool IsNotNullOrEmpty([NotNullWhen(true)] this string? s) => s is { Length: > 0 };

	public static bool IsNotNull([NotNullWhen(true)] this string? s) => s != null;

	public static bool IsNullOrEmpty([NotNullWhen(false)] this string? s) => string.IsNullOrEmpty(s);

	public static bool IsNull([NotNullWhen(false)] this string? s) => s == null;

	public static bool MinMaxLenght(this string? s, int min, int max) => s.IsNotNull() && s.Length >= min && s.Length <= max;

	public static bool IsEmail(this string email) => !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");

	public static string EncodeJson<T>(this T obj) => JsonSerializer.Serialize(obj, Core.JsonSettings);

	public static T DecodeJson<T>(this string json) => JsonSerializer.Deserialize<T>(json, Core.JsonSettings)!;
}

public static class Core {
	public static readonly JsonSerializerOptions? JsonSettings = new() {
		WriteIndented = true,
		ReferenceHandler = ReferenceHandler.IgnoreCycles,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};
}

public static class EnumExtension {
	public static IEnumerable<IdTitleParams> GetValues<T>() => (from int itemType in Enum.GetValues(typeof(T))
		select new IdTitleParams { Title = Enum.GetName(typeof(T), itemType), Id = itemType }).ToList();
}

public static class UtilitiesStatusCodesExtension {
	public static int Value(this USC statusCode) => (int)statusCode;
}

public static class EnumerableExtension {
	public static bool IsNotNullOrEmpty<T>([NotNullWhen(true)] this IEnumerable<T>? list) => list != null && list.Any();

	public static bool IsNotNull<T>([NotNullWhen(true)] this IEnumerable<T>? list) => list != null;

	public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? list) => list == null || list.Any();

	public static bool ContainsAny<T>(this IEnumerable<T> source, IEnumerable<T> target) => source.Any(target.Contains);
}