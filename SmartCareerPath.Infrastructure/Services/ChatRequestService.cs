using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Application.Common;
using SmartCareerPath.Application.DTOs.ChatRequest;
using SmartCareerPath.Application.Interfaces;
using SmartCareerPath.Domain.Entites;
using SmartCareerPath.Domain.Enums;
using SmartCareerPath.Infrastructure.Persistence;

namespace SmartCareerPath.Infrastructure.Services
{
    public class ChatRequestService : IChatRequestService
    {
        private readonly AppDbContext _db;
        public ChatRequestService(AppDbContext db) => _db = db;

        public async Task<ChatRequestDto> SendRequestAsync(
            string seekerId, SendChatRequestDto dto)
        {
            var mentor = await _db.Mentors.FindAsync(dto.MentorId)
                ?? throw new KeyNotFoundException("Mentor not found.");

            var seeker = await _db.Seekers.FindAsync(seekerId)
                ?? throw new KeyNotFoundException("Seeker not found.");

            // Block if a pending request already exists
            var pendingExists = await _db.ChatRequests.AnyAsync(r =>
                r.SeekerId == seekerId &&
                r.MentorId == dto.MentorId &&
                r.Status == ChatRequestStatus.Pending);

            if (pendingExists)
                throw new InvalidOperationException(
                    "You already have a pending request to this mentor.");

            // Block if a chat already exists (already connected)
            var chatExists = await _db.Chats.AnyAsync(c =>
                c.SeekerId == seekerId && c.MentorId == dto.MentorId);

            if (chatExists)
                throw new InvalidOperationException(
                    "You already have an active chat with this mentor.");

            var request = new ChatRequest
            {
                SeekerId = seekerId,
                MentorId = dto.MentorId,
                Status = ChatRequestStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            _db.ChatRequests.Add(request);
            await _db.SaveChangesAsync(); // Save first to get request.Id

            // Notify the mentor (stored in DB — visible when they check their bell)
            _db.Notifications.Add(new Notification
            {
                UserId = dto.MentorId,
                Title = "New Chat Request",
                Message = $"{seeker.FirstName} {seeker.LastName} wants to chat with you.",
                Type = NotificationType.NewChatRequest,
                RelatedEntityId = request.Id,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            return new ChatRequestDto(
                request.Id,
                seekerId, $"{seeker.FirstName} {seeker.LastName}",
                dto.MentorId, $"{mentor.FirstName} {mentor.LastName}",
                request.Status.ToString(), request.RequestedAt, null);
        }

        public async Task<PagedResult<ChatRequestDto>> GetIncomingAsync(
            string mentorId, string? status, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : (pageSize > 50 ? 50 : pageSize);

            var query = _db.ChatRequests.Where(r => r.MentorId == mentorId);

            if (!string.IsNullOrEmpty(status) &&
                Enum.TryParse<ChatRequestStatus>(status, true, out var parsedStatus))
                query = query.Where(r => r.Status == parsedStatus);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.RequestedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ChatRequestDto(
                    r.Id,
                    r.SeekerId, r.Seeker.FirstName + " " + r.Seeker.LastName,
                    r.MentorId, r.Mentor.FirstName + " " + r.Mentor.LastName,
                    r.Status.ToString(), r.RequestedAt, r.RespondedAt))
                .ToListAsync();

            return new PagedResult<ChatRequestDto>
            { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
        }

        public async Task<PagedResult<ChatRequestDto>> GetOutgoingAsync(
            string seekerId, string? status, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : (pageSize > 50 ? 50 : pageSize);

            var query = _db.ChatRequests.Where(r => r.SeekerId == seekerId);

            if (!string.IsNullOrEmpty(status) &&
                Enum.TryParse<ChatRequestStatus>(status, true, out var parsedStatus))
                query = query.Where(r => r.Status == parsedStatus);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.RequestedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ChatRequestDto(
                    r.Id,
                    r.SeekerId, r.Seeker.FirstName + " " + r.Seeker.LastName,
                    r.MentorId, r.Mentor.FirstName + " " + r.Mentor.LastName,
                    r.Status.ToString(), r.RequestedAt, r.RespondedAt))
                .ToListAsync();

            return new PagedResult<ChatRequestDto>
            { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
        }

        public async Task<int> AcceptAsync(int requestId, string mentorId)
        {
            var request = await _db.ChatRequests
                .Include(r => r.Mentor)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.MentorId == mentorId)
                ?? throw new KeyNotFoundException("Chat request not found.");

            if (request.Status != ChatRequestStatus.Pending)
                throw new InvalidOperationException(
                    "This request has already been responded to.");

            // Fix Issue #2: guard against creating a second chat (data corruption / reused request)
            var chatAlreadyExists = await _db.Chats.AnyAsync(c =>
                c.SeekerId == request.SeekerId && c.MentorId == mentorId);

            if (chatAlreadyExists)
                throw new InvalidOperationException(
                    "A chat already exists between this seeker and mentor.");

            request.Status = ChatRequestStatus.Accepted;
            request.RespondedAt = DateTime.UtcNow;

            var chat = new Chat
            {
                SeekerId = request.SeekerId,
                MentorId = mentorId,
                StartDate = DateTime.UtcNow
            };
            _db.Chats.Add(chat);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Fix Issue #3: race condition — two simultaneous Accept calls.
                // The unique index on Chats.(SeekerId, MentorId) from Phase 1 rejects the second.
                // Re-throw as InvalidOperationException so GlobalExceptionMiddleware maps to 400.
                throw new InvalidOperationException(
                    "A chat already exists between this seeker and mentor.");
            }

            // Notify the seeker — RelatedEntityId = chatId so frontend can navigate directly
            _db.Notifications.Add(new Notification
            {
                UserId = request.SeekerId,
                Title = "Chat Request Accepted",
                Message = $"{request.Mentor.FirstName} {request.Mentor.LastName} accepted your request. You can now start chatting!",
                Type = NotificationType.ChatRequestAccepted,
                RelatedEntityId = chat.Id,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            return chat.Id;
        }

        public async Task DeclineAsync(int requestId, string mentorId)
        {
            var request = await _db.ChatRequests
                .Include(r => r.Mentor)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.MentorId == mentorId)
                ?? throw new KeyNotFoundException("Chat request not found.");

            if (request.Status != ChatRequestStatus.Pending)
                throw new InvalidOperationException(
                    "This request has already been responded to.");

            request.Status = ChatRequestStatus.Declined;
            request.RespondedAt = DateTime.UtcNow;

            _db.Notifications.Add(new Notification
            {
                UserId = request.SeekerId,
                Title = "Chat Request Declined",
                Message = $"{request.Mentor.FirstName} {request.Mentor.LastName} declined your chat request.",
                Type = NotificationType.ChatRequestDeclined,
                RelatedEntityId = requestId,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }
    }
}
