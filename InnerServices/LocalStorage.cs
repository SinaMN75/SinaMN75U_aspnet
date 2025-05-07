namespace SinaMN75U.InnerServices;

public interface ILocalStorageService {
	public void SetStringData(string key, string value, TimeSpan? absoluteExpireTime = null, TimeSpan? slidingExpireTime = null);
	public string? GetStringData(string key);
	public void DeleteStringData(string recordId);
	void DeleteAllByPartialKey(string partialKey);
}

public class MemoryCacheService(IMemoryCache cache) : ILocalStorageService {
	public void SetStringData(string key, string value, TimeSpan? absoluteExpireTime = null, TimeSpan? slidingExpireTime = null) {
		MemoryCacheEntryOptions cacheEntryOptions = new();
		if (absoluteExpireTime.HasValue) cacheEntryOptions.SetAbsoluteExpiration(absoluteExpireTime.Value);
		if (slidingExpireTime.HasValue) cacheEntryOptions.SetSlidingExpiration(slidingExpireTime.Value);
		cache.Set(key, value, cacheEntryOptions);
	}

	public string? GetStringData(string key) => cache.Get<string>(key) ?? null;

	public void DeleteStringData(string key) {
		if (string.IsNullOrEmpty(key)) return;
		cache.Remove(key);
	}

	public void DeleteAllByPartialKey(string partialKey) {
		if (string.IsNullOrEmpty(partialKey)) return;

		FieldInfo? coherentState = typeof(MemoryCache).GetField("_coherentState", BindingFlags.NonPublic | BindingFlags.Instance);
		object? coherentStateValue = coherentState?.GetValue(cache);
		PropertyInfo? entriesCollection = coherentStateValue?.GetType().GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
		if (entriesCollection?.GetValue(coherentStateValue) is not ICollection collectionValue) return;
		List<string> keysToRemove = [];
		foreach (object? item in collectionValue) {
			PropertyInfo? methodInfo = item?.GetType().GetProperty("Key");
			object? value = methodInfo?.GetValue(item);
			if (value is string key && key.Contains(partialKey)) keysToRemove.Add(key);
		}

		foreach (string key in keysToRemove) cache.Remove(key);
	}
}