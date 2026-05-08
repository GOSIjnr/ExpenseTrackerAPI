using ExpenseTracker.Application.Common.Responses;
using ExpenseTracker.Application.Constants.Http;

namespace ExpenseTracker.Application.Constants.Responses;

internal static partial class ResponseCatalog
{
    public static class Authorization
    {
        public static readonly OperationFailureResponse CannotDemoteSuperAdmin = new(
            Id: "AUTHZ_CANNOT_DEMOTE_SUPERADMIN",
            StatusCode: HttpStatusCodes.Forbidden,
            Title: "Cannot demote a SuperAdmin.",
            Details: []
        );

        public static readonly OperationFailureResponse CannotPromoteSuperAdmin = new(
            Id: "AUTHZ_CANNOT_PROMOTE_SUPERADMIN",
            StatusCode: HttpStatusCodes.Forbidden,
            Title: "Cannot promote a SuperAdmin.",
            Details: []
        );

        public static readonly OperationFailureResponse CannotActOnSelf = new(
            Id: "AUTHZ_CANNOT_ACT_ON_SELF",
            StatusCode: HttpStatusCodes.Forbidden,
            Title: "You cannot perform this action on your own account.",
            Details: []
        );

        public static readonly OperationFailureResponse AccountLocked = new(
            Id: "AUTHZ_ACCOUNT_LOCKED",
            StatusCode: HttpStatusCodes.Forbidden,
            Title: "This account is currently locked.",
            Details: []
        );

        public static readonly OperationFailureResponse Forbidden = new(
            Id: "AUTHZ_FORBIDDEN",
            StatusCode: HttpStatusCodes.Forbidden,
            Title: "You are not authorized to perform this action.",
            Details: []
        );
    }
}
