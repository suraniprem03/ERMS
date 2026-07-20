using ERMS.Application.Common.Models;
using ERMS.Application.Features.Employees;
using ERMS.Application.Features.Employees.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERMS.API.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize] // Only authenticated users can access employee endpoints
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmployeeDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<EmployeeDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var employees = await _employeeService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IEnumerable<EmployeeDto>>.Ok(employees, "Employees retrieved successfully."));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> GetById(long id, CancellationToken cancellationToken)
    {
        var employee = await _employeeService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<EmployeeDto>.Ok(employee, "Employee retrieved successfully."));
    }

    [HttpPost]
    // Consider adding [Authorize(Roles = "SUPER_ADMIN,ADMIN")] here
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> Create(
        [FromBody] CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid model state."));
        }

        var employee = await _employeeService.CreateAsync(request, cancellationToken);
        
        return CreatedAtAction(
            nameof(GetById),
            new { id = employee.Id },
            ApiResponse<EmployeeDto>.Ok(employee, "Employee created successfully."));
    }

    [HttpPut("{id}")]
    // Consider adding [Authorize(Roles = "SUPER_ADMIN,ADMIN")] here
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> Update(
        long id,
        [FromBody] UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid model state."));
        }

        var employee = await _employeeService.UpdateAsync(id, request, cancellationToken);
        
        return Ok(ApiResponse<EmployeeDto>.Ok(employee, "Employee updated successfully."));
    }

    [HttpPut("{id}/reset-password")]
    [Authorize(Roles = "SUPER_ADMIN,ADMIN")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword(
        long id,
        CancellationToken cancellationToken)
    {
        // By default, reset to a generic password. The employee will be forced to change it.
        var defaultPassword = "Reset!Password123";
        await _employeeService.ResetPasswordAsync(id, defaultPassword, cancellationToken);
        
        return Ok(ApiResponse<object>.Ok(new { }, "Employee password has been reset successfully. They will be forced to change it on their next login."));
    }
}
