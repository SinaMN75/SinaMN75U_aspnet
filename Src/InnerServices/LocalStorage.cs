namespace SinaMN75U.InnerServices;

public interface ILocalStorageService {
	void Set(string key, string value, TimeSpan expireTime);
	string? Get(string key);
	void Delete(string key);
	void DeleteAllByPartialKey(string partialKey);
}

public sealed class StaticCacheService : ILocalStorageService {
	private static readonly ConcurrentDictionary<string, CacheEntry> Cache = new();
	private static int _cleanupLock;

	private record CacheEntry(string Value, DateTime Expiry);

	public void Set(string key, string value, TimeSpan expireTime) {
		if (Cache.Count >= 10_000) {
			string oldestKey = Cache.Keys.First();
			Cache.TryRemove(oldestKey, out _);
		}

		Cache[key] = new CacheEntry(value, DateTime.UtcNow.Add(expireTime));

		if (Interlocked.CompareExchange(ref _cleanupLock, 1, 0) == 0) {
			try {
				CleanupExpiredEntries();
			}
			finally {
				Interlocked.Exchange(ref _cleanupLock, 0);
			}
		}
	}

	public string? Get(string key) {
		if (!Cache.TryGetValue(key, out CacheEntry? entry)) return null;
		if (DateTime.UtcNow < entry.Expiry) return entry.Value;
		Cache.TryRemove(key, out _);
		return null;
	}

	public void Delete(string key) => Cache.TryRemove(key, out _);

	public void DeleteAllByPartialKey(string partialKey) {
		foreach (string key in Cache.Keys
			         .Where(k => k.Contains(partialKey, StringComparison.Ordinal))
			         .ToArray()
		        ) Cache.TryRemove(key, out _);
	}

	private static void CleanupExpiredEntries() {
		foreach (string key in Cache
			         .Where(kvp => DateTime.UtcNow >= kvp.Value.Expiry)
			         .Select(kvp => kvp.Key)
			         .ToArray()
		        ) Cache.TryRemove(key, out _);
	}
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