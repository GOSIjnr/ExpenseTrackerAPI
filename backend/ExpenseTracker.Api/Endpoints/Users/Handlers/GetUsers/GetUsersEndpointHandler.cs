using ExpenseTracker.Api.Extensions.Responses;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Features.Users.GetUsers;
using ExpenseTracker.Application.Features.Users.Models;
using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Api.Endpoints.Users.Handlers.GetUsers;

internal static class GetUsersEndpointHandler
{
    public static async Task<IResult> Handle(
        [AsParameters] GetUsersRequest request,
        IMediator mediator,
        CancellationToken cancellationToken = default
    )
    {
        GetUsersQuery query = new(
            Limit: request.Limit ?? 10,
            Cursor: request.Cursor,
            Id: request.Id,
            UserName: request.UserName
        );

        OperationResult<CursorPage<UserResponse>> result = await mediator.Send(
            query,
            cancellationToken
        );

        return Results.Ok(result.ToApiResponse());
    }
}
