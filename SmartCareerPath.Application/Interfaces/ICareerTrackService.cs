using SmartCareerPath.Application.DTOs.CareerTrack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Interfaces
{
    public interface ICareerTrackService
    {
        Task<IEnumerable<CareerTrackDto>> GetAllAsync();
        Task<CareerTrackDto> GetByIdAsync(int id);
        Task<CareerTrackDto> CreateAsync(CreateCareerTrackDto dto);
        Task<CareerTrackDto> UpdateAsync(int id, UpdateCareerTrackDto dto);
        Task DeleteAsync(int id);  // prevent if Roadmaps or Mentors are attached


    }
}
