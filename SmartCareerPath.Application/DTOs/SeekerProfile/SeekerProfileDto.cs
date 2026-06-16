namespace SmartCareerPath.Application.DTOs.SeekerProfile
{
    public record SeekerProfileDto(
    string Id, string FirstName, string LastName, string Email,
    string? LinkedIn, int? CurrentJobId, string? CurrentJobTitle, string? Phone);
}
