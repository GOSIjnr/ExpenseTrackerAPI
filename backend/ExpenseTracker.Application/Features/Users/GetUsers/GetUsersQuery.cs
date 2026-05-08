using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Features.Users.Models;
using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Application.Features.Users.GetUsers;

public sealed record GetUsersQuery(
    int Limit,
    Guid? Cursor,
    Guid? Id,
    string? UserName
) : IMessage<OperationResult<CursorPage<UserResponse>>>;
