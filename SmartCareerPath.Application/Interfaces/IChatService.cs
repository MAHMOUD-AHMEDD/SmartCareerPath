using SmartCareerPath.Application.Common;
using SmartCareerPath.Application.DTOs.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Interfaces
{
    public interface IChatService
    {
        // REST operations
        Task<ChatDto> CreateChatAsync(string seekerId, CreateChatDto dto);
        Task<PagedResult<ChatDto>> GetUserChatsAsync(string userId, string role, int page, int pageSize);
        // userId removed — authorization is enforced by the controller via UserBelongsToChatAsync
        // before this method is called. Accepting userId here without using it is dead code.
        Task<ChatHistoryDto> GetChatHistoryAsync(int chatId, int page, int pageSize);

        // Called by SignalR Hub
        Task<MessageDto> SaveMessageAsync(
            int chatId, string senderId, string senderRole, string content);

        // Authorization helper
        Task<bool> UserBelongsToChatAsync(string userId, int chatId);
    }
}
