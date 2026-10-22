using ClosedXML.Excel;
using ERMS.Application.Features.Reports;
using ERMS.Domain.Entities;
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
            .Include(c => c.ExpenseCategory)
            .Include(c => c.TravelClaim)
            .Include(c => c.FoodClaim)
            .Include(c => c.HotelClaim)
            .Include(c => c.RechargeClaim)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        using var workbook = new XLWorkbook();

        var employeeGroups = claims
            .Where(c => c.Employee != null)
            .GroupBy(c => c.EmployeeId)
            .ToList();

        if (!employeeGroups.Any())
        {
            var emptyWs = workbook.Worksheets.Add("Expense Report");
            AddTemplateHeader(emptyWs);
            emptyWs.Columns().AdjustToContents();
            using var emptyStream = new MemoryStream();
            workbook.SaveAs(emptyStream);
            return emptyStream.ToArray();
        }

        var usedSheetNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var group in employeeGroups)
        {
            var employee = group.First().Employee;
            var rawName = !string.IsNullOrWhiteSpace(employee.Name) ? employee.Name : $"Employee_{employee.Id}";
            var cleanSheetName = string.Join("", rawName.Where(c => !":\\/?*[]".Contains(c))).Trim();
            if (string.IsNullOrWhiteSpace(cleanSheetName)) cleanSheetName = $"Employee_{employee.Id}";
            if (cleanSheetName.Length > 28) cleanSheetName = cleanSheetName.Substring(0, 28);

            var baseSheetName = cleanSheetName;
            int counter = 1;
            while (usedSheetNames.Contains(cleanSheetName))
            {
                cleanSheetName = $"{baseSheetName}_{counter}";
                counter++;
            }
            usedSheetNames.Add(cleanSheetName);

            var ws = workbook.Worksheets.Add(cleanSheetName);
            AddTemplateHeader(ws);

            var empClaims = group.ToList();

            // Extract all distinct dates for travel, food, and hotel
            var dates = new SortedSet<DateOnly>();
            foreach (var c in empClaims)
            {
                if (c.TravelClaim != null)
                {
                    dates.Add(c.TravelClaim.TravelDate);
                }
                if (c.FoodClaim != null)
                {
                    var fDate = c.FoodClaim.FoodDate 
                             ?? (c.SubmittedAt.HasValue ? DateOnly.FromDateTime(c.SubmittedAt.Value.DateTime) : DateOnly.FromDateTime(c.CreatedAt.DateTime));
                    dates.Add(fDate);
                }
                if (c.HotelClaim != null)
                {
                    dates.Add(c.HotelClaim.CheckInDate);
                }
            }

            int currentRow = 3;
            decimal totalKm = 0;
            decimal totalTravelAmount = 0;
            decimal totalDailyAllowances = 0;
            decimal totalAccommodation = 0;

            foreach (var date in dates)
            {
                var travelClaims = empClaims.Where(c => c.TravelClaim != null && c.TravelClaim.TravelDate == date).Select(c => c.TravelClaim!).ToList();
                
                var foodSum = empClaims.Where(c => c.FoodClaim != null && (
                    c.FoodClaim.FoodDate == date ||
                    (!c.FoodClaim.FoodDate.HasValue && DateOnly.FromDateTime((c.SubmittedAt ?? c.CreatedAt).DateTime) == date)
                )).Sum(c => c.FoodClaim!.Amount);

                var hotelSum = empClaims.Where(c => c.HotelClaim != null && c.HotelClaim.CheckInDate == date).Sum(c => c.HotelClaim!.Amount);

                if (travelClaims.Any())
                {
                    for (int i = 0; i < travelClaims.Count; i++)
                    {
                        var t = travelClaims[i];
                        ws.Cell(currentRow, 1).Value = date.ToString("yyyy-MM-dd");
                        ws.Cell(currentRow, 2).Value = !string.IsNullOrWhiteSpace(t.Day) ? t.Day : date.DayOfWeek.ToString();
                        ws.Cell(currentRow, 3).Value = t.FromLocation;
                        ws.Cell(currentRow, 4).Value = t.ToLocation;
                        ws.Cell(currentRow, 5).Value = !string.IsNullOrWhiteSpace(t.PlaceVisited) ? t.PlaceVisited : t.ToLocation;
                        ws.Cell(currentRow, 6).Value = t.TotalKm;
                        ws.Cell(currentRow, 7).Value = t.CalculatedAmount;

                        totalKm += t.TotalKm;
                        totalTravelAmount += t.CalculatedAmount;

                        if (i == 0 && foodSum > 0)
                        {
                            ws.Cell(currentRow, 8).Value = foodSum;
                            totalDailyAllowances += foodSum;
                        }
                        if (i == 0 && hotelSum > 0)
                        {
                            ws.Cell(currentRow, 9).Value = hotelSum;
                            totalAccommodation += hotelSum;
                        }

                        currentRow++;
                    }
                }
                else
                {
                    // No travel on this date, but had food or hotel
                    ws.Cell(currentRow, 1).Value = date.ToString("yyyy-MM-dd");
                    ws.Cell(currentRow, 2).Value = date.DayOfWeek.ToString();
                    ws.Cell(currentRow, 3).Value = "";
                    ws.Cell(currentRow, 4).Value = "";
                    ws.Cell(currentRow, 5).Value = "";
                    ws.Cell(currentRow, 6).Value = 0;
                    ws.Cell(currentRow, 7).Value = 0;

                    if (foodSum > 0)
                    {
                        ws.Cell(currentRow, 8).Value = foodSum;
                        totalDailyAllowances += foodSum;
                    }
                    if (hotelSum > 0)
                    {
                        ws.Cell(currentRow, 9).Value = hotelSum;
                        totalAccommodation += hotelSum;
                    }

                    currentRow++;
                }
            }

            // Mobile recharge total
            decimal rechargeTotal = empClaims.Where(c => c.RechargeClaim != null).Sum(c => c.RechargeClaim!.Amount);
            decimal subTotal = totalTravelAmount + totalDailyAllowances + totalAccommodation;
            decimal grandTotal = subTotal + rechargeTotal;

            // Empty row
            currentRow++;

            // Row 1: Subtotal
            ws.Cell(currentRow, 6).Value = 0;
            ws.Cell(currentRow, 10).Value = "Total";
            ws.Cell(currentRow, 10).Style.Font.Bold = true;
            ws.Cell(currentRow, 11).Value = subTotal;
            ws.Cell(currentRow, 11).Style.Font.Bold = true;
            currentRow++;

            // Row 2: Category Totals & Mobile Recharge
            ws.Cell(currentRow, 6).Value = totalKm;
            ws.Cell(currentRow, 7).Value = totalTravelAmount;
            ws.Cell(currentRow, 8).Value = totalDailyAllowances;
            ws.Cell(currentRow, 9).Value = totalAccommodation;
            ws.Row(currentRow).Style.Font.Bold = true;

            ws.Cell(currentRow, 10).Value = "Mobile Recharge";
            ws.Cell(currentRow, 10).Style.Font.Bold = true;
            ws.Cell(currentRow, 11).Value = rechargeTotal;
            ws.Cell(currentRow, 11).Style.Font.Bold = true;
            currentRow++;

            // Row 3: Prepared By & Grand Total
            ws.Cell(currentRow, 1).Value = "Prepared By:";
            ws.Cell(currentRow, 1).Style.Font.Bold = true;
            ws.Cell(currentRow, 3).Value = employee.Name;
            ws.Cell(currentRow, 3).Style.Font.Bold = true;

            ws.Cell(currentRow, 10).Value = "Grand Total";
            ws.Cell(currentRow, 10).Style.Font.Bold = true;
            ws.Cell(currentRow, 11).Value = grandTotal;
            ws.Cell(currentRow, 11).Style.Font.Bold = true;
            ws.Cell(currentRow, 11).Style.Fill.BackgroundColor = XLColor.LightYellow;
            currentRow += 2;

            // Row 4: Verified By
            ws.Cell(currentRow, 1).Value = "Verified By:";
            ws.Cell(currentRow, 1).Style.Font.Bold = true;
            currentRow += 2;

            // Row 5: Approved By
            ws.Cell(currentRow, 1).Value = "Approved By:";
            ws.Cell(currentRow, 1).Style.Font.Bold = true;

            ws.Columns().AdjustToContents();
        }

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return memoryStream.ToArray();
    }

    private static void AddTemplateHeader(IXLWorksheet ws)
    {
        // Headers on Row 2 matching user template
        ws.Cell(2, 1).Value = "Date";
        ws.Cell(2, 2).Value = "Day";
        ws.Cell(2, 3).Value = "Start Point";
        ws.Cell(2, 4).Value = "End Point";
        ws.Cell(2, 5).Value = "Place Visited";
        ws.Cell(2, 6).Value = "Total KM's";
        ws.Cell(2, 7).Value = "Amount";
        ws.Cell(2, 8).Value = "Daily Allowances";
        ws.Cell(2, 9).Value = "Accommodation";

        var headerRow = ws.Row(2);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#E2E8F0");
        headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }
}
