using ExpenseTracker.Application.Configurations.Caching;
using ExpenseTracker.Application.Constants.Cache;
using ExpenseTracker.Application.Helpers;
using ExpenseTracker.Application.Models;
using ExpenseTracker.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Application.Services;

public sealed class SessionStateService(
    AppDbContext db,
    ICacheService cacheService,
    IOptions<CacheTimeToLiveOptions> cacheOptions
)
{
    public async Task<SessionData?> GetSessionDataAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        string cacheKey = CacheKeys.SessionById(sessionId);
        SessionData? cachedSession = await cacheService.GetAsync<SessionData>(cacheKey);

        if (cachedSession is not null)
            return cachedSession;

        SessionData? sessionData = await db.UserSessions
            .AsNoTracking()
            .Where(s => s.Id == sessionId)
            .Select(s => new SessionData(
                SessionId: s.Id,
                UserId: s.UserId,
                CreatedAt: s.CreatedAt,
                ExpiresAt: s.ExpiresAt,
                AbsoluteExpiresAt: s.AbsoluteExpiresAt,
                IsRevoked: s.IsRevoked,
                RememberMe: s.RememberMe
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (sessionData is null)
            return null;

        TimeSpan cacheDuration = SessionHelper.CalculateCacheTimeToLive(
            sessionData.ExpiresAt,
            cacheOptions.Value.AuthSessionById.Ttl
        );

        if (cacheDuration > TimeSpan.Zero)
            await cacheService.SetAsync(cacheKey, new CacheEntry<SessionData>(sessionData, cacheDuration));

        return sessionData;
    }
}
