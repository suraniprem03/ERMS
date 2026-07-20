using System.ComponentModel.DataAnnotations;

namespace ERMS.Application.Features.Claims.DTOs;

public class CreateHotelClaimRequest
{
    [Required]
    public DateOnly CheckInDate { get; set; }
    
    [Required]
    public DateOnly CheckOutDate { get; set; }
    
    [Required]
    public decimal Amount { get; set; }
}
