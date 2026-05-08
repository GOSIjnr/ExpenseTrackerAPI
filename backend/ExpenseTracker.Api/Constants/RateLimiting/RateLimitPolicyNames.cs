namespace ExpenseTracker.Api.Constants.RateLimiting;

internal static class RateLimitPolicyNames
{
    public const string Default = "default";

    public static class Base
    {
        public const string GetInfo = "base.get_info";
    }

    public static class Auth
    {
        public const string Register = "auth.register";
        public const string Login = "auth.login";
        public const string Refresh = "auth.refresh";
        public const string Logout = "auth.logout";
    }

    public static class User
    {
        public const string GetUsers = "users.get_users";
        public const string GetCurrent = "users.get_current";
        public const string GetById = "users.get_by_id";
        public const string UpdateCurrent = "users.update_current";
        public const string UpdatePassword = "users.update_password";
        public const string Promote = "users.promote";
        public const string Demote = "users.demote";
        public const string Lock = "users.lock";
        public const string Unlock = "users.unlock";
    }

    public static class Session
    {
        public const string GetCurrent = "sessions.get_current";
        public const string RevokeOne = "sessions.revoke_one";
        public const string RevokeAll = "sessions.revoke_all";
    }
}
