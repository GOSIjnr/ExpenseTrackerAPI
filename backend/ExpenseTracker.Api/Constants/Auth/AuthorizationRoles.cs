using ExpenseTracker.Domain.Entities.Users;

namespace ExpenseTracker.Api.Constants.Auth;

internal static class AuthorizationRoles
{
    public const string User = nameof(UserRole.User);
    public const string Admin = nameof(UserRole.Admin);
    public const string SuperAdmin = nameof(UserRole.SuperAdmin);
}
