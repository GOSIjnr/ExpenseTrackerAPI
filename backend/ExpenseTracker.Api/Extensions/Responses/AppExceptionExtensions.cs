using ExpenseTracker.Api.Models;
using ExpenseTracker.Application.Exceptions;

namespace ExpenseTracker.Api.Extensions.Responses;

internal static class AppExceptionExtensions
{
    extension(AppException exception)
    {
        public ApiResponse<T> ToApiResponse<T>() => new(
            Success: false,
            MessageId: exception.Id,
            Message: exception.Message,
            Details: exception.Details,
            Data: default
        );
    }
}
