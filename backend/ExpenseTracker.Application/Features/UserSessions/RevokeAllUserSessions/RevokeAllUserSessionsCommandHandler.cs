using ExpenseTracker.Application.Constants.Cache;
using ExpenseTracker.Application.Constants.Responses;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Extensions.Responses;
using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain.Entities.Users;
using ExpenseTracker.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.UserSessions.RevokeAllUserSessions;

internal sealed class RevokeAllUserSessionsCommandHandler(
    AppDbContext db,
    ICacheService cacheService
) : IHandler<RevokeAllUserSessionsCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(RevokeAllUserSessionsCommand message, CancellationToken cancellationToken = default)
    {
        if (message.UserId is null)
            throw ResponseCatalog.Auth.InvalidSession.ToException();

        if (message.ActorId != message.UserId)
            throw ResponseCatalog.Authorization.Forbidden.ToException();

        IQueryable<UserSession> query = db.UserSessions
            .Where(s => s.UserId == message.UserId && !s.IsRevoked);

        if (message.SessionId.HasValue)
            query = query.Where(s => s.Id != message.SessionId.Value);

        List<UserSession> sessions = await query.ToListAsync(cancellationToken);

        if (sessions.Count > 0)
        {
            foreach (UserSession session in sessions)
                session.TryRevoke();

            await db.SaveChangesAsync(cancellationToken);

            IEnumerable<string> keys = sessions.Select(s => CacheKeys.SessionById(s.Id));
            await cacheService.RemoveManyAsync(keys);
        }

        return ResponseCatalog.Auth.SessionRevoked.ToOperationResult();
    }
}
