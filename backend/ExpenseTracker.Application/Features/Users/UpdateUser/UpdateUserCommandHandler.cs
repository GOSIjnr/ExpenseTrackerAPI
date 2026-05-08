using ExpenseTracker.Application.Constants.Cache;
using ExpenseTracker.Application.Constants.Responses;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Enums;
using ExpenseTracker.Application.Extensions.Responses;
using ExpenseTracker.Application.Helpers;
using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain.Entities.Users;
using ExpenseTracker.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Features.Users.UpdateUser;

internal sealed class UpdateUserCommandHandler(
    AppDbContext db,
    ICacheService cacheService,
    IEncryptionService encryptionService
) : IHandler<UpdateUserCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(UpdateUserCommand message, CancellationToken cancellationToken = default)
    {
        if (message.UserId is null)
            throw ResponseCatalog.Auth.InvalidSession.ToException();

        User user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == message.UserId.Value, cancellationToken)
            ?? throw ResponseCatalog.User.NotFound.ToException();

        bool requiresSensitiveUpdate = message.FirstName is not null
            || message.MiddleName is not null
            || message.LastName is not null;

        if (!string.IsNullOrWhiteSpace(message.UserName))
        {
            string normalizedUserName = message.UserName.Trim();

            if (!string.Equals(normalizedUserName, user.UserName, StringComparison.Ordinal))
            {
                bool exists = await db.Users
                    .AnyAsync(
                        u => u.Id != user.Id && u.UserName == normalizedUserName,
                        cancellationToken
                    );

                if (exists)
                    throw ResponseCatalog.User.UserNameExists.ToException();

                user.TrySetUserName(normalizedUserName);
            }
        }

        if (requiresSensitiveUpdate)
        {
            UserSensitive sensitiveData = JsonByteArrayConverter.DeserializeFromUtf8Bytes<UserSensitive>(
                encryptionService.Decrypt(user.EncryptedData, CryptoPurpose.UserSensitiveData)
            );

            user.SetSensitiveData(sensitiveData);

            string firstName = message.FirstName ?? sensitiveData.FirstName;
            string lastName = message.LastName ?? sensitiveData.LastName;
            string? middleName = message.MiddleName ?? sensitiveData.MiddleName;

            sensitiveData.UpdateName(firstName, middleName, lastName);

            byte[] sensitiveBytes = JsonByteArrayConverter.SerializeToUtf8Bytes(sensitiveData);

            byte[] encryptedData = encryptionService.Encrypt(
                sensitiveBytes,
                CryptoPurpose.UserSensitiveData
            );

            user.SetEncryptedData(encryptedData);
            user.ClearSensitiveData();
        }

        await db.SaveChangesAsync(cancellationToken);

        string cacheKey = CacheKeys.UserProfileById(user.Id);
        await cacheService.RemoveAsync(cacheKey);

        return ResponseCatalog.User.Updated.ToOperationResult();
    }
}
