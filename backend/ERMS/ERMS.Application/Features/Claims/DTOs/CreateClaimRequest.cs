using System.ComponentModel.DataAnnotations;

namespace ERMS.Application.Features.Claims.DTOs;

public class CreateClaimRequest
{
    [Required]
    public long ExpenseCategoryId { get; set; }

    /// <summary>
    /// If true, the claim will be Submitted immediately. If false, it will be saved as Draft.
    /// </summary>
    public bool IsSubmit { get; set; }

    public CreateTravelClaimRequest? TravelDetails { get; set; }
    public CreateFoodClaimRequest? FoodDetails { get; set; }
    public CreateHotelClaimRequest? HotelDetails { get; set; }
    public CreateRechargeClaimRequest? RechargeDetails { get; set; }
}
