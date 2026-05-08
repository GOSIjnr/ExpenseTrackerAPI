namespace ExpenseTracker.Api.Endpoints.Users.Handlers.UpdateCurrentUserPassword;

internal sealed record UpdateCurrentUserPasswordRequest
{
    public required string CurrentPassword { get; init; }
    public required string NewPassword { get; init; }
    public bool LogoutAll { get; init; } = false;
}
