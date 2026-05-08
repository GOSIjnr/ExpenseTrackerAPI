using ExpenseTracker.Application.Enums;

namespace ExpenseTracker.Application.Models;

public sealed record ResponseDetail(
    string Message,
    ResponseSeverity Severity
);
