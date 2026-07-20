using System.ComponentModel.DataAnnotations;

namespace ERMS.Application.Features.Claims.DTOs;

public class CreateRechargeClaimRequest
{
    [Required]
    public decimal Amount { get; set; }
}
