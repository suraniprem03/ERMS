namespace ERMS.Application.Features.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(long userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task ForceChangePasswordAsync(long userId, ForceChangePasswordRequest request, CancellationToken cancellationToken = default);

    Task<UserInfo> GetMeAsync(long userId, CancellationToken cancellationToken = default);
}
