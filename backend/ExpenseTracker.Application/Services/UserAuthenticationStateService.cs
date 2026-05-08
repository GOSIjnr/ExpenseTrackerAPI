using ExpenseTracker.Application.Configurations.Caching;
using ExpenseTracker.Application.Constants.Cache;
using ExpenseTracker.Application.Models;
using ExpenseTracker.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Application.Services;

public sealed class UserAuthenticationStateService(
    AppDbContext db,
    ICacheService cacheService,
    IOptions<CacheTimeToLiveOptions> cacheTtlOptions
)
{
    public async Task<UserAuthData?> GetUserAuthDataAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        string cacheKey = CacheKeys.UserAuthenticationState(userId);
        UserAuthData? cachedData = await cacheService.GetAsync<UserAuthData>(cacheKey);

        if (cachedData is not null)
            return cachedData;

        UserAuthData? userAuthData = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId && !u.IsDeleted)
            .Select(u => new UserAuthData(
                u.Id,
                u.Role,
                u.IsLocked
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (userAuthData is null)
            return null;

        TimeSpan timeToLive = cacheTtlOptions.Value.UserAuthenticationState.Ttl;
        await cacheService.SetAsync(cacheKey, new CacheEntry<UserAuthData>(userAuthData, timeToLive));

        return userAuthData;
    }
}
