using ERMS.Application.Common.Exceptions;
using ERMS.Application.Common.Interfaces;
using ERMS.Application.Common.Models;
using ERMS.Application.Features.Auth;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERMS.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;
    private readonly IValidator<ForceChangePasswordRequest> _forceChangePasswordValidator;

    public AuthController(
        IAuthService authService,
        IJwtTokenService jwtTokenService,
        IValidator<LoginRequest> loginValidator,
        IValidator<ChangePasswordRequest> changePasswordValidator,
        IValidator<ForceChangePasswordRequest> forceChangePasswordValidator)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
        _loginValidator = loginValidator;
        _changePasswordValidator = changePasswordValidator;
        _forceChangePasswordValidator = forceChangePasswordValidator;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        await ValidateAsync(_loginValidator, request, cancellationToken);

        var response = await _authService.LoginAsync(request, cancellationToken);

        return Ok(ApiResponse<LoginResponse>.Ok(response, "Login successful."));
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        await ValidateAsync(_changePasswordValidator, request, cancellationToken);

        var userId = _jwtTokenService.GetUserId(User)
            ?? throw new UnauthorizedException("User is not authenticated.");

        await _authService.ChangePasswordAsync(userId, request, cancellationToken);

        return Ok(ApiResponse<object>.Ok(new { }, "Password changed successfully."));
    }

    [HttpPost("force-change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> ForceChangePassword(
        [FromBody] ForceChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        await ValidateAsync(_forceChangePasswordValidator, request, cancellationToken);

        var userId = _jwtTokenService.GetUserId(User)
            ?? throw new UnauthorizedException("User is not authenticated.");

        await _authService.ForceChangePasswordAsync(userId, request, cancellationToken);

        return Ok(ApiResponse<object>.Ok(new { }, "Password changed successfully."));
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserInfo>>> GetMe(CancellationToken cancellationToken)
    {
        var userId = _jwtTokenService.GetUserId(User)
            ?? throw new UnauthorizedException("User is not authenticated.");
            
        var response = await _authService.GetMeAsync(userId, cancellationToken);
        return Ok(ApiResponse<UserInfo>.Ok(response, "User profile retrieved successfully."));
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status501NotImplemented)]
    public ActionResult<ApiResponse<object>> RefreshToken()
    {
        // TODO: Implement refresh token flow when refresh token storage is defined.
        return StatusCode(
            StatusCodes.Status501NotImplemented,
            ApiResponse<object>.Fail("Refresh token endpoint is not implemented yet."));
    }

    private static async Task ValidateAsync<T>(
        IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(instance, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new Application.Common.Exceptions.ValidationException(
                "Validation failed.",
                validationResult.Errors
                    .Select(error => error.ErrorMessage)
                    .ToArray());
        }
    }
}
