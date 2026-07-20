using ERMS.Application.Common.Exceptions;
using ERMS.Application.Features.States;
using ERMS.Application.Features.States.DTOs;
using ERMS.Domain.Entities;
using ERMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERMS.Infrastructure.Services;

public class StateService : IStateService
{
    private readonly ERMSDbContext _dbContext;

    public StateService(ERMSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StateDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var state = await _dbContext.States
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException($"State with ID {id} not found.");

        return MapToDto(state);
    }

    public async Task<IEnumerable<StateDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var states = await _dbContext.States
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        return states.Select(MapToDto);
    }

    public async Task<StateDto> CreateAsync(CreateStateRequest request, CancellationToken cancellationToken = default)
    {
        if (await _dbContext.States.AnyAsync(s => s.Code.ToLower() == request.Code.ToLower(), cancellationToken))
        {
            throw new ValidationException($"A State with code '{request.Code}' already exists.");
        }

        var state = new State
        {
            Code = request.Code,
            Name = request.Name,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.States.Add(state);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(state);
    }

    public async Task<StateDto> UpdateAsync(long id, UpdateStateRequest request, CancellationToken cancellationToken = default)
    {
        var state = await _dbContext.States
            .Include(s => s.Areas)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException($"State with ID {id} not found.");

        if (state.IsActive && !request.IsActive)
        {
            if (state.Areas.Any(a => a.IsActive))
            {
                throw new ValidationException("Cannot deactivate a State that has active Areas. Deactivate the Areas first.");
            }
        }

        state.Name = request.Name;
        state.IsActive = request.IsActive;
        state.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(state);
    }

    private static StateDto MapToDto(State state)
    {
        return new StateDto
        {
            Id = state.Id,
            Code = state.Code,
            Name = state.Name,
            IsActive = state.IsActive
        };
    }
}
