using SmartCareerPath.Domain.Entites.Identity;

namespace SmartCareerPath.Domain.Entites
{
    public class Message
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        // Fix: init (not private set) — EF Core 8 cannot write to private setters when
        // hydrating rows from the DB. History queries return null sender IDs without this fix.
        // Object initializers in Message.Create() work fine with init setters.
        public string? SenderSeekerId { get; init; }   // nullable — only one is set
        public string? SenderMentorId { get; init; }   // nullable — only one is set
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Chat Chat { get; set; } = null!;
        public Seeker? SenderSeeker { get; set; }
        public Mentor? SenderMentor { get; set; }

        // Fix: domain-level factory — enforces mutual exclusivity before the request
        // ever reaches the DB check constraint. Phase 8 SaveMessageAsync uses this.
        public static Message Create(int chatId, string? seekerId, string? mentorId, string content)
        {
            if ((seekerId is null) == (mentorId is null))
                throw new ArgumentException(
                    "Exactly one sender must be set — either seekerId or mentorId.");

            return new Message
            {
                ChatId = chatId,
                SenderSeekerId = seekerId,
                SenderMentorId = mentorId,
                Content = content,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
