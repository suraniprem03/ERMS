using System.ComponentModel.DataAnnotations;

namespace ERMS.Application.Features.Employees.DTOs;

public class UpdateEmployeeRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string MobileNumber { get; set; } = string.Empty;

    public bool IsActive { get; set; }
    
    public List<long> AreaIds { get; set; } = new();
}
