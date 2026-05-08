using ExpenseTracker.Application.Constants.Cache;
using ExpenseTracker.Application.Constants.Responses;
using ExpenseTracker.Application.Constants.Services;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Extensions.Responses;
using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain.Entities.Users;
using ExpenseTracker.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Application.Features.Users.UpdateUserPassword;

internal sealed class UpdateUserPasswordCommandHandler(
    AppDbContext db,
    [FromKeyedServices(HashPurpose.Password)] IHashService passwordHashingService,
    ICacheService cacheService
) : IHandler<UpdateUserPasswordCommand, OperationResult<object>>
{
    public async Task<OperationResult<object>> Handle(UpdateUserPasswordCommand message, CancellationToken cancellationToken = default)
    {
        if (message.UserId is null)
            throw ResponseCatalog.Auth.InvalidSession.ToException();

        User user = await db.Users
            .Include(us => us.Sessions.Where(s => !s.IsRevoked))
            .Where(u => u.Id == message.UserId.Value)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw ResponseCatalog.User.NotFound.ToException();

        bool isPasswordValid = passwordHashingService.Verify(message.CurrentPassword, user.PasswordHash);

        if (!isPasswordValid)
            throw ResponseCatalog.Auth.InvalidCurrentPassword.ToException();

        string newPasswordHash = passwordHashingService.Hash(message.NewPassword);
        user.TrySetPasswordHash(newPasswordHash);

        foreach (UserSession session in user.Sessions)
        {
            bool isCurrentSession = message.SessionId.HasValue && session.Id == message.SessionId.Value;

            if (!message.LogoutAll && isCurrentSession)
                continue;

            session.TryRevoke();
        }

        await db.SaveChangesAsync(cancellationToken);

        IEnumerable<string> keys = user.Sessions
            .Where(s => s.IsRevoked)
            .Select(s => CacheKeys.SessionById(s.Id));

        await cacheService.RemoveManyAsync(keys);

        return ResponseCatalog.User.PasswordUpdated.ToOperationResult();
    }
}
