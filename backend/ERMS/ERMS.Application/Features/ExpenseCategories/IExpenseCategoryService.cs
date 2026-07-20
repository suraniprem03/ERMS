using ERMS.Application.Features.ExpenseCategories.DTOs;

namespace ERMS.Application.Features.ExpenseCategories;

public interface IExpenseCategoryService
{
    Task<ExpenseCategoryDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExpenseCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ExpenseCategoryDto> CreateAsync(CreateExpenseCategoryRequest request, CancellationToken cancellationToken = default);
    Task<ExpenseCategoryDto> UpdateAsync(long id, UpdateExpenseCategoryRequest request, CancellationToken cancellationToken = default);
}
