using System.ComponentModel.DataAnnotations;

namespace ERMS.Application.Features.States.DTOs;

public class CreateStateRequest
{
    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
