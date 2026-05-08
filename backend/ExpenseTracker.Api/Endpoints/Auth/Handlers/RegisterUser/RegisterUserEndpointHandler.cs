using ExpenseTracker.Api.Constants.Routes;
using ExpenseTracker.Api.Extensions.Responses;
using ExpenseTracker.Api.Models;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Application.Features.Auth.RegisterUser;
using ExpenseTracker.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Endpoints.Auth.Handlers.RegisterUser;

internal static class RegisterUserEndpointHandler
{
    public static async Task<IResult> Handle(
        [FromBody] RegisterUserCommand request,
        IMediator mediator,
        CancellationToken cancellationToken = default
    )
    {
        OperationResult<Guid> result = await mediator.Send(request, cancellationToken);

        Guid userId = result.Data;
        string location = $"{ApiRoutes.User.Base}/{userId}";

        ApiResponse<object> response = result.WithoutData()
            .ToApiResponse();

        return Results.Created(location, response);
    }
}
