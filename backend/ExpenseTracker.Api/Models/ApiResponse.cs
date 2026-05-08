using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Api.Models;

internal sealed record ApiResponse<T>(
    bool Success,
    string MessageId,
    string Message,
    List<ResponseDetail>? Details,
    T? Data
);
