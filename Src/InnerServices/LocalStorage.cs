namespace SinaMN75U.InnerServices;

public interface ILocalStorageService {
	void Set(string key, string value, TimeSpan expireTime);
	string? Get(string key);
}

public sealed class UMemoryCacheService : ILocalStorageService {
	private const int MaxSize = 10_000;

	private readonly MemoryCache _cache = new(new MemoryCacheOptions {
		SizeLimit = null,
		ExpirationScanFrequency = TimeSpan.FromHours(1)
	});

	private readonly ConcurrentDictionary<string, byte> _keys = new();

	public void Set(string key, string value, TimeSpan expireTime) {
		if (_keys.Count >= MaxSize)
			foreach (KeyValuePair<string, byte> kv in _keys) {
				Delete(kv.Key);
				break;
			}

		MemoryCacheEntryOptions options = new() { AbsoluteExpirationRelativeToNow = expireTime };
		_cache.Set(key, value, options);

		_keys[key] = 0;
	}

	public string? Get(string key) => _cache.TryGetValue(key, out string? value) ? value : null;

	private void Delete(string key) {
		_cache.Remove(key);
		_keys.TryRemove(key, out _);
	}
}