using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Application.Features.UserSessions.GetCurrentUserSessions;

public sealed record GetCurrentUserSessionsQuery(
    Guid? UserId
) : IMessage<OperationResult<IReadOnlyList<SessionData>>>;
