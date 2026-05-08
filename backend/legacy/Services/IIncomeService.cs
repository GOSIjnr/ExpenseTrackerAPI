using ExpenseTracker.Models.DTOs.Income;

namespace ExpenseTracker.Services;

/// <summary>
/// Interface for income management services
/// </summary>
public interface IIncomeService
{
    Task<IncomeResponse> CreateAsync(Guid userId, CreateIncomeRequest request);
    Task<IEnumerable<IncomeResponse>> GetAllAsync(Guid userId);
    Task<IncomeResponse?> GetByIdAsync(Guid id, Guid userId);
    Task<bool> UpdateAsync(Guid userId, UpdateIncomeRequest request);
    Task<bool> DeleteAsync(Guid id, Guid userId);
    Task<double> GetTotalIncomeAsync(Guid userId, int month, int year);
    Task<IncomeSummaryDto> GetIncomeSummaryAsync(Guid userId, int month, int year);
}
