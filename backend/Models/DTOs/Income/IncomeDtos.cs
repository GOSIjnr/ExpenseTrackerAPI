using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Enums;

namespace ExpenseTracker.Models.DTOs.Income;

/// <summary>
/// Request to create a new income record
/// </summary>
public class CreateIncomeRequest
{
    [Required]
    public IncomeSource Source { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public double Amount { get; set; }

    [Required]
    public DateTime DateReceived { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsRecurring { get; set; } = false;

    public RecurrenceFrequency? Frequency { get; set; }
}

/// <summary>
/// Request to update an existing income record
/// </summary>
public class UpdateIncomeRequest
{
    [Required]
    public Guid Id { get; set; }

    public IncomeSource? Source { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public double? Amount { get; set; }

    public DateTime? DateReceived { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool? IsRecurring { get; set; }

    public RecurrenceFrequency? Frequency { get; set; }
}

/// <summary>
/// Income response DTO
/// </summary>
public class IncomeResponse
{
    public Guid Id { get; set; }
    public IncomeSource Source { get; set; }
    public string SourceName => Source.ToString();
    public double Amount { get; set; }
    public DateTime DateReceived { get; set; }
    public string? Description { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrenceFrequency? Frequency { get; set; }
    public string? FrequencyName => Frequency?.ToString();
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Summary of income for dashboard
/// </summary>
public class IncomeSummaryDto
{
    public double TotalMonthlyIncome { get; set; }
    public double TotalAllTimeIncome { get; set; }
    public double SavingsRate { get; set; }
    public double NetCashFlow { get; set; }
    public List<IncomeBySourceDto> IncomeBySource { get; set; } = [];
}

/// <summary>
/// Income grouped by source
/// </summary>
public class IncomeBySourceDto
{
    public IncomeSource Source { get; set; }
    public string SourceName => Source.ToString();
    public double TotalAmount { get; set; }
    public double Percentage { get; set; }
}
