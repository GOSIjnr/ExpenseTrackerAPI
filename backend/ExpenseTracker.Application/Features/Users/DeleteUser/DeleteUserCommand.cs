using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Application.Features.Users.DeleteUser;

public sealed record DeleteUserCommand(
    Guid? ActorId
) : IMessage<OperationResult<object>>;
