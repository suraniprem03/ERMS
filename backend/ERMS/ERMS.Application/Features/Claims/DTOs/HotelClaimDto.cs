namespace ERMS.Application.Features.Claims.DTOs;

public class HotelClaimDto
{
    public long Id { get; set; }
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public decimal Amount { get; set; }
}
