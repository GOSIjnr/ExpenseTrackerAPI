using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Application.Features.Auth.LogoutUser;

public sealed record LogoutUserCommand(
    Guid? SessionId
) : IMessage<OperationResult<object>>;
