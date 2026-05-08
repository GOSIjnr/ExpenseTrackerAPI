namespace ExpenseTracker.Api.Configurations.RateLimiting;

internal sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public ContextRateLimitOptions Default { get; init; } = new();

    public Dictionary<string, ContextRateLimitOptions> Endpoints { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
