namespace SmartCareerPath.Application.DTOs.Mentor
{
    public record MentorDetailDto(
     string Id, string FirstName, string LastName, string Email,
     int YearsOfExperience, int TotalStudentsTaught,
     string? Description, string? Company, string? LinkedIn,
     int? CurrentJobId, string? CurrentJobTitle,
     int? TrackId, string? TrackName, string? Phone);
}
