using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Features.Auth.Models;
using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Application.Features.Auth.LoginUser;

public sealed record LoginUserCommand(
    string Identifier,
    string Password,
    bool RememberMe = false,
    Guid? ActiveSessionId = null
) : IMessage<OperationResult<SessionResult>>;
