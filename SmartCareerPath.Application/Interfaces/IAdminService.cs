using SmartCareerPath.Application.Common;
using SmartCareerPath.Application.DTOs.Admin;

namespace SmartCareerPath.Application.Interfaces
{
    public interface IAdminService
    {
        Task<PagedResult<SeekerSummaryDto>> GetAllSeekersAsync(int page, int pageSize);
        Task<PagedResult<MentorSummaryDto>> GetAllMentorsAsync(int page, int pageSize);
        Task<SeekerDetailDto> GetSeekerByIdAsync(string id);
        Task<MentorDetailDto> GetMentorByIdAsync(string id);
    }
}
