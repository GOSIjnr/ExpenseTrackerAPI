using ExpenseTracker.Application.Constants.Cache;
using ExpenseTracker.Application.Constants.Responses;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Extensions.Responses;
using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain.Entities.Users;
using ExpenseTracker.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Users.DemoteUser;

internal sealed class DemoteUserCommandHandler(
    AppDbContext db,
    ICacheService cacheService
) : IHandler<DemoteUserCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(DemoteUserCommand message, CancellationToken cancellationToken = default)
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

        if (actorAuth.Role is not UserRole.SuperAdmin)
            throw ResponseCatalog.Authorization.Forbidden.ToException();

        User targetUser = await db.Users
            .Where(u => !u.IsDeleted && u.Id == message.TargetId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw ResponseCatalog.User.NotFound.ToException();

        UserRole newRole = targetUser.Role switch
        {
            UserRole.User => throw ResponseCatalog.User.AlreadyBottomRole.ToException(),
            UserRole.Admin => UserRole.User,
            UserRole.SuperAdmin => throw ResponseCatalog.Authorization.CannotDemoteSuperAdmin.ToException(),
            _ => throw ResponseCatalog.User.AlreadyBottomRole.ToException()
        };

        targetUser.TryUpdateRole(newRole);
        await db.SaveChangesAsync(cancellationToken);

        await cacheService.RemoveManyAsync(
        [
            CacheKeys.UserAuthenticationState(targetUser.Id),
            CacheKeys.UserProfileById(targetUser.Id),
        ]);

        return ResponseCatalog.User.Demoted.ToOperationResult();
    }
}
