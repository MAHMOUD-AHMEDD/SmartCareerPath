using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCareerPath.Application.Constants;
using SmartCareerPath.Application.DTOs.QuestionnaireSubmission;
using SmartCareerPath.Application.DTOs.RoadmapProgress;
using SmartCareerPath.Application.DTOs.SeekerProfile;
using SmartCareerPath.Application.Interfaces;
using System.Security.Claims;

namespace SmartCareerPath.API.Controllers
{
    [ApiController]
    [Route("api/seekers/me")]
    [Authorize(Roles = Roles.Seeker)]
    public class SeekerController : ControllerBase
    {
        private readonly ISeekerService _seekerService;

        public SeekerController(ISeekerService seekerService)
            => _seekerService = seekerService;

        private string SeekerId =>
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        [HttpGet]  // GET /api/seekers/me
        public async Task<IActionResult> GetProfile()
            => Ok(await _seekerService.GetProfileAsync(SeekerId));

        [HttpPut]  // PUT /api/seekers/me
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateSeekerProfileDto dto)
            => Ok(await _seekerService.UpdateProfileAsync(SeekerId, dto));

        [HttpGet("recommendations")]  // GET /api/seekers/me/recommendations
        public async Task<IActionResult> GetRecommendations()
            => Ok(await _seekerService.GetRecommendationsAsync(SeekerId));

        [HttpGet("roadmap/{trackId}")]  // GET /api/seekers/me/roadmap/{trackId}
        public async Task<IActionResult> GetRoadmapProgress(int trackId)
            => Ok(await _seekerService.GetRoadmapProgressAsync(SeekerId, trackId));

        [HttpPut("roadmap/items/{itemId}/status")]  // PUT /api/seekers/me/roadmap/items/{itemId}/status
        public async Task<IActionResult> UpdateItemStatus(
            int itemId, [FromBody] UpdateRoadmapItemStatusDto dto)
        {
            await _seekerService.UpdateRoadmapItemStatusAsync(SeekerId, itemId, dto.Status);
            return NoContent();
        }
    }

    [ApiController]
    [Route("api/questions")]
    [Authorize]   // any logged-in user
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionService _questionService;
        private readonly ISeekerService _seekerService;

        public QuestionsController(IQuestionService questionService, ISeekerService seekerService)
        {
            _questionService = questionService;
            _seekerService = seekerService;
        }

        private string UserId =>
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        [HttpGet]  // GET /api/questions
        public async Task<IActionResult> GetAll()
            => Ok(await _questionService.GetAllAsync());

        [HttpPost("submit")]  // POST /api/questions/submit
        [Authorize(Roles = Roles.Seeker)]
        public async Task<IActionResult> Submit([FromBody] SubmitAnswersDto dto)
        {
            await _seekerService.SubmitAnswersAsync(UserId, dto);
            return NoContent();
        }
    }
}
