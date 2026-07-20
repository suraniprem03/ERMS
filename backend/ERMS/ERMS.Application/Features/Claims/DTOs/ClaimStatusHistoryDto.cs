namespace ERMS.Application.Features.Claims.DTOs;

public class ClaimStatusHistoryDto
{
    public long Id { get; set; }
    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public long CreatedBy { get; set; }
}
