namespace SmartCareerPath.Application.DTOs.QuestionnaireSubmission
{
    public record SubmitAnswersDto(
    IEnumerable<MCQAnswerDto> MCQAnswers,
    IEnumerable<OpenTextAnswerDto> OpenTextAnswers);
}
