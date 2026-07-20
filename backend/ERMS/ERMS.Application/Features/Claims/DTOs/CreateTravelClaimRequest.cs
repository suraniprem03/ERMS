using System.ComponentModel.DataAnnotations;

namespace ERMS.Application.Features.Claims.DTOs;

public class CreateTravelClaimRequest
{
    [Required]
    public DateOnly TravelDate { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Day { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string FromLocation { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string ToLocation { get; set; } = string.Empty;
    
    [Required]
    public decimal TotalKm { get; set; }
    
    [Required]
    public decimal RatePerKm { get; set; }
}
