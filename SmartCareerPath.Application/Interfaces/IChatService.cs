using SmartCareerPath.Application.Common;
using SmartCareerPath.Application.DTOs.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Interfaces
{
    // Application/Interfaces/IChatService.cs
    public interface IChatService
    {
        // REST operations
        Task<ChatDto> CreateChatAsync(string seekerId, CreateChatDto dto);
        Task<PagedResult<ChatDto>> GetUserChatsAsync(string userId, string role, int page, int pageSize);
        Task<ChatHistoryDto> GetChatHistoryAsync(
            int chatId, string userId, int page, int pageSize);

        // Called by SignalR Hub
        Task<MessageDto> SaveMessageAsync(
            int chatId, string senderId, string senderRole, string content);

        // Authorization helper
        Task<bool> UserBelongsToChatAsync(string userId, int chatId);
    }
}
