using Microsoft.Extensions.ObjectPool;

namespace SinaMN75U.InnerServices;

public interface ILocalStorageService {
	void Set(string key, string value, TimeSpan expireTime);
	string? Get(string key);
	void Delete(string key);
	void DeleteAllByPartialKey(string partialKey);
	public void DeleteAllExcept(string partialKey, string keepSubstring);
}

public sealed class UMemoryCacheService : ILocalStorageService {
	private readonly MemoryCache _cache;
	private readonly ConcurrentDictionary<string, byte> _keys;
	private readonly ObjectPool<MemoryCacheEntryOptions> _optionsPool;

	private const int MaxSize = 10_000;

	public UMemoryCacheService() {
		_cache = new MemoryCache(new MemoryCacheOptions {
			SizeLimit = null,
			ExpirationScanFrequency = TimeSpan.FromHours(1)
		});

		_keys = new ConcurrentDictionary<string, byte>();

		DefaultObjectPoolProvider provider = new DefaultObjectPoolProvider();
		_optionsPool = provider.Create<MemoryCacheEntryOptions>();
	}

	public void Set(string key, string value, TimeSpan expireTime) {
		if (_keys.Count >= MaxSize) {
			foreach (KeyValuePair<string, byte> kv in _keys) {
				Delete(kv.Key);
				break;
			}
		}

		MemoryCacheEntryOptions options = _optionsPool.Get();
		options.AbsoluteExpirationRelativeToNow = expireTime;

		_cache.Set(key, value, options);
		_optionsPool.Return(options);

		_keys[key] = 0;
	}

	public string? Get(string key) {
		return _cache.TryGetValue(key, out string? value) ? value : null;
	}

	public void Delete(string key) {
		_cache.Remove(key);
		_keys.TryRemove(key, out _);
	}

	public void DeleteAllByPartialKey(string partialKey) {
		foreach (KeyValuePair<string, byte> kv in _keys) {
			if (kv.Key.Contains(partialKey, StringComparison.Ordinal)) {
				_cache.Remove(kv.Key);
				_keys.TryRemove(kv.Key, out _);
			}
		}
	}
	
	public void DeleteAllExcept(string partialKey, string keepSubstring) {
		foreach (KeyValuePair<string, byte> kv in _keys) {
			string key = kv.Key;

			if (!key.Contains(partialKey, StringComparison.Ordinal))
				continue;

			if (_cache.TryGetValue(key, out string? value)) {
				if (value != null && value.Contains(keepSubstring, StringComparison.Ordinal))
					continue;
			}

			_cache.Remove(key);
			_keys.TryRemove(key, out _);
		}
	}
}