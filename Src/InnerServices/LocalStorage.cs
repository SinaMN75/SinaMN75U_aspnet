namespace SinaMN75U.InnerServices;

public interface ILocalStorageService {
	void Set(string key, string value, TimeSpan expireTime);
	string? Get(string key);
	void Delete(string key);
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
	private static readonly ConcurrentDictionary<string, CacheEntry> Cache = new();
	private static int _cleanupLock;

	private record CacheEntry(string Value, DateTime Expiry);

	public void Set(string key, string value, TimeSpan expireTime) {
		DateTime now = DateTime.UtcNow;
		CacheEntry entry = new CacheEntry(value, now.Add(expireTime));

		// Evict if over limit (simple FIFO - use Keys.First() for oldest)
		if (Cache.Count >= 10_000) {
			string oldestKey = Cache.Keys.First();
			Cache.TryRemove(oldestKey, out _);
		}

		Cache[key] = entry;

		// Throttled cleanup (every 5 min, thread-safe)
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
		if (!Cache.TryGetValue(key, out CacheEntry? entry))
			return null;

		if (DateTime.UtcNow < entry.Expiry)
			return entry.Value;

		Cache.TryRemove(key, out _);
		return null;
	}

	public void Delete(string key) => Cache.TryRemove(key, out _);

	public void DeleteAllByPartialKey(string partialKey) {
		string[] keysToDelete = Cache.Keys
			.Where(k => k.Contains(partialKey, StringComparison.Ordinal))
			.ToArray(); // ToArray to avoid modification during enumeration

		foreach (string key in keysToDelete)
			Cache.TryRemove(key, out _);
	}

	private static void CleanupExpiredEntries() {
		DateTime now = DateTime.UtcNow;
		string[] expiredKeys = Cache
			.Where(kvp => now >= kvp.Value.Expiry)
			.Select(kvp => kvp.Key)
			.ToArray();

		foreach (string key in expiredKeys)
			Cache.TryRemove(key, out _);
	}
}