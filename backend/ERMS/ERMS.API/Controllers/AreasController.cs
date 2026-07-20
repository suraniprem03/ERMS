using ERMS.Application.Common.Models;
using ERMS.Application.Features.Areas;
using ERMS.Application.Features.Areas.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERMS.API.Controllers;

[ApiController]
[Route("api/areas")]
[Authorize]
public class AreasController : ControllerBase
{
    private readonly IAreaService _areaService;

    public AreasController(IAreaService areaService)
    {
        _areaService = areaService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AreaDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<AreaDto>>>> GetAll(
        [FromQuery] long? stateId, 
        CancellationToken cancellationToken)
    {
        IEnumerable<AreaDto> areas;
        
        if (stateId.HasValue)
        {
            areas = await _areaService.GetByStateIdAsync(stateId.Value, cancellationToken);
        }
        else
        {
            areas = await _areaService.GetAllAsync(cancellationToken);
        }

        return Ok(ApiResponse<IEnumerable<AreaDto>>.Ok(areas, "Areas retrieved successfully."));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<AreaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AreaDto>>> GetById(long id, CancellationToken cancellationToken)
    {
        var area = await _areaService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<AreaDto>.Ok(area, "Area retrieved successfully."));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AreaDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<AreaDto>>> Create(
        [FromBody] CreateAreaRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid model state."));
        }

        var area = await _areaService.CreateAsync(request, cancellationToken);
        
        return CreatedAtAction(
            nameof(GetById),
            new { id = area.Id },
            ApiResponse<AreaDto>.Ok(area, "Area created successfully."));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<AreaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AreaDto>>> Update(
        long id,
        [FromBody] UpdateAreaRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid model state."));
        }

        var area = await _areaService.UpdateAsync(id, request, cancellationToken);
        
        return Ok(ApiResponse<AreaDto>.Ok(area, "Area updated successfully."));
    }
}
