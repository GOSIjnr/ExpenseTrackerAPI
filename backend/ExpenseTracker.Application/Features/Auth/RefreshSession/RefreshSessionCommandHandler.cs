using ExpenseTracker.Application.Configurations.Caching;
using ExpenseTracker.Application.Configurations.Security;
using ExpenseTracker.Application.Constants.Cache;
using ExpenseTracker.Application.Constants.Responses;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Extensions.Entities;
using ExpenseTracker.Application.Extensions.Responses;
using ExpenseTracker.Application.Features.Auth.Extensions;
using ExpenseTracker.Application.Features.Auth.Models;
using ExpenseTracker.Application.Helpers;
using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain.Entities.Users;
using ExpenseTracker.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Application.Features.Auth.RefreshSession;

internal sealed class RefreshSessionCommandHandler(
    AppDbContext db,
    ICacheService cacheService,
    IOptions<CacheTimeToLiveOptions> cacheTtlOptions,
    IOptions<SessionLifetimeOptions> sessionLifeTimeOptions
) : IHandler<RefreshSessionCommand, OperationResult<SessionResult>>
{
    public async Task<OperationResult<SessionResult>> Handle(RefreshSessionCommand message, CancellationToken cancellationToken = default)
    {
        if (message.SessionId is null)
            throw ResponseCatalog.Auth.InvalidSession.ToException();

        Guid sessionId = message.SessionId.Value;
        string cacheKey = CacheKeys.SessionById(sessionId);
        SessionLifetimeOptions sessionOpts = sessionLifeTimeOptions.Value;

        SessionData? cachedSessionData = await cacheService.GetAsync<SessionData>(cacheKey);
        SessionData sessionData;
        UserSession? session = null;

        if (cachedSessionData is not null && !cachedSessionData.IsExpired())
        {
            sessionData = cachedSessionData;
        }
        else
        {
            session = await db.UserSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

            if (session is null || session.IsExpired())
                throw ResponseCatalog.Auth.InvalidSession.ToException();

            sessionData = session.ToSessionData();
        }

        bool shouldRefresh = sessionData.ShouldRefresh(sessionOpts.ExpiryExtensionTriggerPercent);

        if (shouldRefresh)
        {
            session ??= await db.UserSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

            if (session is null || session.IsExpired())
                throw ResponseCatalog.Auth.InvalidSession.ToException();

            TimeSpan extension = session.RememberMe
                ? sessionOpts.ExtendedExpiryExtension.Duration
                : sessionOpts.StandardExpiryExtension.Duration;

            if (session.TryExtendSession(extension))
            {
                await db.SaveChangesAsync(cancellationToken);
                sessionData = session.ToSessionData();
            }
        }

        TimeSpan cacheTimeToLive = SessionHelper.CalculateCacheTimeToLive(
            sessionData.ExpiresAt,
            cacheTtlOptions.Value.AuthSessionById.Ttl
        );

        CacheEntry<SessionData> entry = new(sessionData, cacheTimeToLive);
        await cacheService.SetAsync(cacheKey, entry);

        SessionResult sessionResult = new(
            sessionData.SessionId,
            sessionData.ToTimestampsResponse()
        );

        return ResponseCatalog.Auth.SessionRefreshed
            .As<SessionResult>()
            .WithData(sessionResult)
            .ToOperationResult();
    }
}
