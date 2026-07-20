using ERMS.Application.Features.Claims.DTOs;

namespace ERMS.Application.Features.Claims;

public interface IClaimService
{
    Task<ClaimDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClaimDto>> GetMyClaimsAsync(long employeeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClaimDto>> GetAllPendingClaimsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ClaimDto>> GetAllClaimsAsync(CancellationToken cancellationToken = default);
    Task<ClaimDto> CreateAsync(long employeeId, CreateClaimRequest request, CancellationToken cancellationToken = default);
    Task<ClaimDto> UpdateDraftAsync(long id, long employeeId, UpdateClaimRequest request, CancellationToken cancellationToken = default);
    Task<ClaimDto> SubmitClaimAsync(long id, long employeeId, CancellationToken cancellationToken = default);
    Task<ClaimDto> ProcessAdminActionAsync(long id, long adminUserId, AdminActionRequest request, CancellationToken cancellationToken = default);
}
