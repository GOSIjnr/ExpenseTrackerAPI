using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Application.Services;

public interface ICacheService
{
    Task SetAsync<T>(string key, CacheEntry<T> entry);
    Task<T?> GetAsync<T>(string key);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);

    Task SetManyAsync<T>(IDictionary<string, CacheEntry<T>> entries);
    Task<IReadOnlyDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys);
    Task RemoveManyAsync(IEnumerable<string> keys);
    Task<IReadOnlyDictionary<string, bool>> ExistsManyAsync(IEnumerable<string> keys);
}
