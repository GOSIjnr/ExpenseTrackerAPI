using ExpenseTracker.Api.Extensions.Claims;
using ExpenseTracker.Api.Extensions.Responses;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Features.Users.DeleteUser;
using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Api.Endpoints.Users.Handlers.DeleteCurrentUser;

public class DeleteCurrentUserEndpointHandler
{
    public static async Task<IResult> Handle(
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken = default
    )
    {
        Guid? actorId = httpContext.User.GetUserId();

        DeleteUserCommand command = new(actorId);
        OperationResult<object> result = await mediator.Send(command, cancellationToken);

        return Results.Ok(result.ToApiResponse());
    }
}
