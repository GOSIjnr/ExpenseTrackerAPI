using ExpenseTracker.Application.Constants.Cache;
using ExpenseTracker.Application.Constants.Responses;
using ExpenseTracker.Application.Constants.Services;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Enums;
using ExpenseTracker.Application.Extensions.Responses;
using ExpenseTracker.Application.Helpers;
using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain.Entities.Users;
using ExpenseTracker.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Application.Features.Users.DeleteUser;

internal sealed class DeleteUserCommandHandler(
    AppDbContext db,
    ICacheService cacheService,
    [FromKeyedServices(HashPurpose.Email)] IHashService emailHashingService,
    [FromKeyedServices(HashPurpose.Password)] IHashService passwordHashingService,
    IEncryptionService encryptionService
) : IHandler<DeleteUserCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(DeleteUserCommand message, CancellationToken cancellationToken = default)
    {
        if (message.ActorId is null)
            throw ResponseCatalog.Auth.InvalidSession.ToException();

        Guid userId = message.ActorId.Value;

        User user = await db.Users
            .Include(u => u.Sessions.Where(s => !s.IsRevoked))
            .Where(u => !u.IsDeleted && u.Id == userId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw ResponseCatalog.User.NotFound.ToException();

        if (user.IsLocked)
            throw ResponseCatalog.Authorization.AccountLocked.ToException();

        if (user.Role is UserRole.SuperAdmin)
            throw ResponseCatalog.Authorization.CannotActOnSelf.ToException();

        if (user.IsDeleted)
            return ResponseCatalog.User.Deleted.ToOperationResult();

        string deletedUserEmail = $"deleted+{user.Id:N}@expense-tracker.local";

        string deletedEmailHash = emailHashingService.Hash(deletedUserEmail);
        string deletedPasswordHash = passwordHashingService.Hash(Guid.NewGuid().ToString());

        UserSensitive tombstone = UserSensitive.Create(
            firstName: "Deleted",
            middleName: null,
            lastName: "User",
            email: deletedUserEmail
        );

        byte[] encrypted = encryptionService.Encrypt(
            JsonByteArrayConverter.SerializeToUtf8Bytes(tombstone),
            CryptoPurpose.UserSensitiveData
        );

        user.TrySetEmailHash(deletedEmailHash);
        user.TrySetPasswordHash(deletedPasswordHash);
        user.SetEncryptedData(encrypted);
        user.TryDelete();

        foreach (UserSession session in user.Sessions)
            session.TryRevoke();

        await db.SaveChangesAsync(cancellationToken);

        IEnumerable<string> keys = [.. user.Sessions
            .Select(s => CacheKeys.SessionById(s.Id)),
            CacheKeys.UserAuthenticationState(user.Id),
            CacheKeys.UserProfileById(user.Id),
        ];

        await cacheService.RemoveManyAsync(keys);

        return ResponseCatalog.User.Deleted.ToOperationResult();
    }
}
