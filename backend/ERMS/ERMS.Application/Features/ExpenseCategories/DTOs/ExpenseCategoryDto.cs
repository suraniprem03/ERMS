namespace ERMS.Application.Features.ExpenseCategories.DTOs;

public class ExpenseCategoryDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public decimal? MaxLimit { get; set; }
    public bool IsDynamic { get; set; }
    public bool IsActive { get; set; }
}
