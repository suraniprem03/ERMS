using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ERMS.Application.Common.Interfaces;
using ERMS.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SecurityClaim = System.Security.Claims.Claim;

namespace ERMS.Infrastructure.Identity;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public (string Token, DateTimeOffset ExpiresAt) GenerateToken(User user, Employee employee, Role role)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

        var claims = new List<SecurityClaim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, employee.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("employee_id", employee.Id.ToString()),
            new("employee_code", employee.EmployeeCode),
            new("name", employee.Name),
            new(ClaimTypes.Role, role.Code)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public long? GetUserId(ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        return long.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
