using SmartCareerPath.Domain.Entites.Identity;

namespace SmartCareerPath.Domain.Entites
{
    public class CareerTrack
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<Roadmap> Roadmaps { get; set; } = [];
        public ICollection<Mentor> Mentors { get; set; } = [];
        public ICollection<Recommendation> Recommendations { get; set; } = [];
    }
}
