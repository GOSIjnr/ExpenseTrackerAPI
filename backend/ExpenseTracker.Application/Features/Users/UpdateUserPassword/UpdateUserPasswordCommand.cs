using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Application.Features.Users.UpdateUserPassword;

public sealed record UpdateUserPasswordCommand(
    Guid? UserId,
    Guid? SessionId,
    string CurrentPassword,
    string NewPassword,
    bool LogoutAll = false
) : IMessage<OperationResult<object>>;
