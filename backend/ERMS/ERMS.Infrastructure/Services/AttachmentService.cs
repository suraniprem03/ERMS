using ERMS.Application.Common.Exceptions;
using ERMS.Application.Features.Claims;
using ERMS.Application.Features.Claims.DTOs;
using ERMS.Domain.Entities;
using ERMS.Domain.Enums;
using ERMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.IO;

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
