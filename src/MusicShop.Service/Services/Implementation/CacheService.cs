using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using MusicShop.Service.Services.Interfaces;

namespace MusicShop.Service.Services.Implementation;

/// <summary>
/// 記憶體快取服務實作
/// 使用 IMemoryCache 搭配鍵值追蹤，支援前綴批次清除
/// </summary>
public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, byte> _keys = new();

    /// <summary>
    /// 預設快取時間（30 分鐘），適用於分類、商品類型等不常變動的資料
    /// </summary>
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    /// <inheritdoc />
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (_cache.TryGetValue(key, out T? cached) && cached is not null)
            return cached;

        var value = await factory();
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(expiration ?? DefaultExpiration)
            .RegisterPostEvictionCallback((evictedKey, _, _, _) =>
            {
                _keys.TryRemove(evictedKey.ToString()!, out _);
            });

        _cache.Set(key, value, options);
        _keys.TryAdd(key, 0);
        return value;
    }

    /// <inheritdoc />
    public void Remove(string key)
    {
        _cache.Remove(key);
        _keys.TryRemove(key, out _);
    }

    /// <inheritdoc />
    public void RemoveByPrefix(string prefix)
    {
        var keysToRemove = _keys.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
        }
    }
}
