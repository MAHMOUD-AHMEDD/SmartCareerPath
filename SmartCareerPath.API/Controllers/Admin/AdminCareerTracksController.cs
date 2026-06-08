using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCareerPath.Application.Constants;
using SmartCareerPath.Application.DTOs;
using SmartCareerPath.Application.DTOs.CareerTrack;
using SmartCareerPath.Application.Interfaces;

namespace SmartCareerPath.API.Controllers.Admin;

[ApiController]
[Route("api/admin/career-tracks")]
[Authorize(Roles = Roles.Admin)]
public class AdminCareerTracksController : ControllerBase
{
    private readonly ICareerTrackService _careerTrackService;

    public AdminCareerTracksController(ICareerTrackService careerTrackService)
        => _careerTrackService = careerTrackService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _careerTrackService.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCareerTrackDto dto)
    {
        var result = await _careerTrackService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _careerTrackService.GetByIdAsync(id));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCareerTrackDto dto)
        => Ok(await _careerTrackService.UpdateAsync(id, dto));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _careerTrackService.DeleteAsync(id);
        return NoContent();
    }
}