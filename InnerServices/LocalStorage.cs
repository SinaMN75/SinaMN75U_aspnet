namespace SinaMN75U.InnerServices;

public interface ILocalStorageService {
	public void SetStringData(string key, string value, TimeSpan? absoluteExpireTime = null, TimeSpan? slidingExpireTime = null);
	public string? GetStringData(string key);
	public void DeleteStringData(string recordId);
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
}