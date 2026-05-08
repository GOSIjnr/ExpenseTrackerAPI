using ExpenseTracker.Application.Configurations.Caching;
using ExpenseTracker.Application.Constants.Cache;
using ExpenseTracker.Application.Constants.Responses;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Extensions.Responses;
using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Application.Features.UserSessions.GetCurrentUserSessions;

internal sealed class GetCurrentUserSessionsQueryHandler(
    AppDbContext db,
    ICacheService cacheService,
    IOptions<CacheTimeToLiveOptions> cacheTtlOptions
) : IHandler<GetCurrentUserSessionsQuery, OperationResult<IReadOnlyList<SessionData>>>
{
    public async Task<OperationResult<IReadOnlyList<SessionData>>> Handle(GetCurrentUserSessionsQuery message, CancellationToken cancellationToken = default)
    {
        if (message.UserId is null)
            throw ResponseCatalog.Auth.InvalidSession.ToException();

        DateTime now = DateTime.UtcNow;

        List<Guid> sessionIds = await db.UserSessions
            .AsNoTracking()
            .Where(s =>
                s.UserId == message.UserId &&
                !s.IsRevoked &&
                now < s.ExpiresAt &&
                now < s.AbsoluteExpiresAt
            )
            .OrderByDescending(s => s.AuditState.CreatedAt)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        if (sessionIds.Count == 0)
        {
            return ResponseCatalog.Session.Retrieved
                .As<IReadOnlyList<SessionData>>()
                .ToOperationResult();
        }

        List<string> keys = [.. sessionIds.Select(CacheKeys.SessionById)];

        IReadOnlyDictionary<string, SessionData?> cacheResults =
            await cacheService.GetManyAsync<SessionData>(keys);

        List<SessionData> sessions = new(sessionIds.Count);
        List<Guid> missingIds = [];

        for (int index = 0; index < sessionIds.Count; index++)
        {
            string key = keys[index];
            SessionData? cached = cacheResults[key];

            if (cached is not null)
            {
                sessions.Add(cached);
            }
            else
            {
                missingIds.Add(sessionIds[index]);
            }
        }

        if (missingIds.Count > 0)
        {
            List<SessionData> missingSessions = await db.UserSessions
                .AsNoTracking()
                .Where(s => missingIds.Contains(s.Id))
                .Select(s => new SessionData(
                    s.Id,
                    s.UserId,
                    s.CreatedAt,
                    s.ExpiresAt,
                    s.AbsoluteExpiresAt,
                    s.IsRevoked,
                    s.RememberMe
                ))
                .ToListAsync(cancellationToken);

            sessions.AddRange(missingSessions);

            Dictionary<string, CacheEntry<SessionData>> cachePayload = missingSessions.ToDictionary(
                s => CacheKeys.SessionById(s.SessionId),
                s => new CacheEntry<SessionData>(s, cacheTtlOptions.Value.AuthSessionById.Ttl)
            );

            await cacheService.SetManyAsync(cachePayload);
        }

        sessions = [.. sessions.OrderByDescending(s => s.CreatedAt)];

        return ResponseCatalog.Session.Retrieved
            .As<IReadOnlyList<SessionData>>()
            .WithData(sessions)
            .ToOperationResult();
    }
}
