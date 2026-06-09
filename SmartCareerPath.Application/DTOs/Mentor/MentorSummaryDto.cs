using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.DTOs.Mentor
{
    public record MentorSummaryDto(
    string Id, string FirstName, string LastName,
    int YearsOfExperience, string? Company, string? LinkedIn,
    int? TrackId, string? TrackName);
}
