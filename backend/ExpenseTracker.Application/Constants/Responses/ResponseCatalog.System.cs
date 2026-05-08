using ExpenseTracker.Application.Common.Responses;

namespace ExpenseTracker.Application.Constants.Responses;

internal static partial class ResponseCatalog
{
    public static class System
    {
        public static readonly OperationOutcomeResponse SuperAdminSeeded = new(
            Id: "SYSTEM_SUPERADMIN_SEEDED",
            Title: "Super administrator seeded successfully.",
            Details: []
        );
    }
}
