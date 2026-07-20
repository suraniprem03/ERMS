using ERMS.Application.Common.Models;
using ERMS.Application.Features.ExpenseCategories;
using ERMS.Application.Features.ExpenseCategories.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERMS.API.Controllers;

[ApiController]
[Route("api/expense-categories")]
[Authorize]
public class ExpenseCategoriesController : ControllerBase
{
    private readonly IExpenseCategoryService _expenseCategoryService;

    public ExpenseCategoriesController(IExpenseCategoryService expenseCategoryService)
    {
        _expenseCategoryService = expenseCategoryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ExpenseCategoryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExpenseCategoryDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var categories = await _expenseCategoryService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IEnumerable<ExpenseCategoryDto>>.Ok(categories, "Expense Categories retrieved successfully."));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ExpenseCategoryDto>>> GetById(long id, CancellationToken cancellationToken)
    {
        var category = await _expenseCategoryService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ExpenseCategoryDto>.Ok(category, "Expense Category retrieved successfully."));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ExpenseCategoryDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<ExpenseCategoryDto>>> Create(
        [FromBody] CreateExpenseCategoryRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid model state."));
        }

        var category = await _expenseCategoryService.CreateAsync(request, cancellationToken);
        
        return CreatedAtAction(
            nameof(GetById),
            new { id = category.Id },
            ApiResponse<ExpenseCategoryDto>.Ok(category, "Expense Category created successfully."));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ExpenseCategoryDto>>> Update(
        long id,
        [FromBody] UpdateExpenseCategoryRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid model state."));
        }

        var category = await _expenseCategoryService.UpdateAsync(id, request, cancellationToken);
        
        return Ok(ApiResponse<ExpenseCategoryDto>.Ok(category, "Expense Category updated successfully."));
    }
}
