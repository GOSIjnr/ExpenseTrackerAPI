using ExpenseTracker.Domain.Entities.Users;

namespace ExpenseTracker.Application.Features.Users.Models;

public sealed record UserResponse(
    Guid Id,
    string UserName,
    string FirstName,
    string? MiddleName,
    string LastName,
    UserRole Role
);
