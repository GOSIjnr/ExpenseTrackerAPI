using ExpenseTracker.Models;
using ExpenseTracker.Models.DTOs.Income;
using ExpenseTracker.Services;
using ExpenseTracker.Utilities.Extension;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers;

/// <summary>
/// Controller for managing user income records
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class IncomeController(IIncomeService incomeService) : ControllerBase
{
    private readonly IIncomeService _incomeService = incomeService;

    private Guid GetUserId()
    {
        if (!User.TryGetUserId(out Guid userId))
        {
            throw new UnauthorizedAccessException("Invalid user token");
        }
        return userId;
    }

    /// <summary>
    /// Create a new income record
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<IncomeResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IncomeResponse>>> Create([FromBody] CreateIncomeRequest request)
    {
        var userId = GetUserId();
        var income = await _incomeService.CreateAsync(userId, request);
        return CreatedAtAction(nameof(GetById), new { id = income.Id }, 
            ApiResponse<IncomeResponse>.Ok(income, "Income created successfully"));
    }

    /// <summary>
    /// Get all income records for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<IncomeResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<IncomeResponse>>>> GetAll()
    {
        var userId = GetUserId();
        var incomes = await _incomeService.GetAllAsync(userId);
        return Ok(ApiResponse<IEnumerable<IncomeResponse>>.Ok(incomes, "Income records retrieved"));
    }

    /// <summary>
    /// Get a specific income record by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IncomeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IncomeResponse>>> GetById(Guid id)
    {
        var userId = GetUserId();
        var income = await _incomeService.GetByIdAsync(id, userId);

        if (income is null)
        {
            return NotFound(ApiResponse<object?>.Fail(null, "Income record not found"));
        }

        return Ok(ApiResponse<IncomeResponse>.Ok(income, "Income record retrieved"));
    }

    /// <summary>
    /// Update an existing income record
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object?>>> Update(Guid id, [FromBody] UpdateIncomeRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest(ApiResponse<object?>.Fail(null, "ID mismatch"));
        }

        var userId = GetUserId();
        var updated = await _incomeService.UpdateAsync(userId, request);

        if (!updated)
        {
            return NotFound(ApiResponse<object?>.Fail(null, "Income record not found"));
        }

        return Ok(ApiResponse<object?>.Ok(null, "Income updated successfully"));
    }

    /// <summary>
    /// Delete an income record
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id)
    {
        var userId = GetUserId();
        var deleted = await _incomeService.DeleteAsync(id, userId);

        if (!deleted)
        {
            return NotFound(ApiResponse<object?>.Fail(null, "Income record not found"));
        }

        return Ok(ApiResponse<object?>.Ok(null, "Income deleted successfully"));
    }

    /// <summary>
    /// Get income summary for the current month
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<IncomeSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IncomeSummaryDto>>> GetSummary([FromQuery] int? month, [FromQuery] int? year)
    {
        var userId = GetUserId();
        var now = DateTime.UtcNow;
        var targetMonth = month ?? now.Month;
        var targetYear = year ?? now.Year;

        var summary = await _incomeService.GetIncomeSummaryAsync(userId, targetMonth, targetYear);
        return Ok(ApiResponse<IncomeSummaryDto>.Ok(summary, "Income summary retrieved"));
    }
}
