using ERMS.Application.Features.Areas.DTOs;

namespace ERMS.Application.Features.Areas;

public interface IAreaService
{
    Task<AreaDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AreaDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<AreaDto>> GetByStateIdAsync(long stateId, CancellationToken cancellationToken = default);
    Task<AreaDto> CreateAsync(CreateAreaRequest request, CancellationToken cancellationToken = default);
    Task<AreaDto> UpdateAsync(long id, UpdateAreaRequest request, CancellationToken cancellationToken = default);
}
