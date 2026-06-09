using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.DTOs.Mentor
{
    public record UpdateMentorProfileDto(
     string FirstName, string LastName,
     int YearsOfExperience,
     string? Description,
     string? Company,
     string? LinkedIn,
     int? CurrentJobId,
     int? TrackId);
}
