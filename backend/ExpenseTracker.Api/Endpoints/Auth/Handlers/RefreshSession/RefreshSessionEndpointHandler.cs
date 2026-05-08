using ExpenseTracker.Api.Constants.Cookies;
using ExpenseTracker.Api.Extensions.Claims;
using ExpenseTracker.Api.Helpers;
using ExpenseTracker.Api.Models;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Features.Auth.Models;
using ExpenseTracker.Application.Features.Auth.RefreshSession;
using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Api.Endpoints.Auth.Handlers.RefreshSession;

internal static class RefreshSessionEndpointHandler
{
    public static async Task<IResult> Handle(
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken = default
    )
    {
        Guid? sessionId = httpContext.User.GetSessionId();

        OperationResult<SessionResult> result = await mediator.Send(
            new RefreshSessionCommand(sessionId),
            cancellationToken
        );

        SessionResult data = result.Data!;

        CookieHelper.SetCookie(
            httpContext.Response,
            CookieKeys.Session,
            data.SessionId.ToString("N"),
            data.Timestamps.ExpiresAt
        );

        ApiResponse<SessionTimestampsResponse> apiResponse = new(
            Success: true,
            MessageId: result.MessageId,
            Message: result.Message,
            Details: result.Details,
            Data: data.Timestamps
        );

        return Results.Ok(apiResponse);
    }
}
