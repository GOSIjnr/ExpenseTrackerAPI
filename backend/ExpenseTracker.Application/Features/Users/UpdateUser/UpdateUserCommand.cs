using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Application.Features.Users.UpdateUser;

public sealed record UpdateUserCommand(
    Guid? UserId,
    string? FirstName,
    string? MiddleName,
    string? LastName,
    string? UserName
) : IMessage<OperationResult<object>>;
