using System.Threading.RateLimiting;
using ExpenseTracker.Api.Configurations.RateLimiting;
using ExpenseTracker.Api.Constants.RateLimiting;
using ExpenseTracker.Api.Models;
using ExpenseTracker.Application.Enums;
using ExpenseTracker.Application.Models;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Api.Extensions.RateLimiting;

internal static class RateLimitingServiceCollectionExtensions
{
    private static readonly ApiResponse<object> TooManyRequestsResponse = new(
        Success: false,
        MessageId: "SYSTEM_TOO_MANY_REQUESTS",
        Message: "Too many requests. Please retry later.",
        Details: [
            new ResponseDetail("Rate limit exceeded for this endpoint.", ResponseSeverity.Error)
        ],
        Data: null
    );

    public static IServiceCollection AddEndpointRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<RateLimitingOptions>, RateLimitingOptionsValidator>();

        services.AddOptions<RateLimitingOptions>()
            .Bind(configuration.GetSection(RateLimitingOptions.SectionName))
            .ValidateOnStart();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsJsonAsync(TooManyRequestsResponse, cancellationToken);
            };

            RateLimitingOptions rateLimiting = configuration
                .GetSection(RateLimitingOptions.SectionName)
                .Get<RateLimitingOptions>()
                ?? new RateLimitingOptions();

            AddPolicy(options, RateLimitPolicyNames.Default, "default", rateLimiting.Default);

            foreach ((string endpointKey, ContextRateLimitOptions endpointPolicy) in rateLimiting.Endpoints)
                AddPolicy(options, endpointKey, endpointKey, endpointPolicy);
        });

        return services;
    }

    private static void AddPolicy(RateLimiterOptions options, string policyName, string endpointKey, ContextRateLimitOptions policy)
    {
        options.AddPolicy(policyName, httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: RateLimitPartitionKeyFactory.Create(httpContext, endpointKey),
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = policy.PermitLimit,
                    Window = TimeSpan.FromSeconds(policy.WindowSeconds),
                    QueueLimit = policy.QueueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    AutoReplenishment = true
                }
            ));
    }
}
