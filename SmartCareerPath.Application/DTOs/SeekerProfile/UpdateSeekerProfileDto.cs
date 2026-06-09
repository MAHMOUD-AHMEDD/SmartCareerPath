namespace SmartCareerPath.Application.DTOs.SeekerProfile
{
    public record UpdateSeekerProfileDto(
    string FirstName, string LastName,
    string? LinkedIn, int? CurrentJobId);
}
