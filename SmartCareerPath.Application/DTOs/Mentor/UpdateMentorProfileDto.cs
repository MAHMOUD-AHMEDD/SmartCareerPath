namespace SmartCareerPath.Application.DTOs.Mentor
{
    public record UpdateMentorProfileDto(
     string FirstName, string LastName,
     int YearsOfExperience,
     string? Description,
     string? Company,
     string? LinkedIn,
     int? CurrentJobId,
     int? TrackId, string? Phone);
}
