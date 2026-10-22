using System.ComponentModel.DataAnnotations;

namespace ERMS.Application.Features.Claims.DTOs;

public class CreateFoodClaimRequest
{
    public DateOnly? FoodDate { get; set; }

    [Required]
    public decimal Amount { get; set; }
}
