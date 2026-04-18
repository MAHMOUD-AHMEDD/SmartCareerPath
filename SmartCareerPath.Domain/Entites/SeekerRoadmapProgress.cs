using SmartCareerPath.Domain.Entites.Identity;

namespace SmartCareerPath.Domain.Entites
{
    public class SeekerRoadmapProgress
    {
        public string SeekerId { get; set; } = string.Empty;
        public int RoadmapItemId { get; set; }
        public string Status { get; set; } = "NotStarted";  // NotStarted | InProgress | Completed
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Seeker Seeker { get; set; } = null!;
        public RoadmapItem RoadmapItem { get; set; } = null!;
    }
}
