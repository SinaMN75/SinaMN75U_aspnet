namespace SinaMN75U.InnerServices;

using System.Collections.Concurrent;

public interface ILocalStorageService {
	public void Set(string key, string value, TimeSpan? expireTime = null);
	public string? Get(string key);
	public void Delete(string recordId);
	void DeleteAllByPartialKey(string partialKey);
}

public class MemoryCacheService(IMemoryCache cache) : ILocalStorageService {
	private readonly ConcurrentDictionary<string, byte> _keys = new();

	public void Set(string key, string value, TimeSpan? expireTime = null) {
		MemoryCacheEntryOptions cacheEntryOptions = new();
		if (expireTime.HasValue) {
			cacheEntryOptions.SetAbsoluteExpiration(expireTime.Value);
			cacheEntryOptions.SetSlidingExpiration(expireTime.Value);
		}

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

public class StaticCacheService : ILocalStorageService {
	private static readonly ConcurrentDictionary<string, (string Value, DateTime? Expiry)> Cache = new();

	public void Set(string key, string value, TimeSpan? expireTime = null) {
		DateTime? expiry = expireTime.HasValue ? DateTime.UtcNow.Add(expireTime.Value) : null;
		Cache[key] = (value, expiry);
	}

	public string? Get(string key) {
		if (!Cache.TryGetValue(key, out (string Value, DateTime? Expiry) entry)) return null;
		if (entry.Expiry == null || DateTime.UtcNow < entry.Expiry) {
			return entry.Value;
		}

		Cache.TryRemove(key, out _);

		return null;
	}

	public void Delete(string key) => Cache.TryRemove(key, out _);

	public void DeleteAllByPartialKey(string partialKey) {
		List<string> keysToRemove = Cache.Keys.Where(k => k.Contains(partialKey)).ToList();
		foreach (string key in keysToRemove) {
			Cache.TryRemove(key, out _);
		}
	}
}