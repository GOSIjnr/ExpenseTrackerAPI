using ExpenseTracker.Application.Constants.Cache;
using ExpenseTracker.Application.Constants.Responses;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Extensions.Responses;
using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain.Entities.Users;
using ExpenseTracker.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Auth.LogoutUser;

internal sealed class LogoutUserCommandHandler(
    AppDbContext db,
    ICacheService cacheService
) : IHandler<LogoutUserCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(LogoutUserCommand message, CancellationToken cancellationToken = default)
    {
        if (!message.SessionId.HasValue)
            return ResponseCatalog.Auth.LogoutSuccessful.ToOperationResult();

        UserSession? session = await db.UserSessions
            .FirstOrDefaultAsync(us => us.Id == message.SessionId && !us.IsRevoked, cancellationToken);

        if (session is null)
            return ResponseCatalog.Auth.LogoutSuccessful.ToOperationResult();

        session.TryRevoke();

        await db.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveAsync(CacheKeys.SessionById(message.SessionId.Value));

        return ResponseCatalog.Auth.LogoutSuccessful.ToOperationResult();
    }
}
