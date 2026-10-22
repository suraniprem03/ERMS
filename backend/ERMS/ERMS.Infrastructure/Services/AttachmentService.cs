using ERMS.Application.Common.Exceptions;
using ERMS.Application.Features.Claims;
using ERMS.Application.Features.Claims.DTOs;
using ERMS.Domain.Entities;
using ERMS.Domain.Enums;
using ERMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.IO.Compression;

namespace ERMS.Infrastructure.Services;

public class AttachmentService : IAttachmentService
{
    private readonly ERMSDbContext _dbContext;
    private readonly string _uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

    public AttachmentService(ERMSDbContext dbContext)
    {
        _dbContext = dbContext;
        if (!Directory.Exists(_uploadDirectory))
        {
            Directory.CreateDirectory(_uploadDirectory);
        }
    }

    public async Task<ClaimAttachmentDto> UploadAttachmentAsync(long claimId, long employeeId, string fileName, string contentType, Stream fileStream)
    {
        var claim = await _dbContext.Claims.FirstOrDefaultAsync(c => c.Id == claimId && c.EmployeeId == employeeId)
            ?? throw new NotFoundException($"Claim with ID {claimId} not found or you do not have permission.");

        if (claim.Status != ClaimStatus.Draft && claim.Status != ClaimStatus.Returned)
        {
            throw new ValidationException("Attachments can only be added to Draft or Returned claims.");
        }

        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".pdf" };

        if (!allowedExtensions.Contains(fileExtension))
        {
            throw new ValidationException($"File type {fileExtension} is not allowed. Only PNG, JPG, JPEG, and PDF are allowed.");
        }

        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(_uploadDirectory, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(stream);
        }

        var attachment = new ClaimAttachment
        {
            ClaimId = claimId,
            OriginalFileName = fileName,
            FileName = uniqueFileName,
            FileExtension = fileExtension,
            ContentType = contentType,
            FilePath = filePath,
            FileSize = new FileInfo(filePath).Length,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = employeeId
        };

        _dbContext.ClaimAttachments.Add(attachment);
        await _dbContext.SaveChangesAsync();

