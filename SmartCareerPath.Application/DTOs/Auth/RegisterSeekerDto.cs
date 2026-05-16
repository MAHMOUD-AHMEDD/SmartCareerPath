namespace SmartCareerPath.Application.DTOs.Auth
{
    public record RegisterSeekerDto
    (

    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? LinkedIn,
    int? CurrentJobId

    );
}
