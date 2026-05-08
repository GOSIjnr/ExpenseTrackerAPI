using ExpenseTracker.Application.Constants.Cache;
using ExpenseTracker.Application.Constants.Responses;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Extensions.Responses;
using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain.Entities.Users;
using ExpenseTracker.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Users.LockUser;

internal sealed class LockUserCommandHandler(
    AppDbContext db,
    ICacheService cacheService
) : IHandler<LockUserCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(LockUserCommand message, CancellationToken cancellationToken = default)
    {
        if (message.ActorId is null)
            throw ResponseCatalog.Auth.InvalidSession.ToException();

        string actorAuthCacheKey = CacheKeys.UserAuthenticationState(message.ActorId.Value);
        UserAuthData? actorAuth = await cacheService.GetAsync<UserAuthData>(actorAuthCacheKey);

        actorAuth ??= await db.Users
            .AsNoTracking()
            .Where(u => !u.IsDeleted && u.Id == message.ActorId)
            .Select(u => new UserAuthData(
                UserId: message.ActorId.Value,
                Role: u.Role,
                IsLocked: u.IsLocked
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (actorAuth is null)
            throw ResponseCatalog.Auth.InvalidSession.ToException();

        if (actorAuth.IsLocked)
            throw ResponseCatalog.Authorization.AccountLocked.ToException();

        if (message.ActorId == message.TargetId)
            throw ResponseCatalog.Authorization.CannotActOnSelf.ToException();

        User targetUser = await db.Users
            .Include(u => u.Sessions.Where(us => !us.IsRevoked))
            .Where(u => !u.IsDeleted && u.Id == message.TargetId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw ResponseCatalog.User.NotFound.ToException();

        if (targetUser.Role is UserRole.SuperAdmin)
            throw ResponseCatalog.Authorization.Forbidden.ToException();

        if (actorAuth.Role is UserRole.Admin && targetUser.Role is UserRole.Admin)
            throw ResponseCatalog.Authorization.Forbidden.ToException();

        if (targetUser.IsLocked)
            return ResponseCatalog.User.Locked.ToOperationResult();

        targetUser.TryLock();

        foreach (UserSession session in targetUser.Sessions)
            session.TryRevoke();

        await db.SaveChangesAsync(cancellationToken);

        IEnumerable<string> keys = [.. targetUser.Sessions
            .Select(s => CacheKeys.SessionById(s.Id)),
            CacheKeys.UserAuthenticationState(targetUser.Id),
        ];

        await cacheService.RemoveManyAsync(keys);

        return ResponseCatalog.User.Locked.ToOperationResult();
    }
}
