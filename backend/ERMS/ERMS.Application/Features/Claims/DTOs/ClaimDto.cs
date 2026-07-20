namespace ERMS.Application.Features.Claims.DTOs;

public class ClaimDto
{
    public long Id { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public long EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    
    public long ExpenseCategoryId { get; set; }
    public string ExpenseCategoryName { get; set; } = string.Empty;
    
    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    
    public string? AdminRemarks { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public long? ApprovedByUserId { get; set; }
    public string? ApprovedByUserName { get; set; }

    public TravelClaimDto? TravelDetails { get; set; }
    public FoodClaimDto? FoodDetails { get; set; }
    public HotelClaimDto? HotelDetails { get; set; }
    public RechargeClaimDto? RechargeDetails { get; set; }

    public List<ClaimStatusHistoryDto> StatusHistory { get; set; } = new();
    public List<ClaimAttachmentDto> Attachments { get; set; } = new();

    public decimal TotalAmount => TravelDetails?.CalculatedAmount 
                               ?? FoodDetails?.Amount 
                               ?? HotelDetails?.Amount 
                               ?? RechargeDetails?.Amount 
                               ?? 0;
}
