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

namespace ExpenseTracker.Application.Features.Seeders.SeedSuperAdmin;

internal sealed class SeedSuperAdminCommandHandler(
    AppDbContext db,
    [FromKeyedServices(HashPurpose.Email)] IHashService emailHashingService,
    [FromKeyedServices(HashPurpose.Password)] IHashService passwordHashingService,
    IEncryptionService encryptionService
) : IHandler<SeedSuperAdminCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(SeedSuperAdminCommand message, CancellationToken cancellationToken = default)
    {
        string emailHash = emailHashingService.Hash(message.Email.Trim());
        string normalizedUserName = message.UserName.Trim();

        var existingUser = await db.Users
            .AsNoTracking()
            .Where(u =>
                !u.IsDeleted &&
                (
                    u.Role == UserRole.SuperAdmin ||
                    u.EmailHash == emailHash ||
                    u.UserName == normalizedUserName
                )
            )
            .Select(u => new
            {
                u.Role,
                u.EmailHash,
                u.UserName
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (existingUser?.Role == UserRole.SuperAdmin)
            return ResponseCatalog.System.SuperAdminSeeded.ToOperationResult();

        if (existingUser?.EmailHash == emailHash)
            throw ResponseCatalog.Auth.EmailAlreadyExists.ToException();

        if (existingUser?.UserName == normalizedUserName)
            throw ResponseCatalog.User.UserNameExists.ToException();

        UserSensitive sensitiveData = UserSensitive.Create(
            message.FirstName.Trim(),
            message.MiddleName?.Trim(),
            message.LastName.Trim(),
            message.Email.Trim()
        );

        byte[] encryptedData = encryptionService.Encrypt(
            JsonByteArrayConverter.SerializeToUtf8Bytes(sensitiveData),
            CryptoPurpose.UserSensitiveData
        );

        User user = new(
            normalizedUserName,
            emailHash,
            passwordHashingService.Hash(message.Password)
        );

        user.SetEncryptedData(encryptedData);
        user.TryUpdateRole(UserRole.SuperAdmin);

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return ResponseCatalog.System.SuperAdminSeeded.ToOperationResult();
    }
}
