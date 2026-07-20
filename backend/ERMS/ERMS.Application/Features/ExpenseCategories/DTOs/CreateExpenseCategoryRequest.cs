using System.ComponentModel.DataAnnotations;

namespace ERMS.Application.Features.ExpenseCategories.DTOs;

public class CreateExpenseCategoryRequest
{
    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public decimal? MaxLimit { get; set; }

    public bool IsDynamic { get; set; }
}
