namespace SmartCareerPath.Application.DTOs.Admin
{
    //used in paginated admin list
    public record SeekerSummaryDto
    (
    string Id,
    string FirstName,
    string LastName,
    string Email
);
}
