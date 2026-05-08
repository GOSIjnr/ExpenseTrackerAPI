using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Application.Features.Users.UnlockUser;

public sealed record UnlockUserCommand(
    Guid? ActorId,
    Guid TargetId
) : IMessage<OperationResult<object>>;
