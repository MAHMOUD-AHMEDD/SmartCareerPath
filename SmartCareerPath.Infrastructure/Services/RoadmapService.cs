using SmartCareerPath.Application.DTOs.Roadmap;
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
    public class RoadmapService : IRoadmapService
    {
        private readonly AppDbContext _db;
        public RoadmapService(AppDbContext db) => _db = db;

        public async Task<IEnumerable<RoadmapDto>> GetByTrackAsync(int trackId)
            => await _db.Roadmaps
                .Where(r => r.TrackId == trackId)
                .Select(r => new RoadmapDto(r.Id, r.Title, r.Description, r.TrackId))
                .ToListAsync();

        public async Task<RoadmapDto> GetByIdAsync(int id)
        {
            var r = await _db.Roadmaps.FindAsync(id)
                ?? throw new KeyNotFoundException($"Roadmap {id} not found.");
            return new RoadmapDto(r.Id, r.Title, r.Description, r.TrackId);
        }

        public async Task<RoadmapDto> CreateAsync(int trackId, CreateRoadmapDto dto)
        {
            var trackExists = await _db.CareerTracks.AnyAsync(t => t.Id == trackId);
            if (!trackExists)
                throw new KeyNotFoundException($"CareerTrack {trackId} not found.");

            // Enforce unique roadmap per track (DB also has unique index as safety net)
            var exists = await _db.Roadmaps.AnyAsync(r => r.TrackId == trackId);
            if (exists)
                throw new InvalidOperationException(
                    "A roadmap already exists for this track.");

            var roadmap = new Roadmap
            { TrackId = trackId, Title = dto.Title, Description = dto.Description };
            _db.Roadmaps.Add(roadmap);
            await _db.SaveChangesAsync();
            return new RoadmapDto(roadmap.Id, roadmap.Title, roadmap.Description, roadmap.TrackId);
        }

        public async Task<RoadmapDto> UpdateAsync(int id, UpdateRoadmapDto dto)
        {
            var roadmap = await _db.Roadmaps.FindAsync(id)
                ?? throw new KeyNotFoundException($"Roadmap {id} not found.");
            roadmap.Title = dto.Title;
            roadmap.Description = dto.Description;
            await _db.SaveChangesAsync();
            return new RoadmapDto(roadmap.Id, roadmap.Title, roadmap.Description, roadmap.TrackId);
        }

        public async Task DeleteAsync(int id)
        {
            // Fix: RoadmapItems use DeleteBehavior.Restrict — deleting a roadmap that has items
            // throws a raw DbUpdateException. Guard here for a clean 400 instead.
            var roadmap = await _db.Roadmaps
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new KeyNotFoundException($"Roadmap {id} not found.");

            if (roadmap.Items.Any())
                throw new InvalidOperationException(
                    "Cannot delete a roadmap that has items. Delete items first.");

            _db.Roadmaps.Remove(roadmap);
            await _db.SaveChangesAsync();
        }
    }
}
