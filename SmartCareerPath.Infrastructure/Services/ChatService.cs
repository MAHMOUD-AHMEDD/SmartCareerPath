using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Application.Common;
using SmartCareerPath.Application.Interfaces;
using SmartCareerPath.Application.Constants;
using SmartCareerPath.Application.DTOs.Chat;
using SmartCareerPath.Domain.Entites;
using SmartCareerPath.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Services
{
    public class ChatService : IChatService
    {
        private readonly AppDbContext _db;
        public ChatService(AppDbContext db) => _db = db;

        public async Task<ChatDto> CreateChatAsync(string seekerId, CreateChatDto dto)
        {
            // Validate mentor exists — load name for response
            var mentor = await _db.Mentors
                .FirstOrDefaultAsync(m => m.Id == dto.MentorId)
                ?? throw new KeyNotFoundException("Mentor not found.");

            // Prevent duplicate chat between same seeker and mentor.
            // DB also enforces this via the unique index on (SeekerId, MentorId) from Phase 1.
            var existingChat = await _db.Chats
                .FirstOrDefaultAsync(c =>
                    c.SeekerId == seekerId && c.MentorId == dto.MentorId);

            if (existingChat is not null)
                throw new InvalidOperationException(
                    "A chat already exists between you and this mentor.");

            var seeker = await _db.Seekers.FindAsync(seekerId)
                ?? throw new KeyNotFoundException("Seeker not found.");

            var chat = new Chat
            {
                SeekerId = seekerId,
                MentorId = dto.MentorId,
                StartDate = DateTime.UtcNow
            };

            _db.Chats.Add(chat);
            await _db.SaveChangesAsync();

            return new ChatDto(
                chat.Id, chat.SeekerId, chat.MentorId,
                $"{seeker.FirstName} {seeker.LastName}",
                $"{mentor.FirstName} {mentor.LastName}",
                chat.StartDate
            );
        }
        public async Task<PagedResult<ChatDto>> GetUserChatsAsync(
    string userId, string role, int page, int pageSize)
        {
            // Clamp pagination — same guard pattern as Phase 6 MentorService
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : (pageSize > 50 ? 50 : pageSize);

            var query = role == Roles.Seeker
                ? _db.Chats.Where(c => c.SeekerId == userId)
                : _db.Chats.Where(c => c.MentorId == userId);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ChatDto(
                    c.Id, c.SeekerId, c.MentorId,
                    $"{c.Seeker.FirstName} {c.Seeker.LastName}",
                    $"{c.Mentor.FirstName} {c.Mentor.LastName}",
                    c.StartDate
                ))
                .ToListAsync();

            return new PagedResult<ChatDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ChatHistoryDto> GetChatHistoryAsync(
               int chatId, string userId, int page, int pageSize)
        {
            // Clamp pagination
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : (pageSize > 50 ? 50 : pageSize);

            var chat = await _db.Chats
                .Include(c => c.Seeker)
                .Include(c => c.Mentor)
                .FirstOrDefaultAsync(c => c.Id == chatId)
                ?? throw new KeyNotFoundException($"Chat {chatId} not found.");

            var messagesQuery = _db.Messages
                .Where(m => m.ChatId == chatId)
                .OrderByDescending(m => m.Timestamp);

            var totalCount = await messagesQuery.CountAsync();

            var messages = await messagesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // OrderByDescending fetches newest pages first (correct for pagination),
            // but within a page messages arrive newest-to-oldest which breaks chat UI rendering.
            // Re-sort the materialised page to ascending before mapping to DTOs.
            messages = messages.OrderBy(m => m.Timestamp).ToList();

            // Resolve sender names in bulk — one query each, not per message (avoids N+1)
            var seekerIds = messages
                .Where(m => m.SenderSeekerId != null)
                .Select(m => m.SenderSeekerId!).Distinct().ToList();

            var mentorIds = messages
                .Where(m => m.SenderMentorId != null)
                .Select(m => m.SenderMentorId!).Distinct().ToList();

            var seekerNames = await _db.Seekers
                .Where(s => seekerIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => $"{s.FirstName} {s.LastName}");

            var mentorNames = await _db.Mentors
                .Where(m => mentorIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => $"{m.FirstName} {m.LastName}");

            var chatDto = new ChatDto(
                chat.Id, chat.SeekerId, chat.MentorId,
                $"{chat.Seeker.FirstName} {chat.Seeker.LastName}",
                $"{chat.Mentor.FirstName} {chat.Mentor.LastName}",
                chat.StartDate
            );

            var messageDtos = messages.Select(m =>
            {
                var (senderId, senderRole, senderName) = m.SenderSeekerId != null
                    ? (m.SenderSeekerId,
                       Roles.Seeker,
                       seekerNames.GetValueOrDefault(m.SenderSeekerId!, "Unknown"))
                    : (m.SenderMentorId!,
                       Roles.Mentor,
                       mentorNames.GetValueOrDefault(m.SenderMentorId!, "Unknown"));

                return new MessageDto(
                    m.Id, m.ChatId, senderId, senderName, senderRole, m.Content, m.Timestamp);
            }).ToList();

            return new ChatHistoryDto(
                chatDto,
                new PagedResult<MessageDto>
                {
                    Items = messageDtos,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                }
            );
        }
        public async Task<ChatHistoryDto> GetChatHistoryAsync(
                     int chatId, int page, int pageSize)
        {
            // Clamp pagination
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : (pageSize > 50 ? 50 : pageSize);

            var chat = await _db.Chats
                .Include(c => c.Seeker)
                .Include(c => c.Mentor)
                .FirstOrDefaultAsync(c => c.Id == chatId)
                ?? throw new KeyNotFoundException($"Chat {chatId} not found.");

            var messagesQuery = _db.Messages
                .Where(m => m.ChatId == chatId)
                .OrderByDescending(m => m.Timestamp);

            var totalCount = await messagesQuery.CountAsync();

            var messages = await messagesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // OrderByDescending fetches newest pages first (correct for pagination),
            // but within a page messages arrive newest-to-oldest which breaks chat UI rendering.
            // Re-sort the materialised page to ascending before mapping to DTOs.
            messages = messages.OrderBy(m => m.Timestamp).ToList();

            // Resolve sender names in bulk — one query each, not per message (avoids N+1)
            var seekerIds = messages
                .Where(m => m.SenderSeekerId != null)
                .Select(m => m.SenderSeekerId!).Distinct().ToList();

            var mentorIds = messages
                .Where(m => m.SenderMentorId != null)
                .Select(m => m.SenderMentorId!).Distinct().ToList();

            var seekerNames = await _db.Seekers
                .Where(s => seekerIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => $"{s.FirstName} {s.LastName}");

            var mentorNames = await _db.Mentors
                .Where(m => mentorIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => $"{m.FirstName} {m.LastName}");

            var chatDto = new ChatDto(
                chat.Id, chat.SeekerId, chat.MentorId,
                $"{chat.Seeker.FirstName} {chat.Seeker.LastName}",
                $"{chat.Mentor.FirstName} {chat.Mentor.LastName}",
                chat.StartDate
            );

            var messageDtos = messages.Select(m =>
            {
                var (senderId, senderRole, senderName) = m.SenderSeekerId != null
                    ? (m.SenderSeekerId,
                       Roles.Seeker,
                       seekerNames.GetValueOrDefault(m.SenderSeekerId!, "Unknown"))
                    : (m.SenderMentorId!,
                       Roles.Mentor,
                       mentorNames.GetValueOrDefault(m.SenderMentorId!, "Unknown"));

                return new MessageDto(
                    m.Id, m.ChatId, senderId, senderName, senderRole, m.Content, m.Timestamp);
            }).ToList();

            return new ChatHistoryDto(
                chatDto,
                new PagedResult<MessageDto>
                {
                    Items = messageDtos,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                }
            );
        }
        public async Task<MessageDto> SaveMessageAsync(
                                                 int chatId, string senderId, string senderRole, string content)
        {
            // Validate content length at service layer — hub also checks IsNullOrWhiteSpace,
            // but a max-length guard must live here so the DB and broadcast are both protected.
            if (string.IsNullOrWhiteSpace(content) || content.Length > 2000)
                throw new ArgumentException(
                    "Message content must be between 1 and 2000 characters.");

            // Defense-in-depth membership check at service layer.
            // The hub always calls UserBelongsToChatAsync before SaveMessageAsync, but the
            // service must not trust its caller. Any future caller (REST endpoint, test, etc.)
            // that skips the hub would bypass authorization entirely without this guard.
            var isMember = await _db.Chats.AnyAsync(c =>
                c.Id == chatId &&
                (c.SeekerId == senderId || c.MentorId == senderId));

            if (!isMember)
                throw new UnauthorizedAccessException(
                    "You are not a participant of this chat.");

            // Resolve sender name BEFORE saving — single round-trip (no N+1 post-save)
            string senderName;
            if (senderRole == Roles.Seeker)
            {
                var seeker = await _db.Seekers.FindAsync(senderId)
                    ?? throw new KeyNotFoundException("Seeker not found.");
                senderName = $"{seeker.FirstName} {seeker.LastName}";
            }
            else
            {
                var mentor = await _db.Mentors.FindAsync(senderId)
                    ?? throw new KeyNotFoundException("Mentor not found.");
                senderName = $"{mentor.FirstName} {mentor.LastName}";
            }

            // Chat existence already verified by the membership check above.
            // FindAsync hits the identity-map cache — no extra DB round-trip.
            var chat = await _db.Chats.FindAsync(chatId)
                ?? throw new KeyNotFoundException($"Chat {chatId} not found.");

            // Message.Create() factory (Phase 1) enforces mutual exclusivity at domain layer.
            // DB check constraint CK_Message_Sender (Phase 1 MessageConfiguration) is the second line of defense.
            var message = Message.Create(
                chatId: chatId,
                seekerId: senderRole == Roles.Seeker ? senderId : null,
                mentorId: senderRole == Roles.Mentor ? senderId : null,
                content: content
            );

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            return new MessageDto(
                message.Id, chatId,
                senderId, senderName, senderRole,
                content, message.Timestamp
            );
        }

        public async Task<bool> UserBelongsToChatAsync(string userId, int chatId)
        {
            return await _db.Chats.AnyAsync(c =>
                c.Id == chatId &&
                (c.SeekerId == userId || c.MentorId == userId));
        }
    }
}
