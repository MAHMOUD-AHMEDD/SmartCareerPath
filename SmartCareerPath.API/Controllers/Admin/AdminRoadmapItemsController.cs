using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCareerPath.Application.Constants;
using SmartCareerPath.Application.DTOs;
using SmartCareerPath.Application.DTOs.RoadmapItem;
using SmartCareerPath.Application.Interfaces;

namespace SmartCareerPath.API.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminRoadmapItemsController : ControllerBase
{
    private readonly IRoadmapItemService _roadmapItemService;

    public AdminRoadmapItemsController(IRoadmapItemService roadmapItemService)
        => _roadmapItemService = roadmapItemService;

    [HttpGet("roadmaps/{roadmapId}/items")]
    public async Task<IActionResult> GetByRoadmap(int roadmapId)
        => Ok(await _roadmapItemService.GetByRoadmapAsync(roadmapId));

    [HttpPost("roadmaps/{roadmapId}/items")]
    public async Task<IActionResult> Create(int roadmapId, [FromBody] CreateRoadmapItemDto dto)
    {
        var result = await _roadmapItemService.CreateAsync(roadmapId, dto);
        return CreatedAtAction(nameof(GetByRoadmap), new { roadmapId }, result);
    }

    [HttpPut("roadmap-items/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoadmapItemDto dto)
        => Ok(await _roadmapItemService.UpdateAsync(id, dto));

    [HttpDelete("roadmap-items/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _roadmapItemService.DeleteAsync(id);
        return NoContent();
    }
}