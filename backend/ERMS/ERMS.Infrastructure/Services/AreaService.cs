using ERMS.Application.Common.Exceptions;
using ERMS.Application.Features.Areas;
using ERMS.Application.Features.Areas.DTOs;
using ERMS.Domain.Entities;
using ERMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERMS.Infrastructure.Services;

public class AreaService : IAreaService
{
    private readonly ERMSDbContext _dbContext;

    public AreaService(ERMSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AreaDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var area = await _dbContext.Areas
            .Include(a => a.State)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Area with ID {id} not found.");

        return MapToDto(area);
    }

    public async Task<IEnumerable<AreaDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var areas = await _dbContext.Areas
            .Include(a => a.State)
            .OrderBy(a => a.State.Name).ThenBy(a => a.Name)
            .ToListAsync(cancellationToken);

        return areas.Select(MapToDto);
    }

    public async Task<IEnumerable<AreaDto>> GetByStateIdAsync(long stateId, CancellationToken cancellationToken = default)
    {
        var areas = await _dbContext.Areas
            .Include(a => a.State)
            .Where(a => a.StateId == stateId)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);

        return areas.Select(MapToDto);
    }

    public async Task<AreaDto> CreateAsync(CreateAreaRequest request, CancellationToken cancellationToken = default)
    {
        var state = await _dbContext.States.FirstOrDefaultAsync(s => s.Id == request.StateId, cancellationToken)
            ?? throw new NotFoundException($"State with ID {request.StateId} not found.");

        if (!state.IsActive)
        {
            throw new ValidationException("Cannot add an Area to an inactive State.");
        }

        if (await _dbContext.Areas.AnyAsync(a => a.Code.ToLower() == request.Code.ToLower() && a.StateId == request.StateId, cancellationToken))
        {
            throw new ValidationException($"An Area with code '{request.Code}' already exists in this State.");
        }

        var area = new Area
        {
            StateId = request.StateId,
            State = state,
            Code = request.Code,
            Name = request.Name,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Areas.Add(area);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(area);
    }

    public async Task<AreaDto> UpdateAsync(long id, UpdateAreaRequest request, CancellationToken cancellationToken = default)
    {
        var area = await _dbContext.Areas
            .Include(a => a.State)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Area with ID {id} not found.");

        area.Name = request.Name;
        area.IsActive = request.IsActive;
        area.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(area);
    }

    private static AreaDto MapToDto(Area area)
    {
        return new AreaDto
        {
            Id = area.Id,
            StateId = area.StateId,
            StateName = area.State?.Name ?? string.Empty,
            Code = area.Code,
            Name = area.Name,
            IsActive = area.IsActive
        };
    }
}
