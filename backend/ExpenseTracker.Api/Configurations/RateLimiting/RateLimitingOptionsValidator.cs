using Microsoft.Extensions.Options;

namespace ExpenseTracker.Api.Configurations.RateLimiting;

internal sealed class RateLimitingOptionsValidator : IValidateOptions<RateLimitingOptions>
{
    public ValidateOptionsResult Validate(string? name, RateLimitingOptions options)
    {
        List<string> errors = [];

        ValidatePolicy("RateLimiting:Default", options.Default, errors);

        foreach (var (endpointKey, policy) in options.Endpoints)
            ValidatePolicy($"RateLimiting:Endpoints:{endpointKey}", policy, errors);

        return errors.Count > 0 ? ValidateOptionsResult.Fail(errors) : ValidateOptionsResult.Success;
    }

    private static void ValidatePolicy(string path, ContextRateLimitOptions context, List<string> errors)
    {
        if (context.PermitLimit <= 0)
            errors.Add($"{path}:PermitLimit must be greater than 0.");

        if (context.WindowSeconds <= 0)
            errors.Add($"{path}:WindowSeconds must be greater than 0.");

        if (context.QueueLimit < 0)
            errors.Add($"{path}:QueueLimit cannot be negative.");
    }
}
