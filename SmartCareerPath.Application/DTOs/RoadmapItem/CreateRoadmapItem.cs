using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.DTOs.RoadmapItem
{
    public record CreateRoadmapItemDto(
    string Title, string Description, int OrderIndex, string? Link);
}
