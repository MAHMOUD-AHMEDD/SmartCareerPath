namespace SmartCareerPath.Application.DTOs.Admin
{
    //used in paginated admin list +  public mentor browsing
    public record MentorSummaryDto(
        string Id,
        string FirstName,
        string LastName,
        int YearsOfExperience,
        string? Company,
        string? LinkedIn,
        int? TrackId,
        string? TrackName
    );
}
