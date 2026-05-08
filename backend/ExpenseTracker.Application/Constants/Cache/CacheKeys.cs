namespace ExpenseTracker.Application.Constants.Cache;

internal static class CacheKeys
{
    public static string SessionById(Guid sessionId)
        => $"expense-tracker:session:id:{sessionId:N}";

    public static string UserAuthenticationState(Guid userId)
        => $"expense-tracker:auth:user:authentication:{userId:N}";

    public static string UserProfileById(Guid userId)
        => $"expense-tracker:user:profile:by-id:{userId:N}";
}
