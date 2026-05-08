using ExpenseTracker.Application.Constants.Cache;
using ExpenseTracker.Application.Constants.Responses;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Extensions.Responses;
using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain.Entities.Users;
using ExpenseTracker.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.UserSessions.RevokeUserSession;

internal sealed class RevokeUserSessionCommandHandler(
    AppDbContext db,
    ICacheService cacheService
) : IHandler<RevokeUserSessionCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(RevokeUserSessionCommand message, CancellationToken cancellationToken = default)
    {
        if (!message.SessionId.HasValue)
            throw ResponseCatalog.Auth.InvalidSession.ToException();

        if (message.ActorId != message.UserId)
            throw ResponseCatalog.Authorization.Forbidden.ToException();

        UserSession? session = await db.UserSessions
            .FirstOrDefaultAsync(s => s.Id == message.SessionId && !s.IsRevoked, cancellationToken);

        if (session is null)
            return ResponseCatalog.Auth.SessionRevoked.ToOperationResult();

        if (session.TryRevoke())
        {
            await db.SaveChangesAsync(cancellationToken);

            string cacheKey = CacheKeys.SessionById(session.Id);
            await cacheService.RemoveAsync(cacheKey);
        }

        return ResponseCatalog.Auth.SessionRevoked.ToOperationResult();
    }
}
