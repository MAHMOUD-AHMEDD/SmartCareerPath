namespace SmartCareerPath.Domain.Entites
{
    public class Roadmap
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TrackId { get; set; }
        public CareerTrack Track { get; set; } = null!;
        public ICollection<RoadmapItem> Items { get; set; } = [];
    }
}
