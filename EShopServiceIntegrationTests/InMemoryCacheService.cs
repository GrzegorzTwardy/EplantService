using EShopApplication;

namespace EShopApplicationTests;

public class InMemoryCacheService : IRedisCacheService
{
    private readonly Dictionary<string, (object Value, DateTime Expiry)> _cache = new();

    public Task<T?> GetAsync<T>(string key)
    {
        if (!_cache.TryGetValue(key, out var entry))
            return Task.FromResult(default(T));

        if (DateTime.UtcNow > entry.Expiry)
        {
            _cache.Remove(key);
            return Task.FromResult(default(T));
        }

        return Task.FromResult((T?)entry.Value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var expiry = expiration.HasValue
            ? DateTime.UtcNow.Add(expiration.Value)
            : DateTime.UtcNow.AddHours(1);

        _cache[key] = (value!, expiry);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}