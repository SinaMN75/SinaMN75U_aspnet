namespace SinaMN75U.InnerServices;

public interface ILocalStorageService {
	void Set(string key, string value, TimeSpan expireTime);
	string? Get(string key);
	void Delete(string recordId);
	void DeleteAllByPartialKey(string partialKey);
}

public class MemoryCacheService(IMemoryCache cache) : ILocalStorageService {
	private readonly ConcurrentDictionary<string, byte> _keys = new();

	public void Set(string key, string value, TimeSpan expireTime) {
		MemoryCacheEntryOptions cacheEntryOptions = new();
		cacheEntryOptions.SetAbsoluteExpiration(expireTime);
		cacheEntryOptions.SetSlidingExpiration(expireTime);

		cache.Set(key, value, cacheEntryOptions);
		_keys.TryAdd(key, 0);
	}

	public string? Get(string key) => cache.Get<string>(key);

	public void Delete(string key) {
		if (string.IsNullOrEmpty(key)) return;
		cache.Remove(key);
		_keys.TryRemove(key, out _);
	}

	public void DeleteAllByPartialKey(string partialKey) {
		if (string.IsNullOrEmpty(partialKey)) return;
		List<string> keysToRemove = _keys.Keys.Where(k => k.Contains(partialKey)).ToList();
		foreach (string key in keysToRemove) {
			cache.Remove(key);
			_keys.TryRemove(key, out _);
		}
	}
}

public sealed class StaticCacheService : ILocalStorageService {
	private static readonly ConcurrentDictionary<string, (string Value, DateTime Expiry)> Cache = new();
	private static DateTime _lastCleanup = DateTime.UtcNow;

	public void Set(string key, string value, TimeSpan expireTime) {
		DateTime now = DateTime.UtcNow;
		Cache[key] = (value, now.Add(expireTime));
		if (now - _lastCleanup <= TimeSpan.FromMinutes(5)) return;
		CleanupExpiredEntries();
		_lastCleanup = now;
	}

	public string? Get(string key) {
		if (!Cache.TryGetValue(key, out (string Value, DateTime Expiry) entry)) return null;
		if (DateTime.UtcNow < entry.Expiry) return entry.Value;
		Cache.TryRemove(key, out _);
		return null;
	}

	public void Delete(string key) => Cache.TryRemove(key, out _);

	public void DeleteAllByPartialKey(string partialKey) {
		foreach (KeyValuePair<string, (string Value, DateTime Expiry)> kvp in Cache) {
			if (kvp.Key.Contains(partialKey, StringComparison.Ordinal)) Cache.TryRemove(kvp.Key, out _);
		}
	}

	private static void CleanupExpiredEntries() {
		foreach (KeyValuePair<string, (string Value, DateTime Expiry)> kvp in Cache) {
			if (DateTime.UtcNow >= kvp.Value.Expiry) Cache.TryRemove(kvp.Key, out _);
		}
	}
}