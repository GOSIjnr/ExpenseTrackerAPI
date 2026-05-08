using ExpenseTracker.Enums;

namespace ExpenseTracker.Models;

/// <summary>
/// Represents a user's income record
/// </summary>
public class Income
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public IncomeSource Source { get; set; }
    public double Amount { get; set; }
    public DateTime DateReceived { get; set; }
    public string? Description { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrenceFrequency? Frequency { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
