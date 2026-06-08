using SmartCareerPath.Application.DTOs.CareerTrack;
using SmartCareerPath.Application.Interfaces;
using SmartCareerPath.Domain.Entites;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Services
{
    public class CareerTrackService : ICareerTrackService
    {
        private readonly AppDbContext _db;
        public CareerTrackService(AppDbContext db) => _db = db;

        public async Task<IEnumerable<CareerTrackDto>> GetAllAsync()
            => await _db.CareerTracks
                .Select(t => new CareerTrackDto(t.Id, t.Name, t.Description))
                .ToListAsync();

        public async Task<CareerTrackDto> GetByIdAsync(int id)
        {
            var track = await _db.CareerTracks.FindAsync(id)
                ?? throw new KeyNotFoundException($"CareerTrack {id} not found.");
            return new CareerTrackDto(track.Id, track.Name, track.Description);
        }

        public async Task<CareerTrackDto> CreateAsync(CreateCareerTrackDto dto)
        {
            var track = new CareerTrack { Name = dto.Name, Description = dto.Description };
            _db.CareerTracks.Add(track);
            await _db.SaveChangesAsync();
            return new CareerTrackDto(track.Id, track.Name, track.Description);
        }

        public async Task<CareerTrackDto> UpdateAsync(int id, UpdateCareerTrackDto dto)
        {
            var track = await _db.CareerTracks.FindAsync(id)
                ?? throw new KeyNotFoundException($"CareerTrack {id} not found.");
            track.Name = dto.Name;
            track.Description = dto.Description;
            await _db.SaveChangesAsync();
            return new CareerTrackDto(track.Id, track.Name, track.Description);
        }

        public async Task DeleteAsync(int id)
        {
            var track = await _db.CareerTracks
                .Include(t => t.Roadmaps)
                .Include(t => t.Mentors)
                .FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new KeyNotFoundException($"Track {id} not found.");

            if (track.Roadmaps.Any())
                throw new InvalidOperationException(
                    "Cannot delete a track with roadmaps. Delete roadmaps first.");

            if (track.Mentors.Any())
                throw new InvalidOperationException(
                    "Cannot delete a track that has mentors assigned to it.");

            _db.CareerTracks.Remove(track);
            await _db.SaveChangesAsync();
        }
    }
}
