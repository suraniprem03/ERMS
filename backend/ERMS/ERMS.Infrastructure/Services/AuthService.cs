using ERMS.Application.Common.Exceptions;
using ERMS.Application.Common.Interfaces;
using ERMS.Application.Features.Auth;
using ERMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERMS.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ERMSDbContext _dbContext;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        ERMSDbContext dbContext,
        IPasswordHasherService passwordHasherService,
        IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _passwordHasherService = passwordHasherService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .Include(u => u.Employee)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(
                u => u.Employee.Email.ToLower() == normalizedEmail,
                cancellationToken);

        if (user is null || !user.IsActive || !user.Employee.IsActive)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (!_passwordHasherService.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.UpdatedBy = user.Id;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var (token, expiresAt) = _jwtTokenService.GenerateToken(user, user.Employee, user.Role);

        return new LoginResponse
        {
            AccessToken = token,
            ExpiresAt = expiresAt,
            IsPasswordChangeRequired = user.IsPasswordChangeRequired,
            User = new UserInfo
            {
                Id = user.Id,
                EmployeeId = user.EmployeeId,
                EmployeeCode = user.Employee.EmployeeCode,
                Name = user.Employee.Name,
                Email = user.Employee.Email,
                RoleCode = user.Role.Code,
                RoleName = user.Role.Name,
                IsPasswordChangeRequired = user.IsPasswordChangeRequired
            }
        };
    }

    public async Task ChangePasswordAsync(
        long userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (!user.IsActive)
        {
            throw new UnauthorizedException("User account is inactive.");
        }

        if (!_passwordHasherService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedException("Current password is incorrect.");
        }

        user.PasswordHash = _passwordHasherService.HashPassword(request.NewPassword);
        user.IsPasswordChangeRequired = false;
        user.PasswordChangedAt = DateTimeOffset.UtcNow;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.UpdatedBy = userId;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ForceChangePasswordAsync(
        long userId,
        ForceChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (!user.IsActive)
        {
            throw new UnauthorizedException("User account is inactive.");
        }

        if (!user.IsPasswordChangeRequired)
        {
            throw new UnauthorizedException("Password change is not required for this user. Use standard change password.");
        }

        user.PasswordHash = _passwordHasherService.HashPassword(request.NewPassword);
        user.IsPasswordChangeRequired = false;
        user.PasswordChangedAt = DateTimeOffset.UtcNow;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.UpdatedBy = userId;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserInfo> GetMeAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .Include(u => u.Employee)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new UnauthorizedException("User not found.");

        return new UserInfo
        {
            Id = user.Id,
            EmployeeId = user.EmployeeId,
            EmployeeCode = user.Employee?.EmployeeCode ?? string.Empty,
            Name = user.Employee?.Name ?? string.Empty,
            Email = user.Employee?.Email ?? string.Empty,
            RoleCode = user.Role?.Code ?? string.Empty,
            RoleName = user.Role?.Name ?? string.Empty,
            IsPasswordChangeRequired = user.IsPasswordChangeRequired
        };
    }
}
