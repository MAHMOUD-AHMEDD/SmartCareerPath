namespace SmartCareerPath.Application.DTOs.RecommendationAi
{
    // Incoming from AI
    public record SaveRecommendationsDto(
        string SeekerId,
        IEnumerable<TrackRecommendationDto> Results);
}
