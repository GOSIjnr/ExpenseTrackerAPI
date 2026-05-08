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

namespace ExpenseTracker.Application.Features.Users.GetUsers;

internal sealed class GetUsersQueryHandler(
    AppDbContext db,
    ICacheService cacheService,
    IOptions<CacheTimeToLiveOptions> cacheTtlOptions,
    IEncryptionService encryptionService
) : IHandler<GetUsersQuery, OperationResult<CursorPage<UserResponse>>>
{
    public async Task<OperationResult<CursorPage<UserResponse>>> Handle(GetUsersQuery message, CancellationToken cancellationToken = default)
    {
        IQueryable<User> query = db.Users.AsNoTracking();

        if (message.Id.HasValue)
            query = query.Where(u => u.Id == message.Id.Value);

        if (!string.IsNullOrWhiteSpace(message.UserName))
        {
            string userName = message.UserName.Trim().ToUpperInvariant();

            query = query.Where(u =>
                u.UserName != null &&
                u.UserName.Contains(userName));
        }

        if (message.Cursor.HasValue)
            query = query.Where(u => u.Id > message.Cursor.Value);

        List<User> usersPage = await query
            .OrderBy(u => u.Id)
            .Take(message.Limit + 1)
            .ToListAsync(cancellationToken);

        bool hasMore = usersPage.Count > message.Limit;

        if (hasMore)
            usersPage = [.. usersPage.Take(message.Limit)];

        List<UserResponse> responses = new(usersPage.Count);
        List<User> missingUsers = [];

        IEnumerable<string> cacheKeys = [.. usersPage.Select(u => CacheKeys.UserProfileById(u.Id))];

        IReadOnlyDictionary<string, UserResponse?> cacheEntries = await cacheService
            .GetManyAsync<UserResponse>(cacheKeys);

        foreach (User user in usersPage)
        {
            string key = CacheKeys.UserProfileById(user.Id);

            if (cacheEntries.TryGetValue(key, out UserResponse? cached) && cached is not null)
            {
                responses.Add(cached);
            }
            else
            {
                missingUsers.Add(user);
            }
        }

        foreach (User user in missingUsers)
        {
            UserSensitive sensitiveData =
                JsonByteArrayConverter.DeserializeFromUtf8Bytes<UserSensitive>(
                    encryptionService.Decrypt(
                        user.EncryptedData,
                        CryptoPurpose.UserSensitiveData
                    )
                );

            user.SetSensitiveData(sensitiveData);

            UserResponse response = user.ToUserResponse();

            CacheEntry<UserResponse> cacheEntry = new(
                response,
                cacheTtlOptions.Value.UserProfileById.Ttl
            );

            string cacheKey = CacheKeys.UserProfileById(user.Id);

            await cacheService.SetAsync(cacheKey, cacheEntry);

            responses.Add(response);
        }

        responses = [.. responses.OrderBy(r => r.Id)];

        Guid? nextCursor = hasMore && responses.Count > 0
            ? responses[^1].Id
            : null;

        CursorPage<UserResponse> page = new(
            Items: responses,
            NextCursor: nextCursor,
            HasMore: hasMore
        );

        return ResponseCatalog.User.Retrieved
            .As<CursorPage<UserResponse>>()
            .WithData(page)
            .ToOperationResult();
    }
}
