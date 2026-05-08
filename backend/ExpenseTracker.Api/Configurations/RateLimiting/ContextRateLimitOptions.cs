namespace ExpenseTracker.Api.Configurations.RateLimiting;

internal sealed class ContextRateLimitOptions
{
    public int PermitLimit { get; init; } = 30;
    public int WindowSeconds { get; init; } = 60;
    public int QueueLimit { get; init; }
}
