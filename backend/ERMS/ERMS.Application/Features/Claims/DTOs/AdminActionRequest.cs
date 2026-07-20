using System.ComponentModel.DataAnnotations;
using ERMS.Domain.Enums;

namespace ERMS.Application.Features.Claims.DTOs;

public class AdminActionRequest
{
    [Required]
    public ClaimStatus Action { get; set; }

    [MaxLength(2000)]
    public string? Remarks { get; set; }
}
