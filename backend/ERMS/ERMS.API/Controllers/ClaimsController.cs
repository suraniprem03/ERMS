using ERMS.Application.Common.Models;
using ERMS.Application.Features.Claims;
using ERMS.Application.Features.Claims.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ERMS.API.Controllers;

[ApiController]
[Route("api/claims")]
[Authorize]
public class ClaimsController : ControllerBase
{
    private readonly IClaimService _claimService;

    public ClaimsController(IClaimService claimService)
    {
        _claimService = claimService;
    }

    private long GetCurrentUserId()
    {
        // Extracts the User ID from the JWT token
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(userIdString, out var userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("Invalid user context.");
    }

    private long GetCurrentEmployeeId()
    {
        // Extracts the Employee ID from the JWT token
        var empIdString = User.FindFirst("EmployeeId")?.Value ?? User.FindFirst("employee_id")?.Value;
        if (long.TryParse(empIdString, out var empId))
        {
            return empId;
        }
        throw new UnauthorizedAccessException("User does not have an associated employee account.");
    }

    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ClaimDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ClaimDto>>>> GetMyClaims(CancellationToken cancellationToken)
    {
        var employeeId = GetCurrentEmployeeId();
        var claims = await _claimService.GetMyClaimsAsync(employeeId, cancellationToken);
        return Ok(ApiResponse<IEnumerable<ClaimDto>>.Ok(claims, "My claims retrieved successfully."));
    }

    [HttpGet("pending")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ClaimDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ClaimDto>>>> GetPendingClaims(CancellationToken cancellationToken)
    {
        var claims = await _claimService.GetAllPendingClaimsAsync(cancellationToken);
        return Ok(ApiResponse<IEnumerable<ClaimDto>>.Ok(claims, "Pending claims retrieved successfully."));
    }

    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ClaimDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ClaimDto>>>> GetAllClaims(CancellationToken cancellationToken)
    {
        var claims = await _claimService.GetAllClaimsAsync(cancellationToken);
        return Ok(ApiResponse<IEnumerable<ClaimDto>>.Ok(claims, "All claims retrieved successfully."));
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<ClaimDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ClaimDto>>> GetById(long id, CancellationToken cancellationToken)
    {
        var claim = await _claimService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ClaimDto>.Ok(claim, "Claim retrieved successfully."));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ClaimDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<ClaimDto>>> Create(
        [FromBody] CreateClaimRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid model state."));
        }

        var employeeId = GetCurrentEmployeeId();
        var claim = await _claimService.CreateAsync(employeeId, request, cancellationToken);
        
        return CreatedAtAction(
            nameof(GetById),
            new { id = claim.Id },
            ApiResponse<ClaimDto>.Ok(claim, "Claim created successfully."));
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<ClaimDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ClaimDto>>> Update(
        long id,
        [FromBody] UpdateClaimRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid model state."));
        }

        var employeeId = GetCurrentEmployeeId();
        var claim = await _claimService.UpdateDraftAsync(id, employeeId, request, cancellationToken);
        
        return Ok(ApiResponse<ClaimDto>.Ok(claim, "Claim updated successfully."));
    }

    [HttpPost("{id:long}/submit")]
    [ProducesResponseType(typeof(ApiResponse<ClaimDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ClaimDto>>> Submit(long id, CancellationToken cancellationToken)
    {
        var employeeId = GetCurrentEmployeeId();
        var claim = await _claimService.SubmitClaimAsync(id, employeeId, cancellationToken);
        return Ok(ApiResponse<ClaimDto>.Ok(claim, "Claim submitted successfully."));
    }

    [HttpPost("{id:long}/process")]
    // Consider adding [Authorize(Roles = "SUPER_ADMIN,ADMIN")] here
    [ProducesResponseType(typeof(ApiResponse<ClaimDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ClaimDto>>> ProcessAdminAction(
        long id,
        [FromBody] AdminActionRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid model state."));
        }

        var adminUserId = GetCurrentUserId();
        var claim = await _claimService.ProcessAdminActionAsync(id, adminUserId, request, cancellationToken);
        
        return Ok(ApiResponse<ClaimDto>.Ok(claim, "Admin action processed successfully."));
    }
}
