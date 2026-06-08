using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCareerPath.Application.Constants;
using SmartCareerPath.Application.DTOs;
using SmartCareerPath.Application.DTOs.Roadmap;
using SmartCareerPath.Application.Interfaces;

namespace SmartCareerPath.API.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminRoadmapsController : ControllerBase
{
    private readonly IRoadmapService _roadmapService;

    public AdminRoadmapsController(IRoadmapService roadmapService)
        => _roadmapService = roadmapService;

    [HttpGet("career-tracks/{trackId}/roadmaps")]
    public async Task<IActionResult> GetByTrack(int trackId)
        => Ok(await _roadmapService.GetByTrackAsync(trackId));

    [HttpPost("career-tracks/{trackId}/roadmaps")]
    public async Task<IActionResult> Create(int trackId, [FromBody] CreateRoadmapDto dto)
    {
        var result = await _roadmapService.CreateAsync(trackId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("roadmaps/{id}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _roadmapService.GetByIdAsync(id));

    [HttpPut("roadmaps/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoadmapDto dto)
        => Ok(await _roadmapService.UpdateAsync(id, dto));

    [HttpDelete("roadmaps/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _roadmapService.DeleteAsync(id);
        return NoContent();
    }
}