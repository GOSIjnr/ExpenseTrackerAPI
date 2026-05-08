namespace ExpenseTracker.Api.Endpoints.Users.Handlers.UpdateCurrentUser;

internal sealed record UpdateCurrentUserRequest
{
    public string? FirstName { get; init; }
    public string? MiddleName { get; init; }
    public string? LastName { get; init; }
    public string? UserName { get; init; }
}
