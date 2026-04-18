namespace SmartCareerPath.Domain.Entites
{
    public class QuestionOption
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Question Question { get; set; } = null!;
    }
}
