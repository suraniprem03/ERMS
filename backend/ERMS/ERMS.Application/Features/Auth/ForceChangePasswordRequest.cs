namespace ERMS.Application.Features.Auth;

public class ForceChangePasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;

    public string ConfirmPassword { get; set; } = string.Empty;
}
