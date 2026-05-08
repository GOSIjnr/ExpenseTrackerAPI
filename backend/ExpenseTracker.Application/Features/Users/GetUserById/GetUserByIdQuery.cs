using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Features.Users.Models;
using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Application.Features.Users.GetUserById;

public sealed record GetUserByIdQuery(
    Guid? Id
) : IMessage<OperationResult<UserResponse>>;
