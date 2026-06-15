using SmartCareerPath.Application.Common;
using SmartCareerPath.Application.DTOs.Notification;

namespace SmartCareerPath.Application.Interfaces
{
    public interface INotificationService
    {
        Task<PagedResult<NotificationDto>> GetMyNotificationsAsync(
            string userId, int page, int pageSize);

        // Returns the badge count — call this on every page load for the navbar bell
        Task<int> GetUnreadCountAsync(string userId);

        Task MarkAsReadAsync(int notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
    }
}
