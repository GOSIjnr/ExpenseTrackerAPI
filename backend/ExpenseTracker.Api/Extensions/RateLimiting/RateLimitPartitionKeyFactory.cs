using System.Security.Claims;
using ExpenseTracker.Api.Constants.Auth;

namespace ExpenseTracker.Api.Extensions.RateLimiting;

internal static class RateLimitPartitionKeyFactory
{
    public static string Create(HttpContext httpContext, string contextName)
    {
        string userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        string sessionId = httpContext.User.FindFirstValue(SessionClaimTypes.SessionId) ?? "no-session";
        string remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";

        return $"{contextName}:user:{userId}:session:{sessionId}:ip:{remoteIp}";
    }
}
