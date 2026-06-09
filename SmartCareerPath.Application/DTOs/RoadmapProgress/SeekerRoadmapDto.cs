namespace SmartCareerPath.Application.DTOs.RoadmapProgress
{
    public record SeekerRoadmapDto(
    int RoadmapId, string Title, string Description,
    IEnumerable<SeekerRoadmapItemDto> Items);
}
