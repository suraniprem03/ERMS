using ERMS.Application.Features.Claims.DTOs;

namespace ERMS.Application.Features.Claims;

public interface IAttachmentService
{
    Task<ClaimAttachmentDto> UploadAttachmentAsync(long claimId, long employeeId, string fileName, string contentType, Stream fileStream);
    Task<(Stream FileStream, string ContentType, string FileName)> DownloadAttachmentAsync(long attachmentId, long employeeId, bool isAdmin);
    Task DeleteAttachmentAsync(long attachmentId, long employeeId);
}
