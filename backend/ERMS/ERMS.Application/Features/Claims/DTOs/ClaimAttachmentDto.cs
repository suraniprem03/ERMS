namespace ERMS.Application.Features.Claims.DTOs;

public class ClaimAttachmentDto
{
    public long Id { get; set; }
    public long ClaimId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
