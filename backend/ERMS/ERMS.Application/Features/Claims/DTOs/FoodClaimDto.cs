namespace ERMS.Application.Features.Claims.DTOs;

public class FoodClaimDto
{
    public long Id { get; set; }
    public DateOnly? FoodDate { get; set; }
    public decimal Amount { get; set; }
}
