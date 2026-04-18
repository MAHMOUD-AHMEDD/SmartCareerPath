using SmartCareerPath.Domain.Entites.Identity;

namespace SmartCareerPath.Domain.Entites
{
    public class Answer
    {
        public int Id { get; set; }
        public string SeekerId { get; set; } = string.Empty;
        public int QuestionId { get; set; }
        public string AnswerText { get; set; } = string.Empty;
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
        public Seeker Seeker { get; set; } = null!;
        public Question Question { get; set; } = null!;
    }
}
