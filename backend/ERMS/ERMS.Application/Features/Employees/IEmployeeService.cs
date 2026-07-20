using ERMS.Application.Features.Employees.DTOs;

namespace ERMS.Application.Features.Employees;

public interface IEmployeeService
{
    Task<EmployeeDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeDto> UpdateAsync(long id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(long id, string newPassword, CancellationToken cancellationToken = default);
}
