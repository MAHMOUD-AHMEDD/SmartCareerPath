using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCareerPath.Application.DTOs.RecommendationAi;
using SmartCareerPath.Application.Interfaces;
using SmartCareerPath.Infrastructure.Persistence;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SmartCareerPath.Infrastructure.Services
{
    public class AiRecommendationService : IAiRecommendationService
    {
        private readonly AppDbContext _db;
        private readonly IRecommendationService _recommendationService;
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ILogger<AiRecommendationService> _logger;

        public AiRecommendationService(
            AppDbContext db,
            IRecommendationService recommendationService,
            HttpClient http,
            IConfiguration config,
            ILogger<AiRecommendationService> logger)
        {
            _db = db;
            _recommendationService = recommendationService;
            _http = http;
            _config = config;
            _logger = logger;
        }

        public async Task CallAndSaveAsync(string seekerId)
        {
            try
            {
                // 1. Load all 27 questions ordered by Id (insertion order = question order)
                var questions = await _db.Questions
                    .Include(q => q.Options)
                    .Where(q => q.QuestionType == "MCQ")
                    .OrderBy(q => q.Id)
                    .ToListAsync();

                if (questions.Count != 27)
                {
                    _logger.LogWarning(
                        "Expected 27 MCQ questions, found {Count}. Skipping AI call.",
                        questions.Count);
                    return;
                }

                // 2. Load seeker's selected options
                var selectedOptions = await _db.SeekerQuestionOptions
                    .Where(s => s.SeekerId == seekerId)
                    .ToDictionaryAsync(s => s.QuestionId, s => s.OptionId);

                // 3. Build boolean array — true if seeker selected the "Yes" option
                var answers = new bool[27];
                for (int i = 0; i < questions.Count; i++)
                {
                    var question = questions[i];
                    var yesOption = question.Options
                        .OrderBy(o => o.Id)
                        .FirstOrDefault(o => o.OptionText == "Yes");

                    if (yesOption is null) continue;

                    if (selectedOptions.TryGetValue(question.Id, out var chosenOptionId))
                        answers[i] = chosenOptionId == yesOption.Id;
                }

                // 4. Call Flask API
                var baseUrl = _config["AiService:BaseUrl"]!.TrimEnd('/');
                var response = await _http.PostAsJsonAsync(
                    $"{baseUrl}/predict",
                    new { answers });

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "AI service returned {StatusCode}. Skipping save.",
                        response.StatusCode);
                    return;
                }

                var result = await response.Content
                    .ReadFromJsonAsync<AiPredictResponse>();

                if (result?.Top3 is null || result.Top3.Count == 0) return;

                // 5. Map AI track names to DB track IDs
                var trackNames = result.Top3;
                var tracks = await _db.CareerTracks
                    .Where(t => trackNames.Contains(t.Name))
                    .ToDictionaryAsync(t => t.Name, t => t.Id);

                var recommendations = new List<TrackRecommendationDto>();
                for (int rank = 0; rank < trackNames.Count; rank++)
                {
                    if (tracks.TryGetValue(trackNames[rank], out var trackId))
                        recommendations.Add(new TrackRecommendationDto(trackId, rank + 1));
                }

                if (!recommendations.Any()) return;

                // 6. Save via existing service (delete-then-insert, transactional)
                await _recommendationService.SaveRecommendationsAsync(new SaveRecommendationsDto(
                    seekerId, recommendations));

                _logger.LogInformation(
                    "Saved {Count} AI recommendations for seeker {SeekerId}.",
                    recommendations.Count, seekerId);
            }
            catch (Exception ex)
            {
                // Never crash the submit flow — AI call is best-effort
                _logger.LogError(ex,
                    "AI recommendation call failed for seeker {SeekerId}.", seekerId);
            }
        }

        // Matches the Flask response shape: { "top3": ["Data Science", "Development", ...] }
        private class AiPredictResponse
        {
            [JsonPropertyName("top3")]
            public List<string> Top3 { get; set; } = [];
        }
    }
}
