using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Application.Features.UserSessions.RevokeUserSession;

public sealed record RevokeUserSessionCommand(
    Guid? ActorId,
    Guid? UserId,
    Guid? SessionId
) : IMessage<OperationResult<object>>;
