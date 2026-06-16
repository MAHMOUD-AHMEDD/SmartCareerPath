using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Application.Common;
using SmartCareerPath.Application.DTOs.Admin;
using SmartCareerPath.Application.Interfaces;
using SmartCareerPath.Domain.Entites.Identity;
using SmartCareerPath.Infrastructure.Persistence;

namespace SmartCareerPath.Infrastructure.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        public AdminService(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

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
                seeker.LinkedIn, seeker.CurrentJobId, seeker.CurrentJob?.Value, seeker.Phone);
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
                mentor.TrackId, mentor.Track?.Name, mentor.Phone);
        }

        public async Task DeleteSeekerAsync(string seekerId)
        {
            var exists = await _db.Seekers.AnyAsync(s => s.Id == seekerId);
            if (!exists) throw new KeyNotFoundException($"Seeker {seekerId} not found.");

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // 1. Notifications addressed to this seeker (FK: Notification.UserId)
                await _db.Notifications
                    .Where(n => n.UserId == seekerId)
                    .ExecuteDeleteAsync();

                // 2. Chat requests sent by this seeker
                await _db.ChatRequests
                    .Where(r => r.SeekerId == seekerId)
                    .ExecuteDeleteAsync();

                // 3. Messages inside seeker's chats, then the chats themselves
                var chatIds = await _db.Chats
                    .Where(c => c.SeekerId == seekerId)
                    .Select(c => c.Id)
                    .ToListAsync();

                if (chatIds.Any())
                {
                    await _db.Messages
                        .Where(m => chatIds.Contains(m.ChatId))
                        .ExecuteDeleteAsync();

                    await _db.Chats
                        .Where(c => c.SeekerId == seekerId)
                        .ExecuteDeleteAsync();
                }

                // 4. Seeker-specific learning data
                await _db.SeekerRoadmapProgress
                    .Where(p => p.SeekerId == seekerId)
                    .ExecuteDeleteAsync();

                await _db.SeekerQuestionOptions
                    .Where(o => o.SeekerId == seekerId)
                    .ExecuteDeleteAsync();

                await _db.Answers
                    .Where(a => a.SeekerId == seekerId)
                    .ExecuteDeleteAsync();

                await _db.Recommendations
                    .Where(r => r.SeekerId == seekerId)
                    .ExecuteDeleteAsync();

                // 5. Refresh token
                await _db.RefreshTokens
                    .Where(r => r.UserId == seekerId)
                    .ExecuteDeleteAsync();

                // Step 6 — Identity side tables
                await _db.Database.ExecuteSqlRawAsync(
                    "DELETE FROM [dbo].[AspNetUserRoles] WHERE [UserId] = {0}", seekerId);
                await _db.Database.ExecuteSqlRawAsync(
                    "DELETE FROM [dbo].[AspNetUserClaims] WHERE [UserId] = {0}", seekerId);
                await _db.Database.ExecuteSqlRawAsync(
                    "DELETE FROM [dbo].[AspNetUserLogins] WHERE [UserId] = {0}", seekerId);
                await _db.Database.ExecuteSqlRawAsync(
                    "DELETE FROM [dbo].[AspNetUserTokens] WHERE [UserId] = {0}", seekerId);

                // Step 7 — Seekers row (TPT child table — must go before AspNetUsers)
                await _db.Database.ExecuteSqlRawAsync(
                    "DELETE FROM [dbo].[Seekers] WHERE [Id] = {0}", seekerId);

                // Step 8 — AspNetUsers (TPT parent — must be last)
                await _db.Database.ExecuteSqlRawAsync(
                    "DELETE FROM [dbo].[AspNetUsers] WHERE [Id] = {0}", seekerId);

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteMentorAsync(string mentorId)
        {
            var exists = await _db.Mentors.AnyAsync(m => m.Id == mentorId);
            if (!exists) throw new KeyNotFoundException($"Mentor {mentorId} not found.");

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // 1. Notifications addressed to this mentor
                await _db.Notifications
                    .Where(n => n.UserId == mentorId)
                    .ExecuteDeleteAsync();

                // 2. Chat requests sent to this mentor
                await _db.ChatRequests
                    .Where(r => r.MentorId == mentorId)
                    .ExecuteDeleteAsync();

                // 3. Messages inside mentor's chats, then the chats themselves
                var chatIds = await _db.Chats
                    .Where(c => c.MentorId == mentorId)
                    .Select(c => c.Id)
                    .ToListAsync();

                if (chatIds.Any())
                {
                    await _db.Messages
                        .Where(m => chatIds.Contains(m.ChatId))
                        .ExecuteDeleteAsync();

                    await _db.Chats
                        .Where(c => c.MentorId == mentorId)
                        .ExecuteDeleteAsync();
                }

                // 4. Refresh token
                await _db.RefreshTokens
                    .Where(r => r.UserId == mentorId)
                    .ExecuteDeleteAsync();

                // Step 5 — Identity side tables
                await _db.Database.ExecuteSqlRawAsync(
                    "DELETE FROM [dbo].[AspNetUserRoles] WHERE [UserId] = {0}", mentorId);
                await _db.Database.ExecuteSqlRawAsync(
                    "DELETE FROM [dbo].[AspNetUserClaims] WHERE [UserId] = {0}", mentorId);
                await _db.Database.ExecuteSqlRawAsync(
                    "DELETE FROM [dbo].[AspNetUserLogins] WHERE [UserId] = {0}", mentorId);
                await _db.Database.ExecuteSqlRawAsync(
                    "DELETE FROM [dbo].[AspNetUserTokens] WHERE [UserId] = {0}", mentorId);

                // Step 6 — Mentors row (TPT child — before AspNetUsers)
                await _db.Database.ExecuteSqlRawAsync(
                    "DELETE FROM [dbo].[Mentors] WHERE [Id] = {0}", mentorId);

                // Step 7 — AspNetUsers (TPT parent — last)
                await _db.Database.ExecuteSqlRawAsync(
                    "DELETE FROM [dbo].[AspNetUsers] WHERE [Id] = {0}", mentorId);

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}
