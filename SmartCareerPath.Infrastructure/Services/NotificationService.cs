using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Application.Common;
using SmartCareerPath.Application.DTOs.Notification;
using SmartCareerPath.Application.Interfaces;
using SmartCareerPath.Infrastructure.Persistence;

namespace SmartCareerPath.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _db;
        public NotificationService(AppDbContext db) => _db = db;

        public async Task<PagedResult<NotificationDto>> GetMyNotificationsAsync(
            string userId, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : (pageSize > 50 ? 50 : pageSize);

            var query = _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NotificationDto(
                    n.Id, n.Title, n.Message,
                    n.Type.ToString(),    // enum → string for DTO
                    n.IsRead, n.RelatedEntityId, n.CreatedAt))
                .ToListAsync();

            return new PagedResult<NotificationDto>
            { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
        }

        public async Task<int> GetUnreadCountAsync(string userId)
            => await _db.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId)
                ?? throw new KeyNotFoundException("Notification not found.");

            notification.IsRead = true;
            await _db.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            // ExecuteUpdateAsync: bulk update without loading all rows into memory
            await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(n => n.SetProperty(x => x.IsRead, true));
        }
    }
}
