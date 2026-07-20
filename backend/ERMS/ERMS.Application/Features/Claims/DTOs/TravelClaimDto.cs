namespace ERMS.Application.Features.Claims.DTOs;

public class TravelClaimDto
{
    public long Id { get; set; }
    public DateOnly TravelDate { get; set; }
    public string Day { get; set; } = string.Empty;
    public string FromLocation { get; set; } = string.Empty;
    public string ToLocation { get; set; } = string.Empty;
    public decimal TotalKm { get; set; }
    public decimal RatePerKm { get; set; }
    public decimal CalculatedAmount { get; set; }
}
