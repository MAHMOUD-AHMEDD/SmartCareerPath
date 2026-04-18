using SmartCareerPath.Domain.Entites.Identity;

namespace SmartCareerPath.Domain.Entites
{
    public class Message
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public string? SenderSeekerId { get; set; }   // nullable — only one is set
        public string? SenderMentorId { get; set; }   // nullable — only one is set
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Chat Chat { get; set; } = null!;
        public Seeker? SenderSeeker { get; set; }
        public Mentor? SenderMentor { get; set; }

        public static Message Create(
        int chatId, string? seekerId, string? mentorId, string content)
        {
            if ((seekerId == null) == (mentorId == null))
                throw new InvalidOperationException(
                    "Exactly one sender must be set — either SeekerId or MentorId.");

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
