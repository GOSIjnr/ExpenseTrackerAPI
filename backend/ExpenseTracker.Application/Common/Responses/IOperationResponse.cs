using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Application.Common.Responses;

internal interface IOperationResponse
{
    public string Id { get; }
    public string Title { get; }
    public ResponseDetail[] Details { get; }
}
