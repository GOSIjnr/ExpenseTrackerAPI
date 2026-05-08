using ExpenseTracker.Api.Extensions.Claims;
using ExpenseTracker.Api.Extensions.Responses;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Features.Users.GetUserById;
using ExpenseTracker.Application.Features.Users.Models;
using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Api.Endpoints.Users.Handlers.GetCurrentUser;

internal static class GetCurrentUserEndpointHandler
{
    public static async Task<IResult> Handle(
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken = default
    )
    {
        Guid? userId = httpContext.User.GetUserId();

        OperationResult<UserResponse> result = await mediator.Send(
            new GetUserByIdQuery(userId),
            cancellationToken
        );

        return Results.Ok(result.ToApiResponse());
    }
}
