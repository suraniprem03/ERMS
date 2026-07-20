using ERMS.Application.Common.Models;
using ERMS.Application.Features.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERMS.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("claims/export")]
    // Consider restricting to Admin roles only
    public async Task<IActionResult> ExportClaimsExcel(CancellationToken cancellationToken)
    {
        var fileBytes = await _reportService.GenerateClaimsExcelReportAsync(cancellationToken);
        
        var fileName = $"Claims_Report_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
