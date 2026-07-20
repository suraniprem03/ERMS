using System.ComponentModel.DataAnnotations;

namespace ERMS.Application.Features.Areas.DTOs;

public class UpdateAreaRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
