using ExpenseTracker.Api.Extensions.Claims;
using ExpenseTracker.Api.Extensions.Responses;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Features.UserSessions.RevokeUserSession;
using ExpenseTracker.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Endpoints.Sessions.Handlers.RevokeCurrentUserSession;

internal static class RevokeCurrentUserSessionEndpointHandler
{
    public static async Task<IResult> Handle(
        [FromRoute] Guid id,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken = default
    )
    {
        Guid? actorId = httpContext.User.GetUserId();

        RevokeUserSessionCommand command = new(
            ActorId: actorId,
            UserId: actorId,
            SessionId: id
        );

        OperationResult<object> result = await mediator.Send(
            command,
            cancellationToken
        );

        return Results.Ok(result.ToApiResponse());
    }
}
