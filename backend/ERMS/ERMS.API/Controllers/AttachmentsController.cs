using ERMS.Application.Common.Models;
using ERMS.Application.Features.Claims;
using ERMS.Application.Features.Claims.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ERMS.API.Controllers;

[ApiController]
[Route("api/attachments")]
[Authorize]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;

    public AttachmentsController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    private long GetCurrentEmployeeId()
    {
        var empIdString = User.FindFirst("EmployeeId")?.Value ?? User.FindFirst("employee_id")?.Value;
        if (long.TryParse(empIdString, out var empId))
        {
            return empId;
        }
        return 0; // Means they are likely an admin without an employee record
    }
    
    private bool IsAdmin()
    {
        return User.IsInRole("SUPER_ADMIN") || User.IsInRole("ADMIN");
    }

    [HttpPost("claim/{claimId}")]
    public async Task<ActionResult<ApiResponse<ClaimAttachmentDto>>> Upload(long claimId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<object>.Fail("No file uploaded."));
        }

        var employeeId = GetCurrentEmployeeId();
        if (employeeId == 0) return Unauthorized(ApiResponse<object>.Fail("User must be an employee."));

        using var stream = file.OpenReadStream();
        var attachment = await _attachmentService.UploadAttachmentAsync(
            claimId, 
            employeeId, 
            file.FileName, 
            file.ContentType, 
            stream);

        return Ok(ApiResponse<ClaimAttachmentDto>.Ok(attachment, "File uploaded successfully."));
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(long id)
    {
        var employeeId = GetCurrentEmployeeId();
        var (fileStream, contentType, fileName) = await _attachmentService.DownloadAttachmentAsync(id, employeeId, IsAdmin());
        
        return File(fileStream, contentType, fileName);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(long id)
    {
        var employeeId = GetCurrentEmployeeId();
        if (employeeId == 0) return Unauthorized(ApiResponse<object>.Fail("User must be an employee."));

        await _attachmentService.DeleteAttachmentAsync(id, employeeId);
        return Ok(ApiResponse<object>.Ok(null, "Attachment deleted successfully."));
    }
}
