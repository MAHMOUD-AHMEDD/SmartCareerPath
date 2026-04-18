using SmartCareerPath.Domain.Entites.Identity;

namespace SmartCareerPath.Domain.Entites
{
    public class Recommendation
    {
        public string SeekerId { get; set; } = string.Empty;
        public int TrackId { get; set; }
        public int Rank { get; set; }
        public DateTime RecommendedAt { get; set; } = DateTime.UtcNow;
        public Seeker Seeker { get; set; } = null!;
        public CareerTrack Track { get; set; } = null!;
    }
}
