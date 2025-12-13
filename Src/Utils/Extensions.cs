namespace SinaMN75U.Utils;

public static class UExtensions {
	public static bool IsNotNullOrEmpty([NotNullWhen(true)] this string? s) => s is { Length: > 0 };
	public static bool IsNotNullOrEmpty<T>([NotNullWhen(true)] this IEnumerable<T>? list) => list != null && list.Any();
	public static bool IsNotNullOrEmpty([NotNullWhen(true)] this IEnumerable<string>? list) => list != null && list.Any();
	public static bool IsNotNullOrZero([NotNullWhen(true)] this int? s) => s != null && s != 0;
	public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? list) => list == null || !list.Any();
	public static bool IsNull([NotNullWhen(false)] this string? s) => s == null;
	public static bool IsNotNull([NotNullWhen(true)] this Guid? s) => s != null;
	public static bool IsNotNull([NotNullWhen(true)] this string? s) => s != null;
	public static bool IsNotNull([NotNullWhen(true)] this int? s) => s != null;
	public static bool IsNotNull([NotNullWhen(true)] this decimal? s) => s != null;
	public static bool IsNotNull<T>([NotNullWhen(true)] this T? obj) where T : class => obj != null;
	public static bool IsGuid(this string s) => Guid.TryParse(s, out Guid _);

	public static string ToJson<T>(this T obj) => JsonSerializer.Serialize(obj, UJsonOptions.Default);
	public static T FromJson<T>(this string json) => JsonSerializer.Deserialize<T>(json, UJsonOptions.Default)!;
	
	public static IEnumerable<IdTitleParams> GetValues<T>() where T : Enum => Enum.GetValues(typeof(T)).Cast<int>()
		.Select(item => new IdTitleParams {
			Title = Enum.GetName(typeof(T), item),
			Id = item
		}).ToList();
	
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

	extension<T>(IEnumerable<T> list) {
		public void RemoveRangeIfExist(IEnumerable<T> itemsToRemove) {
			ArgumentNullException.ThrowIfNull(list);
			ArgumentNullException.ThrowIfNull(itemsToRemove);
			HashSet<T> itemsToRemoveSet = new(itemsToRemove);
			list.RemoveAll(item => itemsToRemoveSet.Contains(item));
		}

		public IEnumerable<T> RemoveAll(Func<T, bool> predicate) {
			ArgumentNullException.ThrowIfNull(list);
			ArgumentNullException.ThrowIfNull(predicate);
			return list.Where(item => !predicate(item));
		}
	}
}