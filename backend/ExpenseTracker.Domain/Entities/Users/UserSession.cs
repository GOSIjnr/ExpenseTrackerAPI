using ExpenseTracker.Domain.Abstractions;
using ExpenseTracker.Domain.Components.Auditing;
using ExpenseTracker.Domain.Helpers;

namespace ExpenseTracker.Domain.Entities.Users;

public sealed class UserSession : IEntity, IAuditable
{
    public AuditState AuditState { get; private set; } = new();

    private UserSession() { }

    public UserSession(Guid userId, bool rememberMe, TimeSpan slidingLifetime, TimeSpan absoluteLifetime)
    {
        UserId = Guard.AgainstEmptyGuid(userId, nameof(userId));
        RememberMe = rememberMe;
        (ExpiresAt, AbsoluteExpiresAt) = ValidateLifetimes(slidingLifetime, absoluteLifetime);

        AuditState.UpdateAudit();
    }

    public Guid Id { get; private set; } = Guid.CreateVersion7();

    public DateTime CreatedAt => AuditState.CreatedAt;
    public DateTime UpdatedAt => AuditState.UpdatedAt;

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public bool RememberMe { get; private set; }

    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }
    public DateTime AbsoluteExpiresAt { get; private set; }

    public bool IsExpired()
    {
        DateTime now = DateTime.UtcNow;
        return now >= ExpiresAt || now >= AbsoluteExpiresAt || IsRevoked;
    }

    public bool TryExtendSession(TimeSpan slidingLifetime)
    {
        if (IsRevoked)
            return false;

        DateTime now = DateTime.UtcNow;
        DateTime newExpiry = now.Add(slidingLifetime);

        if (newExpiry > AbsoluteExpiresAt)
            newExpiry = AbsoluteExpiresAt;

        if (newExpiry <= ExpiresAt)
            return false;

        ExpiresAt = newExpiry;
        AuditState.UpdateAudit();
        return true;
    }

    public bool TryRevoke()
    {
        if (IsRevoked)
            return false;

        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;

        AuditState.UpdateAudit();
        return true;
    }

    private static (DateTime, DateTime) ValidateLifetimes(TimeSpan slidingLifetime, TimeSpan absoluteLifetime)
    {
        if (slidingLifetime <= TimeSpan.Zero)
            throw new ArgumentException(
                "Sliding lifetime must be positive.", nameof(slidingLifetime)
            );

        if (absoluteLifetime < slidingLifetime)
            throw new ArgumentException(
                "Absolute lifetime must be greater than or equal to sliding lifetime.",
                nameof(absoluteLifetime)
            );

        DateTime now = DateTime.UtcNow;

        return (now.Add(slidingLifetime), now.Add(absoluteLifetime));
    }
}
