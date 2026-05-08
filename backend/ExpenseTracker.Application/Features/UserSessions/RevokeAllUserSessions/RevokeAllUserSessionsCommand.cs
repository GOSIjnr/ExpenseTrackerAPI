using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Application.Features.UserSessions.RevokeAllUserSessions;

public sealed record RevokeAllUserSessionsCommand(
    Guid? ActorId,
    Guid? UserId,
    Guid? SessionId
) : IMessage<OperationResult<object>>;
