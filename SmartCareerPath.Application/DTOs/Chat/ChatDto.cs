using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.DTOs.Chat
{
    public record ChatDto(
    int Id, string SeekerId, string MentorId,
    string SeekerName, string MentorName, DateTime StartDate);
}
