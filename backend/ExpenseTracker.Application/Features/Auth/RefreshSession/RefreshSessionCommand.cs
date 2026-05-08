using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Features.Auth.Models;
using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Application.Features.Auth.RefreshSession;

public sealed record RefreshSessionCommand(
    Guid? SessionId
) : IMessage<OperationResult<SessionResult>>;
