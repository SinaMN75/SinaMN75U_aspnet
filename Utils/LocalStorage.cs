namespace SinaMN75U.Utils;

public static class LocalStorage {
	public static void SetStringData(
		this IDistributedCache cache,
		string key,
		string value,
		TimeSpan? absoluteExpireTime = null,
		TimeSpan? slidingExpireTime = null
	) => cache.SetStringAsync(key, value, new DistributedCacheEntryOptions {
		AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromSeconds(60),
		SlidingExpiration = slidingExpireTime
	});

	public static Task<string?> GetStringData(this IDistributedCache cache, string recordId) => cache.GetStringAsync(recordId);

	public static Task DeleteStringData(this IDistributedCache cache, string recordId) => cache.RemoveAsync(recordId);
}