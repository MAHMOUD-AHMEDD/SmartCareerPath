namespace SmartCareerPath.Domain.Entites.Identity
{
    public class Mentor : AppUser
    {
        public int YearsOfExperience { get; set; }
        public int TotalStudentsTaught { get; set; }
        public string? Description { get; set; }
        public string? Company { get; set; }
        public string? LinkedIn { get; set; }
        public int? CurrentJobId { get; set; }
        public LookupValue? CurrentJob { get; set; }
        public int? TrackId { get; set; }
        public CareerTrack? Track { get; set; }
        public ICollection<Chat> Chats { get; set; } = [];
    }
}
