using System.ComponentModel.DataAnnotations;

namespace ERMS.Application.Features.Employees.DTOs;

public class CreateEmployeeRequest
{
    [MaxLength(50)]
    public string? EmployeeCode { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string MobileNumber { get; set; } = string.Empty;

    public List<long> AreaIds { get; set; } = new();
}
