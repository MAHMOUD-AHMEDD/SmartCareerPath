namespace SmartCareerPath.Application.DTOs.RecommendationAi
{
    // Response
    public record SaveRecommendationsResultDto(
        string SeekerId,
        int SavedCount,
        DateTime SavedAt);
}
