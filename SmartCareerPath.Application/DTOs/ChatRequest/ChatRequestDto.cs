namespace SmartCareerPath.Application.DTOs.ChatRequest
{
    public record ChatRequestDto(
    int Id,
    string SeekerId,
    string SeekerName,
    string MentorId,
    string MentorName,
    string Status,
    DateTime RequestedAt,
    DateTime? RespondedAt
);

}
