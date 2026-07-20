using System.ComponentModel.DataAnnotations;

namespace ERMS.Application.Features.States.DTOs;

public class UpdateStateRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
