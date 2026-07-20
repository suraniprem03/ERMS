using System.Security.Claims;
using ERMS.Domain.Entities;

namespace ERMS.Application.Common.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTimeOffset ExpiresAt) GenerateToken(User user, Employee employee, Role role);

    long? GetUserId(ClaimsPrincipal principal);
}
