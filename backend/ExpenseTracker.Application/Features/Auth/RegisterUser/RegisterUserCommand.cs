using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Application.Features.Auth.RegisterUser;

public sealed record RegisterUserCommand(
    string FirstName,
    string? MiddleName,
    string LastName,
    string UserName,
    string Email,
    string Password
) : IMessage<OperationResult<Guid>>;
