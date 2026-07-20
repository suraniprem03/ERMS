using ERMS.Application.Common.Interfaces;
using ERMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
namespace ERMS.Infrastructure.Identity;

public class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string HashPassword(string password) =>
        _passwordHasher.HashPassword(null!, password);

    public bool VerifyPassword(string password, string passwordHash)
    {
        var result = _passwordHasher.VerifyHashedPassword(null!, passwordHash, password);

        return result is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
