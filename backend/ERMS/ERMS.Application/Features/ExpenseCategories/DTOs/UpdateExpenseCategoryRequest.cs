using System.ComponentModel.DataAnnotations;

namespace ERMS.Application.Features.ExpenseCategories.DTOs;

public class UpdateExpenseCategoryRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public decimal? MaxLimit { get; set; }

    public bool IsDynamic { get; set; }

    public bool IsActive { get; set; }
}
