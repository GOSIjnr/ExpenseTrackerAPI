using ExpenseTracker.Api.Extensions.Claims;
using ExpenseTracker.Api.Extensions.Responses;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Features.Users.UnlockUser;
using ExpenseTracker.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Endpoints.Users.Handlers.UnlockUser;

internal static class UnlockUserEndpointHandler
{
    public static async Task<IResult> Handle(
        [FromRoute] Guid id,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken = default
    )
    {
        Guid? actorId = httpContext.User.GetUserId();

        UnlockUserCommand command = new(actorId, id);
        OperationResult<object> result = await mediator.Send(command, cancellationToken);

        return Results.Ok(result.ToApiResponse());
    }
}
