namespace ERMS.Application.Features.Areas.DTOs;

public class AreaDto
{
    public long Id { get; set; }
    public long StateId { get; set; }
    public string StateName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
