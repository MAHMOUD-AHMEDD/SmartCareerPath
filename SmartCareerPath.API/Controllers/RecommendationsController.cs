using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCareerPath.API.Attributes;
using SmartCareerPath.Application.DTOs.RecommendationAi;
using SmartCareerPath.Application.Interfaces;

namespace SmartCareerPath.API.Controllers
{
    [ApiController]
    [Route("api/recommendations")]
    public class RecommendationsController : ControllerBase
    {
        private readonly IRecommendationService _service;
        public RecommendationsController(IRecommendationService service)
            => _service = service;

        // Called BY the AI service — secured with API key
        [HttpPost]
        [RequireApiKey]  // our custom attribute
        [AllowAnonymous] // JWT not required — API key is the auth mechanism
        public async Task<IActionResult> SaveRecommendations(
            [FromBody] SaveRecommendationsDto dto)
        {
            var result = await _service.SaveRecommendationsAsync(dto);
            return Ok(result);
        }
    }

    // Seeker reads their own recommendations via the Seeker controller:
    // GET /api/seekers/me/recommendations  ← already planned in Phase 5
}
