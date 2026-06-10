using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Application.Common;
using SmartCareerPath.Application.DTOs.Mentor;
using SmartCareerPath.Application.Interfaces;
using SmartCareerPath.Domain.Entites.Identity;
using SmartCareerPath.Infrastructure.Persistence;

namespace SmartCareerPath.Infrastructure.Services
{
    public class MentorService : IMentorService
    {
        private readonly AppDbContext _db;
        public MentorService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<MentorSummaryDto>> GetAllAsync(
            int page, int pageSize, int? trackId = null)
        {
            // Fix: clamp pagination — same guard as Phase 3 AdminService
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : (pageSize > 50 ? 50 : pageSize);

            var query = _db.Mentors.AsQueryable();

            if (trackId.HasValue)
                query = query.Where(m => m.TrackId == trackId);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(m => m.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MentorSummaryDto(
                    m.Id, m.FirstName, m.LastName,
                    m.YearsOfExperience, m.Company, m.LinkedIn,
                    m.TrackId, m.Track != null ? m.Track.Name : null
                ))
                .ToListAsync();

            return new PagedResult<MentorSummaryDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<MentorSummaryDto> GetByIdAsync(string mentorId)
        {
            var mentor = await _db.Mentors
                .Where(m => m.Id == mentorId)
                .Select(m => new MentorSummaryDto(
                    m.Id, m.FirstName, m.LastName,
                    m.YearsOfExperience, m.Company, m.LinkedIn,
                    m.TrackId, m.Track != null ? m.Track.Name : null))
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException($"Mentor '{mentorId}' not found.");
            return mentor;
        }

        public async Task<MentorDetailDto> GetOwnProfileAsync(string mentorId)
        {
            var mentor = await _db.Mentors
                .Include(m => m.Track)
                .Include(m => m.CurrentJob)
                .FirstOrDefaultAsync(m => m.Id == mentorId)
                ?? throw new KeyNotFoundException("Mentor not found.");
            return MapToDetailDto(mentor);
        }
        public async Task<MentorDetailDto> UpdateOwnProfileAsync(
    string mentorId, UpdateMentorProfileDto dto)
        {
            var mentor = await _db.Mentors
                .FirstOrDefaultAsync(m => m.Id == mentorId)
                ?? throw new KeyNotFoundException("Mentor not found.");

            if (dto.TrackId.HasValue)
            {
                var trackExists = await _db.CareerTracks.AnyAsync(t => t.Id == dto.TrackId);
                if (!trackExists)
                    throw new KeyNotFoundException($"CareerTrack {dto.TrackId} not found.");
            }

            if (dto.CurrentJobId.HasValue)
            {
                var jobExists = await _db.LookupValues.AnyAsync(v => v.Id == dto.CurrentJobId);
                if (!jobExists)
                    throw new KeyNotFoundException($"LookupValue {dto.CurrentJobId} not found.");
            }

            mentor.FirstName = dto.FirstName;
            mentor.LastName = dto.LastName;
            mentor.YearsOfExperience = dto.YearsOfExperience;
            mentor.Description = dto.Description;
            mentor.Company = dto.Company;
            mentor.LinkedIn = dto.LinkedIn;
            mentor.CurrentJobId = dto.CurrentJobId;
            mentor.TrackId = dto.TrackId;

            await _db.SaveChangesAsync();

            // Fix: reload navigation properties after save — TrackId/CurrentJobId FKs
            // were updated but the in-memory nav properties still hold the old objects.
            // Without this, MapToDetailDto returns stale track/job names.
            await _db.Entry(mentor).Reference(m => m.Track).LoadAsync();
            await _db.Entry(mentor).Reference(m => m.CurrentJob).LoadAsync();

            return MapToDetailDto(mentor);
        }
        private static MentorDetailDto MapToDetailDto(Mentor mentor) => new(
       mentor.Id, mentor.FirstName, mentor.LastName, mentor.Email!,
       mentor.YearsOfExperience, mentor.TotalStudentsTaught,
       mentor.Description, mentor.Company, mentor.LinkedIn,
      mentor.CurrentJobId, mentor.CurrentJob?.Value,
      mentor.TrackId, mentor.Track?.Name);

    }
}
