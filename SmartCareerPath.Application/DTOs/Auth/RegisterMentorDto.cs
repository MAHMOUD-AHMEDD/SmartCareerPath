namespace SmartCareerPath.Application.DTOs.Auth
{
    public record RegisterMentorDto
    (

    string FirstName,
    string LastName,
    string Email,
    string Password,
    int YearsOfExperience,
    string? Company,
    string? Description,
    string? LinkedIn,
    int? CurrentJobId,
    int? TrackId



     );
}
