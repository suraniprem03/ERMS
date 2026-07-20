using System.ComponentModel.DataAnnotations;

namespace ERMS.Application.Features.Claims.DTOs;

public class CreateFoodClaimRequest
{
    [Required]
    public decimal Amount { get; set; }
}
