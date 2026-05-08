namespace ExpenseTracker.Application.Features.Auth.Models;

public sealed record SessionResult(
    Guid SessionId,
    SessionTimestampsResponse Timestamps
);