        return MapToDto(attachment);
    }

    public async Task<(Stream FileStream, string ContentType, string FileName)> DownloadAttachmentAsync(long attachmentId, long employeeId, bool isAdmin)
    {
        var attachment = await _dbContext.ClaimAttachments
            .Include(a => a.Claim)
            .FirstOrDefaultAsync(a => a.Id == attachmentId)
            ?? throw new NotFoundException($"Attachment with ID {attachmentId} not found.");

        if (!isAdmin && attachment.Claim.EmployeeId != employeeId)
        {
            throw new UnauthorizedAccessException("You do not have permission to download this attachment.");
        }

        if (!File.Exists(attachment.FilePath))
        {
            throw new NotFoundException("Physical file not found on disk.");
        }

        var stream = new FileStream(attachment.FilePath, FileMode.Open, FileAccess.Read);
        return (stream, attachment.ContentType, attachment.OriginalFileName);
    }

    public async Task DeleteAttachmentAsync(long attachmentId, long employeeId)
    {
        var attachment = await _dbContext.ClaimAttachments
            .Include(a => a.Claim)
            .FirstOrDefaultAsync(a => a.Id == attachmentId)
            ?? throw new NotFoundException($"Attachment with ID {attachmentId} not found.");

        if (attachment.Claim.EmployeeId != employeeId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this attachment.");
        }

        if (attachment.Claim.Status != ClaimStatus.Draft && attachment.Claim.Status != ClaimStatus.Returned)
        {
            throw new ValidationException("Attachments can only be deleted from Draft or Returned claims.");
        }

        if (File.Exists(attachment.FilePath))
        {
            File.Delete(attachment.FilePath);
        }

        _dbContext.ClaimAttachments.Remove(attachment);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<(byte[] ZipBytes, string FileName)> DownloadMonthlyBillsZipAsync(DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken = default)
    {
        var startUtc = startDate.ToUniversalTime();
        var endUtc = endDate.ToUniversalTime();
        var startDateOnly = DateOnly.FromDateTime(startDate.Date);
        var endDateOnly = DateOnly.FromDateTime(endDate.Date);

        var attachments = await _dbContext.ClaimAttachments
            .Include(a => a.Claim)
                .ThenInclude(c => c.Employee)
            .Include(a => a.Claim)
                .ThenInclude(c => c.ExpenseCategory)
            .Include(a => a.Claim)
                .ThenInclude(c => c.TravelClaim)
            .Include(a => a.Claim)
                .ThenInclude(c => c.FoodClaim)
            .Include(a => a.Claim)
                .ThenInclude(c => c.HotelClaim)
            .Where(a => a.Claim.Status == ClaimStatus.Approved && (
                (a.CreatedAt >= startUtc && a.CreatedAt <= endUtc)
                || (a.Claim.SubmittedAt.HasValue && a.Claim.SubmittedAt >= startUtc && a.Claim.SubmittedAt <= endUtc)
                || (a.Claim.CreatedAt >= startUtc && a.Claim.CreatedAt <= endUtc)
                || (a.Claim.TravelClaim != null && a.Claim.TravelClaim.TravelDate >= startDateOnly && a.Claim.TravelClaim.TravelDate <= endDateOnly)
                || (a.Claim.FoodClaim != null && a.Claim.FoodClaim.FoodDate.HasValue && a.Claim.FoodClaim.FoodDate >= startDateOnly && a.Claim.FoodClaim.FoodDate <= endDateOnly)
                || (a.Claim.HotelClaim != null && a.Claim.HotelClaim.CheckInDate >= startDateOnly && a.Claim.HotelClaim.CheckInDate <= endDateOnly)))
            .ToListAsync(cancellationToken);

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            var employeeGroups = attachments
                .Where(a => a.Claim?.Employee != null)
                .GroupBy(a => a.Claim.EmployeeId);

            foreach (var group in employeeGroups)
            {
                var employee = group.First().Claim.Employee;
                var rawEmpName = !string.IsNullOrWhiteSpace(employee.Name) ? employee.Name : $"Employee_{employee.Id}";
                var rawEmpCode = !string.IsNullOrWhiteSpace(employee.EmployeeCode) ? employee.EmployeeCode : $"{employee.Id}";

                var cleanEmpName = string.Join("_", rawEmpName.Split(Path.GetInvalidFileNameChars())).Trim();
                var cleanEmpCode = string.Join("_", rawEmpCode.Split(Path.GetInvalidFileNameChars())).Trim();
                var folderName = $"{cleanEmpName}_{cleanEmpCode}";

                var usedEntryNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var att in group)
                {
                    if (string.IsNullOrEmpty(att.FilePath) || !File.Exists(att.FilePath))
                    {
                        continue;
                    }

                    var claimNumber = att.Claim?.ClaimNumber ?? $"CLM_{att.ClaimId}";
                    var categoryName = att.Claim?.ExpenseCategory?.Name ?? "Expense";
                    var cleanCategory = string.Join("_", categoryName.Split(Path.GetInvalidFileNameChars())).Trim();
                    var cleanOriginalName = string.Join("_", (att.OriginalFileName ?? "receipt").Split(Path.GetInvalidFileNameChars())).Trim();

                    var entryFileName = $"{folderName}/{claimNumber}_{cleanCategory}_{cleanOriginalName}";

                    // Ensure entry name is unique inside the zip archive
                    int collisionCounter = 1;
                    var baseEntryName = entryFileName;
                    while (usedEntryNames.Contains(entryFileName))
                    {
                        var ext = Path.GetExtension(baseEntryName);
                        var withoutExt = Path.ChangeExtension(baseEntryName, null);
                        entryFileName = $"{withoutExt}_{collisionCounter}{ext}";
                        collisionCounter++;
                    }
                    usedEntryNames.Add(entryFileName);

                    var entry = archive.CreateEntry(entryFileName, CompressionLevel.Optimal);
                    using var entryStream = entry.Open();
                    using var fileStream = new FileStream(att.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    await fileStream.CopyToAsync(entryStream, cancellationToken);
                }
            }
        }

        var zipFileName = $"Bills_{startDate:yyyyMMdd}_to_{endDate:yyyyMMdd}.zip";
        return (memoryStream.ToArray(), zipFileName);
    }

    private static ClaimAttachmentDto MapToDto(ClaimAttachment attachment)
    {
        return new ClaimAttachmentDto
        {
            Id = attachment.Id,
            ClaimId = attachment.ClaimId,
            FileName = attachment.FileName,
            OriginalFileName = attachment.OriginalFileName,
            FileExtension = attachment.FileExtension,
            ContentType = attachment.ContentType,
            FileSize = attachment.FileSize,
            CreatedAt = attachment.CreatedAt
        };
    }
}
