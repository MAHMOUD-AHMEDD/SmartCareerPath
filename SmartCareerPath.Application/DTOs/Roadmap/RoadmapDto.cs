using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.DTOs.Roadmap
{
    public record RoadmapDto(int Id, string Title, string Description, int TrackId);
}
