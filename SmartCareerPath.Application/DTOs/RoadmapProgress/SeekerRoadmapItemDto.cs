namespace SmartCareerPath.Application.DTOs.RoadmapProgress
{
    public record SeekerRoadmapItemDto(
    int Id, string Title, string Description,
    int OrderIndex, string Status,  // seeker-specific status
    string? Link);
}
