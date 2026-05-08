using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Api.Endpoints.Users.Handlers.GetUsers;

internal sealed record GetUsersRequest
{
    public Guid? Cursor { get; init; }

    [Range(1, 100)]
    public int? Limit { get; init; }

    public Guid? Id { get; init; }
    public string? UserName { get; init; }
}
