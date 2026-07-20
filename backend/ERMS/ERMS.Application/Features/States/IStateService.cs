using ERMS.Application.Features.States.DTOs;

namespace ERMS.Application.Features.States;

public interface IStateService
{
    Task<StateDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IEnumerable<StateDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<StateDto> CreateAsync(CreateStateRequest request, CancellationToken cancellationToken = default);
    Task<StateDto> UpdateAsync(long id, UpdateStateRequest request, CancellationToken cancellationToken = default);
}
