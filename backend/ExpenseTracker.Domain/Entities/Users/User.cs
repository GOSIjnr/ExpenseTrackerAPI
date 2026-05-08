using ExpenseTracker.Domain.Abstractions;
using ExpenseTracker.Domain.Components.Auditing;
using ExpenseTracker.Domain.Components.Security;
using ExpenseTracker.Domain.Helpers;

namespace ExpenseTracker.Domain.Entities.Users;

public sealed class User : IEntity, IAuditable, IHasSensitiveData<UserSensitive>
{
    public AuditState AuditState { get; private set; } = new();
    public SensitiveDataState<UserSensitive> SensitiveDataState { get; private set; } = new();

    private User() { }

    public User(string userName, string emailHash, string passwordHash)
    {
        UserName = ValidateUserName(userName);
        EmailHash = ValidateEmailHash(emailHash);
        PasswordHash = ValidatePasswordHash(passwordHash);

        AuditState.UpdateAudit();
    }

    public Guid Id { get; private set; } = Guid.CreateVersion7();

    public DateTime CreatedAt => AuditState.CreatedAt;
    public DateTime UpdatedAt => AuditState.UpdatedAt;

    public byte[] EncryptedData => SensitiveDataState.EncryptedData;
    public UserSensitive? SensitiveData => SensitiveDataState.SensitiveData;

    public ICollection<UserSession> Sessions { get; private set; } = [];

    public string UserName { get; private set; } = string.Empty;
    public string EmailHash { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; } = UserRole.User;

    public bool IsLocked { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public void SetEncryptedData(byte[] newData)
    {
        SensitiveDataState.SetEncryptedData(newData);
        AuditState.UpdateAudit();
    }

    public void SetSensitiveData(UserSensitive data) => SensitiveDataState.SetSensitiveData(data);
    public void ClearSensitiveData() => SensitiveDataState.ClearSensitiveData();

    public bool TrySetUserName(string newUserName)
    {
        if (IsDeleted)
            return false;

        string validated = ValidateUserName(newUserName);

        if (UserName == validated)
            return false;

        UserName = validated;
        AuditState.UpdateAudit();
        return true;
    }

    public bool TrySetEmailHash(string newEmailHash)
    {
        if (IsDeleted)
            return false;

        string validated = ValidateEmailHash(newEmailHash);

        if (EmailHash == validated)
            return false;

        EmailHash = validated;
        AuditState.UpdateAudit();
        return true;
    }

    public bool TrySetPasswordHash(string newPasswordHash)
    {
        if (IsDeleted)
            return false;

        string validated = ValidatePasswordHash(newPasswordHash);

        if (PasswordHash == validated)
            return false;

        PasswordHash = validated;
        AuditState.UpdateAudit();
        return true;
    }

    public bool TryUpdateRole(UserRole newRole)
    {
        if (IsDeleted)
            return false;

        UserRole validated = Guard.AgainstInvalidEnum(newRole, nameof(newRole));

        if (Role == validated)
            return false;

        Role = validated;
        AuditState.UpdateAudit();
        return true;
    }

    public bool TryLock()
    {
        if (IsDeleted || IsLocked)
            return false;

        IsLocked = true;
        AuditState.UpdateAudit();
        return true;
    }

    public bool TryUnlock()
    {
        if (IsDeleted || !IsLocked)
            return false;

        IsLocked = false;
        AuditState.UpdateAudit();
        return true;
    }

    public bool TryDelete()
    {
        if (IsDeleted)
            return false;

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;

        AuditState.UpdateAudit();
        return true;
    }

    private static string ValidateUserName(string userName)
    {
        string value = Guard.AgainstNullOrWhiteSpace(userName, nameof(userName));
        Guard.AgainstInvalidLength(value, UserLimits.UserNameMinLength, UserLimits.UserNameMaxLength, nameof(userName));
        Guard.AgainstInvalidFormat(value, UserLimits.UserNameRegex(), nameof(userName));

        return value;
    }

    private static string ValidateEmailHash(string emailHash)
    {
        string value = Guard.AgainstNullOrWhiteSpace(emailHash, nameof(emailHash));
        Guard.AgainstInvalidExactLength(value, UserLimits.EmailHashLength, nameof(emailHash));

        return value;
    }

    private static string ValidatePasswordHash(string passwordHash)
    {
        string value = Guard.AgainstNullOrWhiteSpace(passwordHash, nameof(passwordHash));
        Guard.AgainstInvalidExactLength(value, UserLimits.PasswordHashLength, nameof(passwordHash));

        return value;
    }
}
