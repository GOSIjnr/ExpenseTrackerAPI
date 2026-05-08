using ExpenseTracker.Api.Extensions.Claims;
using ExpenseTracker.Api.Extensions.Responses;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Features.UserSessions.GetCurrentUserSessions;
using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Api.Endpoints.Sessions.Handlers.GetCurrentUserSessions;

internal static class GetCurrentUserSessionsEndpointHandler
{
    public static async Task<IResult> Handle(
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken = default
    )
    {
        Guid? userId = httpContext.User.GetUserId();

        OperationResult<IReadOnlyList<SessionData>> result = await mediator.Send(
            new GetCurrentUserSessionsQuery(userId),
            cancellationToken
        );

        return Results.Ok(result.ToApiResponse());
    }
}
