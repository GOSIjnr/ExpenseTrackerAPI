using ExpenseTracker.Data;
using ExpenseTracker.Models;
using ExpenseTracker.Models.DTOs.Income;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services;

/// <summary>
/// Service for managing user income records
/// </summary>
internal class IncomeService(ExpenseTrackerDbContext dbContext) : IIncomeService
{
    private readonly ExpenseTrackerDbContext _dbContext = dbContext;

    public async Task<IncomeResponse> CreateAsync(Guid userId, CreateIncomeRequest request)
    {
        // Preserve the calendar date by extracting date components and creating UTC midnight
        var requestDate = request.DateReceived;
        var dateAsUtc = new DateTime(requestDate.Year, requestDate.Month, requestDate.Day, 0, 0, 0, DateTimeKind.Utc);

        Income income = new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Source = request.Source,
            Amount = request.Amount,
            DateReceived = dateAsUtc,
            Description = request.Description,
            IsRecurring = request.IsRecurring,
            Frequency = request.Frequency,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Incomes.AddAsync(income);
        await _dbContext.SaveChangesAsync();

        return MapToResponse(income);
    }

    public async Task<IEnumerable<IncomeResponse>> GetAllAsync(Guid userId)
    {
        var incomes = await _dbContext.Incomes
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.DateReceived)
            .ToListAsync();

        return incomes.Select(MapToResponse);
    }

    public async Task<IncomeResponse?> GetByIdAsync(Guid id, Guid userId)
    {
        var income = await _dbContext.Incomes
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

        return income is null ? null : MapToResponse(income);
    }

    public async Task<bool> UpdateAsync(Guid userId, UpdateIncomeRequest request)
    {
        var existing = await _dbContext.Incomes
            .FirstOrDefaultAsync(i => i.Id == request.Id && i.UserId == userId);

        if (existing is null) return false;

        if (request.Source.HasValue)
            existing.Source = request.Source.Value;

        if (request.Amount.HasValue)
            existing.Amount = request.Amount.Value;

        if (request.DateReceived.HasValue)
        {
            var date = request.DateReceived.Value;
            existing.DateReceived = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
        }

        if (request.Description is not null)
            existing.Description = request.Description;

        if (request.IsRecurring.HasValue)
            existing.IsRecurring = request.IsRecurring.Value;

        if (request.Frequency.HasValue)
            existing.Frequency = request.Frequency.Value;

        _dbContext.Incomes.Update(existing);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var income = await _dbContext.Incomes
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

        if (income is null) return false;

        _dbContext.Incomes.Remove(income);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<double> GetTotalIncomeAsync(Guid userId, int month, int year)
    {
        // Fetch to memory to avoid PostgreSQL UTC conversion issues
        var incomes = await _dbContext.Incomes
            .Where(i => i.UserId == userId)
            .ToListAsync();

        return incomes
            .Where(i => i.DateReceived.Month == month && i.DateReceived.Year == year)
            .Sum(i => i.Amount);
    }

    public async Task<IncomeSummaryDto> GetIncomeSummaryAsync(Guid userId, int month, int year)
    {
        // Get all user incomes
        var allIncomes = await _dbContext.Incomes
            .Where(i => i.UserId == userId)
            .ToListAsync();

        // Get all user expenses for comparison
        var monthlyExpenses = await _dbContext.Expenses
            .Where(e => e.UserId == userId)
            .ToListAsync();

        // Calculate monthly totals
        double monthlyIncome = allIncomes
            .Where(i => i.DateReceived.Month == month && i.DateReceived.Year == year)
            .Sum(i => i.Amount);

        double monthlyExpense = monthlyExpenses
            .Where(e => e.DateOfExpense.Month == month && e.DateOfExpense.Year == year)
            .Sum(e => e.Amount);

        // Calculate all-time income
        double allTimeIncome = allIncomes.Sum(i => i.Amount);

        // Calculate savings rate and net cash flow
        double savingsRate = monthlyIncome > 0 
            ? ((monthlyIncome - monthlyExpense) / monthlyIncome) * 100 
            : 0;
        double netCashFlow = monthlyIncome - monthlyExpense;

        // Group income by source for current month
        var incomeBySource = allIncomes
            .Where(i => i.DateReceived.Month == month && i.DateReceived.Year == year)
            .GroupBy(i => i.Source)
            .Select(g => new IncomeBySourceDto
            {
                Source = g.Key,
                TotalAmount = g.Sum(i => i.Amount),
                Percentage = monthlyIncome > 0 ? (g.Sum(i => i.Amount) / monthlyIncome) * 100 : 0
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToList();

        return new IncomeSummaryDto
        {
            TotalMonthlyIncome = monthlyIncome,
            TotalAllTimeIncome = allTimeIncome,
            SavingsRate = Math.Round(savingsRate, 1),
            NetCashFlow = netCashFlow,
            IncomeBySource = incomeBySource
        };
    }

    private static IncomeResponse MapToResponse(Income income) => new()
    {
        Id = income.Id,
        Source = income.Source,
        Amount = income.Amount,
        DateReceived = income.DateReceived,
        Description = income.Description,
        IsRecurring = income.IsRecurring,
        Frequency = income.Frequency,
        CreatedAt = income.CreatedAt
    };
}
