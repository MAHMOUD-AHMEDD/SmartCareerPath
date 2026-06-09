using SmartCareerPath.Application.Common;
using SmartCareerPath.Application.DTOs.Mentor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Interfaces
{
    public interface IMentorService
    {
        // Seeker / public browsing
        Task<PagedResult<MentorSummaryDto>> GetAllAsync(
            int page, int pageSize, int? trackId = null);

        // Fix: returns MentorSummaryDto (not MentorDetailDto) — public endpoint must not
        // expose email. Phase 6 test explicitly expects SummaryDto from GET /api/mentors/{id}.
        Task<MentorSummaryDto> GetByIdAsync(string mentorId);

        // Mentor self-management
        Task<MentorDetailDto> GetOwnProfileAsync(string mentorId);
        Task<MentorDetailDto> UpdateOwnProfileAsync(
            string mentorId, UpdateMentorProfileDto dto);
    }
}
