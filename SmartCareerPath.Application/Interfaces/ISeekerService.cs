using SmartCareerPath.Application.DTOs.QuestionnaireSubmission;
using SmartCareerPath.Application.DTOs.Recommendation;
using SmartCareerPath.Application.DTOs.RoadmapProgress;
using SmartCareerPath.Application.DTOs.SeekerProfile;

namespace SmartCareerPath.Application.Interfaces
{
    public interface ISeekerService
    {
        Task<SeekerProfileDto> GetProfileAsync(string seekerId);
        Task<SeekerProfileDto> UpdateProfileAsync(string seekerId, UpdateSeekerProfileDto dto);
        Task SubmitAnswersAsync(string seekerId, SubmitAnswersDto dto);
        Task<IEnumerable<RecommendationDto>> GetRecommendationsAsync(string seekerId);
        Task<SeekerRoadmapDto> GetRoadmapProgressAsync(string seekerId, int trackId);
        Task UpdateRoadmapItemStatusAsync(string seekerId, int itemId, UpdateRoadmapItemStatusDto dto);
    }
}
