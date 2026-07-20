namespace ERMS.Application.Features.Auth;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public bool IsPasswordChangeRequired { get; set; }

    public UserInfo User { get; set; } = new();
}

public class UserInfo
{
    public long Id { get; set; }

    public long EmployeeId { get; set; }

    public string EmployeeCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string RoleCode { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;
    
    public bool IsPasswordChangeRequired { get; set; }
}
