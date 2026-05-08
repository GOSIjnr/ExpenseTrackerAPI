using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Application.Features.Seeders.SeedSuperAdmin;

public sealed record SeedSuperAdminCommand(
    string FirstName,
    string? MiddleName,
    string LastName,
    string UserName,
    string Email,
    string Password
) : IMessage<OperationResult<object>>;
