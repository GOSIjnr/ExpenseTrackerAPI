using ExpenseTracker.Application.Configurations.Caching;
using ExpenseTracker.Application.Constants.Cache;
using ExpenseTracker.Application.Constants.Responses;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Enums;
using ExpenseTracker.Application.Extensions.Entities;
using ExpenseTracker.Application.Extensions.Responses;
using ExpenseTracker.Application.Features.Users.Models;
using ExpenseTracker.Application.Helpers;
using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain.Entities.Users;
using ExpenseTracker.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Application.Features.Users.GetUserById;

internal sealed class GetUserByIdQueryHandler(
    AppDbContext db,
    ICacheService cacheService,
    IOptions<CacheTimeToLiveOptions> cacheTtlOptions,
    IEncryptionService encryptionService
) : IHandler<GetUserByIdQuery, OperationResult<UserResponse>>
{
    public async Task<OperationResult<UserResponse>> Handle(GetUserByIdQuery message, CancellationToken cancellationToken = default)
    {
        if (message.Id is null)
            throw ResponseCatalog.Auth.InvalidSession.ToException();

        string userProfileCacheKey = CacheKeys.UserProfileById(message.Id.Value);
        UserResponse? cachedUserProfile = await cacheService.GetAsync<UserResponse>(userProfileCacheKey);

        if (cachedUserProfile is not null)
            return ResponseCatalog.User.Retrieved
                .As<UserResponse>()
                .WithData(cachedUserProfile)
                .ToOperationResult();

        User user = await db.Users
            .Where(u => !u.IsDeleted && u.Id == message.Id.Value)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw ResponseCatalog.User.NotFound.ToException();

        UserSensitive sensitiveData = JsonByteArrayConverter.DeserializeFromUtf8Bytes<UserSensitive>(
            encryptionService.Decrypt(user.EncryptedData, CryptoPurpose.UserSensitiveData)
        );

        user.SetSensitiveData(sensitiveData);
        UserResponse response = user.ToUserResponse();

        CacheEntry<UserResponse> cacheEntry = new(response, cacheTtlOptions.Value.UserProfileById.Ttl);
        await cacheService.SetAsync(userProfileCacheKey, cacheEntry);

        return ResponseCatalog.User.Retrieved
            .As<UserResponse>()
            .WithData(response)
            .ToOperationResult();
    }
}
