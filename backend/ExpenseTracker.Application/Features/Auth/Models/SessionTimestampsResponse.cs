namespace ExpenseTracker.Application.Features.Auth.Models;

public sealed record SessionTimestampsResponse(
    DateTime ExpiresAt,
    DateTime AbsoluteExpiresAt
);
