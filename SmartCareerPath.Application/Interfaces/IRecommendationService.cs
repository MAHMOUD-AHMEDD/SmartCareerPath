using SmartCareerPath.Application.DTOs.Recommendation;
using SmartCareerPath.Application.DTOs.RecommendationAi;

namespace SmartCareerPath.Application.Interfaces
{
    public interface IRecommendationService
    {
        Task<SaveRecommendationsResultDto> SaveRecommendationsAsync(
            SaveRecommendationsDto dto);

        Task<IEnumerable<RecommendationDto>> GetSeekerRecommendationsAsync(
            string seekerId);
    }
}
