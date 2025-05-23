namespace SinaMN75U.InnerServices;

public interface ILocalStorageService {
	public void Set(string key, string value, TimeSpan? expireTime = null);
	public string? Get(string key);
	public void Delete(string recordId);
	void DeleteAllByPartialKey(string partialKey);
}

public class MemoryCacheService(IMemoryCache cache) : ILocalStorageService {
	public void Set(string key, string value, TimeSpan? expireTime = null) {
		MemoryCacheEntryOptions cacheEntryOptions = new();
		if (expireTime.HasValue) {
			cacheEntryOptions.SetAbsoluteExpiration(expireTime.Value);
			cacheEntryOptions.SetSlidingExpiration(expireTime.Value);
		}
		cache.Set(key, value, cacheEntryOptions);
	}

	public string? Get(string key) => cache.Get<string>(key) ?? null;

	public void Delete(string key) {
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