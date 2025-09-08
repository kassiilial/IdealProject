using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Config;

namespace Services;

public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly CacheSettings _settings;

    public DistributedCacheService(IDistributedCache cache, IOptions<CacheSettings> options)
    {
        _cache = cache;
        _settings = options.Value;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var data = await _cache.GetAsync(key);
        if (data == null) return default;
        var json = Encoding.UTF8.GetString(data);
        return JsonSerializer.Deserialize<T>(json);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        var json = JsonSerializer.Serialize(value);
        var data = Encoding.UTF8.GetBytes(json);
        var options = new DistributedCacheEntryOptions();
        var expire = ttl ?? TimeSpan.FromSeconds(_settings?.DefaultTtlSeconds ?? 300);
        if (_settings?.UseSlidingExpiration == true)
            options.SetSlidingExpiration(expire);
        else
            options.SetAbsoluteExpiration(expire);
        await _cache.SetAsync(key, data, options);
    }
}
