using SmartCareerPath.Application.DTOs.Roadmap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Interfaces
{
    public interface IRoadmapService
    {
        Task<IEnumerable<RoadmapDto>> GetByTrackAsync(int trackId);
        Task<RoadmapDto> GetByIdAsync(int id);
        Task<RoadmapDto> CreateAsync(int trackId, CreateRoadmapDto dto);
        Task<RoadmapDto> UpdateAsync(int id, UpdateRoadmapDto dto);
        Task DeleteAsync(int id);
    }
}
