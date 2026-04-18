namespace SmartCareerPath.Domain.Entites
{
    public class RoadmapItem
    {
        public int Id { get; set; }
        public int RoadmapId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public string DefaultStatus { get; set; } = "NotStarted";  // template default only
        public string? Link { get; set; }
        public Roadmap Roadmap { get; set; } = null!;
        public ICollection<SeekerRoadmapProgress> SeekerProgress { get; set; } = [];


    }
}
