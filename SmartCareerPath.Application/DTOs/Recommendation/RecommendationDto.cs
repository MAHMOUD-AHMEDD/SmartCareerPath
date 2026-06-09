namespace SmartCareerPath.Application.DTOs.Recommendation
{
    public record RecommendationDto(
    int TrackId, string TrackName, string TrackDescription,
    int Rank, DateTime RecommendedAt);
}
