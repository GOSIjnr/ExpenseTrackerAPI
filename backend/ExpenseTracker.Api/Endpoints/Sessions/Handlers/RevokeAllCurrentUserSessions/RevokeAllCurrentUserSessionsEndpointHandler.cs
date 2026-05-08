using ExpenseTracker.Api.Extensions.Claims;
using ExpenseTracker.Api.Extensions.Responses;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Features.UserSessions.RevokeAllUserSessions;
using ExpenseTracker.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Endpoints.Sessions.Handlers.RevokeAllCurrentUserSessions;

internal static class RevokeAllCurrentUserSessionsEndpointHandler
{
    public static async Task<IResult> Handle(
        HttpContext httpContext,
        IMediator mediator,
        [FromQuery] bool keepCurrentUserSession = false,
        CancellationToken cancellationToken = default
    )
    {
        Guid? actorId = httpContext.User.GetUserId();
        Guid? sessionIdToKeep = keepCurrentUserSession ? httpContext.User.GetSessionId() : null;

        RevokeAllUserSessionsCommand command = new(
            ActorId: actorId,
            UserId: actorId,
            SessionId: sessionIdToKeep
        );

        OperationResult<object> result = await mediator.Send(
            command,
            cancellationToken
        );

        return Results.Ok(result.ToApiResponse());
    }
}
