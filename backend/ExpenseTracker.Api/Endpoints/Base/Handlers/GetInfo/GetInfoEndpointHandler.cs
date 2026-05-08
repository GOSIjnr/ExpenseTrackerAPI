using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.Endpoints.Base.Handlers.GetInfo;

internal static class GetInfoEndpointHandler
{
    private static readonly string[] Features =
    [
        "Authentication & Session Management (Cookie-based auth with secure sessions)",
        "User Profile Management & Admin Controls",
    ];

    public static IResult Handle()
    {
        ApiResponse<object> response = new(
            Success: true,
            MessageId: "INFO_API_RETRIEVED",
            Message: "API information retrieved successfully.",
            Details: null,
            Data: new
            {
                Name = "ExpenseTracker API",
                Version = "1.0.0",
                Description = "The ExpenseTracker API is the backend platform for identity, session management, and user management. Other expense-tracking domains are intentionally parked in legacy code while the new layered backend is rebuilt.",
                Documentation = new
                {
                    OpenApiJson = "/openapi/v1.json",
                    ScalarUI = new
                    {
                        Url = "/scalar",
                        Environment = "Development"
                    }
                },
                Features,
            }
        );

        return Results.Ok(response);
    }
}
