using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Services;
using Microsoft.Extensions.Caching.Memory;

namespace ExpenseTracker.Infrastructure.Services;

internal sealed class InMemoryCacheService(IMemoryCache cache) : ICacheService
{
    private readonly IMemoryCache _cache = cache;

    public Task SetAsync<T>(string key, CacheEntry<T> entry)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Task.CompletedTask;

        if (entry.TimeToLive.HasValue)
            _cache.Set(key, entry.Value, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = entry.TimeToLive.Value,
                Size = 1,
            });
        else
            _cache.Set(key, entry.Value, new MemoryCacheEntryOptions
            {
                Size = 1,
            });

        return Task.CompletedTask;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Task.FromResult<T?>(default);

        _cache.TryGetValue(key, out T? value);

        return Task.FromResult(value);
    }

    public Task RemoveAsync(string key)
    {
        if (!string.IsNullOrWhiteSpace(key))
            _cache.Remove(key);

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Task.FromResult(false);

        return Task.FromResult(_cache.TryGetValue(key, out _));
    }

    public Task SetManyAsync<T>(IDictionary<string, CacheEntry<T>> entries)
    {
        foreach ((string key, CacheEntry<T> entry) in entries)
        {
            if (string.IsNullOrWhiteSpace(key))
                continue;

            if (entry.TimeToLive.HasValue)
                _cache.Set(key, entry.Value, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = entry.TimeToLive.Value,
                    Size = 1,
                });
            else
                _cache.Set(key, entry.Value);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys)
    {
        Dictionary<string, T?> results = [];

        foreach (string key in keys)
        {
            if (string.IsNullOrWhiteSpace(key))
                continue;

            results[key] = _cache.TryGetValue(key, out T? value)
                ? value
                : default;
        }

        return Task.FromResult<IReadOnlyDictionary<string, T?>>(results);
    }

    public Task RemoveManyAsync(IEnumerable<string> keys)
    {
        foreach (string key in keys)
        {
            if (!string.IsNullOrWhiteSpace(key))
                _cache.Remove(key);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyDictionary<string, bool>> ExistsManyAsync(IEnumerable<string> keys)
    {
        Dictionary<string, bool> results = [];

        foreach (string key in keys)
        {
            if (string.IsNullOrWhiteSpace(key))
                continue;

            results[key] = _cache.TryGetValue(key, out _);
        }

        return Task.FromResult<IReadOnlyDictionary<string, bool>>(results);
    }
}
