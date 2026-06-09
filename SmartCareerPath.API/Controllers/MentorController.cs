using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCareerPath.Application.Constants;
using SmartCareerPath.Application.DTOs.Mentor;
using SmartCareerPath.Application.Interfaces;
using System.Security.Claims;

namespace SmartCareerPath.API.Controllers
{
    // Public browsing — any authenticated user
    [ApiController]
    [Route("api/mentors")]
    [Authorize]
    public class MentorsController : ControllerBase
    {
        private readonly IMentorService _mentorService;

        // Fix: constructor was missing — _mentorService was used but never injected.
        public MentorsController(IMentorService mentorService)
            => _mentorService = mentorService;
        // GET /api/mentors?page=1&pageSize=10&trackId=2
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? trackId = null)
            => Ok(await _mentorService.GetAllAsync(page, pageSize, trackId));

        // GET /api/mentors/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
            => Ok(await _mentorService.GetByIdAsync(id));
    }

    // Mentor self-management
    [ApiController]
    [Route("api/mentors/me")]
    [Authorize(Roles = Roles.Mentor)]
    public class MentorProfileController : ControllerBase
    {
        private readonly IMentorService _mentorService;

        // Fix: constructor was missing — _mentorService was used but never injected.
        public MentorProfileController(IMentorService mentorService)
            => _mentorService = mentorService;

        private string MentorId =>
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        // GET /api/mentors/me
        [HttpGet]
        public async Task<IActionResult> GetProfile()
            => Ok(await _mentorService.GetOwnProfileAsync(MentorId));

        // PUT /api/mentors/me
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateMentorProfileDto dto)
            => Ok(await _mentorService.UpdateOwnProfileAsync(MentorId, dto));
    }
}
