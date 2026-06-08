using SmartCareerPath.Application.DTOs.RoadmapItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Interfaces
{
    public interface IRoadmapItemService
    {
        Task<IEnumerable<RoadmapItemDto>> GetByRoadmapAsync(int roadmapId);
        Task<RoadmapItemDto> CreateAsync(int roadmapId, CreateRoadmapItemDto dto);
        Task<RoadmapItemDto> UpdateAsync(int id, UpdateRoadmapItemDto dto);
        Task DeleteAsync(int id);
        Task ReorderAsync(int roadmapId, IEnumerable<int> orderedItemIds); // bonus
    }
}
