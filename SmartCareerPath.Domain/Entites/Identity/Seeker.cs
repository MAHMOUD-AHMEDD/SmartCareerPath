namespace SmartCareerPath.Domain.Entites.Identity
{
    public class Seeker : AppUser
    {
        public string? LinkedIn { get; set; }
        public int? CurrentJobId { get; set; }
        public LookupValue? CurrentJob { get; set; }
        public ICollection<Answer> Answers { get; set; } = [];
        public ICollection<SeekerQuestionOption> SelectedOptions { get; set; } = [];
        public ICollection<Recommendation> Recommendations { get; set; } = [];
        public ICollection<SeekerRoadmapProgress> RoadmapProgress { get; set; } = [];
        public ICollection<Chat> Chats { get; set; } = [];
    }
}
