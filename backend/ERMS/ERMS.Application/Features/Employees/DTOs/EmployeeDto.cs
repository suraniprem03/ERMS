namespace ERMS.Application.Features.Employees.DTOs;

public class EmployeeDto
{
    public long Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    
    public string RoleName { get; set; } = string.Empty;
    public List<long> AssignedAreaIds { get; set; } = new();
}
