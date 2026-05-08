using ExpenseTracker.Application.Common.Responses;
using ExpenseTracker.Application.Exceptions;

namespace ExpenseTracker.Application.Extensions.Responses;

internal static class OperationFailureResponseExtensions
{
    extension(OperationFailureResponse response)
    {
        public AppException ToException() => new(response);
    }
}
