using ERMS.Application.Common.Exceptions;
using ERMS.Application.Features.ExpenseCategories;
using ERMS.Application.Features.ExpenseCategories.DTOs;
using ERMS.Domain.Entities;
using ERMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERMS.Infrastructure.Services;

public class ExpenseCategoryService : IExpenseCategoryService
{
    private readonly ERMSDbContext _dbContext;

    public ExpenseCategoryService(ERMSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExpenseCategoryDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.ExpenseCategories
            .FirstOrDefaultAsync(ec => ec.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Expense Category with ID {id} not found.");

        return MapToDto(category);
    }

    public async Task<IEnumerable<ExpenseCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _dbContext.ExpenseCategories
            .OrderBy(ec => ec.DisplayOrder)
            .ToListAsync(cancellationToken);

        return categories.Select(MapToDto);
    }

    public async Task<ExpenseCategoryDto> CreateAsync(CreateExpenseCategoryRequest request, CancellationToken cancellationToken = default)
    {
        if (await _dbContext.ExpenseCategories.AnyAsync(ec => ec.Code.ToLower() == request.Code.ToLower(), cancellationToken))
        {
            throw new ValidationException($"An Expense Category with code '{request.Code}' already exists.");
        }

        var category = new ExpenseCategory
        {
            Code = request.Code,
            Name = request.Name,
            DisplayOrder = request.DisplayOrder,
            MaxLimit = request.MaxLimit,
            IsDynamic = request.IsDynamic,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.ExpenseCategories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(category);
    }

    public async Task<ExpenseCategoryDto> UpdateAsync(long id, UpdateExpenseCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.ExpenseCategories
            .FirstOrDefaultAsync(ec => ec.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Expense Category with ID {id} not found.");

        category.Name = request.Name;
        category.DisplayOrder = request.DisplayOrder;
        category.MaxLimit = request.MaxLimit;
        category.IsDynamic = request.IsDynamic;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(category);
    }

    private static ExpenseCategoryDto MapToDto(ExpenseCategory category)
    {
        return new ExpenseCategoryDto
        {
            Id = category.Id,
            Code = category.Code,
            Name = category.Name,
            DisplayOrder = category.DisplayOrder,
            MaxLimit = category.MaxLimit,
            IsDynamic = category.IsDynamic,
            IsActive = category.IsActive
        };
    }
}
