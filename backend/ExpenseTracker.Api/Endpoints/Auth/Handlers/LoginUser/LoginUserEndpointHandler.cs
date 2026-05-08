using ExpenseTracker.Api.Constants.Cookies;
using ExpenseTracker.Api.Helpers;
using ExpenseTracker.Api.Models;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Features.Auth.LoginUser;
using ExpenseTracker.Application.Features.Auth.Models;
using ExpenseTracker.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Endpoints.Auth.Handlers.LoginUser;

internal static class LoginUserEndpointHandler
{
    public static async Task<IResult> Handle(
        [FromBody] LoginUserRequest message,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken = default
    )
    {
        string? rawSessionId = CookieHelper.GetCookie(httpContext.Request, CookieKeys.Session);

        Guid? activeSessionId = Guid.TryParse(rawSessionId, out Guid sessionId)
            ? sessionId
            : null;

        LoginUserCommand command = new(
            Identifier: message.Identifier,
            Password: message.Password,
            RememberMe: message.RememberMe,
            ActiveSessionId: activeSessionId
        );

        OperationResult<SessionResult> result = await mediator.Send(command, cancellationToken);
        SessionResult data = result.Data!;

        CookieHelper.SetCookie(
            httpContext.Response,
            CookieKeys.Session,
            data.SessionId.ToString("N"),
            expiresUtc: data.Timestamps.ExpiresAt
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
