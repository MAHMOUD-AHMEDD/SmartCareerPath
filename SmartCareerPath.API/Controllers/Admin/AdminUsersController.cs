using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCareerPath.Application.Constants;
using SmartCareerPath.Application.Interfaces;

namespace SmartCareerPath.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = Roles.Admin)]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminUsersController(IAdminService adminService)
            => _adminService = adminService;

        [HttpGet("seekers")]
        public async Task<IActionResult> GetSeekers(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
            => Ok(await _adminService.GetAllSeekersAsync(page, pageSize));

        [HttpGet("mentors")]
        public async Task<IActionResult> GetMentors(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
            => Ok(await _adminService.GetAllMentorsAsync(page, pageSize));

        [HttpGet("seekers/{id}")]
        public async Task<IActionResult> GetSeeker(string id)
            => Ok(await _adminService.GetSeekerByIdAsync(id));

        [HttpGet("mentors/{id}")]
        public async Task<IActionResult> GetMentor(string id)
            => Ok(await _adminService.GetMentorByIdAsync(id));
    }
}
