using SmartCareerPath.Application.Common;
using SmartCareerPath.Application.DTOs.ChatRequest;

namespace SmartCareerPath.Application.Interfaces
{
    public interface IChatRequestService
    {
        Task<ChatRequestDto> SendRequestAsync(string seekerId, SendChatRequestDto dto);

        // status param: "Pending" | "Accepted" | "Declined" | null (all)
        Task<PagedResult<ChatRequestDto>> GetIncomingAsync(
            string mentorId, string? status, int page, int pageSize);

        Task<PagedResult<ChatRequestDto>> GetOutgoingAsync(
            string seekerId, string? status, int page, int pageSize);

        // Returns the new chatId so frontend can navigate directly to the chat
        Task<int> AcceptAsync(int requestId, string mentorId);

        Task DeclineAsync(int requestId, string mentorId);
    }
}
