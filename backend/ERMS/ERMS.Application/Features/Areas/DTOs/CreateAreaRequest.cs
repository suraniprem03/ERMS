using System.ComponentModel.DataAnnotations;

namespace ERMS.Application.Features.Areas.DTOs;

public class CreateAreaRequest
{
    [Required]
    public long StateId { get; set; }

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
