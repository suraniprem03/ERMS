using ERMS.Application.Common.Models;
using ERMS.Application.Features.States;
using ERMS.Application.Features.States.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERMS.API.Controllers;

[ApiController]
[Route("api/states")]
[Authorize]
public class StatesController : ControllerBase
{
    private readonly IStateService _stateService;

    public StatesController(IStateService stateService)
    {
        _stateService = stateService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StateDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<StateDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var states = await _stateService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IEnumerable<StateDto>>.Ok(states, "States retrieved successfully."));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StateDto>>> GetById(long id, CancellationToken cancellationToken)
    {
        var state = await _stateService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<StateDto>.Ok(state, "State retrieved successfully."));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StateDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<StateDto>>> Create(
        [FromBody] CreateStateRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid model state."));
        }

        var state = await _stateService.CreateAsync(request, cancellationToken);
        
        return CreatedAtAction(
            nameof(GetById),
            new { id = state.Id },
            ApiResponse<StateDto>.Ok(state, "State created successfully."));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StateDto>>> Update(
        long id,
        [FromBody] UpdateStateRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid model state."));
        }

        var state = await _stateService.UpdateAsync(id, request, cancellationToken);
        
        return Ok(ApiResponse<StateDto>.Ok(state, "State updated successfully."));
    }
}
