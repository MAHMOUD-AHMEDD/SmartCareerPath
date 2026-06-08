using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCareerPath.Application.Interfaces;

namespace SmartCareerPath.API.Controllers;

[ApiController]
[Route("api/career-tracks")]
[Authorize]
public class CareerTracksController : ControllerBase
{
    private readonly ICareerTrackService _careerTrackService;

    public CareerTracksController(ICareerTrackService careerTrackService)
        => _careerTrackService = careerTrackService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _careerTrackService.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _careerTrackService.GetByIdAsync(id));
}