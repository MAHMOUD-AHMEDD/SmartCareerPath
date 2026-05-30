using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Application.Common;
using SmartCareerPath.Application.DTOs.Admin;
using SmartCareerPath.Application.Interfaces;
using SmartCareerPath.Infrastructure.Persistence;

namespace SmartCareerPath.Infrastructure.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _db;
        public AdminService(AppDbContext db) => _db = db;

        public async Task<PagedResult<SeekerSummaryDto>> GetAllSeekersAsync(int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : (pageSize > 50 ? 50 : pageSize);

            var query = _db.Seekers.AsQueryable();
            var total = await query.CountAsync();
            var items = await query
                .OrderBy(s => s.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SeekerSummaryDto(s.Id, s.FirstName, s.LastName, s.Email!))
                .ToListAsync();
            return new PagedResult<SeekerSummaryDto>
            { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
        }

        public async Task<PagedResult<MentorSummaryDto>> GetAllMentorsAsync(int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : (pageSize > 50 ? 50 : pageSize);

            var query = _db.Mentors.Include(m => m.Track).AsQueryable();
            var total = await query.CountAsync();
            var items = await query
                .OrderBy(m => m.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MentorSummaryDto(
                    m.Id, m.FirstName, m.LastName,
                    m.YearsOfExperience, m.Company, m.LinkedIn,
                    m.TrackId, m.Track != null ? m.Track.Name : null))
                .ToListAsync();
            return new PagedResult<MentorSummaryDto>
            { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
        }

        public async Task<SeekerDetailDto> GetSeekerByIdAsync(string id)
        {
            var seeker = await _db.Seekers
                .Include(s => s.CurrentJob)
                .FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new KeyNotFoundException($"Seeker {id} not found.");
            return new SeekerDetailDto(
                seeker.Id, seeker.FirstName, seeker.LastName, seeker.Email!,
                seeker.LinkedIn, seeker.CurrentJobId, seeker.CurrentJob?.Value);
        }

        public async Task<MentorDetailDto> GetMentorByIdAsync(string id)
        {
            var mentor = await _db.Mentors
                .Include(m => m.Track)
                .Include(m => m.CurrentJob)
                .FirstOrDefaultAsync(m => m.Id == id)
                ?? throw new KeyNotFoundException($"Mentor {id} not found.");
            return new MentorDetailDto(
                mentor.Id, mentor.FirstName, mentor.LastName, mentor.Email!,
                mentor.YearsOfExperience, mentor.TotalStudentsTaught,
                mentor.Description, mentor.Company, mentor.LinkedIn,
                mentor.CurrentJobId, mentor.CurrentJob?.Value,
                mentor.TrackId, mentor.Track?.Name);
        }
    }
}
