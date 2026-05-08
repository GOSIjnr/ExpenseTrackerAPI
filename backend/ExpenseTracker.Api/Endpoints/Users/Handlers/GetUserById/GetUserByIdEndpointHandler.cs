using ExpenseTracker.Api.Extensions.Responses;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Features.Users.GetUserById;
using ExpenseTracker.Application.Features.Users.Models;
using ExpenseTracker.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Endpoints.Users.Handlers.GetUserById;

internal static class GetUserByIdEndpointHandler
{
    public static async Task<IResult> Handle(
        [FromRoute] Guid id,
        IMediator mediator,
        CancellationToken cancellationToken = default
    )
    {
        OperationResult<UserResponse> result = await mediator.Send(
            new GetUserByIdQuery(id),
            cancellationToken
        );

        return Results.Ok(result.ToApiResponse());
    }
}
