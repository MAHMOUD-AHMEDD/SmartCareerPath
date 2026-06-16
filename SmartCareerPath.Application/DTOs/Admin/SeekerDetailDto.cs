namespace SmartCareerPath.Application.DTOs.Admin
{
    //used in admin single-seeker view
    public record SeekerDetailDto(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string? LinkedIn,
    int? CurrentJobId,
    string? CurrentJobTitle,
    string? Phone
);
}
