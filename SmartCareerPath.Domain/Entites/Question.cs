namespace SmartCareerPath.Domain.Entites
{
    public class Question
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;  // "MCQ" or "OpenText"
        public ICollection<QuestionOption> Options { get; set; } = [];
        public ICollection<Answer> Answers { get; set; } = [];
    }
}
