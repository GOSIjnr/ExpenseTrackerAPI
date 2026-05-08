using ExpenseTracker.Application.Features.Auth.Models;
using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Application.Features.Auth.Extensions;

internal static class SessionDataExtensions
{
    extension(SessionData sessionData)
    {
        public SessionTimestampsResponse ToTimestampsResponse() => new(
            sessionData.ExpiresAt,
            sessionData.AbsoluteExpiresAt
        );
    }
}
