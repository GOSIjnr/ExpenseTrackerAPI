namespace ExpenseTracker.Api.Endpoints.Auth.Handlers.LoginUser;

internal sealed record LoginUserRequest
{
    public required string Identifier { get; init; }
    public required string Password { get; init; }
    public bool RememberMe { get; init; } = false;
}
