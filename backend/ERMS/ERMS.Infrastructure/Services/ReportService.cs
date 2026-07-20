using ClosedXML.Excel;
using ERMS.Application.Features.Reports;
using ERMS.Domain.Enums;
using ERMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERMS.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly ERMSDbContext _dbContext;

    public ReportService(ERMSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<byte[]> GenerateClaimsExcelReportAsync(CancellationToken cancellationToken = default)
    {
        var claims = await _dbContext.Claims
            .Include(c => c.Employee)
                .ThenInclude(e => e.EmployeeAreas)
                .ThenInclude(ea => ea.Area)
            .Include(c => c.ExpenseCategory)
            .Include(c => c.TravelClaim)
            .Include(c => c.FoodClaim)
            .Include(c => c.HotelClaim)
            .Include(c => c.RechargeClaim)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Claims Report");

        // Headers
        worksheet.Cell(1, 1).Value = "Claim Number";
        worksheet.Cell(1, 2).Value = "Employee Name";
        worksheet.Cell(1, 3).Value = "Employee Code";
        worksheet.Cell(1, 4).Value = "Employee Areas";
        worksheet.Cell(1, 5).Value = "Category";
        worksheet.Cell(1, 6).Value = "Amount (₹)";
        worksheet.Cell(1, 7).Value = "Expense Details";
        worksheet.Cell(1, 8).Value = "Status";
        worksheet.Cell(1, 9).Value = "Created At";
        worksheet.Cell(1, 10).Value = "Submitted At";
        worksheet.Cell(1, 11).Value = "Action Date";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Data
        for (int i = 0; i < claims.Count; i++)
        {
            var claim = claims[i];
            var row = i + 2;

            decimal totalAmount = claim.TravelClaim?.CalculatedAmount 
                               ?? claim.FoodClaim?.Amount 
                               ?? claim.HotelClaim?.Amount 
                               ?? claim.RechargeClaim?.Amount 
                               ?? 0;

            string employeeAreas = claim.Employee?.EmployeeAreas != null 
                ? string.Join(", ", claim.Employee.EmployeeAreas.Select(ea => ea.Area?.Name).Where(name => !string.IsNullOrEmpty(name)))
                : string.Empty;

            string expenseDetails = string.Empty;
            if (claim.TravelClaim != null)
            {
                expenseDetails = $"Travel Date: {claim.TravelClaim.TravelDate:yyyy-MM-dd} | Day: {claim.TravelClaim.Day} | From: {claim.TravelClaim.FromLocation} | To: {claim.TravelClaim.ToLocation} | KM: {claim.TravelClaim.TotalKm} | Rate/KM: ₹{claim.TravelClaim.RatePerKm}";
            }
            else if (claim.HotelClaim != null)
            {
                expenseDetails = $"Check-In: {claim.HotelClaim.CheckInDate:yyyy-MM-dd} | Check-Out: {claim.HotelClaim.CheckOutDate:yyyy-MM-dd}";
            }
            else if (claim.FoodClaim != null)
            {
                expenseDetails = "Food/Meals Expense";
            }
            else if (claim.RechargeClaim != null)
            {
                expenseDetails = "Mobile/Internet Recharge";
            }

            string actionDate = string.Empty;
            if (claim.Status == ClaimStatus.Approved)
                actionDate = claim.ApprovedAt?.ToString("yyyy-MM-dd HH:mm") ?? string.Empty;
            else if (claim.Status == ClaimStatus.Rejected || claim.Status == ClaimStatus.Returned)
                actionDate = claim.UpdatedAt?.ToString("yyyy-MM-dd HH:mm") ?? string.Empty;

            worksheet.Cell(row, 1).Value = claim.ClaimNumber;
            worksheet.Cell(row, 2).Value = claim.Employee?.Name;
            worksheet.Cell(row, 3).Value = claim.Employee?.EmployeeCode;
            worksheet.Cell(row, 4).Value = employeeAreas;
            worksheet.Cell(row, 5).Value = claim.ExpenseCategory?.Name;
            worksheet.Cell(row, 6).Value = totalAmount;
            worksheet.Cell(row, 7).Value = expenseDetails;
            worksheet.Cell(row, 8).Value = claim.Status.ToString();
            worksheet.Cell(row, 9).Value = claim.CreatedAt.ToString("yyyy-MM-dd HH:mm");
            worksheet.Cell(row, 10).Value = claim.SubmittedAt?.ToString("yyyy-MM-dd HH:mm");
            worksheet.Cell(row, 11).Value = actionDate;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
