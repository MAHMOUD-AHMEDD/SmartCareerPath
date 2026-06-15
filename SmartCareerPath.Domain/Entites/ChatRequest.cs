using SmartCareerPath.Domain.Entites.Identity;
using SmartCareerPath.Domain.Enums;

namespace SmartCareerPath.Domain.Entites
{
    public class ChatRequest
    {
        public int Id { get; set; }
        public string SeekerId { get; set; } = string.Empty;
        public string MentorId { get; set; } = string.Empty;

        // EF Core stores this as a string via HasConversion<string>() in the configuration.
        public ChatRequestStatus Status { get; set; } = ChatRequestStatus.Pending;
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }

        public Seeker Seeker { get; set; } = null!;
        public Mentor Mentor { get; set; } = null!;
    }
}
