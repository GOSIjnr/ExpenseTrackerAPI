using ExpenseTracker.Api.Constants.RateLimiting;
using ExpenseTracker.Api.Constants.Routes;
using ExpenseTracker.Api.Endpoints.Base.Handlers.GetInfo;

namespace ExpenseTracker.Api.Endpoints.Base;

internal sealed class BaseEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.ApiBasePath, GetInfoEndpointHandler.Handle)
            .RequireRateLimiting(RateLimitPolicyNames.Base.GetInfo)
            .ExcludeFromDescription();
    }
}
