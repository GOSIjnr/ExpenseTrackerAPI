using ExpenseTracker.Api.Constants.Cookies;
using ExpenseTracker.Api.Extensions.Claims;
using ExpenseTracker.Api.Extensions.Responses;
using ExpenseTracker.Api.Helpers;
using ExpenseTracker.Api.Models;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Features.Auth.LogoutUser;
using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Api.Endpoints.Auth.Handlers.LogoutUser;

internal static class LogoutUserEndpointHandler
{
    public static async Task<IResult> Handle(
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken = default
    )
    {
        Guid? sessionId = httpContext.User.GetSessionId();

        OperationResult<object> result = await mediator.Send(
            new LogoutUserCommand(sessionId),
            cancellationToken
        );

        CookieHelper.DeleteCookie(httpContext.Response, CookieKeys.Session);

        ApiResponse<object> apiResponse = result.ToApiResponse();

        return Results.Ok(apiResponse);
    }
}
