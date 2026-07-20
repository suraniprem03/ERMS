using ERMS.Application.Common.Exceptions;
using ERMS.Application.Common.Interfaces;
using ERMS.Application.Features.Employees;
using ERMS.Application.Features.Employees.DTOs;
using ERMS.Domain.Entities;
using ERMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERMS.Infrastructure.Services;

public class EmployeeService : IEmployeeService
{
    private readonly ERMSDbContext _dbContext;
    private readonly IPasswordHasherService _passwordHasherService;

    public EmployeeService(ERMSDbContext dbContext, IPasswordHasherService passwordHasherService)
    {
        _dbContext = dbContext;
        _passwordHasherService = passwordHasherService;
    }

    public async Task<EmployeeDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var employee = await _dbContext.Employees
            .Include(e => e.User)
                .ThenInclude(u => u!.Role)
            .Include(e => e.EmployeeAreas)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Employee with ID {id} not found.");

        return MapToDto(employee);
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _dbContext.Employees
            .Include(e => e.User)
                .ThenInclude(u => u!.Role)
            .Include(e => e.EmployeeAreas)
            .ToListAsync(cancellationToken);

        return employees.Select(MapToDto);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await _dbContext.Employees.AnyAsync(e => e.Email.ToLower() == normalizedEmail, cancellationToken))
        {
            throw new ValidationException("An employee with this email already exists.");
        }

        if (string.IsNullOrWhiteSpace(request.EmployeeCode))
        {
            // Auto generate EMP + random suffix based on timestamp
            request.EmployeeCode = $"EMP{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()[^6..]}";
        }

        if (await _dbContext.Employees.AnyAsync(e => e.EmployeeCode == request.EmployeeCode, cancellationToken))
        {
            throw new ValidationException("An employee with this employee code already exists.");
        }

        var employeeRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Code == "EMPLOYEE", cancellationToken)
            ?? throw new NotFoundException("Default EMPLOYEE role not found in the database.");

        var employee = new Employee
        {
            EmployeeCode = request.EmployeeCode,
            Name = request.Name,
            Email = request.Email,
            MobileNumber = request.MobileNumber,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        if (request.AreaIds.Any())
        {
            foreach (var areaId in request.AreaIds.Distinct())
            {
                employee.EmployeeAreas.Add(new EmployeeArea { AreaId = areaId, CreatedAt = DateTimeOffset.UtcNow });
            }
        }

        var user = new User
        {
            Employee = employee,
            RoleId = employeeRole.Id,
            PasswordHash = _passwordHasherService.HashPassword("P@ssw0rd123!"),
            IsPasswordChangeRequired = true,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Employees.Add(employee);
        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync(cancellationToken);

        employee.User = user;
        user.Role = employeeRole;

        return MapToDto(employee);
    }

    public async Task<EmployeeDto> UpdateAsync(long id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await _dbContext.Employees
            .Include(e => e.User)
                .ThenInclude(u => u!.Role)
            .Include(e => e.EmployeeAreas)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Employee with ID {id} not found.");

        employee.Name = request.Name;
        employee.MobileNumber = request.MobileNumber;
        employee.UpdatedAt = DateTimeOffset.UtcNow;
        
        // Handle activation / deactivation
        if (employee.IsActive != request.IsActive)
        {
            employee.IsActive = request.IsActive;
            if (employee.User != null)
            {
                employee.User.IsActive = request.IsActive;
                employee.User.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        // Handle Areas Update
        _dbContext.EmployeeAreas.RemoveRange(employee.EmployeeAreas);
        
        if (request.AreaIds.Any())
        {
            foreach (var areaId in request.AreaIds.Distinct())
            {
                employee.EmployeeAreas.Add(new EmployeeArea { EmployeeId = employee.Id, AreaId = areaId, CreatedAt = DateTimeOffset.UtcNow });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(employee);
    }

    public async Task ResetPasswordAsync(long id, string newPassword, CancellationToken cancellationToken = default)
    {
        var employee = await _dbContext.Employees
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Employee with ID {id} not found.");

        if (employee.User == null)
        {
            throw new NotFoundException($"User account for employee ID {id} not found.");
        }

        employee.User.PasswordHash = _passwordHasherService.HashPassword(newPassword);
        employee.User.IsPasswordChangeRequired = true;
        employee.User.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            Name = employee.Name,
            Email = employee.Email,
            MobileNumber = employee.MobileNumber,
            IsActive = employee.IsActive,
            RoleName = employee.User?.Role?.Name ?? string.Empty,
            AssignedAreaIds = employee.EmployeeAreas.Select(ea => ea.AreaId).ToList()
        };
    }
}
