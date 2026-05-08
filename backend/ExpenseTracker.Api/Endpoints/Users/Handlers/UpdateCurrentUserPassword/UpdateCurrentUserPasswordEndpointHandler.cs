using ExpenseTracker.Api.Extensions.Claims;
using ExpenseTracker.Api.Extensions.Responses;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Features.Users.UpdateUserPassword;
using ExpenseTracker.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Endpoints.Users.Handlers.UpdateCurrentUserPassword;

internal static class UpdateCurrentUserPasswordEndpointHandler
{
    public static async Task<IResult> Handle(
        [FromBody] UpdateCurrentUserPasswordRequest request,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken = default
    )
    {
        Guid? userId = httpContext.User.GetUserId();
        Guid? sessionId = httpContext.User.GetSessionId();

        UpdateUserPasswordCommand command = new(
            UserId: userId,
            SessionId: sessionId,
            CurrentPassword: request.CurrentPassword,
            NewPassword: request.NewPassword,
            LogoutAll: request.LogoutAll
        );

        OperationResult<object> result = await mediator.Send(command, cancellationToken);

        return Results.Ok(result.ToApiResponse());
    }
}
