using SmartCareerPath.Domain.Entites.Identity;

namespace SmartCareerPath.Domain.Entites
{
    public class SeekerQuestionOption
    {
        public string SeekerId { get; set; } = string.Empty;
        public int QuestionId { get; set; }
        public int OptionId { get; set; }
        public DateTime SelectedAt { get; set; } = DateTime.UtcNow;
        public Seeker Seeker { get; set; } = null!;
        public Question Question { get; set; } = null!;
        public QuestionOption Option { get; set; } = null!;
    }
}
