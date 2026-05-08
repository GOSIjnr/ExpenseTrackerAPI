using ExpenseTracker.Application.Configurations.Caching;
using ExpenseTracker.Application.Configurations.Security;
using ExpenseTracker.Application.Constants.Cache;
using ExpenseTracker.Application.Constants.Responses;
using ExpenseTracker.Application.Constants.Services;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Application.Features.Auth.LoginUser;

internal sealed class LoginUserCommandHandler(
    AppDbContext db,
    [FromKeyedServices(HashPurpose.Email)] IHashService emailHashingService,
    [FromKeyedServices(HashPurpose.Password)] IHashService passwordHashingService,
    ICacheService cacheService,
    IOptions<CacheTimeToLiveOptions> cacheTtlOptions,
    IOptions<SessionLifetimeOptions> sessionLifetimeOptions
) : IHandler<LoginUserCommand, OperationResult<SessionResult>>
{
    public async Task<OperationResult<SessionResult>> Handle(LoginUserCommand message, CancellationToken cancellationToken = default)
    {
        string emailHash = emailHashingService.Hash(message.Identifier.Trim());

        var userCredentials = await db.Users
            .AsNoTracking()
            .Where(u =>
                (u.EmailHash == emailHash || u.UserName == message.Identifier)
                && !u.IsDeleted
            )
            .Select(u => new
            {
                u.Id,
                u.PasswordHash,
                u.IsLocked
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw ResponseCatalog.Auth.InvalidCredentials.ToException();

        if (userCredentials.IsLocked)
            throw ResponseCatalog.Authorization.AccountLocked.ToException();

        if (!passwordHashingService.Verify(message.Password, userCredentials.PasswordHash))
            throw ResponseCatalog.Auth.InvalidCredentials.ToException();

        SessionLifetimeOptions opts = sessionLifetimeOptions.Value;

        if (message.ActiveSessionId.HasValue)
        {
            UserSession? existingSession = await db.UserSessions
                .FirstOrDefaultAsync(us =>
                    us.Id == message.ActiveSessionId.Value
                    && us.UserId == userCredentials.Id
                    && !us.IsRevoked,
                    cancellationToken
                );

            existingSession?.TryRevoke();
        }

        TimeSpan lifetime = message.RememberMe
            ? opts.ExtendedSessionDuration.Duration
            : opts.StandardSessionDuration.Duration;

        UserSession session = new(
            userId: userCredentials.Id,
            rememberMe: message.RememberMe,
            slidingLifetime: lifetime,
            absoluteLifetime: opts.AbsoluteSessionLimit.Duration
        );

        db.UserSessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);

        SessionData sessionData = session.ToSessionData();

        TimeSpan cacheTimeToLive = SessionHelper.CalculateCacheTimeToLive(
            session.ExpiresAt,
            cacheTtlOptions.Value.AuthSessionById.Ttl
        );

        CacheEntry<SessionData> entry = new(sessionData, cacheTimeToLive);
        string cacheKey = CacheKeys.SessionById(session.Id);

        await cacheService.SetAsync(cacheKey, entry);

        SessionResult sessionResult = new(
            session.Id,
            session.ToTimestampsResponse()
        );

        return ResponseCatalog.Auth.LoginSuccessful
            .As<SessionResult>()
            .WithData(sessionResult)
            .ToOperationResult();
    }
}
