namespace ExpenseTracker.Application.Models;

public sealed record CacheEntry<T>(
    T Value,
    TimeSpan? TimeToLive
);
