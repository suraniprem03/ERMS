namespace ERMS.Application.Features.Reports;

public interface IReportService
{
    Task<byte[]> GenerateClaimsExcelReportAsync(CancellationToken cancellationToken = default);
}
