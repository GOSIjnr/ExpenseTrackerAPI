using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Application.Common.Responses;

internal sealed record OperationFailureResponse(
    string Id,
    int StatusCode,
    string Title,
    ResponseDetail[] Details
) : BaseOperationResponse<OperationFailureResponse>(Id, Title, Details);
