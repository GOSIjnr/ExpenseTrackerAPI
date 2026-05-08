using ExpenseTracker.Domain.Entities.Users;

namespace ExpenseTracker.Application.Models;

public sealed record UserAuthData(
    Guid UserId,
    UserRole Role,
    bool IsLocked
);
