using SmartCareerPath.Domain.Entites.Identity;

namespace SmartCareerPath.Domain.Entites
{
    public class Chat
    {
        public int Id { get; set; }
        public string SeekerId { get; set; } = string.Empty;
        public string MentorId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public Seeker Seeker { get; set; } = null!;
        public Mentor Mentor { get; set; } = null!;
        public ICollection<Message> Messages { get; set; } = [];
    }
}
