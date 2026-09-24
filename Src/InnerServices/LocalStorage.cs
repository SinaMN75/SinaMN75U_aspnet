namespace SinaMN75U.InnerServices;

public interface ILocalStorageService {
	void Set(string key, string value, TimeSpan expireTime);
	string? Get(string key);
}

public sealed class UMemoryCacheService : ILocalStorageService {
	private const int MaxSize = 10_000;

	private readonly MemoryCache _cache = new(new MemoryCacheOptions {
		SizeLimit = null,
		ExpirationScanFrequency = TimeSpan.FromMinutes(1)
	});

	private readonly ConcurrentDictionary<string, byte> _keys = new();

	public void Set(string key, string value, TimeSpan expireTime) {
		if (_keys.Count >= MaxSize && !_keys.ContainsKey(key)) {
			_cache.Compact(0);
			if (_keys.Count >= MaxSize)
				foreach (KeyValuePair<string, byte> kv in _keys) {
					_cache.Remove(kv.Key);
					break;
				}
		}

		MemoryCacheEntryOptions options = new() { AbsoluteExpirationRelativeToNow = expireTime };
		options.RegisterPostEvictionCallback(OnEvicted, _keys);
		_keys[key] = 0;
		_cache.Set(key, value, options);
	}

	public string? Get(string key) => _cache.TryGetValue(key, out string? value) ? value : null;

	private static void OnEvicted(object key, object? value, EvictionReason reason, object? state) {
		if (reason == EvictionReason.Replaced || state is not ConcurrentDictionary<string, byte> keys) return;
		keys.TryRemove((string)key, out _);
	}
}
