namespace ERMS.Application.Features.Claims.DTOs;

public class TravelClaimDto
{
    public long Id { get; set; }
    public DateOnly TravelDate { get; set; }
    public string Day { get; set; } = string.Empty;
    public string FromLocation { get; set; } = string.Empty;
    public string ToLocation { get; set; } = string.Empty;
    public string? PlaceVisited { get; set; }
    public string? ModeOfTransport { get; set; }
    public decimal TotalKm { get; set; }
    public decimal RatePerKm { get; set; }
    public decimal CalculatedAmount { get; set; }
}
