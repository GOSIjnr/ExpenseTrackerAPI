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

namespace ExpenseTracker.Application.Features.Auth.RegisterUser;

internal sealed class RegisterUserCommandHandler(
    AppDbContext db,
    [FromKeyedServices(HashPurpose.Email)] IHashService emailHashingService,
    [FromKeyedServices(HashPurpose.Password)] IHashService passwordHashingService,
    IEncryptionService encryptionService
) : IHandler<RegisterUserCommand, OperationResult<Guid>>
{
    public async Task<OperationResult<Guid>> Handle(RegisterUserCommand message, CancellationToken cancellationToken = default)
    {
        string emailHash = emailHashingService.Hash(message.Email.Trim());
        string normalizedUserName = message.UserName.Trim();

        var existingUser = await db.Users
            .AsNoTracking()
            .Where(u =>
                (u.EmailHash == emailHash || u.UserName == normalizedUserName)
                && !u.IsDeleted
            )
            .Select(u => new { u.EmailHash, u.UserName })
            .FirstOrDefaultAsync(cancellationToken);

        if (existingUser is not null)
        {
            if (existingUser.EmailHash == emailHash)
                throw ResponseCatalog.Auth.EmailAlreadyExists.ToException();

            throw ResponseCatalog.User.UserNameExists.ToException();
        }

        UserSensitive sensitiveData = UserSensitive.Create(
            firstName: message.FirstName.Trim(),
            middleName: message.MiddleName?.Trim(),
            lastName: message.LastName.Trim(),
            email: message.Email.Trim()
        );

        byte[] sensitiveDataBytes = JsonByteArrayConverter.SerializeToUtf8Bytes(sensitiveData);
        byte[] encryptedData = encryptionService.Encrypt(sensitiveDataBytes, CryptoPurpose.UserSensitiveData);

        string passwordHash = passwordHashingService.Hash(message.Password);

        User user = new(
            userName: normalizedUserName,
            emailHash: emailHash,
            passwordHash: passwordHash
        );

        user.SetEncryptedData(encryptedData);

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return ResponseCatalog.Auth.RegistrationSuccessful
            .As<Guid>()
            .WithData(user.Id)
            .ToOperationResult();
    }
}
