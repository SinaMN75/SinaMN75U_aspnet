namespace SinaMN75U.Utils;

public static class UExtensions {
	public static decimal ToDecimal([NotNullWhen(true)] this string s) => decimal.Parse(s);
	public static bool IsNotNullOrEmpty([NotNullWhen(true)] this string? s) => s is { Length: > 0 };
	public static bool IsNotNullOrEmpty([NotNullWhen(true)] this Guid? s) => s != null;
	public static bool IsNotNullOrEmpty<T>([NotNullWhen(true)] this IEnumerable<T>? list) => list != null && list.Any();
	public static bool IsNotNullOrZero([NotNullWhen(true)] this int? s) => s != null && s != 0;
	public static bool IsNotNullOrZero([NotNullWhen(true)] this decimal? s) => s != null && s != 0;
	public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? list) => list == null || !list.Any();
	public static string? NullIfEmpty(this string? s) => string.IsNullOrWhiteSpace(s) ? null : s;
	public static bool IsNull([NotNullWhen(false)] this string? s) => s == null;
	public static bool IsNotNull([NotNullWhen(true)] this object? s) => s != null;
	public static bool IsGuid(this string s) => Guid.TryParse(s, out Guid _);
	public static string ToJson<T>(this T obj) => JsonSerializer.Serialize(obj, Core.Default);
	public static int ToInt(this string s) => int.Parse(s);
	public static int ToInt(this decimal s) => (int)s;
	public static string ToIntString(this decimal s) => ((int)s).ToString();
	public static string ToDecimalString(this decimal s) => s.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);
	public static string? ToBase64(this byte[]? s) => s == null ? null : Convert.ToBase64String(s);
	public static Guid ToGuid(this string s) => Guid.Parse(s);
	public static byte[]? FromBase64(this string? s) => s == null ? null : Convert.FromBase64String(s);
	public static T FromJson<T>(this string json) => JsonSerializer.Deserialize<T>(json, Core.Default)!;
	public static T Random<T>(this List<T> list) => list[new Random().Next(list.Count)];
	public static IEnumerable<IdTitleParams> GetValues<T>() where T : Enum => Enum.GetValues(typeof(T)).Cast<int>().Select(item => new IdTitleParams { Title = Enum.GetName(typeof(T), item), Id = item }).ToList();
	public static int GetNumber<T>(this T value) where T : Enum => Convert.ToInt32(value);
	public static string GetString<T>(this T value) where T : Enum => value.ToString();

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
		if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind == JsonValueKind.String) return value.GetString();
		return null;
	}

	public static decimal? GetDecimalOrNull(this JsonElement element, string propertyName) {
		if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out JsonElement value)) return null;
		if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out decimal d)) return d;
		if (value.ValueKind == JsonValueKind.String && decimal.TryParse(value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal ds)) return ds;
		return null;
	}

	public static bool? GetBoolOrNull(this JsonElement element, string propertyName) {
		if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind is JsonValueKind.False or JsonValueKind.True) return value.GetBoolean();
		return null;
	}

	public static int? GetIntOrNull(this JsonElement element, string propertyName) {
		if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out JsonElement value))
			if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int intValue))
				return intValue;
		return null;
	}

	public static async Task<string> ReadBodyOnceAsync(this HttpContext context) {
		if (context.Items.TryGetValue("__U_RequestBody", out object? cached) && cached is string cachedBody) return cachedBody;
		context.Request.EnableBuffering();
		string body;
		using (StreamReader reader = new(context.Request.Body, Encoding.UTF8, leaveOpen: true)) body = await reader.ReadToEndAsync();
		context.Request.Body.Seek(0, SeekOrigin.Begin);
		context.Items["__U_RequestBody"] = body;
		return body;
	}

	public static void CaptureForApiLog(this IHttpContextAccessor accessor, Exception ex) => accessor.HttpContext.CaptureForApiLog(ex);

	public static void CaptureForApiLog(this HttpContext? context, Exception ex) => context?.Items[UConstants.ApiLogExceptionKey] = ex;

	public static string ToBase58(this byte[] bytes) {
		if (bytes.Length == 0) return string.Empty;

		BigInteger value = new(bytes, isUnsigned: true, isBigEndian: true);
		StringBuilder result = new();
		const string alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

		while (value > 0) {
			value = BigInteger.DivRem(value, 58, out BigInteger remainder);
			result.Insert(0, alphabet[(int)remainder]);
		}

		foreach (byte b in bytes) {
			if (b != 0) break;
			result.Insert(0, '1');
		}

		return result.ToString();
	}

	public static string ToBase58(this string value) => Encoding.UTF8.GetBytes(value).ToBase58();
	public static string ToBase58(this Guid value) => value.ToByteArray().ToBase58();
	public static string ToBase58(this int value) => ToBase58((BigInteger)value);
	public static string ToBase58(this long value) => ToBase58((BigInteger)value);
	public static string ToBase58(this uint value) => ToBase58((BigInteger)value);
	public static string ToBase58(this ulong value) => ToBase58((BigInteger)value);
	public static string FromBase58String(this string value) => Encoding.UTF8.GetString(value.FromBase58());
	public static Guid FromBase58Guid(this string value) => new(value.FromBase58());

	private static string ToBase58(BigInteger value) {
		ArgumentOutOfRangeException.ThrowIfNegative(value);
		if (value == 0) return "1";
		StringBuilder result = new();
		while (value > 0) {
			value = BigInteger.DivRem(value, 58, out BigInteger remainder);
			result.Insert(0, "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz"[(int)remainder]);
		}

		return result.ToString();
	}

	public static byte[] FromBase58(this string value) {
		if (string.IsNullOrEmpty(value)) return [];
		BigInteger result = 0;
		foreach (char c in value) {
			int index = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz".IndexOf(c);
			if (index < 0) throw new FormatException($"Invalid Base58 character: '{c}'.");
			result = result * 58 + index;
		}

		byte[] bytes = result == 0 ? [] : result.ToByteArray(isUnsigned: true, isBigEndian: true);
		int leadingOnes = value.TakeWhile(c => c == '1').Count();
		if (leadingOnes == 0) return bytes;
		byte[] resultBytes = new byte[leadingOnes + bytes.Length];
		bytes.CopyTo(resultBytes, leadingOnes);
		return resultBytes;
	}
}